using Microsoft.Extensions.DependencyInjection;
using TravelPlanner.Application.Services;

namespace TravelPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITripService, TripService>();
        return services;
    }
}

