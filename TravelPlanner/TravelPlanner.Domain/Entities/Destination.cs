namespace TravelPlanner.Domain.Entities
{
    public class Destination
    {
        public int Id { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public int TripId { get; set; }
        public Trip? Trip { get; set; }
    }
}
