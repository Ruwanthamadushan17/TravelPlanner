using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TravelPlanner.MigrationsRunner;

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

var baseConn = builder.Configuration.GetConnectionString("Sql")
    ?? throw new InvalidOperationException("No base connection string provided.");
var csb = new SqlConnectionStringBuilder(baseConn);

var cmdTimeout = int.TryParse(builder.Configuration["MIGRATION_COMMAND_TIMEOUT"], out var t) ? t : 180;
var maxRetry = int.TryParse(builder.Configuration["SQL_MAX_RETRIES"], out var r) ? r : 3;

using var app = builder.Build();
using var scope = app.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrator");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    // Migrations live in Infrastructure -> keep null
    string? migrationsAssembly = null;

    await MigrationRunner.RunAsync(
        csb.ConnectionString,
        commandTimeoutSeconds: cmdTimeout,
        maxRetryCount: maxRetry,
        useAppLock: true,
        migrationsAssembly: migrationsAssembly,
        logger: logger,
        ct: cts.Token);

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
