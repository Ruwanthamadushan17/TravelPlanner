using Azure.Identity;
using TravelPlanner.Api.Middleware;
using TravelPlanner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration sources
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

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
