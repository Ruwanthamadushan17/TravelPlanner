using TravelPlanner.Application.Abstractions;

namespace TravelPlanner.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TravelPlannerDb _db;

    public UnitOfWork(TravelPlannerDb db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

