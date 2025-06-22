namespace EventBookSyst.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
        public List<Booking> Bookings { get; set; } = new();
        public int? EventTypeID { get; set; }
        public EventType? EventType { get; set; }
    }
}
