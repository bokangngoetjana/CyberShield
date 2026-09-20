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
        private readonly ILogger<MessageScannerController> _logger;

        public MessageScannerController(IMessageScannerService scanner, ILogger<MessageScannerController> logger)
        {
            _scanner = scanner;
            _logger = logger;
        }

        [HttpPost("scan")]
        public async Task<ActionResult<ScanResult>> Scan([FromBody] MessageScanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text is required.");

            try
            {
                return Ok(await _scanner.ScanAsync(request.Text));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message scan failed. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                var status = ex switch
                {
                    OperationCanceledException => StatusCodes.Status504GatewayTimeout,
                    HttpRequestException or System.Text.Json.JsonException => StatusCodes.Status502BadGateway,
                    _ => StatusCodes.Status500InternalServerError
                };
                return Problem(statusCode: status, title: "Message scan failed.",
                    detail: $"Unable to complete the scan. Reference: {HttpContext.TraceIdentifier}");
            }
        }
    }
}
