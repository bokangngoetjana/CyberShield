using CyberShield.Models;

namespace CyberShield.Services
{
    public interface ILinkCheckService
    {
        Task<LinkCheckResult> CheckUrlAsync(string url);
    }
}
