using System.IO.Compression;
using BoltonCup.Application.Services;
using BoltonCup.Common.Utilities;
using BoltonCup.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Admin.Services;

/// <summary>A zip entry that did not become an album image, and why, in words an uploader can act on.</summary>
public sealed record AlbumImportFailure(string FileName, string Reason);

/// <summary>How many zip entries became album images, and which ones did not.</summary>
public sealed record AlbumImportResult(int Imported, IReadOnlyList<AlbumImportFailure> Failures);

public enum AlbumImportStage
{
    Uploading,
    Importing,
}

/// <summary>
/// Import progress: bytes received while <see cref="AlbumImportStage.Uploading"/>, images
/// processed while <see cref="AlbumImportStage.Importing"/>.
/// </summary>
public sealed record AlbumImportProgress(AlbumImportStage Stage, long Done, long Total);

/// <summary>The whole zip was rejected. <see cref="Exception.Message"/> is safe to show the uploader.</summary>
public sealed class AlbumImportException(string message, Exception? innerException = null)
    : Exception(message, innerException);

/// <summary>Imports every image in an uploaded zip into an album, resizing each one on the way in.</summary>
public class AlbumZipImporter(IAlbumService _albums, ILogger<AlbumZipImporter> _logger)
{
    const long MaxZipSize = 10L * 1024 * 1024 * 1024;
    const long MaxEntrySize = 50L * 1024 * 1024;
    const long MaxImagePixels = 60_000_000;
    const long TargetImageSizeKB = 1500;
    const int MaxImageDimension = 2560;
    const long DiskSpaceMargin = 512L * 1024 * 1024;
    const int UploadProgressStep = 4 * 1024 * 1024;

