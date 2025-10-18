using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Abstractions;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Trip>> ListAsync(string? ownerEmail = null, CancellationToken ct = default);
    Task AddAsync(Trip trip, CancellationToken ct = default);
    void Remove(Trip trip);
}

