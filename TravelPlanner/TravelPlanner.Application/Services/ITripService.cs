using TravelPlanner.Application.Models;

namespace TravelPlanner.Application.Services;

public interface ITripService
{
    Task<TripDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TripDto>> ListAsync(string? ownerEmail = null, CancellationToken ct = default);
    Task<TripDto> CreateAsync(CreateTripRequest request, CancellationToken ct = default);
}

