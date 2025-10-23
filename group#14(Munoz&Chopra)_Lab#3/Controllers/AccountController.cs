using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Models;
using Microsoft.AspNetCore.Mvc;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            // Check if username or email already exists
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "Username already exists";
                return View();
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var allowedRoles = new[] { "listener", "podcaster", "admin" };
            if (!allowedRoles.Contains(model.Role?.ToLower()))
            {
                ViewBag.Error = "Invalid role selected";
                return View();
            }

            _context.Users.Add(model);
            _context.SaveChanges();

            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("Role", model.Role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View(); 
        }
    }
}
