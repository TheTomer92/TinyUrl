using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TinyUrl.Services;

namespace TinyUrl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TinyUrlController : ControllerBase
    {
        private readonly TinyUrlService _tinyUrlService;

        public TinyUrlController(TinyUrlService tinyUrlService)
        {
            _tinyUrlService = tinyUrlService;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            var shortUrl = await _tinyUrlService.CreateShortUrlAsync(request.LongUrl);
            return CreatedAtAction(nameof(ShortenUrl), new { shortUrl = shortUrl }, new { shortUrl });
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectUrl(string shortUrl)
        {
            var longUrl = await _tinyUrlService.GetLongUrlAsync(shortUrl);
            return string.IsNullOrEmpty(longUrl) ? NotFound() : Redirect(longUrl);
        }
    }

    public class ShortenUrlRequest
    {
        [Required]
        [Url]
        public required string LongUrl { get; set; }
    }
}
