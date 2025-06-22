using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookSyst.Models
{
    public class Venue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }
        public List<Event> Events { get; set; } = new();
        public List<Booking> Bookings { get; set; } = new();
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
