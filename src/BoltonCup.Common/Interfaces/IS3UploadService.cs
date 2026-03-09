using Microsoft.AspNetCore.Components.Forms;

namespace BoltonCup.Common;

public interface IS3UploadService
{
    Task<string> UploadAsync(IBrowserFile file, bool resize = true, CancellationToken cancellationToken = default);
}