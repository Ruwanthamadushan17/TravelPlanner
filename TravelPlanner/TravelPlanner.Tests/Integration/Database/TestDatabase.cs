using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;
using TravelPlanner.MigrationsRunner;

namespace TravelPlanner.Tests.Integration.Database
{
    public class TestDatabase : IAsyncLifetime
    {
        private MsSqlContainer _container = default!;
        private Respawner _respawner = default!;

        public string ConnectionString => new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        }.ConnectionString;

        public async Task InitializeAsync()
        {
            _container = new MsSqlBuilder()
                .WithPassword("Password123!")
                .Build();

            await _container.StartAsync();

            await MigrationRunner.RunAsync(
                ConnectionString,
                commandTimeoutSeconds: 120,
                maxRetryCount: 0,
                useAppLock: false
            );

            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = new[] { "dbo" },
                TablesToIgnore = new Table[] { new("__EFMigrationsHistory", "dbo") },
                WithReseed = true
            });
        }

        public async Task ResetAsync()
        {
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }

        public async Task DisposeAsync() => await _container.DisposeAsync();
    }
}
