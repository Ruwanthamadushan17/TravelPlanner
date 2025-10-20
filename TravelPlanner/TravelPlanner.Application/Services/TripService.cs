using AutoMapper;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Models;

namespace TravelPlanner.Application.Services;

public class TripService : ITripService
{
    private readonly ITripRepository _trips;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TripService(ITripRepository trips, IUnitOfWork uow, IMapper mapper)
    {
        _trips = trips;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TripDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var trip = await _trips.GetByIdAsync(id, ct);
        return trip is null ? null : _mapper.Map<TripDto>(trip);
    }

    public async Task<IReadOnlyList<TripDto>> ListAsync(string? ownerEmail = null, CancellationToken ct = default)
    {
        var items = await _trips.ListAsync(ownerEmail, ct);
        return _mapper.Map<IReadOnlyList<TripDto>>(items);
    }

    public async Task<TripDto> CreateAsync(CreateTripRequest request, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Domain.Entities.Trip>(request);
        await _trips.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<TripDto>(entity);
    }
}
