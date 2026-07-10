using SkiaSharp;

namespace BoltonCup.Common.Imaging;

/// <summary>
/// Draws a team roster card matching the Bolton Cup Instagram template (1080x1220):
/// header (logo + outlined team name), a FORWARDS 3x3 grid, then a DEFENSE 2x3 grid
/// beside a single GOALIE block. Defense fills two of the page's three columns and the
/// goalie occupies the third, so every section shares the same 3-column rhythm. All
/// geometry is expressed as constants below so the layout can be tuned against sample.png.
/// </summary>
public sealed class RosterImageRenderer : IRosterImageRenderer
{
    private const int Width = 1080;
    private const int Height = 1220;
    private const float Margin = 24f;
    private const float ContentW = Width - 2 * Margin;

    private const float HeaderHeight = 150f;
    private const float BarHeight = 44f;
    // Fixed block row height with a tight inter-row gap keeps the rows dense rather than
    // stretching to fill the canvas; the packed content is centered vertically. Sized so the
    // three-forward-rows + three-defense-rows stack still fits the fixed canvas height.
    private const float RowH = 140f;
    private const float GapY = 6f;
    private const float SectionGapY = 12f;

    private const int ForwardCols = 3;
    private const int ForwardRows = 3;
    private const int DefenseCols = 2;
    private const int DefenseRows = 3;

    // The lower section splits into DEFENSE (left) and GOALIE (right).
    private const float DefenseRegionFraction = 2f / 3f;
    private const float RegionGap = 8f;

    // Player-block sub-column split: jersey number | captaincy+logo | name details.
    private const float JerseyColFraction = 0.50f;
    private const float MidColFraction = 0.18f;
    // Jersey number height as a fraction of the block height (large, sample-style numbers).
    private const float JerseyHeightFraction = 0.95f;

    // Horizontal glyph scale applied to all text (<1 = skinnier / more condensed).
    private const float FontScaleX = 0.98f;

    // Extra spacing between letters in the player-detail stack, as a fraction of font size.
    private const float DetailTrackingFactor = 0.05f;

    // Stroke weight added to player-detail glyphs (fraction of font size) to thicken the text.
    private const float DetailStrokeFactor = 0.01f;

    // Fixed player-detail font sizes, identical across every block.
    private const float FirstNameSize = 14f * 1.3f;
    private const float LastNameSize = 22f * 1.3f;
    private const float DetailSize = 14f * 1.3f;

    private static SKFont CreateFont(SKTypeface typeface, float size) =>
        new(typeface, size)
        {
            ScaleX = FontScaleX,
            Edging = SKFontEdging.Antialias,
            Subpixel = true,
            Hinting = SKFontHinting.None,
        };

