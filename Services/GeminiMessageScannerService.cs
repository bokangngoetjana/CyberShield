using CyberShield.Models;
using System.Text;
using System.Text.Json;

namespace CyberShield.Services
{
    public class GeminiMessageScannerService : IMessageScannerService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiMessageScannerService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing");
        }
        public async Task<ScanResult> ScanAsync(string text)
        {
            var prompt = $@"
You are a scam-detection engine. Analyze the following message for scam tactics:
urgency, impersonation, requests for payment or personal info, suspicious links,
or too-good-to-be-true offers.

Respond ONLY with valid JSON, no markdown, no backticks:
{{
  ""riskLevel"": ""Green"" | ""Orange"" | ""Red"",
  ""trustScore"": <integer 0-100>,
  ""signals"": [""short phrase per red flag found""],
  ""summary"": ""one sentence explanation""
}}

Message: """"""{text}""""""
";
            var requestBody = new { contents = new[] { new { parts = new [] { new { text = prompt } } } } };
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var response = await _http.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var rawText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
            var cleaned = rawText.Replace("```json", "").Replace("```", "").Trim();

            var parsed = JsonSerializer.Deserialize<GeminiScanShape>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new GeminiScanShape();

            return new ScanResult
            {
                RiskLevel = Enum.TryParse<RiskLevel>(parsed.RiskLevel, out var level) ? level : RiskLevel.Orange,
                TrustScore = parsed.TrustScore,
                Signals = parsed.Signals ?? new List<string>(),
                Summary = parsed.Summary ?? "No summary available."
            };
        }
        private class GeminiScanShape
        {
            public string? RiskLevel { get; set; }
            public int TrustScore { get; set; }
            public List<string>? Signals { get; set; }
            public string? Summary { get; set; }
        }
    }
}
