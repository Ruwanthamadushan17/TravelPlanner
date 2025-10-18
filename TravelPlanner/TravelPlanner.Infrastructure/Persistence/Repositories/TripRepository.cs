using Microsoft.EntityFrameworkCore;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence.Repositories;

public class TripRepository : ITripRepository
{
    private readonly TravelPlannerDb _db;

    public TripRepository(TravelPlannerDb db)
    {
        _db = db;
    }

    public async Task AddAsync(Trip trip, CancellationToken ct = default)
    {
        await _db.Trips.AddAsync(trip, ct);
    }

    public async Task<Trip?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Trips
            .Include(t => t.Destinations)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<Trip>> ListAsync(string? ownerEmail = null, CancellationToken ct = default)
    {
        var query = _db.Trips
            .Include(t => t.Destinations)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(ownerEmail))
        {
            query = query.Where(t => t.OwnerEmail == ownerEmail);
        }

        return await query
            .OrderBy(t => t.StartDate)
            .ToListAsync(ct);
    }

    public void Remove(Trip trip)
    {
        _db.Trips.Remove(trip);
    }
}

