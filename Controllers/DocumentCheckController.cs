using CyberShield.Services;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentCheckController : ControllerBase
    {
        private readonly IDocumentForensicsService _imageForensics;
        private readonly PdfForensicsService _pdfForensics;
        private readonly DocxForensicsService _docxForensics;

        public DocumentCheckController(IDocumentForensicsService imageForensics, PdfForensicsService pdfForensics, DocxForensicsService docxForensics)
        {
            _imageForensics = imageForensics;
            _pdfForensics = pdfForensics;
            _docxForensics = docxForensics;
        }

        [HttpPost("check-image")]
        public async Task<IActionResult> CheckImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            using var stream = file.OpenReadStream();

            var result = extension switch
            {
                ".pdf" => _pdfForensics.Analyze(stream),
                ".docx" => _docxForensics.Analyze(stream),
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => await _imageForensics.AnalyzeImageAsync(stream),
                _ => null
            };
            if (result == null)
                return BadRequest("Unsupported file type. Please upload an image, PDF, or Word document.");

            return Ok(result);
        }
    }
}
