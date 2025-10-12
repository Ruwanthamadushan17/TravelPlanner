namespace TravelPlanner.Domain.Entities
{
    public class DaySuggestion
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int DayStepId { get; set; }
        public DayStep? DayStep { get; set; }
    }
}
