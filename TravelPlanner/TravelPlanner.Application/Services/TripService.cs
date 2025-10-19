using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Validation;

namespace TravelPlanner.Application.Services;

public class TripService : ITripService
{
    private readonly ITripRepository _trips;
    private readonly IUnitOfWork _uow;

    public TripService(ITripRepository trips, IUnitOfWork uow)
    {
        _trips = trips;
        _uow = uow;
    }

    public async Task<TripDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var trip = await _trips.GetByIdAsync(id, ct);
        return trip?.ToDto();
    }

    public async Task<IReadOnlyList<TripDto>> ListAsync(string? ownerEmail = null, CancellationToken ct = default)
    {
        var items = await _trips.ListAsync(ownerEmail, ct);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<TripDto> CreateAsync(CreateTripRequest request, CancellationToken ct = default)
    {
        CreateTripRequestValidator.ValidateAndThrow(request);

        var entity = request.ToEntity();
        await _trips.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}