    /// <summary>
    /// Saves the zip to a temp file, then imports each supported image in order. Per-image problems
    /// are collected as <see cref="AlbumImportFailure"/>s and do not stop the import.
    /// </summary>
    /// <exception cref="AlbumImportException">The zip as a whole was not accepted.</exception>
    public async Task<AlbumImportResult> ImportAsync(
        int albumId,
        IBrowserFile file,
        IProgress<AlbumImportProgress> progress,
        CancellationToken cancellationToken = default)
    {
        ThrowIfNotAcceptable(file);

        var tempPath = Path.GetTempFileName();
        try
        {
            await ReceiveAsync(file, tempPath, progress, cancellationToken);

            using var archive = OpenArchive(tempPath);
            var images = AlbumZipEntries.SelectImages(archive).ToList();
            var failures = AlbumZipEntries.SelectUnsupportedFiles(archive)
                .Select(e => new AlbumImportFailure(e.FullName, UnsupportedTypeReason(e.Name)))
                .ToList();

            if (images.Count == 0)
            {
                throw new AlbumImportException(NoImagesMessage(failures.Count));
            }

            var imported = 0;
            for (var i = 0; i < images.Count; i++)
            {
                var failure = await TryImportEntryAsync(albumId, images[i], cancellationToken);
                if (failure is null)
                {
                    imported++;
                }
                else
                {
                    failures.Add(failure);
                }

                progress.Report(new AlbumImportProgress(AlbumImportStage.Importing, i + 1, images.Count));
            }

            return new AlbumImportResult(imported, failures);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    static void ThrowIfNotAcceptable(IBrowserFile file)
    {
        if (!string.Equals(Path.GetExtension(file.Name), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new AlbumImportException($"\"{file.Name}\" is not a .zip file. Compress the photos into a .zip and upload that.");
        }

        if (file.Size == 0)
        {
            throw new AlbumImportException($"\"{file.Name}\" is empty.");
        }

        if (file.Size > MaxZipSize)
        {
            throw new AlbumImportException(
                $"\"{file.Name}\" is {FormatSize(file.Size)}, which is over the {FormatSize(MaxZipSize)} limit. Split the photos into smaller zips.");
        }
    }

    async Task ReceiveAsync(IBrowserFile file, string tempPath, IProgress<AlbumImportProgress> progress, CancellationToken cancellationToken)
    {
        var freeSpace = new DriveInfo(Path.GetTempPath()).AvailableFreeSpace;
        if (freeSpace < file.Size + DiskSpaceMargin)
        {
            _logger.LogError("Rejected a {Size} byte album zip: only {FreeSpace} bytes free in {TempPath}", file.Size, freeSpace, Path.GetTempPath());
            throw new AlbumImportException(
                $"The server does not have enough free disk space for a {FormatSize(file.Size)} zip right now. Try a smaller zip or ask a developer to free up space.");
        }

        try
        {
            await using var tempStream = File.Create(tempPath);
            await using var browserStream = file.OpenReadStream(MaxZipSize, cancellationToken);
            var buffer = new byte[81920];
            long received = 0;
            long lastReported = 0;
            int read;
            while ((read = await browserStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await tempStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                received += read;
                if (received - lastReported >= UploadProgressStep)
                {
                    lastReported = received;
                    progress.Report(new AlbumImportProgress(AlbumImportStage.Uploading, received, file.Size));
                }
            }

            progress.Report(new AlbumImportProgress(AlbumImportStage.Uploading, received, file.Size));
        }
        catch (IOException e) when (e is not EndOfStreamException)
        {
            _logger.LogWarning(e, "Album zip upload of {Size} bytes did not finish", file.Size);
            throw new AlbumImportException(
                "The upload was interrupted before it finished. Keep this tab open and connected for the whole upload, then try again.",
                e);
        }
    }

    ZipArchive OpenArchive(string path)
    {
        try
        {
            return ZipFile.OpenRead(path);
        }
        catch (InvalidDataException e)
        {
            _logger.LogWarning(e, "Uploaded album zip could not be opened");
            throw new AlbumImportException(
                "The file could not be opened as a zip. It may be damaged, incomplete, or another format (such as .rar or .7z) renamed to .zip.",
                e);
        }
    }

    async Task<AlbumImportFailure?> TryImportEntryAsync(int albumId, ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        if (entry.Length > MaxEntrySize)
        {
            return new AlbumImportFailure(entry.FullName, $"The file is {FormatSize(entry.Length)}; images must be {FormatSize(MaxEntrySize)} or smaller.");
        }

        try
        {
            await using var entryStream = entry.Open();
            var result = await ImageResizer.ResizeAsync(entryStream, TargetImageSizeKB, MaxImageDimension, MaxImagePixels);
            await using var content = result.Content;
            var extension = result.Converted ? ".webp" : Path.GetExtension(entry.Name);
            var contentType = result.Converted ? "image/webp" : ContentTypeFor(extension);
            await _albums.AddImageAsync(albumId, content, extension, contentType, cancellationToken);

            return null;
        }
        catch (ImageTooLargeException e)
        {
            return new AlbumImportFailure(
                entry.FullName,
                $"The image is {e.Width}×{e.Height} pixels; images must be {MaxImagePixels / 1_000_000} megapixels or fewer.");
        }
        catch (InvalidDataException e)
        {
            _logger.LogWarning(e, "Album image '{EntryName}' for album {AlbumId} could not be read", entry.FullName, albumId);
            return new AlbumImportFailure(entry.FullName, "The image could not be read. It may be damaged, or not really the type its extension says.");
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            _logger.LogError(e, "Failed to import album image '{EntryName}' into album {AlbumId}", entry.FullName, albumId);
            return new AlbumImportFailure(entry.FullName, "The image could not be saved because of a server error. Try again; if it keeps failing, ask a developer to check the logs.");
        }
    }

    static string UnsupportedTypeReason(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var supported = string.Join(", ", AlbumZipEntries.ImageExtensions);
        return string.IsNullOrEmpty(extension)
            ? $"The file has no extension. Supported types are {supported}."
            : $"{extension} files are not supported. Convert to one of {supported}.";
    }

    static string NoImagesMessage(int unsupportedCount)
    {
        var supported = string.Join(", ", AlbumZipEntries.ImageExtensions);
        return unsupportedCount == 0
            ? $"The zip has no images in it. Supported types are {supported}."
            : $"The zip has no supported images: all {unsupportedCount} file(s) are other types. Convert them to one of {supported} (iPhone .heic photos included).";
    }

    static string FormatSize(long bytes) => bytes >= 1024L * 1024 * 1024
        ? $"{bytes / (1024d * 1024 * 1024):0.#} GB"
        : $"{bytes / (1024d * 1024):0.#} MB";

    static string ContentTypeFor(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg",
    };
}