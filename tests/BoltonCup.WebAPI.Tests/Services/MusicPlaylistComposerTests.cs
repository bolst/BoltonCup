using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class MusicPlaylistComposerTests
{
    private static TournamentMusicTrack Track(string fileKey, string? trackId, bool basePool) => new()
    {
        TournamentId = 1,
        AudioFileKey = fileKey,
        TrackId = trackId,
        ProviderType = MusicProviderType.Spotify,
        Title = fileKey,
        IsInBasePool = basePool,
    };

    private static IEnumerable<(MusicProviderType, string?)> Reqs(params string?[] ids)
        => ids.Select(id => (MusicProviderType.Spotify, id));

    [Fact]
    public void Compose_PutsMatchedRequestsFirst_ThenBasePool()
    {
        var library = new[]
        {
            Track("base1", null, basePool: true),
            Track("base2", null, basePool: true),
            Track("reqA", "A", basePool: false),
            Track("reqB", "B", basePool: false),
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs("A", "B"), library));

        result.Should().Equal("reqA", "reqB", "base1", "base2");
    }

    [Fact]
    public void Compose_DeDupes_WhenRequestIsAlsoInBasePool()
    {
        var library = new[]
        {
            Track("x", "X", basePool: true),
            Track("base1", null, basePool: true),
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs("X"), library));

        result.Should().Equal("x", "base1");
    }

    [Fact]
    public void Compose_ExcludesUnmatchedRequests()
    {
        var library = new[] { Track("base1", null, basePool: true) };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs("NOPE"), library));

        result.Should().Equal("base1");
    }

    [Fact]
    public void Compose_DoesNotMatchAcrossProviders()
    {
        var library = new[]
        {
            new TournamentMusicTrack
            {
                TournamentId = 1, AudioFileKey = "other", TrackId = "A",
                ProviderType = (MusicProviderType)999, Title = "other", IsInBasePool = false,
            },
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs("A"), library));

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compose_WithNoRequests_ReturnsBasePoolOnly()
    {
        var library = new[]
        {
            Track("base1", null, basePool: true),
            Track("base2", null, basePool: true),
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs(), library));

        result.Should().Equal("base1", "base2");
    }

    [Fact]
    public void Compose_SkipsNullAndBlankRequestIds()
    {
        var library = new[]
        {
            Track("reqA", "A", basePool: false),
            Track("base1", null, basePool: true),
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs(null, "", "  ", "A"), library));

        result.Should().Equal("reqA", "base1");
    }

    [Fact]
    public void Compose_ExcludesNonBaseTracksThatAreNotRequested()
    {
        var library = new[]
        {
            Track("orphan", "Z", basePool: false),
            Track("base1", null, basePool: true),
        };

        var result = Keys(MusicPlaylistComposer.Compose(Reqs(), library));

        result.Should().Equal("base1");
    }

    private static List<string> Keys(IEnumerable<TournamentMusicTrack> tracks)
        => tracks.Select(t => t.AudioFileKey).ToList();
}
