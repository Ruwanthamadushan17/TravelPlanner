using Microsoft.Extensions.DependencyInjection;

namespace TravelPlanner.Infrastructure
{
    public static class HealthCheckExtensions
    {
        public static IHealthChecksBuilder AddInfrastructureChecks(this IHealthChecksBuilder checks) =>
            checks.AddDbContextCheck<TravelPlannerDb>("sql", tags: new[] { "ready" });
    }
}
