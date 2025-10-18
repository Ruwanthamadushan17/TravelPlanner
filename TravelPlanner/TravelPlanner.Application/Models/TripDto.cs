namespace TravelPlanner.Application.Models;

public class TripDto
{
    public int Id { get; set; }
    public required string OwnerEmail { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Budget { get; set; }
    public List<DestinationDto> Destinations { get; set; } = new();
}

