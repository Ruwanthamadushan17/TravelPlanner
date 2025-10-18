using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Models;

public static class Mapping
{
    public static Trip ToEntity(this CreateTripRequest r)
    {
        var trip = new Trip
        {
            OwnerEmail = r.OwnerEmail,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Budget = r.Budget
        };

        foreach (var d in r.Destinations)
        {
            trip.Destinations.Add(new Destination
            {
                City = d.City,
                Country = d.Country
            });
        }

        return trip;
    }

    public static TripDto ToDto(this Trip t)
    {
        return new TripDto
        {
            Id = t.Id,
            OwnerEmail = t.OwnerEmail,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Budget = t.Budget,
            Destinations = t.Destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                City = d.City,
                Country = d.Country
            }).ToList()
        };
    }
}

