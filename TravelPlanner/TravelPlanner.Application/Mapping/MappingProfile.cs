using AutoMapper;
using TravelPlanner.Application.Models;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DestinationCreate, Destination>();
        CreateMap<CreateTripRequest, Trip>()
            .ForMember(d => d.Destinations, opt => opt.MapFrom(s => s.Destinations));

        CreateMap<Destination, DestinationDto>();
        CreateMap<Trip, TripDto>();
    }
}

