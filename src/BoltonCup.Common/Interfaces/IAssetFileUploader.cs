using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common;

public interface IAssetFileUploader
{
    Task<string> UploadAsync(IBrowserFile file, bool resize = true, CancellationToken cancellationToken = default);
}