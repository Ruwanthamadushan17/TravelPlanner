using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TravelPlanner.Infrastructure;

namespace TravelPlanner.Tests.Integration.Api
{
    public class ApiFactory : WebApplicationFactory<Program>
    { 
        private readonly string _conn; 
        public ApiFactory(string connectionString) => _conn = connectionString; 
        protected override void ConfigureWebHost(IWebHostBuilder builder) 
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                var overrides = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Sql"] = _conn,
                    ["MIGRATION_COMMAND_TIMEOUT"] = "120",
                    ["SQL_MAX_RETRIES"] = "0",
                    ["KeyVault:VaultUri"] = ""
                };
                cfg.AddInMemoryCollection(overrides);
            });

            builder.ConfigureServices(services => 
            { 
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TravelPlannerDb>)); 
                if (descriptor != null) 
                    services.Remove(descriptor);
                
                services.AddDbContext<TravelPlannerDb>(opt => opt.UseSqlServer(_conn)); 
            }); 
        } 
    }
}
