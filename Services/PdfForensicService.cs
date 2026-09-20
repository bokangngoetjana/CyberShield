using CyberShield.Models;
using UglyToad.PdfPig;

namespace CyberShield.Services;

public class PdfForensicsService
{
    private static readonly string[] EditingTools = { "photoshop", "acrobat pro", "ilovepdf", "smallpdf", "pdfescape" };

    public DocumentCheckResult Analyze(Stream pdfStream)
    {
        var signals = new List<string>();

        using var document = PdfDocument.Open(pdfStream);
        var info = document.Information;

        // Producer/Creator check
        var producer = info.Producer ?? string.Empty;
        var creator = info.Creator ?? string.Empty;
        var combined = (producer + " " + creator).ToLowerInvariant();

        if (EditingTools.Any(tool => combined.Contains(tool)))
            signals.Add($"PDF was processed with an editing tool ({(string.IsNullOrWhiteSpace(producer) ? creator : producer)})");

        // Creation vs modification date check
        if (TryParsePdfDate(info.CreationDate, out var created) && TryParsePdfDate(info.ModifiedDate, out var modified))
        {
            var gap = (modified - created).TotalDays;
            if (gap > 1)
                signals.Add($"Document was modified {gap:F0} day(s) after it was created");
        }
        else
        {
            signals.Add("Creation or modification date metadata is missing");
        }

        if (string.IsNullOrWhiteSpace(producer) && string.IsNullOrWhiteSpace(creator))
            signals.Add("No producer/creator metadata found");

        int trustScore = 90;
        if (signals.Any(s => s.Contains("editing tool"))) trustScore -= 30;
        if (signals.Any(s => s.Contains("modified"))) trustScore -= 30;
        if (signals.Any(s => s.Contains("missing") || s.Contains("No producer"))) trustScore -= 15;
        trustScore = Math.Clamp(trustScore, 0, 100);

        var riskLevel = trustScore >= 70 ? RiskLevel.Green : trustScore >= 40 ? RiskLevel.Orange : RiskLevel.Red;

        return new DocumentCheckResult
        {
            RiskLevel = riskLevel,
            TrustScore = trustScore,
            Signals = signals,
            Summary = riskLevel switch
            {
                RiskLevel.Red => "This PDF shows signs of being edited after its original creation.",
                RiskLevel.Orange => "This PDF has some metadata inconsistencies worth a closer look.",
                _ => "No significant signs of tampering detected in this PDF."
            }
        };
    }

    private bool TryParsePdfDate(string? pdfDate, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(pdfDate)) return false;

        // PDF dates look like: D:20240115103000
        var cleaned = pdfDate.Replace("D:", "").Substring(0, Math.Min(14, pdfDate.Length));
        return DateTime.TryParseExact(cleaned, "yyyyMMddHHmmss",
            null, System.Globalization.DateTimeStyles.None, out result);
    }
}