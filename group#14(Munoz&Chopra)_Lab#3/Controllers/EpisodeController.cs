using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

        public async Task<IActionResult> Details(int id)
        {
            // Check if user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Retrieve episode from SQL database
            var episode = await _context.Episodes
                .Include(e => e.Podcast)
                .FirstOrDefaultAsync(e => e.EpisodeID == id);

            if (episode == null || episode.Podcast == null)
                return NotFound();

            // Query DynamoDB for comments related to episode
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "EpisodeID-index",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "EpisodeID = :v_episodeId",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":v_episodeId", episode.EpisodeID }
                    }
                },
                BackwardSearch = true 
            };

            var search = _dbContext.FromQueryAsync<Comment>(queryConfig);
            var comments = await search.GetRemainingAsync();

            // Get all user IDs from comments
            var userIds = comments.Select(c => c.UserID).Distinct().ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.Username);

            var userComment = comments.Select(c => new CommentViewModel
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
                Comments = userComment
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int episodeId, string text)
        {
            // Check if user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Get the logged-in user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return BadRequest("User not found.");

            // Get the episode to ensure it exists
            var episode = await _context.Episodes.FirstOrDefaultAsync(e => e.EpisodeID == episodeId);
            if (episode == null)
                return NotFound();

            // Create the DynamoDB comment object
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

    }
}
