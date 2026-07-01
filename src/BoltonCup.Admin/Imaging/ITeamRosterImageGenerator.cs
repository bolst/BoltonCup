using BoltonCup.Common.Imaging;

namespace BoltonCup.Admin.Imaging;

public interface ITeamRosterImageGenerator
{
    /// <summary>Renders the roster PNG bytes only — no upload, no log row. Use for live preview.</summary>
    Task<byte[]> RenderAsync(int teamId, RosterColorway colorway, CancellationToken cancellationToken = default);

    /// <summary>Renders, uploads to storage, and records a log row.</summary>
    Task<GeneratedImageResult> GenerateAsync(int teamId, RosterColorway colorway,
        CancellationToken cancellationToken = default);
}

public sealed record GeneratedImageResult(byte[] Png, string StorageKey, string ContentType);
