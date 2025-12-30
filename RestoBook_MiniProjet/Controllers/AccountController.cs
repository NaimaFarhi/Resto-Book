using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoBook_MiniProjet.Data;
using RestoBook_MiniProjet.Models;
using RestoBook_MiniProjet.Services;
using RestoBook_MiniProjet.ViewModels;
using RestoBook_MiniProjet.ViewModels.Account;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace RestoBook_MiniProjet.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher _hasher;

        public AccountController(AppDbContext db, PasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // ======================
        // REGISTER
        // ======================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("", "Email already exists");
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = _hasher.Hash(model.Password),
            Role = "Customer",
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }


        // ======================
        // LOGIN
        // ======================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !_hasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("MyReservations", "Reservations");
        }

        // ======================
        // LOGOUT
        // ======================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            

            // Add cache headers to response
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Account");
        }

        // ======================
        // ACCESS DENIED
        // ======================
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
