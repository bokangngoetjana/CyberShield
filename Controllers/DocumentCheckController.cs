using CyberShield.Services;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentCheckController : ControllerBase
    {
        private readonly IDocumentForensicsService _forensics;

        public DocumentCheckController(IDocumentForensicsService forensics)
        {
            _forensics = forensics;
        }

        [HttpPost("check-image")]
        public async Task<IActionResult> CheckImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var result = await _forensics.AnalyzeImageAsync(stream);
            return Ok(result);
        }
    }
}
