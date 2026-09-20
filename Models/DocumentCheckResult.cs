namespace CyberShield.Models
{
    public class DocumentCheckResult
    {
        public RiskLevel RiskLevel { get; set; }
        public int TrustScore { get; set; }
        public List<string> Signals { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
