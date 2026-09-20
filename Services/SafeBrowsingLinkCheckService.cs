using System.Text;
using System.Text.Json;
using CyberShield.Models;

namespace CyberShield.Services;

public class SafeBrowsingLinkCheckService : ILinkCheckService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public SafeBrowsingLinkCheckService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["SafeBrowsing:ApiKey"] ?? throw new InvalidOperationException("Safe Browsing API key missing");
    }

    public async Task<LinkCheckResult> CheckUrlAsync(string url)
    {
        var requestBody = new
        {
            client = new { clientId = "cybershield", clientVersion = "1.0.0" },
            threatInfo = new
            {
                threatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE", "POTENTIALLY_HARMFUL_APPLICATION" },
                platformTypes = new[] { "ANY_PLATFORM" },
                threatEntryTypes = new[] { "URL" },
                threatEntries = new[] { new { url } }
            }
        };

        var apiUrl = $"https://safebrowsing.googleapis.com/v4/threatMatches:find?key={_apiKey}";

        var response = await _http.PostAsync(apiUrl,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var signals = new List<string>();
        bool isMatch = doc.RootElement.TryGetProperty("matches", out var matches) && matches.GetArrayLength() > 0;

        if (isMatch)
        {
            foreach (var match in matches.EnumerateArray())
            {
                var threatType = match.GetProperty("threatType").GetString();
                signals.Add($"Flagged by Google Safe Browsing as: {threatType}");
            }
        }
        else
        {
            signals.Add("No known threats found in Google Safe Browsing database");
        }

        // Basic heuristic signals on top of Safe Browsing, cheap and no extra API calls
        if (url.Contains("bit.ly") || url.Contains("tinyurl") || url.Contains("t.co"))
            signals.Add("URL uses a link shortener, real destination is hidden");

        if (!url.StartsWith("https://"))
            signals.Add("Connection is not secured with HTTPS");

        var riskLevel = isMatch ? RiskLevel.Red
            : signals.Count > 1 ? RiskLevel.Orange
            : RiskLevel.Green;

        var trustScore = isMatch ? 5 : signals.Count > 1 ? 55 : 90;

        return new LinkCheckResult
        {
            RiskLevel = riskLevel,
            TrustScore = trustScore,
            Signals = signals,
            Summary = isMatch
                ? "This URL matches known threat databases. Do not visit or enter information."
                : signals.Count > 1
                    ? "This URL has some suspicious characteristics worth caution."
                    : "This URL appears safe based on available checks."
        };
    }
}