using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Infrastructure.Persistence;
using TravelPlanner.Infrastructure.Persistence.Repositories;

namespace TravelPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Sql");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing required connection string: ConnectionStrings:Sql");
        }

        services.AddDbContext<TravelPlannerDb>(opt =>
            opt.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
