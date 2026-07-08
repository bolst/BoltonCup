using BoltonCup.Core;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class MusicQueueEngineTests
{
    // Fisher-Yates with rng.Next(k) == k - 1 leaves the input order unchanged, so shuffles are deterministic
    // and assertions can be exact.
    private sealed class IdentityRandom : Random
    {
        public override int Next(int maxValue) => maxValue - 1;
    }

    private static readonly Random Identity = new IdentityRandom();

    private static TournamentMusicQueue NewQueue() => new() { TournamentId = 1 };

    [Fact]
    public void Reconcile_FirstBuild_ShufflesDeckAndStartsAtTop()
    {
        var q = NewQueue();

        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);

        q.Deck.Should().Equal(1, 2, 3);
        q.DeckCursor.Should().Be(0);
        MusicQueueEngine.BuildOrder(q).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Advance_ToDeckSong_MovesCursorSoOrderLeadsWithCurrent()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);

        MusicQueueEngine.Advance(q, 2);

        q.DeckCursor.Should().Be(1);
        q.CurrentTrackId.Should().Be(2);
        // Resume-current: the order (and thus a reload + play) leads with the current song, not the next.
        MusicQueueEngine.BuildOrder(q).Should().Equal(2, 3);
    }

    [Fact]
    public void ResumeScenario_PlaySkipReload_ResumesTheSkippedToSong()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3, 4], Identity);

        MusicQueueEngine.Advance(q, 1); // press play → A
        MusicQueueEngine.Advance(q, 2); // skip → B

        // "Reload": the served order leads with B; pressing play reports B again (no drift).
        var afterReload = MusicQueueEngine.BuildOrder(q);
        afterReload[0].Should().Be(2);
        MusicQueueEngine.Advance(q, afterReload[0]);
        q.DeckCursor.Should().Be(1);
        q.CurrentTrackId.Should().Be(2);
    }

    [Fact]
    public void Advance_ToAlreadyPlayedSong_IsNoOp()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.Advance(q, 3); // cursor at end

        MusicQueueEngine.Advance(q, 1); // 1 is behind the cursor → ignored

        q.DeckCursor.Should().Be(2);
        q.CurrentTrackId.Should().Be(3);
    }

    [Fact]
    public void Advance_UnknownId_IsNoOp()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);

        MusicQueueEngine.Advance(q, 999);

        q.DeckCursor.Should().Be(0);
        q.CurrentTrackId.Should().BeNull();
    }

    [Fact]
    public void EnqueueNext_PutsPlayerSongsFirst_AndResetsAnchor()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.Advance(q, 2);

        MusicQueueEngine.EnqueueNext(q, [10, 11]);

        q.Priority.Should().Equal(10, 11);
        q.CurrentTrackId.Should().BeNull();
        // Player songs lead, then the base pool continues from where it was (cursor still at 2).
        MusicQueueEngine.BuildOrder(q).Should().Equal(10, 11, 2, 3);
    }

    [Fact]
    public void Advance_IntoPriority_DropsEarlierPlayerSongs()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.EnqueueNext(q, [10, 11, 12]);

        MusicQueueEngine.Advance(q, 11); // skipped past 10

        q.Priority.Should().Equal(11, 12);
        q.CurrentTrackId.Should().Be(11);
        MusicQueueEngine.BuildOrder(q).Should().Equal(11, 12, 1, 2, 3);
    }

    [Fact]
    public void Advance_FromPriorityIntoDeck_ClearsPriority()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.EnqueueNext(q, [10, 11]);
        MusicQueueEngine.Advance(q, 10);

        MusicQueueEngine.Advance(q, 1); // moved into the base pool

        q.Priority.Should().BeEmpty();
        q.DeckCursor.Should().Be(0);
        MusicQueueEngine.BuildOrder(q).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Reconcile_AppendsNewlyEligible_KeepingCursorOnCurrentSong()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.Advance(q, 2); // current = 2

        MusicQueueEngine.Reconcile(q, [1, 2, 3, 4], Identity);

        q.Deck.Should().Contain(4);
        q.Deck[q.DeckCursor].Should().Be(2); // cursor still on the current song
        MusicQueueEngine.BuildOrder(q)[0].Should().Be(2);
    }

    [Fact]
    public void Reconcile_DropsRemovedIds_AndFixesCursor()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.Advance(q, 3); // current = 3, cursor = 2

        MusicQueueEngine.Reconcile(q, [1, 2], Identity); // 3 removed

        q.Deck.Should().Equal(1, 2);
        q.DeckCursor.Should().BeInRange(0, 1);
    }

    [Fact]
    public void RollOver_ReshufflesTheFullCatalog_AndResetsToTop()
    {
        var q = NewQueue();
        MusicQueueEngine.Reconcile(q, [1, 2, 3], Identity);
        MusicQueueEngine.Advance(q, 3);
        MusicQueueEngine.EnqueueNext(q, [10]);

        MusicQueueEngine.RollOver(q, [1, 2, 3], Identity);

        q.Deck.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        q.DeckCursor.Should().Be(0);
        q.Priority.Should().BeEmpty();
        q.CurrentTrackId.Should().BeNull();
    }

    [Fact]
    public void FullCycle_PlaysEverySongOnceBeforeRepeating()
    {
        var q = NewQueue();
        var eligible = new[] { 5, 6, 7, 8, 9 };
        MusicQueueEngine.Reconcile(q, eligible, new Random(1234));

        var played = new List<int>();
        // Walk the whole cycle: play the current song (BuildOrder[0]), advance to the next in the deck.
        for (var i = 0; i < eligible.Length; i++)
        {
            var order = MusicQueueEngine.BuildOrder(q);
            played.Add(order[0]);
            if (order.Count > 1)
                MusicQueueEngine.Advance(q, order[1]);
        }

        played.Should().OnlyHaveUniqueItems();
        played.Should().BeEquivalentTo(eligible); // every song exactly once, no repeat within the cycle
    }
}
