namespace TravelPlanner.Domain.Entities
{
    public class DayStep : ItineraryStep
    {
        public int Day { get; set; }
        public string Theme { get; set; } = string.Empty;
        public List<DaySuggestion> Suggestions { get; set; } = new();
    }
}
