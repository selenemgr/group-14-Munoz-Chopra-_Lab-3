using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace group_14_Munoz_Chopra__Lab_3.Controllers
{
    public class AudioController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;

        public AudioController(IConfiguration config, IAmazonS3 s3Client)
        {
            _config = config;
            _s3Client = s3Client;
        }

        [HttpGet]
        public async Task<IActionResult> Play(string url)
        {
            Console.WriteLine($"Full URL: {url}");
            var bucket = _config["AWS:BucketName"];

            var uri = new Uri(url);
            var key = uri.AbsolutePath.TrimStart('/');

            Console.WriteLine($"Bucket: {bucket}, Key: {key}");

            try
            {
                var response = await _s3Client.GetObjectAsync(bucket, key);
                return File(response.ResponseStream,
                            response.Headers.ContentType ?? "audio/mpeg",
                            enableRangeProcessing: true);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"S3 Error: {ex.Message}");
                return NotFound($"S3 object '{key}' not found.");
            }
        }
    }
}
