using CyberShield.Models;

namespace CyberShield.Services
{
    public interface IMessageScannerService
    {
        Task<ScanResult> ScanAsync(string text);
    }
}
