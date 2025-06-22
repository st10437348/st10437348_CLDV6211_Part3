using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBookSyst.Models;

namespace EventBookSyst.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.Id.ToString().Contains(searchString) ||
                    b.Event.Name.Contains(searchString));
            }

            var result = await bookings.ToListAsync();
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.Id == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewBag.Events = _context.Event.ToList();
                ViewBag.Venues = _context.Venue.ToList();
                return View(booking);
            }

            var conflict = await _context.Booking
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date);

            if (conflict)
            {
                TempData["ErrorMessage"] = "This venue is already booked for the selected date."; 
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewBag.Events = _context.Event.ToList();
                ViewBag.Venues = _context.Venue.ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Booking.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Failed to create booking. Please try again.");
                }
            }

            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View(booking);
        }



        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Booking deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> BookingDate(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> BookingDate(int id, Booking model)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
                return NotFound();

            if (booking.BookingDate == model.BookingDate)
            {
                TempData["InfoMessage"] = "No changes were made.";
            }
            else
            {
                booking.BookingDate = model.BookingDate;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Date edited successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}