using EventBookSyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookSyst.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchType, int? venueId, int? eventTypeID, DateTime? startDate, DateTime? endDate)
        {
            var eventDataQuery = _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (eventTypeID.HasValue)
                eventDataQuery = eventDataQuery.Where(e => e.EventTypeID == eventTypeID);

            if (venueId.HasValue)
                eventDataQuery = eventDataQuery.Where(e => e.VenueId == venueId);

            if (startDate.HasValue && endDate.HasValue)
                eventDataQuery = eventDataQuery.Where(e => e.EventDate >= startDate && e.EventDate <= endDate);

            ViewBag.Venue = _context.Venue.ToList();
            ViewBag.EventType = _context.EventType.ToList();

            var eventData = await eventDataQuery.ToListAsync();
            return View(eventData);
        }

        public IActionResult Create()
        {
            ViewBag.Venue = _context.Venue.ToList();
            ViewBag.EventType = _context.EventType.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventData);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Venue = _context.Venue.ToList();
            ViewBag.EventType = _context.EventType.ToList();

            return View(eventData);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventData = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventData == null)
            {
                return NotFound();
            }

            return View(eventData);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventData = await _context.Event.FindAsync(id);

            if (eventData == null)
            {
                return NotFound();
            }
            ViewBag.Venue = _context.Venue.ToList();
            ViewBag.EventType = _context.EventType.ToList();

            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventData)
        {
            if (id != eventData.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Event
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == eventData.Id);

                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    if (existingEvent.Name == eventData.Name &&
                        existingEvent.Description == eventData.Description &&
                        existingEvent.EventDate == eventData.EventDate &&
                        existingEvent.VenueId == eventData.VenueId &&
                        existingEvent.EventTypeID == eventData.EventTypeID) 
                    {
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(eventData);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Event edited successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventData.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Venue = _context.Venue.ToList();
            ViewBag.EventType = _context.EventType.ToList();

            return View(eventData);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventData = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventData == null)
            {
                return NotFound();
            }

            return View(eventData);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            var hasBookings = await _context.Booking.AnyAsync(b => b.EventId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete event because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }
    }
}






