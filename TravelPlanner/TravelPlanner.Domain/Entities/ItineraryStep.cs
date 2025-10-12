namespace TravelPlanner.Domain.Entities
{
    public abstract class ItineraryStep
    {
        public int Id { get; set; }
        public int OrderWithInPlan { get; set; }
        public int ItineraryId { get; set; }
        public Itinerary? Itinerary { get; set; }
    }
}
