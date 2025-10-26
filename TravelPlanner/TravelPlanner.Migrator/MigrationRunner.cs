using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using TravelPlanner.Infrastructure;

namespace TravelPlanner.MigrationsRunner;

public static class MigrationRunner
{
    public static async Task RunAsync(
        string connectionString,
        int commandTimeoutSeconds = 180,
        int maxRetryCount = 3,
        bool useAppLock = true,
        string? migrationsAssembly = null,   // keep null since your migrations are in Infrastructure
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; }));
        services.AddDbContext<TravelPlannerDb>(o =>
        {
            o.UseSqlServer(connectionString, sql =>
            {
                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                    sql.MigrationsAssembly(migrationsAssembly);
                sql.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(10), null);
                sql.CommandTimeout(commandTimeoutSeconds);
            });
        });

        await using var provider = services.BuildServiceProvider();
        var log = logger ?? provider.GetRequiredService<ILoggerFactory>().CreateLogger("MigrationRunner");

        await EnsureDatabaseExistsAsync(connectionString, ct);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        if (useAppLock)
        {
            using var cmd = new SqlCommand("sp_getapplock", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Resource", "TravelPlanner.Migrations");
            cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
            cmd.Parameters.AddWithValue("@LockOwner", "Session");
            cmd.Parameters.AddWithValue("@LockTimeout", 60000);
            var ret = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
            ret.Direction = ParameterDirection.ReturnValue;
            await cmd.ExecuteNonQueryAsync(ct);
            var result = (int)ret.Value;
            if (result < 0) throw new TimeoutException($"Unable to acquire applock. sp_getapplock -> {result}");
        }

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelPlannerDb>();

        var pending = await db.Database.GetPendingMigrationsAsync(ct);
        if (!pending.Any())
        {
            log.LogInformation("No pending migrations.");
            return;
        }

        log.LogInformation("Applying {Count} migration(s)…", pending.Count());
        await db.Database.MigrateAsync(ct);
        log.LogInformation("Migrations applied successfully.");
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
        var dbName = csb.InitialCatalog;
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Connection string must include Initial Catalog/Database.");

        try
        {
            await using var test = new SqlConnection(connectionString);
            await test.OpenAsync(ct);
            await test.CloseAsync();
            return;
        }
        catch (SqlException ex) when (ex.Number == 4060)
        {
            // DB missing; create on master
        }

        var masterCsb = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        await using var master = new SqlConnection(masterCsb.ConnectionString);
        await master.OpenAsync(ct);
        var cmdText = @"IF DB_ID(@name) IS NULL CREATE DATABASE [" + dbName + @"];";
        await using var cmd = new SqlCommand(cmdText, master);
        cmd.Parameters.AddWithValue("@name", dbName);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
