using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.ViewModels.Tables;

namespace RestoBook_MiniProjet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TablesController : Controller
    {
        private readonly AppDbContext _db;

        public TablesController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // LIST
        // =========================
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var query = _db.RestaurantTables.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(t => 
                    t.TableNumber.ToString().Contains(searchString) ||
                    (t.IsActive ? "active" : "inactive").Contains(lowerSearch)
                );
            }

            // Order by Last Added (Id Descending)
            query = query.OrderByDescending(t => t.Id);

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var tables = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TableListViewModel
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TableList", tables);
            }

            return View(tables);
        }

        // =========================
        // CREATE
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TableCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var table = new RestaurantTable
            {
                TableNumber = model.TableNumber,
                Capacity = model.Capacity,
                IsActive = model.IsActive
            };

            _db.RestaurantTables.Add(table);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // =========================
        // EDIT
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _db.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();

            var model = new TableEditViewModel
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsActive = table.IsActive
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TableEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var table = await _db.RestaurantTables.FindAsync(model.Id);
            if (table == null) return NotFound();

            table.TableNumber = model.TableNumber;
            table.Capacity = model.Capacity;
            table.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _db.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();

            _db.RestaurantTables.Remove(table);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
