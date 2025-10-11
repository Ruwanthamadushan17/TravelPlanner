namespace TravelPlanner.Domain.Entities
{
    public class Itinerary
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public required Trip Trip { get; set; }
        public decimal TotalCost { get; set; }
        public int TotalTravelMinutes { get; set; }
        public string PlanJson { get; set; } = "{}";
        public DateTimeOffset CreatedAt { get; set; }
    }
}
