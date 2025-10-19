namespace TravelPlanner.Application.Models;

public record CreateTripRequest(
    string OwnerEmail,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    List<DestinationCreate> Destinations
);