    // scale supersamples the whole card: the pixel buffer is `scale`x the logical 1080x1220
    // layout, so text and the logo stay crisp when zoomed. Use 1 for cheap previews.
    public byte[] Render(RosterImageModel model, float scale = 1f)
    {
        var primary = ParseColor(model.PrimaryHex, new SKColor(0x20, 0x20, 0x20));
        var secondary = ParseColor(model.SecondaryHex, SKColors.White);
        var tertiary = ParseColor(model.TertiaryHex, SKColors.Black);
        var palette = BuildPalette(model.Colorway, primary, secondary, tertiary);

        using var typeface = SKTypeface.FromData(SKData.CreateCopy(model.FontTtf))
                             ?? SKTypeface.Default;
        using var logo = DecodeLogo(model.LogoPng);

        var pixelW = (int)MathF.Round(Width * scale);
        var pixelH = (int)MathF.Round(Height * scale);
        using var surface = SKSurface.Create(new SKImageInfo(pixelW, pixelH));
        var canvas = surface.Canvas;
        canvas.Clear(palette.Background);
        canvas.Scale(scale);

        using var fillPaint = new SKPaint();
        fillPaint.IsAntialias = true;
        fillPaint.Style = SKPaintStyle.Fill;
        using var strokePaint = new SKPaint();
        strokePaint.IsAntialias = true;
        strokePaint.Style = SKPaintStyle.Stroke;

        const float forwardGridHeight = ForwardRows * RowH + (ForwardRows - 1) * GapY;

        // Defenders bottom-align within the 3-row defense band. Rows freed at the top host a team's
        // overflow forwards (those beyond the 9 the forwards grid holds) and stay empty otherwise. The
        // DEFENSE/GOALIE bars sit directly on top of the defenders — below any overflow row — so the
        // spilled forwards never read as defenders sitting under the DEFENSE label.
        var defenseCount = Math.Min(model.Defense.Count, DefenseCols * DefenseRows);
        var defenderRows = Math.Min(DefenseRows, (defenseCount + DefenseCols - 1) / DefenseCols);
        var freedRows = DefenseRows - defenderRows;

        var overflowBandHeight = freedRows > 0 ? freedRows * RowH + (freedRows - 1) * GapY : 0f;
        var defenderGridHeight = defenderRows > 0 ? defenderRows * RowH + (defenderRows - 1) * GapY : 0f;

        var totalHeight = HeaderHeight + SectionGapY
                          + BarHeight + SectionGapY + forwardGridHeight + SectionGapY
                          + (freedRows > 0 ? overflowBandHeight + SectionGapY : 0f)
                          + BarHeight + SectionGapY + defenderGridHeight;

        // Center the densely-packed content vertically within the fixed canvas.
        var cursorY = Math.Max(Margin, (Height - totalHeight) / 2f);

        DrawHeader(canvas, typeface, model.TeamName, palette.TitleFill, palette.TitleOutline, logo,
            new SKRect(Margin, cursorY, Margin + ContentW, cursorY + HeaderHeight));
        cursorY += HeaderHeight + SectionGapY;

        // FORWARDS
        DrawSectionBar(canvas, typeface, "FORWARDS", palette,
            new SKRect(Margin, cursorY, Margin + ContentW, cursorY + BarHeight));
        cursorY += BarHeight + SectionGapY;

        // Column geometry up front so every jersey number can share one page-wide size.
        var forwardColW = (ContentW - (ForwardCols - 1) * RegionGap) / ForwardCols;
        var defenseRegionW = ContentW * DefenseRegionFraction - RegionGap / 2;
        var goalieRegionX = Margin + defenseRegionW + RegionGap;
        var goalieRegionW = Margin + ContentW - goalieRegionX;
        var defenseColW = (defenseRegionW - (DefenseCols - 1) * RegionGap) / DefenseCols;

        // One jersey-number size for the whole page: the largest that still fits the narrowest block.
        var minBlockW = Math.Min(forwardColW, Math.Min(defenseColW, goalieRegionW));
        var jerseyFontSize = ComputeJerseyFontSize(typeface, minBlockW);

        DrawGrid(canvas, typeface, palette, model.Forwards,
            Margin, cursorY, forwardColW, RowH, ForwardCols, ForwardRows, jerseyFontSize);

        cursorY += forwardGridHeight + SectionGapY;

        // Overflow forwards (beyond the 9 that fit above) fill the freed rows, above the DEFENSE bar.
        if (freedRows > 0)
        {
            var overflow = OverflowForwards(model.Forwards, freedRows * DefenseCols);
            DrawGrid(canvas, typeface, palette, overflow,
                Margin, cursorY, defenseColW, RowH, DefenseCols, freedRows, jerseyFontSize);
            cursorY += overflowBandHeight + SectionGapY;
        }

        // DEFENSE | GOALIE bars, shifted down to sit directly on top of the defenders.
        DrawSectionBar(canvas, typeface, "DEFENSE", palette,
            new SKRect(Margin, cursorY, Margin + defenseRegionW, cursorY + BarHeight));
        DrawSectionBar(canvas, typeface, "GOALIE", palette,
            new SKRect(goalieRegionX, cursorY, goalieRegionX + goalieRegionW, cursorY + BarHeight));
        cursorY += BarHeight + SectionGapY;

        DrawGrid(canvas, typeface, palette, model.Defense,
            Margin, cursorY, defenseColW, RowH, DefenseCols, defenderRows, jerseyFontSize);

        // Single goalie block, kept the same height as one defense row so it isn't oversized.
        var goalie = model.Goalies.Count > 0 ? model.Goalies[0] : EmptyCell();
        DrawPlayerBlock(canvas, typeface, palette, goalie,
            new SKRect(goalieRegionX, cursorY, goalieRegionX + goalieRegionW, cursorY + RowH), jerseyFontSize);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private void DrawHeader(SKCanvas canvas, SKTypeface typeface, string teamName,
        SKColor titleFill, SKColor titleOutline, SKBitmap? logo, SKRect rect)
    {
        var logoSize = rect.Height;
        var textX = rect.Left;
        if (logo is not null)
        {
            var dst = new SKRect(rect.Left, rect.Top, rect.Left + logoSize, rect.Top + logoSize);
            DrawBitmapContained(canvas, logo, dst);
            textX = dst.Right + 20f;
        }

        var name = teamName.ToUpperInvariant();

        // Scale the title to fill the width from after the logo to where the bars end (with a small
        // right padding), capped by the header height so short names don't become oversized.
        const float rightPadding = 8f;
        var avail = rect.Right - textX - rightPadding;
        using var probe = CreateFont(typeface, 100f);
        var width100 = probe.MeasureText(name);
        var widthSize = width100 > 0 ? 100f * (avail / width100) : rect.Height;
        var size = Math.Min(widthSize, rect.Height * 0.95f);

        using var font = CreateFont(typeface, size);
        font.Embolden = true;
        var baseline = VerticalCenterBaseline(font, rect.MidY);

        using var fill = new SKPaint();
        fill.IsAntialias = true;
        fill.Style = SKPaintStyle.Fill;
        fill.Color = titleFill;
        using var stroke = new SKPaint();
        stroke.IsAntialias = true;
        stroke.Style = SKPaintStyle.Stroke;
        stroke.Color = titleOutline;
        // Draw the outline UNDERNEATH the fill: a centred stroke drawn on top of the fill eats into
        // each glyph by half its width, so the surviving outer band varies letter-to-letter and looks
        // uneven. Stroking first and painting the fill over it leaves a uniform outer band (= half the
        // stroke width) with the glyph interior fully intact.
        stroke.StrokeWidth = Math.Max(2f, size * 0.022f);
        stroke.StrokeJoin = SKStrokeJoin.Round;
        canvas.DrawText(name, textX, baseline, SKTextAlign.Left, font, stroke);
        canvas.DrawText(name, textX, baseline, SKTextAlign.Left, font, fill);
    }

    private static void DrawSectionBar(SKCanvas canvas, SKTypeface typeface, string label,
        Palette palette, SKRect rect)
    {
        using var bg = new SKPaint();
        bg.IsAntialias = true;
        bg.Style = SKPaintStyle.Fill;
        bg.Color = palette.PositionBarBackground;
        canvas.DrawRect(rect, bg);

        const float borderWidth = 3f;
        using var border = new SKPaint();
        border.IsAntialias = true;
        border.Style = SKPaintStyle.Stroke;
        border.Color = palette.PositionBarOutline;
        border.StrokeWidth = borderWidth;
        var inset = rect;
        inset.Inflate(-borderWidth / 2f, -borderWidth / 2f);
        canvas.DrawRect(inset, border);

        var size = FitTextSize(typeface, label, rect.Width - 24f, rect.Height * 0.82f);
        using var font = CreateFont(typeface, size);
        font.Embolden = true;
        using var text = new SKPaint();
        text.IsAntialias = true;
        text.Style = SKPaintStyle.Fill;
        text.Color = palette.PositionBarFill;
        var baseline = VerticalCenterBaseline(font, rect.MidY);
        canvas.DrawText(label, rect.MidX, baseline, SKTextAlign.Center, font, text);
    }

    private void DrawGrid(SKCanvas canvas, SKTypeface typeface, Palette palette,
        IReadOnlyList<RosterPlayerCell> cells, float originX, float originY,
        float colW, float rowH, int cols, int rows, float jerseyFontSize)
    {
        for (var i = 0; i < cols * rows; i++)
        {
            var cell = i < cells.Count ? cells[i] : EmptyCell();
            var col = i % cols;
            var row = i / cols;
            var x = originX + col * (colW + RegionGap);
            var y = originY + row * (rowH + GapY);
            DrawPlayerBlock(canvas, typeface, palette, cell, new SKRect(x, y, x + colW, y + rowH), jerseyFontSize);
        }
    }

    // The forwards a team carries beyond the 9 the forwards grid holds, laid out into `slotCount`
    // freed defense-region slots left-to-right. Slots past the overflow count render as empty.
    private static IReadOnlyList<RosterPlayerCell> OverflowForwards(
        IReadOnlyList<RosterPlayerCell> forwards, int slotCount)
    {
        const int forwardGridSlots = ForwardCols * ForwardRows;

        var cells = new RosterPlayerCell[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            var index = forwardGridSlots + i;
            cells[i] = index < forwards.Count ? forwards[index] : EmptyCell();
        }
        return cells;
    }

    private void DrawPlayerBlock(SKCanvas canvas, SKTypeface typeface, Palette palette,
        RosterPlayerCell cell, SKRect rect, float jerseyFontSize)
    {
        using var detail = new SKPaint();
        detail.IsAntialias = true;
        detail.Style = SKPaintStyle.Fill;
        detail.Color = palette.DetailText;
        using var detailDim = new SKPaint();
        detailDim.IsAntialias = true;
        detailDim.Style = SKPaintStyle.Fill;
        detailDim.Color = palette.DetailText.WithAlpha(0xCC);

        // Nothing renders for an empty padding slot — not even the team logo.
        if (cell.IsEmpty)
            return;

        var rowH = rect.Height / 4f;
        var jerseyW = rect.Width * JerseyColFraction;
        var midW = rect.Width * MidColFraction;

        // Sub-col 1: jersey number — zero-padded to 2 digits, fixed size and fixed digit-cell
        // width so every number on the page is identical in height and width.
        if (cell.JerseyNumber is { } number)
        {
            var text = number.ToString().PadLeft(2, '0');
            DrawFixedWidthNumber(canvas, typeface, text, rect.Left + jerseyW / 2f, rect.MidY,
                jerseyFontSize, palette.JerseyFill, palette.JerseyOutline);
        }

        var midCenterX = rect.Left + jerseyW + midW / 2f;

        // Sub-col 2 row 1: captaincy.
        if (cell.Captaincy is { } cap)
        {
            var capSize = FitTextSize(typeface, cap.ToString(), midW - 4f, rowH * 1.15f);
            using var capFont = CreateFont(typeface, capSize);
            capFont.Embolden = true;
            var baseline = VerticalCenterBaseline(capFont, rect.Top + rowH * 0.5f);
            DrawTextFilledOutlined(canvas, cap.ToString(), midCenterX, baseline,
                SKTextAlign.Center, capFont, palette.CaptaincyFill, palette.CaptaincyOutline, Math.Max(1.5f, capSize * 0.05f));
        }

        // Sub-col 2 rows 3-4: the player's previous-team logo (bottom half), centered in the mid column.
        using var logo = DecodeLogo(cell.PreviousTeamLogoPng);
        if (logo is not null)
        {
            var logoBox = new SKRect(rect.Left + jerseyW + 2f, rect.Top + rowH * 2f, rect.Left + jerseyW + midW - 2f, rect.Bottom - 4f);
            // Shrink the logo to ~80% of the mid-column box and nudge it left toward the jersey number.
            const float logoScale = 0.8f;
            const float logoShiftLeft = 8f;
            var logoCenterX = logoBox.MidX - logoShiftLeft;
            var logoCenterY = logoBox.MidY;
            var logoHalfW = logoBox.Width * logoScale / 2f;
            var logoHalfH = logoBox.Height * logoScale / 2f;
            logoBox = new SKRect(logoCenterX - logoHalfW, logoCenterY - logoHalfH,
                logoCenterX + logoHalfW, logoCenterY + logoHalfH);
            DrawBitmapContained(canvas, logo, logoBox);
        }

        // Sub-col 3: name / year / hometown, tightly stacked and vertically centered in the block.
        var stack = new List<(string Text, float Size, SKPaint Paint)>
        {
            (cell.FirstName.ToUpperInvariant(), FirstNameSize, detail),
            (cell.LastName.ToUpperInvariant(), LastNameSize, detail),
        };
        if (cell.BirthYear is { } year)
        {
            stack.Add((year.ToString(), DetailSize, detailDim));
        }
        stack.Add((cell.Hometown.ToUpperInvariant(), DetailSize, detailDim));

        DrawTextStack(canvas, typeface, stack, rect.Right - 4f, rect.MidY);
    }

    // Draws right-aligned text rows at fixed sizes, stacked with tight leading, vertically centered on centerY.
    private static void DrawTextStack(SKCanvas canvas, SKTypeface typeface,
        IReadOnlyList<(string Text, float Size, SKPaint Paint)> rows, float right, float centerY)
    {
        const float lineFactor = 1.1f;

        var visible = rows.Where(r => !string.IsNullOrEmpty(r.Text)).ToList();
        var totalHeight = visible.Sum(r => r.Size * lineFactor);
        var top = centerY - totalHeight / 2f;

        foreach (var (text, size, paint) in visible)
        {
            var lineHeight = size * lineFactor;
            using var font = CreateFont(typeface, size);
            using var stroke = new SKPaint();
            stroke.IsAntialias = true;
            stroke.Style = SKPaintStyle.Stroke;
            stroke.Color = paint.Color;
            stroke.StrokeWidth = size * DetailStrokeFactor;
            stroke.StrokeJoin = SKStrokeJoin.Round;
            var baseline = VerticalCenterBaseline(font, top + lineHeight / 2f);
            DrawTrackedTextRight(canvas, text, right, baseline, font, paint, stroke, size * DetailTrackingFactor);
            top += lineHeight;
        }
    }

    private static void DrawTextFilledOutlined(SKCanvas canvas, string text, float x, float baseline,
        SKTextAlign align, SKFont font, SKColor fill, SKColor outline, float strokeWidth)
    {
        using var fillPaint = new SKPaint();
        fillPaint.IsAntialias = true;
        fillPaint.Style = SKPaintStyle.Fill;
        fillPaint.Color = fill;
        using var strokePaint = new SKPaint();
        strokePaint.IsAntialias = true;
        strokePaint.Style = SKPaintStyle.Stroke;
        strokePaint.Color = outline;
        strokePaint.StrokeWidth = strokeWidth;
        strokePaint.StrokeJoin = SKStrokeJoin.Round;
        canvas.DrawText(text, x, baseline, align, font, fillPaint);
        canvas.DrawText(text, x, baseline, align, font, strokePaint);
    }

    // Largest jersey size that fits the narrowest block, using a fixed two-digit width budget.
    private static float ComputeJerseyFontSize(SKTypeface typeface, float minBlockW)
    {
        var heightBound = RowH * JerseyHeightFraction;
        var avail = minBlockW * JerseyColFraction - 6f;
        if (avail <= 0)
        {
            return heightBound;
        }
        using var font = CreateFont(typeface, heightBound);
        var twoDigitWidth = MaxDigitWidth(font) * 2f;
        return twoDigitWidth <= avail ? heightBound : Math.Max(6f, heightBound * (avail / twoDigitWidth));
    }

    private static float MaxDigitWidth(SKFont font)
    {
        var max = 0f;
        for (var d = '0'; d <= '9'; d++)
        {
            max = Math.Max(max, font.MeasureText(d.ToString()));
        }
        return max;
    }

    // Renders each digit centered in a fixed-width cell, so equal-length numbers share an exact width.
    private static void DrawFixedWidthNumber(SKCanvas canvas, SKTypeface typeface, string text,
        float centerX, float centerY, float size, SKColor fill, SKColor outline)
    {
        using var font = CreateFont(typeface, size);
        var cellWidth = MaxDigitWidth(font);
        var baseline = VerticalCenterBaseline(font, centerY);
        var strokeWidth = Math.Max(1.5f, size * 0.018f);
        var x = centerX - cellWidth * text.Length / 2f;
        foreach (var ch in text)
        {
            DrawTextFilledOutlined(canvas, ch.ToString(), x + cellWidth / 2f, baseline,
                SKTextAlign.Center, font, fill, outline, strokeWidth);
            x += cellWidth;
        }
    }

    private static void DrawBitmapContained(SKCanvas canvas, SKBitmap bitmap, SKRect dst)
    {
        var scale = Math.Min(dst.Width / bitmap.Width, dst.Height / bitmap.Height);
        var w = bitmap.Width * scale;
        var h = bitmap.Height * scale;
        var x = dst.MidX - w / 2f;
        var y = dst.MidY - h / 2f;
        using var image = SKImage.FromBitmap(bitmap);
        var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
        canvas.DrawImage(image, new SKRect(x, y, x + w, y + h), sampling);
    }

    // Total width of text drawn with `tracking` extra pixels between each letter (no trailing gap).
    private static float MeasureTrackedWidth(SKFont font, string text, float tracking)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0f;
        }
        var width = 0f;
        foreach (var ch in text)
        {
            width += font.MeasureText(ch.ToString()) + tracking;
        }
        return width - tracking;
    }

    // Right-aligns at `right`, advancing each glyph by its width plus the tracking gap.
    // A non-null stroke is drawn over each glyph to thicken it.
    private static void DrawTrackedTextRight(SKCanvas canvas, string text, float right, float baseline,
        SKFont font, SKPaint fill, SKPaint? stroke, float tracking)
    {
        var x = right - MeasureTrackedWidth(font, text, tracking);
        foreach (var ch in text)
        {
            var s = ch.ToString();
            canvas.DrawText(s, x, baseline, SKTextAlign.Left, font, fill);
            if (stroke is not null)
            {
                canvas.DrawText(s, x, baseline, SKTextAlign.Left, font, stroke);
            }
            x += font.MeasureText(s) + tracking;
        }
    }

    private static float FitTextSize(SKTypeface typeface, string text, float maxWidth, float maxSize)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
        {
            return maxSize;
        }
        using var font = CreateFont(typeface, maxSize);
        var width = font.MeasureText(text);
        if (width <= maxWidth)
        {
            return maxSize;
        }
        return Math.Max(6f, maxSize * (maxWidth / width));
    }

    private static float VerticalCenterBaseline(SKFont font, float centerY)
    {
        var metrics = font.Metrics;
        return centerY - (metrics.Ascent + metrics.Descent) / 2f;
    }

    private static SKBitmap? DecodeLogo(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }
        try
        {
            return SKBitmap.Decode(bytes);
        }
        catch
        {
            return null;
        }
    }

    private static SKColor ParseColor(string? hex, SKColor fallback)
        => !string.IsNullOrWhiteSpace(hex) && SKColor.TryParse(hex, out var color) ? color : fallback;

    private static RosterPlayerCell EmptyCell() => new() { IsEmpty = true };

    private static Palette BuildPalette(RosterColorway colorway, SKColor primary, SKColor secondary, SKColor tertiary)
    {
        SKColor Resolve(RosterColor c) => c switch
        {
            RosterColor.Primary => primary,
            RosterColor.Secondary => secondary,
            RosterColor.Tertiary => tertiary,
            RosterColor.White => SKColors.White,
            RosterColor.Black => SKColors.Black,
            _ => primary,
        };

        return new Palette(
            Background: Resolve(colorway.Background),
            TitleFill: Resolve(colorway.TitleFill),
            TitleOutline: Resolve(colorway.TitleOutline),
            PositionBarBackground: Resolve(colorway.PositionBarBackground),
            PositionBarOutline: Resolve(colorway.PositionBarOutline),
            PositionBarFill: Resolve(colorway.PositionBarFill),
            JerseyFill: Resolve(colorway.JerseyNumber),
            JerseyOutline: Resolve(colorway.JerseyNumberOutline),
            DetailText: Resolve(colorway.PlayerDetailText),
            CaptaincyFill: Resolve(colorway.CaptaincyFill),
            CaptaincyOutline: Resolve(colorway.CaptaincyOutline));
    }

    private sealed record Palette(
        SKColor Background,
        SKColor TitleFill,
        SKColor TitleOutline,
        SKColor PositionBarBackground,
        SKColor PositionBarOutline,
        SKColor PositionBarFill,
        SKColor JerseyFill,
        SKColor JerseyOutline,
        SKColor DetailText,
        SKColor CaptaincyFill,
        SKColor CaptaincyOutline);
}
