namespace BoltonCup.Data;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Supabase.Gotrue;
using SkiaSharp;

public interface ICacheService
{
    Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration);
    void Clear();
}
public class CacheService : ICacheService
{
    private readonly IMemoryCache memoryCache;
    private CancellationTokenSource resetCacheToken = new();
    public CacheService(IMemoryCache _memoryCache)
    {
        memoryCache = _memoryCache;
    }

    public async Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration)
    {
        if (!memoryCache.TryGetValue(cacheKey, out T? cacheEntry))
        {
            cacheEntry = await factory();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
            };
            cacheOptions.AddExpirationToken(new CancellationChangeToken(resetCacheToken.Token));

            memoryCache.Set(cacheKey, cacheEntry, cacheOptions);
        }

        return cacheEntry!;
    }

    public void Clear()
    {
        resetCacheToken.Cancel();
        resetCacheToken.Dispose();
        resetCacheToken = new CancellationTokenSource();
    }



}

public class OTPLogin
{

    private readonly Supabase.Client SBClient;

    public OTPLogin(Supabase.Client _SBClient)
    {
        SBClient = _SBClient;
    }

    public async Task<bool> SendOTPToken(string email)
    {
        SignInWithPasswordlessEmailOptions options = new(email)
        {
            FlowType = Constants.OAuthFlowType.PKCE,
        };
        var verifier = await SBClient.Auth.SignInWithOtp(options);
        return true;
    }

    public async Task<bool> ValidateOTPToken(string token, string email)
    {
        var session = await SBClient.Auth.VerifyOTP(email, token, Constants.EmailOtpType.Email);
        return true;
    }

}

// thanks Mike
public static class ImageResizerSkia
{
    public static async Task<byte[]> ResizeImageToApproxSizeAsync(Stream imageStream, long targetSizeKB = 1000)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        byte[] originalBytes = memoryStream.ToArray();

        // If the image is already smaller than the target size, return it as is
        if (originalBytes.Length <= targetSizeKB * 1024)
        {
            return originalBytes;
        }

        using var originalBitmap = SKBitmap.Decode(new MemoryStream(originalBytes));
        int quality = 90; // Start with a high quality
        float scaleFactor = 1.0f; // Start with no scaling
        const float minScaleFactor = 0.5f; // Do not scale below 50% of the original size
        const float scaleDecrement = 0.1f; // Reduce scaling by 10% in each iteration

        long imageSizeBytes;
        byte[] imageBytes;

        do
        {
            // Apply scaling only if the scaleFactor is less than 1 (i.e., downscaling)
            var resizedBitmap = scaleFactor < 1.0f
                ? ResizeBitmap(originalBitmap, scaleFactor)
                : originalBitmap;

            // Convert the resized bitmap to a byte array with the current quality setting
            imageBytes = BitmapToByteArray(resizedBitmap, quality);
            imageSizeBytes = imageBytes.Length;

            // Adjust the quality or scale factor based on the current size
            if (imageSizeBytes > targetSizeKB * 1024 && quality > 50)
            {
                quality -= 10; // Decrease quality if the image is too large, but do it gradually
            }
            else if (imageSizeBytes < targetSizeKB * 1024 / 2 && scaleFactor > minScaleFactor)
            {
                scaleFactor -= scaleDecrement; // Decrease the scale factor (resize the image smaller)
            }
            else
            {
                break; // Exit the loop if the size is within the target range
            }

        } while (imageSizeBytes > targetSizeKB * 1024 || imageSizeBytes < targetSizeKB * 1024 / 2);

        return imageBytes;
    }
    private static SKBitmap ResizeBitmap(SKBitmap originalBitmap, float scaleFactor)
    {
        int newWidth = (int)(originalBitmap.Width * scaleFactor);
        int newHeight = (int)(originalBitmap.Height * scaleFactor);

        // Create a new scaled bitmap
        var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);

        return resizedBitmap;
    }

    private static byte[] BitmapToByteArray(SKBitmap bitmap, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality); // Use JPEG encoding with adjustable quality
        return data.ToArray();
    }
}