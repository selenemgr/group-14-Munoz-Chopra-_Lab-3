using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Models;
using group_14_Munoz_Chopra__Lab_3.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Get all podcasts and create view model to display on home page
            var podcasts = _context.Podcasts
              .Include(p => p.Creator)
              .Include(p => p.Subscriptions) 
              .Where(p => p.Creator != null && p.Creator.Role.ToLower() == "podcaster")
              .Select(p => new PodcastViewModel
              {
                  PodcastID = p.PodcastID,
                  Title = p.Title,
                  Description = p.Description,
                  CreatorUsername = p.Creator.Username,
                  SubscriberCount = p.Subscriptions.Count()
              })
              .ToList();

            return View(podcasts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
