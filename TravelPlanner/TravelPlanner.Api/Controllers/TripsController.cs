using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Services;

namespace TravelPlanner.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TripsController : ControllerBase
{
    private readonly ITripService _service;

    public TripsController(ITripService service)
        => _service = service;

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TripDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripDto>> GetById(int id, CancellationToken ct)
    {
        var trip = await _service.GetByIdAsync(id, ct);
        if (trip is null) return NotFound();
        return Ok(trip);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TripDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TripDto>>> List([FromQuery] string? ownerEmail, CancellationToken ct)
    {
        var items = await _service.ListAsync(ownerEmail, ct);
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TripDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripDto>> Create([FromBody] CreateTripRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
