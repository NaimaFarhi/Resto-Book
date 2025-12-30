using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.ViewModels.Admin;

namespace RestoBook_MiniProjet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var model = new AdminDashboardViewModel
            {
                TotalReservations = await _db.Reservations.CountAsync(),
                PendingReservations = await _db.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.Pending),
                ConfirmedReservations = await _db.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.Confirmed),
                CancelledReservations = await _db.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.Cancelled),
                TodayReservations = await _db.Reservations
                    .CountAsync(r => r.ReservationDate.Date == today),
                TotalTables = await _db.RestaurantTables.CountAsync(),
                TotalCustomers = await _db.Users
                    .CountAsync(u => u.Role == "Customer"),

                // ADD THESE TWO LINES:
                TotalMenuItems = await _db.MenuItems.CountAsync(),
                TotalCategories = await _db.MenuCategories.Where(c => c.IsActive).CountAsync()
            };

            // Calculate last 7 days stats
            var sevenDaysAgo = today.AddDays(-6);
            var weeklyData = await _db.Reservations
                .Where(r => r.ReservationDate.Date >= sevenDaysAgo && r.ReservationDate.Date <= today)
                .GroupBy(r => r.ReservationDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Fill holes (days with 0 reservations)
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                var record = weeklyData.FirstOrDefault(w => w.Date == date);
                model.WeeklyReservationsDates.Add(date.ToString("dd/MM"));
                model.WeeklyReservationsCounts.Add(record?.Count ?? 0);
            }

            return View(model);
        }
    }
}