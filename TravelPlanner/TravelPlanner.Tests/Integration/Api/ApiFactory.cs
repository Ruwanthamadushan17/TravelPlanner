using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TravelPlanner.Infrastructure;

namespace TravelPlanner.Tests.Integration.Api
{
    public class ApiFactory : WebApplicationFactory<Program>
    { 
        private readonly string _conn; 
        public ApiFactory(string connectionString) => _conn = connectionString;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Sql", _conn);

            return base.CreateHost(builder);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder) 
        {
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
