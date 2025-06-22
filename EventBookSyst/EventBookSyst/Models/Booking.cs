namespace EventBookSyst.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int VenueId { get; set; }
        public DateTime BookingDate { get; set; } 

        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}
