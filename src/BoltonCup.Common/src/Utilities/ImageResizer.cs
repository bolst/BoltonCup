using SkiaSharp;

namespace BoltonCup.Common.Utilities;

/// <summary>
/// The outcome of a resize. <see cref="Converted"/> is false when the input was already under
/// the target size and is returned untouched, so the caller should keep the original extension
/// and content type rather than treat it as webp.
/// </summary>
public sealed record ImageResizeResult(Stream Content, bool Converted);

/// <summary>Thrown when an image's pixel count exceeds the limit the caller passed to the resizer.</summary>
public sealed class ImageTooLargeException(int width, int height, long maxPixels)
    : Exception($"The image is {width}×{height} pixels, which exceeds the {maxPixels:N0} pixel limit.")
{
    public int Width { get; } = width;

    public int Height { get; } = height;
}

public static class ImageResizer
{
    // do not scale below 50% original size
    const float MIN_SCALE_FACTOR = 0.5f;
    // reduce scaling by 10% in each iteration
    const float SCALE_DEC = 0.1f;
    // start with 90% quality
    const int INIT_QUALITY = 90;
    // do not go below 50% quality
    const int MIN_QUALITY = 50;
    // reduce quality by 10% in each iteration
    const int QUALITY_STEP = 10;

    /// <summary>
    /// Re-encodes the image as webp until it fits under <paramref name="targetSizeKB"/>. When
    /// <paramref name="maxDimension"/> is set, an image that needs re-encoding is first scaled so its
    /// longest edge is at most that many pixels. Inputs already under the target are returned untouched.
    /// When <paramref name="maxPixels"/> is set, an image that needs re-encoding is rejected before it
    /// is decoded if its width × height exceeds it, which bounds the memory a decode can take.
    /// </summary>
    /// <exception cref="InvalidDataException">The image format is unsupported or the data is corrupt.</exception>
    /// <exception cref="ImageTooLargeException">The image exceeds <paramref name="maxPixels"/>.</exception>
    public static async Task<ImageResizeResult> ResizeAsync(
        Stream imageStream,
        long targetSizeKB = 500,
        int? maxDimension = null,
        long? maxPixels = null)
    {
        var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var targetSizeBytes = targetSizeKB * 1024;
        if (memoryStream.Length <= targetSizeBytes)
        {
            memoryStream.Position = 0;
            return new ImageResizeResult(memoryStream, Converted: false);
        }

        SKBitmap originalBitmap;
        using (var codec = SKCodec.Create(memoryStream))
        {
            if (codec is null)
            {
                throw new InvalidDataException("The image could not be decoded.");
            }

            var (width, height) = (codec.Info.Width, codec.Info.Height);
            if (maxPixels is { } pixelLimit && (long)width * height > pixelLimit)
            {
                throw new ImageTooLargeException(width, height, pixelLimit);
            }

            var decoded = SKBitmap.Decode(codec);
            if (decoded is null)
            {
                throw new InvalidDataException("The image could not be decoded.");
            }

            originalBitmap = decoded.ApplyExifOrientation(codec.EncodedOrigin);
        }
        await memoryStream.DisposeAsync();

        if (maxDimension is { } max && Math.Max(originalBitmap.Width, originalBitmap.Height) > max)
        {
            var bounded = originalBitmap.ResizeBitmap((float)max / Math.Max(originalBitmap.Width, originalBitmap.Height));
            originalBitmap.Dispose();
            originalBitmap = bounded;
        }

        // start with high quality and no scaling
        var quality = INIT_QUALITY;
        var scaleFactor = 1.0f;
        MemoryStream? finalStream = null;

        while (true)
        {
            var previousStream = finalStream;

            var resizedBitmap = scaleFactor < 1.0f
                ? originalBitmap.ResizeBitmap(scaleFactor)
                : null;

            try
            {
                finalStream = (resizedBitmap ?? originalBitmap).EncodeToStream(quality);
            }
            finally
            {
                resizedBitmap?.Dispose();
            }

            await (previousStream?.DisposeAsync() ?? ValueTask.CompletedTask);

            if (finalStream.Length <= targetSizeBytes)
            {
                break;
            }

            if (quality > MIN_QUALITY)
            {
                quality = Math.Max(MIN_QUALITY, quality - QUALITY_STEP);
            }
            else if (scaleFactor > MIN_SCALE_FACTOR)
            {
                scaleFactor = MathF.Max(MIN_SCALE_FACTOR, scaleFactor - SCALE_DEC);
            }
            else
            {
                break;
            }
        }

        originalBitmap.Dispose();
        finalStream!.Position = 0;
        return new ImageResizeResult(finalStream, Converted: true);
    }
}

static class SkiaExtensions
{
    internal static SKBitmap ResizeBitmap(this SKBitmap bitmap, float scaleFactor)
    {
        var newWidth = (int)(bitmap.Width * scaleFactor);
        var newHeight = (int)(bitmap.Height * scaleFactor);
        return bitmap.Resize(
            new SKImageInfo(newWidth, newHeight),
            new SKSamplingOptions(SKCubicResampler.Mitchell)
        );
    }

    internal static MemoryStream EncodeToStream(this SKBitmap bitmap, int quality)
    {
        var stream = new MemoryStream();
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality);
        data.SaveTo(stream);
        return stream;
    }

    /// <summary>
    /// Rotates or flips a decoded bitmap to match how it should display, undoing the EXIF
    /// orientation the decoder left in place. Disposes <paramref name="bitmap"/> when it returns
    /// a different instance.
    /// </summary>
    internal static SKBitmap ApplyExifOrientation(this SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (origin is SKEncodedOrigin.TopLeft or SKEncodedOrigin.Default)
        {
            return bitmap;
        }

        var swapsDimensions = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = swapsDimensions ? bitmap.Height : bitmap.Width;
        var height = swapsDimensions ? bitmap.Width : bitmap.Height;

        var oriented = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(oriented))
        {
            switch (origin)
            {
                case SKEncodedOrigin.TopRight:
                    canvas.Translate(width, 0);
                    canvas.Scale(-1, 1);
                    break;
                case SKEncodedOrigin.BottomRight:
                    canvas.Translate(width, height);
                    canvas.RotateDegrees(180);
                    break;
                case SKEncodedOrigin.BottomLeft:
                    canvas.Translate(0, height);
                    canvas.Scale(1, -1);
                    break;
                case SKEncodedOrigin.LeftTop:
                    canvas.RotateDegrees(90);
                    canvas.Scale(1, -1);
                    break;
                case SKEncodedOrigin.RightTop:
                    canvas.Translate(width, 0);
                    canvas.RotateDegrees(90);
                    break;
                case SKEncodedOrigin.RightBottom:
                    canvas.Translate(width, height);
                    canvas.RotateDegrees(90);
                    canvas.Scale(-1, 1);
                    break;
                case SKEncodedOrigin.LeftBottom:
                    canvas.Translate(0, height);
                    canvas.RotateDegrees(-90);
                    break;
            }

            canvas.DrawBitmap(bitmap, 0, 0);
        }

        bitmap.Dispose();
        return oriented;
    }
}