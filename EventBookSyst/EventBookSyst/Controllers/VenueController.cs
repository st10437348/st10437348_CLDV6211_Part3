using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBookSyst.Models;
using Azure.Storage.Blobs;

namespace EventBookSyst.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var venue = await _context.Venue.ToListAsync();
            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (venue.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Please upload an image for the venue.");
            }

            if (ModelState.IsValid)
            {
                var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                venue.ImageUrl = blobUrl;

                _context.Venue.Add(venue);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }



        public async Task<IActionResult> Details(int id)
        {
            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.Id == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venue
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.Id == venue.Id);

                    if (existingVenue == null)
                    {
                        return NotFound();
                    }

                    string newImageUrl = existingVenue.ImageUrl;

                    if (venue.ImageFile != null)
                    {
                        newImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
                    }

                    if (existingVenue.Name == venue.Name &&
                        existingVenue.Location == venue.Location &&
                        existingVenue.Capacity == venue.Capacity &&
                        existingVenue.ImageUrl == newImageUrl)
                    {
                        TempData["InfoMessage"] = "No changes were made.";
                        return RedirectToAction(nameof(Index));
                    }

                    venue.ImageUrl = newImageUrl;

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Venue edited successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.Id == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);

            if (venue == null)
            {
                return NotFound();
            }

            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete venue because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.Id == id);
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=eventeasestorageao;AccountKey=YwTEMPAvQlw4Myjda24gF1qNVSfvgCREtnmdZHEPZFmfF1+rgyfvOXIO75ZA9kutN4QLBYJzTSHR+AStHTPI3A==;EndpointSuffix=core.windows.net";
            var containerName = "cldv6211imagecontainer";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }
    }
}


