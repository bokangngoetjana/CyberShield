using CyberShield.Models;
using DocumentFormat.OpenXml.Packaging;

namespace CyberShield.Services;

public class DocxForensicsService
{
    public DocumentCheckResult Analyze(Stream docxStream)
    {
        var signals = new List<string>();

        using var document = WordprocessingDocument.Open(docxStream, false);
        var coreProps = document.PackageProperties;

        var created = coreProps.Created;
        var modified = coreProps.Modified;
        var revision = coreProps.Revision;
        var creator = coreProps.Creator;
        var lastModifiedBy = coreProps.LastModifiedBy;

        if (created.HasValue && modified.HasValue)
        {
            var gap = (modified.Value - created.Value).TotalDays;
            if (gap > 1)
                signals.Add($"Document was modified {gap:F0} day(s) after it was created");
        }
        else
        {
            signals.Add("Creation or modification date metadata is missing");
        }

        if (!string.IsNullOrWhiteSpace(creator) && !string.IsNullOrWhiteSpace(lastModifiedBy) &&
            !creator.Equals(lastModifiedBy, StringComparison.OrdinalIgnoreCase))
        {
            signals.Add($"Document was authored by '{creator}' but last edited by a different person: '{lastModifiedBy}'");
        }

        if (int.TryParse(revision, out var revisionCount) && revisionCount > 10)
            signals.Add($"Document has a high revision count ({revisionCount}), suggesting significant editing");

        int trustScore = 90;
        if (signals.Any(s => s.Contains("modified"))) trustScore -= 25;
        if (signals.Any(s => s.Contains("different person"))) trustScore -= 30;
        if (signals.Any(s => s.Contains("revision count"))) trustScore -= 20;
        if (signals.Any(s => s.Contains("missing"))) trustScore -= 15;
        trustScore = Math.Clamp(trustScore, 0, 100);

        var riskLevel = trustScore >= 70 ? RiskLevel.Green : trustScore >= 40 ? RiskLevel.Orange : RiskLevel.Red;

        return new DocumentCheckResult
        {
            RiskLevel = riskLevel,
            TrustScore = trustScore,
            Signals = signals,
            Summary = riskLevel switch
            {
                RiskLevel.Red => "This document shows strong signs of post-creation editing.",
                RiskLevel.Orange => "This document has some metadata inconsistencies worth a closer look.",
                _ => "No significant signs of tampering detected in this document."
            }
        };
    }
}