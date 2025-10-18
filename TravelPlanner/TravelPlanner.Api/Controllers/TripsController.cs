using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Validation;

namespace TravelPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _trips;
    private readonly IUnitOfWork _uow;

    public TripsController(ITripRepository trips, IUnitOfWork uow)
    {
        _trips = trips;
        _uow = uow;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TripDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripDto>> GetById(int id, CancellationToken ct)
    {
        var trip = await _trips.GetByIdAsync(id, ct);
        if (trip is null) return NotFound();
        return Ok(trip.ToDto());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TripDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripDto>>> List([FromQuery] string? ownerEmail, CancellationToken ct)
    {
        var items = await _trips.ListAsync(ownerEmail, ct);
        return Ok(items.Select(t => t.ToDto()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TripDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripDto>> Create([FromBody] CreateTripRequest request, CancellationToken ct)
    {
        CreateTripRequestValidator.ValidateAndThrow(request);

        var entity = request.ToEntity();
        await _trips.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }
}
