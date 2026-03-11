using BoltonCup.Common.Utilities;
using BoltonCup.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common.Services;

public class AssetFileUploader : IAssetFileUploader
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;
    private const int _maxFileSize = 10 * 1024 * 1024; // 10 MB

    public AssetFileUploader(IStorageService storageService)
    {
        _httpClient = new HttpClient();
        _storageService = storageService;
    }
    
    public async Task<string> UploadAsync(IBrowserFile file, bool resize = true, CancellationToken cancellationToken = default)
    {
        var ext = resize ? ".webp" : Path.GetExtension(file.Name);
        var mime = resize ? "image/webp" : file.ContentType;
        var putUrl = await _storageService.GeneratePreSignedPutUrl(ext, mime, cancellationToken);
        if (putUrl is null)
            throw new InvalidOperationException("Failed to generate pre-signed URL for upload.");

        await using var fileStream = file.OpenReadStream(_maxFileSize, cancellationToken);
        var uploadStream = fileStream;
        if (resize)
        {
            var resizer = new ImageResizer();
            uploadStream = await resizer.ResizeAsync(fileStream);
        }

        using var content = new StreamContent(uploadStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
        var response = await _httpClient.PutAsync(putUrl.UploadUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return putUrl.TempKey;
    }
}