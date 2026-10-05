using System.IO.Compression;
using BoltonCup.Application.Services;
using FluentAssertions;
using Xunit;

namespace BoltonCup.Application.Tests.Services;

public class AlbumZipEntriesTests
{
    static ZipArchive BuildArchive(IEnumerable<string> entryNames)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var name in entryNames)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open());
                writer.Write("data");
            }
        }

        stream.Position = 0;
        return new ZipArchive(stream, ZipArchiveMode.Read);
    }

    [Fact]
    public void SelectImages_SkipsMacosxAndNonImageEntries()
    {
        using var archive = BuildArchive(
        [
            "photos/b.jpg",
            "photos/a.png",
            "__MACOSX/photos/._a.png",
            "photos/.DS_Store",
            "photos/notes.txt",
            "photos/",
        ]);

        var entries = AlbumZipEntries.SelectImages(archive).Select(e => e.FullName).ToList();

        entries.Should().Equal("photos/a.png", "photos/b.jpg");
    }

    [Fact]
    public void SelectImages_OrdersByFullNameOrdinalIgnoreCase()
    {
        using var archive = BuildArchive(["z.jpg", "A.jpg", "m.webp"]);

        var entries = AlbumZipEntries.SelectImages(archive).Select(e => e.FullName).ToList();

        entries.Should().Equal("A.jpg", "m.webp", "z.jpg");
    }

    [Fact]
    public void SelectImages_AcceptsAllSupportedExtensions()
    {
        using var archive = BuildArchive(["a.jpg", "b.jpeg", "c.png", "d.webp"]);

        var entries = AlbumZipEntries.SelectImages(archive).Select(e => e.FullName).ToList();

        entries.Should().HaveCount(4);
    }

    [Fact]
    public void SelectUnsupportedFiles_ReturnsOnlyUserFilesWithOtherExtensions()
    {
        using var archive = BuildArchive(
        [
            "photos/b.HEIC",
            "photos/a.jpg",
            "photos/notes.txt",
            "__MACOSX/photos/._a.jpg",
            "photos/.DS_Store",
            "photos/",
        ]);

        var entries = AlbumZipEntries.SelectUnsupportedFiles(archive).Select(e => e.FullName).ToList();

        entries.Should().Equal("photos/b.HEIC", "photos/notes.txt");
    }
}