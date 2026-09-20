using CyberShield.Models;
using CyberShield.Services;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageScannerController : ControllerBase
    {
        private readonly IMessageScannerService _scanner;
        public MessageScannerController(IMessageScannerService scanner)
        {
            _scanner = scanner;
        }
        [HttpPost("scan")]
        public async Task<ActionResult<ScanResult>> Scan([FromBody] MessageScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text is required.");

            var result = await _scanner.ScanAsync(request.Text);
            return Ok(result);
        }
    }
}
