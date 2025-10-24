using Amazon.DynamoDBv2.DataModel;
using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Models;
using group_14_Munoz_Chopra__Lab_3.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDynamoDBContext _dbContext;

        public EpisodeController(ApplicationDbContext context, IDynamoDBContext dbContext)
        {
            _context = context;
            _dbContext = dbContext;
        }

        // =========================================================
        // SHOW ALL EPISODES
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var episodes = await _context.Episodes
                .Include(e => e.Podcast)
                .OrderByDescending(e => e.ReleaseDate)
                .ToListAsync();

            return View(episodes);
        }

        // =========================================================
        // EPISODE DETAILS (with comments)
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var episode = await _context.Episodes
                .Include(e => e.Podcast)
                .FirstOrDefaultAsync(e => e.EpisodeID == id);

            if (episode == null || episode.Podcast == null)
                return NotFound();

            // ✅ Fetch comments from DynamoDB by EpisodeID
            // ✅ Safer DynamoDB fetch (works even if EpisodeID isn’t the hash key)
            var comments = await _dbContext.ScanAsync<Comment>(
                new List<ScanCondition> {
        new ScanCondition("EpisodeID", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, episode.EpisodeID)
                }).GetRemainingAsync();


            var userIds = comments.Select(c => c.UserID).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.Username);

            var commentVM = comments.Select(c => new CommentViewModel
            {
                CommentID = c.CommentID,
                EpisodeID = c.EpisodeID,
                UserID = c.UserID,
                Username = users.TryGetValue(c.UserID, out var name) ? name : "Unknown",
                Text = c.Text,
                Timestamp = c.Timestamp
            }).OrderByDescending(c => c.Timestamp).ToList();

            var model = new EpisodeDetailsViewModel
            {
                EpisodeID = episode.EpisodeID,
                Title = episode.Title,
                AudioFileURL = episode.AudioFileURL,
                ReleaseDate = episode.ReleaseDate,
                DurationMinutes = episode.DurationMinutes,
                PlayCount = episode.PlayCount,
                NumberOfViews = episode.NumberOfViews,
                PodcastID = episode.Podcast.PodcastID,
                PodcastTitle = episode.Podcast.Title,
                Comments = commentVM
            };

            return View(model);
        }

        // =========================================================
        // ADD COMMENT (DynamoDB)
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> AddComment(int episodeId, string text)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return BadRequest("User not found.");

            var episode = await _context.Episodes.FirstOrDefaultAsync(e => e.EpisodeID == episodeId);
            if (episode == null) return NotFound();

            var comment = new Comment
            {
                CommentID = new Random().Next(1, int.MaxValue),
                EpisodeID = episode.EpisodeID,
                PodcastID = episode.PodcastID,
                UserID = user.UserID,
                Text = text,
                Timestamp = DateTime.UtcNow
            };

            await _dbContext.SaveAsync(comment);
            return RedirectToAction("Details", new { id = episodeId });
        }

        // =========================================================
        // ✅ EDIT COMMENT (within 24 hours)
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, int episodeId, string text)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // find the current user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return BadRequest("User not found.");

            // load the comment from DynamoDB
            var comment = await _dbContext.LoadAsync<Comment>(commentId, episodeId);
            if (comment == null) return NotFound();

            // allow editing only if same user and within 24 hours
            if (comment.UserID == user.UserID && (DateTime.UtcNow - comment.Timestamp).TotalHours <= 24)
            {
                comment.Text = text;
                await _dbContext.SaveAsync(comment);
                TempData["Message"] = "✅ Comment updated successfully.";
            }
            else
            {
                TempData["Message"] = "❌ Comment can no longer be edited (24-hour limit).";
            }

            return RedirectToAction("Details", new { id = episodeId });
        }

        // =========================================================
        // CREATE EPISODE
        // =========================================================
        [HttpGet]
        public IActionResult Create(int? podcastId)
        {
            ViewBag.Podcasts = _context.Podcasts.ToList();

            var episode = new Episode();
            if (podcastId.HasValue)
            {
                episode.PodcastID = podcastId.Value;
            }

            return View(episode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Episode episode, IFormFile? audioFile)
        {
            if (audioFile != null && audioFile.Length > 0)
            {
                Directory.CreateDirectory("wwwroot/uploads");
                var filePath = Path.Combine("wwwroot/uploads", Path.GetFileName(audioFile.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                episode.AudioFileURL = "/uploads/" + Path.GetFileName(audioFile.FileName);
            }

            episode.ReleaseDate = DateTime.UtcNow;
            episode.PlayCount = 0;
            episode.NumberOfViews = 0;

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            // ✅ Redirect to the podcast’s details page
            return RedirectToAction("Details", "Podcast", new { id = episode.PodcastID });
        }

        // =========================================================
        // EDIT EPISODE
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            ViewBag.Podcasts = _context.Podcasts.ToList();
            return View(episode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Episode updated, IFormFile? audioFile)
        {
            if (id != updated.EpisodeID) return BadRequest();

            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            episode.Title = updated.Title;
            episode.DurationMinutes = updated.DurationMinutes;
            episode.ReleaseDate = updated.ReleaseDate;

            if (audioFile != null && audioFile.Length > 0)
            {
                Directory.CreateDirectory("wwwroot/uploads");
                var filePath = Path.Combine("wwwroot/uploads", Path.GetFileName(audioFile.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }
                episode.AudioFileURL = "/uploads/" + Path.GetFileName(audioFile.FileName);
            }

            await _context.SaveChangesAsync();

            // ✅ Redirect back to podcast details
            return RedirectToAction("Details", "Podcast", new { id = episode.PodcastID });
        }

        // =========================================================
        // DELETE EPISODE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            var podcastId = episode.PodcastID;

            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Podcast", new { id = podcastId });
        }

        // =========================================================
        // INCREMENT VIEW COUNT
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> IncrementViews(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            episode.NumberOfViews++;
            await _context.SaveChangesAsync();

            return Ok(episode.NumberOfViews);
        }
    }
}
