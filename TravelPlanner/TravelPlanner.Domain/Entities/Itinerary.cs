namespace TravelPlanner.Domain.Entities
{
    public class Itinerary
    {
        public int Id { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalTravelMinutes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int TripId { get; set; }
        public Trip? Trip { get; set; }
        public List<ItineraryStep> Steps { get; set; } = new();
    }
}
