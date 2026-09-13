using BoltonCup.Common.Imaging;
using FluentAssertions;
using SkiaSharp;
using Xunit;

namespace BoltonCup.WebAPI.Tests;

public class RosterImageRendererTests
{
    const string SystemFont = "/System/Library/Fonts/Supplemental/DIN Condensed Bold.ttf";

    [Fact]
    public void Render_ProducesPngOfExpectedSize()
    {
        // Dev-only visual smoke test: depends on a macOS system font. Skip elsewhere (e.g. Linux CI).
        if (!File.Exists(SystemFont))
        {
            return;
        }

        var fontBytes = File.ReadAllBytes(SystemFont);
        var logoBytes = BuildSampleLogo();

        var model = new RosterImageModel
        {
            TeamName = "RED STRIPE ROUGHRIDERS",
            PrimaryHex = "#CE1126",
            SecondaryHex = "#FFFFFF",
            TertiaryHex = "#111111",
            LogoPng = logoBytes,
            FontTtf = fontBytes,
            Colorway = new RosterColorway
            {
                Background = RosterColor.White,
                TitleFill = RosterColor.Primary,
                TitleOutline = RosterColor.Black,
                PositionBarBackground = RosterColor.Black,
                PositionBarOutline = RosterColor.Black,
                PositionBarFill = RosterColor.Black,
                JerseyNumber = RosterColor.Primary,
                JerseyNumberOutline = RosterColor.Black,
                PlayerDetailText = RosterColor.Black,
                CaptaincyFill = RosterColor.Primary,
                CaptaincyOutline = RosterColor.Black,
            },
            Forwards = BuildCells(9, 'F'),
            Defense = BuildCells(6, 'D'),
            Goalies = BuildCells(1, 'G'),
        };

        var renderer = new RosterImageRenderer();
        var png = renderer.Render(model);

        png.Should().NotBeNullOrEmpty();
        using var decoded = SKBitmap.Decode(png);
        decoded.Width.Should().Be(1080);
        decoded.Height.Should().Be(1220);

        // Write to disk for manual visual inspection against sample.png.
        File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "roster-test.png"), png);
    }

    static IReadOnlyList<RosterPlayerCell> BuildCells(int count, char group)
    {
        var names = new[]
        {
            ("Conor", "Pardovich"), ("Ryan", "Macpherson"), ("Kyle", "Macpherson"),
            ("Adam", "Lepain"), ("Brian", "Waddick"), ("Trevor", "Brouwer"),
            ("Lazar", "Dragicevic"), ("Mark", "Mccabe"), ("Sean", "Peltier"),
        };
        var cells = new List<RosterPlayerCell>();
        for (var i = 0; i < count; i++)
        {
            var (first, last) = names[i % names.Length];
            cells.Add(new RosterPlayerCell
            {
                JerseyNumber = (group, i) switch { ('G', _) => 1, _ => (i * 7 + 3) % 99 },
                FirstName = first,
                LastName = last,
                BirthYear = 2000 + i,
                Captaincy = i == 0 ? 'C' : i == 1 ? 'A' : null,
            });
        }
        return cells;
    }

    static byte[] BuildSampleLogo()
    {
        using var surface = SKSurface.Create(new SKImageInfo(200, 200));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        using var circle = new SKPaint { IsAntialias = true, Color = new SKColor(0xFF, 0xFF, 0xFF), Style = SKPaintStyle.Fill };
        canvas.DrawCircle(100, 100, 90, circle);
        using var ring = new SKPaint { IsAntialias = true, Color = new SKColor(0x00, 0x00, 0x00), Style = SKPaintStyle.Stroke, StrokeWidth = 10 };
        canvas.DrawCircle(100, 100, 90, ring);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}