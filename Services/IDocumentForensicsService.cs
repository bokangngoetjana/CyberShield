using CyberShield.Models;

namespace CyberShield.Services
{
    public interface IDocumentForensicsService
    {
        Task<DocumentCheckResult> AnalyzeImageAsync(Stream imageStream);
    }
}
