using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using TravelPlanner.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var secretsFile = builder.Configuration["Secrets:FilePath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "secrets.development.json");
    if (File.Exists(secretsFile))
    {
        builder.Configuration.AddJsonFile(secretsFile, optional: false, reloadOnChange: true);
    }
}
else if (builder.Environment.IsProduction())
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrWhiteSpace(vaultUri))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
    }
}

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; });

var baseConn = builder.Configuration.GetConnectionString("Sql") ?? throw new InvalidOperationException("No base connection string provided.");

var csb = new SqlConnectionStringBuilder(baseConn);

var cmdTimeout = int.TryParse(builder.Configuration["MIGRATION_COMMAND_TIMEOUT"], out var t) ? t : 180;
var maxRetry = int.TryParse(builder.Configuration["SQL_MAX_RETRIES"], out var r) ? r : 3;

builder.Services.AddDbContext<TravelPlannerDb>(o =>
{
    o.UseSqlServer(csb.ConnectionString, sql =>
    {
        sql.EnableRetryOnFailure(maxRetry, TimeSpan.FromSeconds(10), null);
        sql.CommandTimeout(cmdTimeout);
    });
    
});

var app = builder.Build();

var cts = new CancellationTokenSource();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TravelPlannerDb>();

    // Fast no-op if nothing to do
    var pending = await db.Database.GetPendingMigrationsAsync(cts.Token);
    if (pending.Count() == 0)
    {
        logger.LogInformation("No pending migrations. Exiting.");
        Environment.ExitCode = 0;
        return;
    }

    await EnsureDatabaseExistsAsync(cts.Token);

    await using var conn = new SqlConnection(csb.ConnectionString);
    await conn.OpenAsync(cts.Token);

    // Exclusive applock to avoid concurrent migrators
    var attempts = 0;
    while (true)
    {
        attempts++;
        using var cmd = new SqlCommand("sp_getapplock", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Resource", "TravelPlanner.Migrations");
        cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
        cmd.Parameters.AddWithValue("@LockOwner", "Session");
        cmd.Parameters.AddWithValue("@LockTimeout", 60000);

        var ret = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
        ret.Direction = ParameterDirection.ReturnValue;

        await cmd.ExecuteNonQueryAsync(cts.Token);
        var result = (int)ret.Value;

        if (result >= 0) break;                 // granted/converted
        if (result is -1 or -2 or -3)           // timeout/cancel/deadlock
        {
            if (attempts >= 5) throw new TimeoutException("Unable to acquire applock.");
            await Task.Delay(TimeSpan.FromSeconds(5 * attempts), cts.Token);
            continue;
        }
        throw new Exception($"sp_getapplock failed ({result}).");
    }

    logger.LogInformation($"Applying {pending.Count()} migration(s)…");
    await db.Database.MigrateAsync(cts.Token);
    logger.LogInformation("Migrations applied successfully.");
    Environment.ExitCode = 0;
}
catch (OperationCanceledException)
{
    logger.LogWarning("Migration cancelled.");
    Environment.ExitCode = 143;
}
catch (Exception ex)
{
    logger.LogError(ex, "Migration failed.");
    Environment.ExitCode = 1;
}

async Task EnsureDatabaseExistsAsync(CancellationToken token)
{
    var dbName = csb.InitialCatalog;
    if (string.IsNullOrWhiteSpace(dbName))
        throw new InvalidOperationException("Connection string must include Initial Catalog/Database.");

    try
    {
        await using var test = new SqlConnection(csb.ConnectionString);
        await test.OpenAsync(token);
        await test.CloseAsync();
        return;
    }
    catch (SqlException ex) when (ex.Number == 4060)
    {
        // Database missing -> create via master
    }

    var masterCsb = new SqlConnectionStringBuilder(csb.ConnectionString) { InitialCatalog = "master" };
    await using var master = new SqlConnection(masterCsb.ConnectionString);
    await master.OpenAsync(token);

    var cmdText = @"IF DB_ID(@name) IS NULL CREATE DATABASE [" + dbName + @"];";
    await using var cmd = new SqlCommand(cmdText, master);
    cmd.Parameters.AddWithValue("@name", dbName);
    await cmd.ExecuteNonQueryAsync(token);
}
