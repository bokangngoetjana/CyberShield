namespace CyberShield.Models
{
    public enum RiskLevel { Green, Orange, Red }
    public class ScanResult
    {
        public RiskLevel RiskLevel { get; set; }
        public int TrustScore { get; set; }
        public List<string> Signals { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
    public class MessageScanRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
