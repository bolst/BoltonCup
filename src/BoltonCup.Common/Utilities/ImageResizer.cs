using SkiaSharp;

namespace BoltonCup.Common.Utilities;

public class ImageResizer
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
    

    public async Task<byte[]> ResizeAsync(Stream imageStream, long targetSizeKB = 500)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var originalBytes = memoryStream.ToArray();
        
        // if image is alr smaller than target size, return
        if (originalBytes.Length <= targetSizeKB * 1024)
            return originalBytes;
        
        using var originalBitmap = SKBitmap.Decode(new MemoryStream(originalBytes));
        // start with high quality and no scaling
        var quality = INIT_QUALITY;
        var scaleFactor = 1.0f;

        long imageSizeBytes;
        byte[] imageBytes;

        do
        {
            var resizedBitmap = scaleFactor < 1.0f 
                ? ResizeBitmap(originalBitmap, scaleFactor) 
                : originalBitmap;
            imageBytes = BitmapToByteArray(resizedBitmap, quality);
            imageSizeBytes = imageBytes.Length;

            if (imageSizeBytes > targetSizeKB * 1024 && quality > MIN_QUALITY)
                quality -= QUALITY_STEP;
            else if (imageSizeBytes < targetSizeKB * 1024 / 2 && scaleFactor > MIN_SCALE_FACTOR)
                scaleFactor -= SCALE_DEC;
            else
                break;

        } while (imageSizeBytes > targetSizeKB * 1024 || imageSizeBytes < targetSizeKB * 1024 / 2);

        return imageBytes;
    }



    private static SKBitmap ResizeBitmap(SKBitmap originalBitmap, float scaleFactor) 
        => originalBitmap.Resize(
            new SKImageInfo((int)(originalBitmap.Width * scaleFactor), (int)(originalBitmap.Height * scaleFactor)), 
            new SKSamplingOptions(SKCubicResampler.Mitchell)
        );

    private static byte[] BitmapToByteArray(SKBitmap bitmap, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        return data.ToArray();
    }
    
}