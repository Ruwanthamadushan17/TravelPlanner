namespace TravelPlanner.Domain.Entities
{
    public class Trip
    {
        public int Id { get; set; }
        public required string OwnerEmail { get; set; } = default!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Budget { get; set; }
        public List<Destination> Destinations { get; } = new();
    }
}
