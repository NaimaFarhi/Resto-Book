using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.ViewModels.Menu;

namespace RestoBook_MiniProjet.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _db;

        public MenuController(AppDbContext db)
        {
            _db = db;
        }

        // =====================================
        // PUBLIC MENU (All users)
        // =====================================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _db.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Include(c => c.MenuItems.Where(m => m.IsAvailable))
                .ToListAsync();

            var menuViewModel = categories.Select(c => new PublicMenuViewModel
            {
                CategoryName = c.Name,
                Items = c.MenuItems.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl,
                    IsAvailable = m.IsAvailable
                }).ToList()
            }).ToList();

            return View(menuViewModel);
        }

        // =====================================
        // ADMIN - MANAGE CATEGORIES
        // =====================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _db.MenuCategories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new MenuCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(MenuCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new MenuCategory
            {
                Name = model.Name,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            _db.MenuCategories.Add(category);
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Category created successfully.";
            return RedirectToAction("ManageCategories");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _db.MenuCategories.FindAsync(id);
            if (category == null) return NotFound();

            var model = new MenuCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditCategory(MenuCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = await _db.MenuCategories.FindAsync(model.Id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.DisplayOrder = model.DisplayOrder;
            category.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Category updated successfully.";
            return RedirectToAction("ManageCategories");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.MenuCategories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.MenuItems.Any())
            {
                TempData[UiMessageType.Error] = "Cannot delete category with menu items. Delete items first.";
                return RedirectToAction("ManageCategories");
            }

            _db.MenuCategories.Remove(category);
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Category deleted successfully.";
            return RedirectToAction("ManageCategories");
        }

        // =====================================
        // ADMIN - MANAGE MENU ITEMS
        // =====================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageItems()
        {
            var items = await _db.MenuItems
                .Include(m => m.MenuCategory)
                .OrderBy(m => m.MenuCategory.DisplayOrder)
                .ThenBy(m => m.Name)
                .Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    CategoryName = m.MenuCategory.Name,
                    ImageUrl = m.ImageUrl,
                    IsAvailable = m.IsAvailable
                })
                .ToListAsync();

            return View(items);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateItem()
        {
            ViewBag.Categories = await _db.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.MenuCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
                return View(model);
            }

            var item = new MenuItem
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                MenuCategoryId = model.MenuCategoryId,
                ImageUrl = model.ImageUrl,
                IsAvailable = model.IsAvailable,
                CreatedAt = DateTime.Now
            };

            _db.MenuItems.Add(item);
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Menu item created successfully.";
            return RedirectToAction("ManageItems");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            var model = new MenuItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                MenuCategoryId = item.MenuCategoryId,
                ImageUrl = item.ImageUrl,
                IsAvailable = item.IsAvailable
            };

            ViewBag.Categories = await _db.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditItem(MenuItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.MenuCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
                return View(model);
            }

            var item = await _db.MenuItems.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.Price = model.Price;
            item.MenuCategoryId = model.MenuCategoryId;
            item.ImageUrl = model.ImageUrl;
            item.IsAvailable = model.IsAvailable;

            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Menu item updated successfully.";
            return RedirectToAction("ManageItems");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _db.MenuItems.FindAsync(id);
            if (item == null) return NotFound();

            _db.MenuItems.Remove(item);
            await _db.SaveChangesAsync();

            TempData[UiMessageType.Success] = "Menu item deleted successfully.";
            return RedirectToAction("ManageItems");
        }
    }
}