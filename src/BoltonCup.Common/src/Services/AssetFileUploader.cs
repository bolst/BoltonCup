using BoltonCup.Common.Utilities;
using BoltonCup.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common.Services;

public class AssetFileUploader : IAssetFileUploader
{
    readonly HttpClient _httpClient;
    readonly IStorageService _storageService;
    const int MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public AssetFileUploader(IStorageService storageService)
    {
        _httpClient = new HttpClient();
        _storageService = storageService;
    }

    public async Task<string> UploadAsync(IBrowserFile file, bool resize = true, long? maxFileSize = null, CancellationToken cancellationToken = default)
    {
        await using var fileStream = file.OpenReadStream(maxFileSize ?? MaxFileSize, cancellationToken);

        Stream uploadStream = fileStream;
        var ext = Path.GetExtension(file.Name);
        var mime = file.ContentType;
        if (resize)
        {
            var result = await ImageResizer.ResizeAsync(fileStream);
            uploadStream = result.Content;
            if (result.Converted)
            {
                ext = ".webp";
                mime = "image/webp";
            }
        }

        var upload = await _storageService.GenerateUploadCredentialsAsync(ext, mime, cancellationToken);
        if (upload is null)
        {
            throw new InvalidOperationException("Failed to generate pre-signed URL for upload.");
        }

        using var content = new StreamContent(uploadStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
        // R2 rejects chunked uploads with 411 Length Required, so send an explicit Content-Length. The browser
        // file stream isn't seekable (no Length), so fall back to the reported file size for non-resized uploads.
        content.Headers.ContentLength = uploadStream.CanSeek ? uploadStream.Length : file.Size;
        var response = await _httpClient.PutAsync(upload.UploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return upload.TempKey;
    }
}