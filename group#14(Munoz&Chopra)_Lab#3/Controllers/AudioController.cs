using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.S3;
using group_14_Munoz_Chopra__Lab_3.Data;
using group_14_Munoz_Chopra__Lab_3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class AudioController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;
        private readonly ApplicationDbContext _context;


        public AudioController(IConfiguration config, IAmazonS3 s3Client, ApplicationDbContext context)
        {
            _config = config;
            _s3Client = s3Client;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Play(int episodeId, string url)
        {
            // Check if user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            // Track user interaction
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var episode = await _context.Episodes.FirstOrDefaultAsync(e => e.EpisodeID == episodeId);
            if (episode == null) return NotFound();

            var interaction = await _context.EpisodeUserInteractions
                .FirstOrDefaultAsync(i => i.EpisodeID == episodeId && i.UserID == user.UserID);

            if (interaction == null)
            {
                interaction = new EpisodeUserInteraction
                {
                    EpisodeID = episodeId,
                    UserID = user.UserID,
                    Viewed = true,
                    Played = true
                };
                _context.EpisodeUserInteractions.Add(interaction);
                episode.PlayCount += 1;
            }
            else
            {
                if (!interaction.Played)
                {
                    interaction.Played = true;
                    episode.PlayCount += 1;
                }
            }

            await _context.SaveChangesAsync();

            // Play S3 audio
            var bucket = _config["AWS:BucketName"];

            var uri = new Uri(url);
            var key = uri.AbsolutePath.TrimStart('/');

            try
            {
                var response = await _s3Client.GetObjectAsync(bucket, key);
                return File(
                    response.ResponseStream,
                    response.Headers.ContentType ?? "audio/mpeg",
                    enableRangeProcessing: true
                );
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error: {ex.Message}");
                return NotFound($"S3 object '{key}' not found.");
            }
        }
    }
}
