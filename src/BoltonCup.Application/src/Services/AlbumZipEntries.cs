using System.IO.Compression;

namespace BoltonCup.Application.Services;

/// <summary>Picks the image entries out of an uploaded zip archive, in a stable import order.</summary>
public static class AlbumZipEntries
{
    /// <summary>The file extensions the album importer accepts.</summary>
    public static readonly IReadOnlyList<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// The archive's image entries, sorted by full path (ordinal, case-insensitive). Skips
    /// directories, the macOS resource-fork folder and dot-files.
    /// </summary>
    public static IEnumerable<ZipArchiveEntry> SelectImages(ZipArchive archive) => archive.Entries
        .Where(e => IsUserFile(e) && HasImageExtension(e))
        .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The archive's files that are not supported images, such as <c>.heic</c> or <c>.txt</c>,
    /// sorted like <see cref="SelectImages"/>. Directories, the macOS resource-fork folder and
    /// dot-files are not included.
    /// </summary>
    public static IEnumerable<ZipArchiveEntry> SelectUnsupportedFiles(ZipArchive archive) => archive.Entries
        .Where(e => IsUserFile(e) && !HasImageExtension(e))
        .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase);

    static bool IsUserFile(ZipArchiveEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Name))
        {
            return false;
        }

        if (entry.FullName.StartsWith("__MACOSX/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !entry.Name.StartsWith('.');
    }

    static bool HasImageExtension(ZipArchiveEntry entry) =>
        ImageExtensions.Contains(Path.GetExtension(entry.Name), StringComparer.OrdinalIgnoreCase);
}