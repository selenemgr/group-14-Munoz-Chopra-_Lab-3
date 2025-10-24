using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Filters;
using group_14_Munoz_Chopra__Lab_3.Models;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    [AdminOnly] // ✅ Restrict access to Admins
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ✅ Admin Dashboard
        public IActionResult Index()
        {
            var totalUsers = _db.Users.Count();
            var totalPodcasts = _db.Podcasts.Count();
            var totalEpisodes = _db.Episodes.Count();

            var top5 = _db.Episodes
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
            var users = _db.Users.AsNoTracking().ToList();
            return View(users); // Views/Admin/Users.cshtml
        }

        // ✅ Update Role (int fixed + TempData)
        [HttpPost]
        public IActionResult SetRole(int userId, string role)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            user.Role = role;
            _db.SaveChanges();

            TempData["Message"] = $"✅ Role for {user.Username} updated to {role}.";
            return RedirectToAction(nameof(Users));
        }

        // ✅ Delete User (int fixed + TempData)
        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            _db.Users.Remove(user);
            _db.SaveChanges();

            TempData["Message"] = $"🗑️ User {user.Username} deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // ✅ Manage Episodes (Global)
        public IActionResult Episodes(string? q, string? host)
        {
            var episodes = _db.Episodes
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

        // ✅ Update Episode Views
        [HttpPost]
        public IActionResult UpdateViews(int episodeId, int views)
        {
            var episode = _db.Episodes.FirstOrDefault(e => e.EpisodeID == episodeId);
            if (episode == null)
                return NotFound();

            episode.NumberOfViews = views;
            _db.SaveChanges();

            TempData["Message"] = $"👁️ Views for '{episode.Title}' updated to {views}.";
            return RedirectToAction(nameof(Episodes));
        }

        // ✅ Delete Episode
        [HttpPost]
        public IActionResult DeleteEpisode(int episodeId)
        {
            var episode = _db.Episodes.FirstOrDefault(e => e.EpisodeID == episodeId);
            if (episode == null)
                return NotFound();

            _db.Episodes.Remove(episode);
            _db.SaveChanges();

            TempData["Message"] = $"🗑️ Episode '{episode.Title}' deleted successfully.";
            return RedirectToAction(nameof(Episodes));
        }

        // ✅ Popular Episodes
        public IActionResult Popular()
        {
            var top = _db.Episodes
                .OrderByDescending(e => e.NumberOfViews)
                .Take(20)
                .ToList();

            return View(top); // Views/Admin/Popular.cshtml
        }

        // ✅ Most Recent Episodes
        public IActionResult Recent()
        {
            var recent = _db.Episodes
                .OrderByDescending(e => e.ReleaseDate)
                .Take(20)
                .ToList();

            return View(recent); // Views/Admin/Recent.cshtml
        }
    }
}
