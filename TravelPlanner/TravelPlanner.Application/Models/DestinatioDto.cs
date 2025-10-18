namespace TravelPlanner.Application.Models
{
    public class DestinationDto
    {
        public int Id { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
    }
}
