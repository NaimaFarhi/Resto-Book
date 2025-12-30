using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.ViewModels.Reservations;
using System.Security.Claims;

namespace RestoBook_MiniProjet.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _db;

        public ReservationsController(AppDbContext db)
        {
            _db = db;
        }

        // =====================================
        // CUSTOMER - MY RESERVATIONS
        // =====================================
        public async Task<IActionResult> MyReservations()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reservations = await _db.Reservations
                .Include(r => r.RestaurantTable)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationListViewModel
                {
                    Id = r.Id,
                    ReservationDate = r.ReservationDate,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status,
                    TableNumber = r.RestaurantTable.TableNumber,
                    CustomerName = r.User.FullName,
                    SpecialRequest = r.SpecialRequest  
                })
                .ToListAsync();

            return View(reservations);
        }

        // =====================================
        // CREATE (GET)
        // =====================================
        [HttpGet]
        public IActionResult Create()
        {
            // No need to pass tables - automatic assignment
            return View();
        }

        // =====================================
        // HELPER: FIND BEST TABLE
        // =====================================
        private async Task<RestaurantTable?> FindBestAvailableTable(DateTime reservationDate, int numberOfGuests)
        {
            // Get all active tables that can accommodate the guests
            var availableTables = await _db.RestaurantTables
                .Where(t => t.IsActive && t.Capacity >= numberOfGuests)
                .ToListAsync();

            if (!availableTables.Any())
                return null;

            // Get reserved table IDs for the given date
            var reservedTableIds = await _db.Reservations
                .Where(r => r.ReservationDate.Date == reservationDate.Date &&
                           r.Status != ReservationStatus.Cancelled)
                .Select(r => r.RestaurantTableId)
                .ToListAsync();

            // Filter out reserved tables
            var freeTables = availableTables
                .Where(t => !reservedTableIds.Contains(t.Id))
                .ToList();

            if (!freeTables.Any())
                return null;

            // Find the best match: smallest table that fits the guests
            var bestTable = freeTables
                .OrderBy(t => t.Capacity)
                .FirstOrDefault(t => t.Capacity >= numberOfGuests);

            return bestTable;
        }

        // =====================================
        // CREATE (POST)
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Create(ReservationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Validate number of guests
            if (model.NumberOfGuests < 1)
            {
                ModelState.AddModelError("NumberOfGuests", "Number of guests must be at least 1.");
                return View(model);
            }

            // Find best available table automatically
            var assignedTable = await FindBestAvailableTable(model.ReservationDate, model.NumberOfGuests);

            if (assignedTable == null)
            {
                ModelState.AddModelError("",
                    $"Sorry, no tables are available for {model.NumberOfGuests} guests on {model.ReservationDate:MMM dd, yyyy}. Please try a different date.");
                return View(model);
            }

            // Handle Admin vs Customer booking
            if (User.IsInRole("Admin"))
            {
                if (string.IsNullOrWhiteSpace(model.GuestName))
                {
                    ModelState.AddModelError("GuestName", "Guest Name is required for Admin bookings.");
                    return View(model);
                }
            }
            else
            {
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            var reservation = new Reservation
            {
                UserId = User.IsInRole("Admin") ? null : userId,
                GuestName = User.IsInRole("Admin") ? model.GuestName : null,
                RestaurantTableId = assignedTable.Id,
                ReservationDate = model.ReservationDate,
                NumberOfGuests = model.NumberOfGuests,
                SpecialRequest = model.SpecialRequest, 
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] =
                $"Reservation created successfully. Table {assignedTable.TableNumber} (capacity: {assignedTable.Capacity}) has been assigned.";

            if (User.IsInRole("Admin"))
                return RedirectToAction("Index");
            else
                return RedirectToAction("MyReservations");
        }

        // =====================================
        // EDIT (GET)
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool isAdmin = User.IsInRole("Admin");

            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            if (!isAdmin && reservation.UserId != userId)
                return Forbid();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData[UiMessageType.Error] =
                    "Only pending reservations can be edited.";
                return RedirectToAction(isAdmin ? "Index" : "MyReservations");
            }

            var model = new ReservationEditViewModel
            {
                Id = reservation.Id,
                ReservationDate = reservation.ReservationDate,
                NumberOfGuests = reservation.NumberOfGuests,
                RestaurantTableId = reservation.RestaurantTableId,
                Status = reservation.Status,
                GuestName = reservation.GuestName,
                SpecialRequest = reservation.SpecialRequest  // ← MAKE SURE THIS IS HERE
            };

            // Only admins can manually change tables
            if (isAdmin)
            {
                ViewBag.Tables = _db.RestaurantTables.Where(t => t.IsActive).ToList();
            }

            return View(model);
        }

        // =====================================
        // EDIT (POST)
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Edit(ReservationEditViewModel model)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool isAdmin = User.IsInRole("Admin");

            var reservation = await _db.Reservations.FindAsync(model.Id);
            if (reservation == null) return NotFound();

            if (!isAdmin && reservation.UserId != userId)
                return Forbid();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData[UiMessageType.Error] =
                    "This reservation can no longer be modified.";
                return RedirectToAction(isAdmin ? "Index" : "MyReservations");
            }

            if (!ModelState.IsValid)
            {
                if (isAdmin)
                {
                    ViewBag.Tables = _db.RestaurantTables.Where(t => t.IsActive).ToList();
                }
                return View(model);
            }

            // For customers: if they change date or guest count, reassign table
            if (!isAdmin)
            {
                bool needsReassignment =
                    reservation.ReservationDate.Date != model.ReservationDate.Date ||
                    reservation.NumberOfGuests != model.NumberOfGuests;

                if (needsReassignment)
                {
                    var newTable = await FindBestAvailableTable(model.ReservationDate, model.NumberOfGuests);

                    if (newTable == null)
                    {
                        ModelState.AddModelError("",
                            $"No tables available for {model.NumberOfGuests} guests on {model.ReservationDate:MMM dd, yyyy}.");
                        return View(model);
                    }

                    reservation.RestaurantTableId = newTable.Id;
                    TempData[UiMessageType.Info] =
                        $"Table reassigned to Table {newTable.TableNumber} (capacity: {newTable.Capacity}).";
                }

                reservation.ReservationDate = model.ReservationDate;
                reservation.NumberOfGuests = model.NumberOfGuests;
                reservation.SpecialRequest = model.SpecialRequest;  // ← MAKE SURE THIS IS HERE
            }
            else
            {
                // Admin can manually change everything
                reservation.ReservationDate = model.ReservationDate;
                reservation.NumberOfGuests = model.NumberOfGuests;
                reservation.RestaurantTableId = model.RestaurantTableId;
                reservation.Status = model.Status;
                reservation.GuestName = model.GuestName;
                reservation.SpecialRequest = model.SpecialRequest;  // ← MAKE SURE THIS IS HERE
            }

            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Reservation updated successfully.";
            return RedirectToAction(isAdmin ? "Index" : "MyReservations");
        }

        // =====================================
        // CANCEL (CUSTOMER & ADMIN)
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool isAdmin = User.IsInRole("Admin");

            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            if (!isAdmin && reservation.UserId != userId)
                return Forbid();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData[UiMessageType.Error] =
                    "This reservation has already been processed.";
                return RedirectToAction(isAdmin ? "Index" : "MyReservations");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Info] = "Reservation cancelled successfully.";
            return RedirectToAction(isAdmin ? "Index" : "MyReservations");
        }

        // =====================================
        // ADMIN - ALL RESERVATIONS
        // =====================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString, DateTime? searchDate, int page = 1)
        {
            var query = _db.Reservations
                .Include(r => r.RestaurantTable)
                .Include(r => r.User)
                .AsQueryable();

            if (searchDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate.Date == searchDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                bool isTime = TimeSpan.TryParse(searchString, out TimeSpan timeVal);

                query = query.Where(r =>
                    (r.User != null && r.User.FullName.ToLower().Contains(lowerSearch)) ||
                    (r.GuestName != null && r.GuestName.ToLower().Contains(lowerSearch)) ||
                    r.RestaurantTable.TableNumber.ToString().Contains(searchString) ||
                    r.Status.ToString().ToLower().Contains(lowerSearch) ||
                    (isTime && r.ReservationDate.Hour == timeVal.Hours && r.ReservationDate.Minute == timeVal.Minutes)
                );
            }

            // Order by Closest Date (Ascending)
            query = query.OrderBy(r => r.ReservationDate);

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var reservations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReservationListViewModel
                {
                    Id = r.Id,
                    ReservationDate = r.ReservationDate,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status,
                    TableNumber = r.RestaurantTable.TableNumber,
                    CustomerName = r.User != null ? r.User.FullName : r.GuestName ?? "Unknown",
                    SpecialRequest = r.SpecialRequest 
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ReservationList", reservations);
            }

            return View(reservations);
        }

        // =====================================
        // ADMIN - CONFIRM
        // =====================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData[UiMessageType.Error] =
                    "This reservation has already been processed.";
                return RedirectToAction("Index");
            }

            reservation.Status = ReservationStatus.Confirmed;
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] =
                "Reservation confirmed successfully.";
            return RedirectToAction("Index");
        }

        // =====================================
        // ADMIN - REFUSE
        // =====================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Refuse(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData[UiMessageType.Error] =
                    "This reservation has already been processed.";
                return RedirectToAction("Index");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Info] =
                "Reservation refused successfully.";
            return RedirectToAction("Index");
        }
    }
}