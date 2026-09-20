using CyberShield.Models;
using CyberShield.Services;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinkCheckController : ControllerBase
    {
        private readonly ILinkCheckService _linkCheck;

        public LinkCheckController(ILinkCheckService linkCheck)
        {
            _linkCheck = linkCheck;
        }

        [HttpPost("check-url")]
        public async Task<ActionResult<LinkCheckResult>> CheckUrl([FromBody] LinkCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("URL is required.");

            var result = await _linkCheck.CheckUrlAsync(request.Url);
            return Ok(result);
        }
    }
}
