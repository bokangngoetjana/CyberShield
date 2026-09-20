namespace CyberShield.Models
{
    public class LinkCheckRequest
    {
        public string Url { get; set; } = string.Empty;
    }
    public class LinkCheckResult
    {
        public RiskLevel RiskLevel { get; set; }
        public int TrustScore { get; set; }
        public List<string> Signals { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
