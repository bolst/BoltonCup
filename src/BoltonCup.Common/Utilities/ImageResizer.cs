using SkiaSharp;

namespace BoltonCup.Common.Utilities;

public static class ImageResizer
{
    // do not scale below 50% original size
    private const float MIN_SCALE_FACTOR = 0.5f;
    // reduce scaling by 10% in each iteration
    private const float SCALE_DEC = 0.1f;
    // start with 90% quality
    private const int INIT_QUALITY = 90;
    // do not go below 50% quality
    private const int MIN_QUALITY = 50;
    // reduce quality by 10% in each iteration
    private const int QUALITY_STEP = 10;
    

    public static async Task<Stream> ResizeAsync(Stream imageStream, long targetSizeKB = 500)
    {
        var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var targetSizeBytes = targetSizeKB * 1024;
        if (memoryStream.Length <= targetSizeBytes)
            return memoryStream;

        var originalBitmap = SKBitmap.Decode(memoryStream);
        await memoryStream.DisposeAsync();

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
                break;

            if (quality > MIN_QUALITY)
                quality = Math.Max(MIN_QUALITY, quality - QUALITY_STEP);
            else if (scaleFactor > MIN_SCALE_FACTOR)
                scaleFactor = MathF.Max(MIN_SCALE_FACTOR, scaleFactor - SCALE_DEC);
            else
                break;
        }

        finalStream!.Position = 0;
        return finalStream;
    }
    
}

internal static class SkiaExtensions
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
}