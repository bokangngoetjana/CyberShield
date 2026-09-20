using CyberShield.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace CyberShield.Services
{
    public class ImageForensicsService : IDocumentForensicsService
    {
        // Software names commonly left in EXIF by editing tools
        private static readonly string[] EditingSoftwareMarkers =
        {
            "photoshop", "gimp", "lightroom", "affinity", "paint.net", "canva"
        };
        public async Task<DocumentCheckResult> AnalyzeImageAsync(Stream imageStream)
        {
            var signals = new List<string>();

            // Copy stream so we can read it more than once (ImageSharp consumes it)
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var image = await Image.LoadAsync<Rgba32>(memoryStream);

            // --- 1. EXIF metadata check ---
            var exif = image.Metadata.ExifProfile;

            if (exif == null || exif.Values.Count == 0)
            {
                signals.Add("No EXIF metadata found (common in screenshots, downloads, or stripped/edited images)");
            }
            else
            {
                var softwareTag = exif.Values.FirstOrDefault(v =>
                    v.Tag.ToString().Equals("Software", StringComparison.OrdinalIgnoreCase));

                if (softwareTag?.GetValue() is string softwareValue &&
                    EditingSoftwareMarkers.Any(marker => softwareValue.ToLowerInvariant().Contains(marker)))
                {
                    signals.Add($"Image metadata shows it was processed with editing software: {softwareValue}");
                }
            }

            // --- 2. Error Level Analysis (ELA) ---
            // Re-encode at a known JPEG quality, then diff against the original pixel-by-pixel.
            // Regions that were edited/pasted often compress differently, showing up as
            // higher average error than the rest of the image.
            memoryStream.Position = 0;
            using var original = await Image.LoadAsync<Rgba32>(memoryStream);
            using var recompressed = original.Clone();

            using var recompressedStream = new MemoryStream();
            await recompressed.SaveAsync(recompressedStream, new JpegEncoder { Quality = 90 });
            recompressedStream.Position = 0;
            using var recompressedImage = await Image.LoadAsync<Rgba32>(recompressedStream);

            double totalDifference = 0;
            int sampledPixels = 0;
            int width = Math.Min(original.Width, recompressedImage.Width);
            int height = Math.Min(original.Height, recompressedImage.Height);


            // Sample every 4th pixel for speed on larger images
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    var p1 = original[x, y];
                    var p2 = recompressedImage[x, y];

                    int diff = Math.Abs(p1.R - p2.R) + Math.Abs(p1.G - p2.G) + Math.Abs(p1.B - p2.B);
                    totalDifference += diff;
                    sampledPixels++;
                }
            }

            double averageError = sampledPixels > 0 ? totalDifference / sampledPixels : 0;

            // Thresholds are heuristic, tune based on test images during your demo prep
            if (averageError > 15)
            {
                signals.Add($"Error Level Analysis shows elevated compression inconsistency (score: {averageError:F1}), which can indicate localized editing");
            }
            else
            {
                signals.Add($"Error Level Analysis shows consistent compression levels (score: {averageError:F1})");
            }

            // --- Combine into a Trust Score ---
            int trustScore = 90;
            if (signals.Any(s => s.Contains("editing software"))) trustScore -= 35;
            if (signals.Any(s => s.Contains("No EXIF"))) trustScore -= 10;
            if (signals.Any(s => s.Contains("elevated compression"))) trustScore -= 35;
            trustScore = Math.Clamp(trustScore, 0, 100);

            var riskLevel = trustScore >= 70 ? RiskLevel.Green
                : trustScore >= 40 ? RiskLevel.Orange
                : RiskLevel.Red;

            return new DocumentCheckResult
            {
                RiskLevel = riskLevel,
                TrustScore = trustScore,
                Signals = signals,
                Summary = riskLevel switch
                {
                    RiskLevel.Red => "This image shows strong signs of digital manipulation.",
                    RiskLevel.Orange => "This image has some characteristics worth a closer look.",
                    _ => "No significant signs of tampering detected in this image."
                }
            };
        }
    }
}
