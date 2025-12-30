using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.ViewModels.Availability;

namespace RestoBook_MiniProjet.Controllers
{
    public class AvailabilityController : Controller
    {
        private readonly AppDbContext _db;

        public AvailabilityController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date, TimeSpan? time)
        {
            var selectedDate = date ?? DateTime.Today;

            // Fetch all reservations for the day to check overlaps in memory
            var reservations = await _db.Reservations
                .Where(r =>
                    r.ReservationDate.Date == selectedDate.Date &&
                    r.Status != ReservationStatus.Cancelled
                )
                .Select(r => new { r.RestaurantTableId, r.ReservationDate })
                .ToListAsync();

            var model = await _db.RestaurantTables
                .Where(t => t.IsActive)
                .Select(t => new AvailabilityViewModel
                {
                    SelectedDate = selectedDate,
                    TableId = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity
                })
                .ToListAsync();

            // Check Availability
            foreach (var item in model)
            {
                // Find conflicting reservation for this table
                var conflict = reservations
                    .Where(r => r.RestaurantTableId == item.TableId)
                    .FirstOrDefault(r => 
                    {
                        if (time.HasValue)
                        {
                            // Time Slot Check: Overlap with 90-minute window
                            // Existing: [ResTime, ResTime + 90m]
                            // Requested: [ReqTime, ReqTime + 90m]
                            
                            var reqTime = selectedDate.Date.Add(time.Value);
                            var resTime = r.ReservationDate;
                            
                            return reqTime < resTime.AddMinutes(90) && resTime < reqTime.AddMinutes(90);
                        }
                        else
                        {
                            // Day View: Any reservation makes it "Unavailable" (or partially booked)
                            return true;
                        }
                    });

                if (conflict != null)
                {
                    item.IsAvailable = false;
                    item.ReservationTime = conflict.ReservationDate.ToString("HH:mm");
                    item.ReservationPeriod = $"{conflict.ReservationDate:HH:mm} - {conflict.ReservationDate.AddMinutes(90):HH:mm}";
                }
                else
                {
                    item.IsAvailable = true;
                }
            }

            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedTime = time?.ToString(@"hh\:mm");
            return View(model);
        }
    }
}
