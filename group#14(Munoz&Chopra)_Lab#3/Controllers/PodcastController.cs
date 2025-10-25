using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class PodcastController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PodcastController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Details(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var podcast = _context.Podcasts
                .Include(p => p.Creator)
                .Include(p => p.Subscriptions)
                .Include(p => p.Episodes)
                .FirstOrDefault(p => p.PodcastID == id && p.Creator.Role.ToLower() == "podcaster");

            if (podcast == null)
            {
                return NotFound();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            var model = new PodcastDetailsViewModel
            {
                PodcastID = podcast.PodcastID,
                Title = podcast.Title,
                Description = podcast.Description,
                CreatorUsername = podcast.Creator?.Username ?? "Unknown",
                CreatedDate = podcast.CreatedDate,
                SubscriberCount = podcast.Subscriptions.Count(),
                IsSubscribed = user != null && podcast.Subscriptions.Any(s => s.UserID == user.UserID),
                Episodes = podcast.Episodes.Select(e => new EpisodeViewModel
                {
                    EpisodeID = e.EpisodeID,
                    Title = e.Title,
                    ReleaseDate = e.ReleaseDate,
                    DurationMinutes = e.DurationMinutes,
                    PlayCount = e.PlayCount,
                    NumberOfViews = e.NumberOfViews,
                    AudioFileURL = e.AudioFileURL
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Subscribe(int podcastId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return BadRequest();

            var existingSubscription = _context.Subscriptions.FirstOrDefault(s => s.UserID == user.UserID && s.PodcastID == podcastId);
            if (existingSubscription == null)
            {
                _context.Subscriptions.Add(new Models.Subscription
                {
                    UserID = user.UserID,
                    PodcastID = podcastId
                });
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = podcastId });
        }

        [HttpPost]
        public IActionResult Unsubscribe(int podcastId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return BadRequest();

            var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserID == user.UserID && s.PodcastID == podcastId);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = podcastId });
        }

        public IActionResult Subscriptions()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return BadRequest();

            // Get all podcasts the user is subscribed to
            var subscribedPodcasts = _context.Subscriptions
                .Where(s => s.UserID == user.UserID)
                .Include(s => s.Podcast)
                    .ThenInclude(p => p.Creator)
                .Select(s => new PodcastViewModel
                {
                    PodcastID = s.Podcast.PodcastID,
                    Title = s.Podcast.Title,
                    Description = s.Podcast.Description,
                    CreatorUsername = s.Podcast.Creator.Username,
                    SubscriberCount = s.Podcast.Subscriptions.Count()
                })
                .ToList();

            return View(subscribedPodcasts);
        }
    }
}
