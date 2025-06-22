namespace EventBookSyst.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }
        public string Name { get; set; }
        public List<Event> Events { get; set; } = new();
    }
}

