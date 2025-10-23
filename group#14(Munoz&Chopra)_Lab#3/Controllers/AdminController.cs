using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    [AdminOnly] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Admin Dashboard
        public IActionResult Index()
        {
            var totalUsers = _context.Users.Count();
            var totalPodcasts = _context.Podcasts.Count();
            var totalEpisodes = _context.Episodes.Count();

            var top5 = _context.Episodes
                .OrderByDescending(e => e.NumberOfViews)
                .Take(5)
                .Select(e => new { e.Title, e.NumberOfViews })
                .ToList();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalPodcasts = totalPodcasts;
            ViewBag.TotalEpisodes = totalEpisodes;
            ViewBag.Top5 = top5;

            return View(); // Views/Admin/Index.cshtml
        }

        // ✅ Manage Users
        public IActionResult Users()
        {
            var users = _context.Users.ToList();
            return View(users); // Views/Admin/Users.cshtml
        }

        [HttpPost]
        public IActionResult SetRole(int userId, string role)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            user.Role = role;
            _context.SaveChanges();
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction(nameof(Users));
        }

        // ✅ Manage Episodes (Global)
        public IActionResult Episodes(string? q, string? host)
        {
            var episodes = _context.Episodes
                .Include(e => e.Podcast)
                .ThenInclude(p => p.Creator)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                episodes = episodes.Where(e =>
                    e.Title.Contains(q) || e.Podcast.Title.Contains(q));

            if (!string.IsNullOrWhiteSpace(host))
                episodes = episodes.Where(e =>
                    e.Podcast.Creator.Username.Contains(host));

            var list = episodes
                .OrderByDescending(e => e.ReleaseDate)
                .ToList();

            return View(list); // Views/Admin/Episodes.cshtml
        }

        [HttpPost]
        public IActionResult UpdateViews(int episodeId, int views)
        {
            var episode = _context.Episodes.FirstOrDefault(e => e.EpisodeID == episodeId);
            if (episode == null)
                return NotFound();

            episode.NumberOfViews = views;
            _context.SaveChanges();
            return RedirectToAction(nameof(Episodes));
        }

        [HttpPost]
        public IActionResult DeleteEpisode(int episodeId)
        {
            var episode = _context.Episodes.FirstOrDefault(e => e.EpisodeID == episodeId);
            if (episode == null)
                return NotFound();

            _context.Episodes.Remove(episode);
            _context.SaveChanges();
            return RedirectToAction(nameof(Episodes));
        }

        // ✅ View Popular Episodes
        public IActionResult Popular()
        {
            var top = _context.Episodes
                .OrderByDescending(e => e.NumberOfViews)
                .Take(20)
                .ToList();

            return View(top); // Views/Admin/Popular.cshtml
        }

        // ✅ View Most Recent Episodes
        public IActionResult Recent()
        {
            var recent = _context.Episodes
                .OrderByDescending(e => e.ReleaseDate)
                .Take(20)
                .ToList();

            return View(recent); // Views/Admin/Recent.cshtml
        }
    }
}
