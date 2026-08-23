using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// Pure deck logic for a tournament's shared music rotation.
/// Mutates a <see cref="TournamentMusicQueue"/> in place.
/// </summary>
public static class MusicQueueEngine
{
    /// <summary>
    /// Reconciles the deck to the current eligible base-pool catalog: a first build shuffles the deck; later
    /// calls append newly-eligible ids to the unplayed tail, drop ids no longer eligible, and keep the cursor
    /// pointing at the same current song.
    /// </summary>
    public static void Reconcile(TournamentMusicQueue q, IReadOnlyList<int> eligibleIds, Random rng)
    {
        var eligible = eligibleIds.Distinct().ToList();
        var eligibleSet = eligible.ToHashSet();

        if (q.Deck.Count == 0)
        {
            q.Deck = Shuffle(eligible, rng);
            q.DeckCursor = 0;
            return;
        }

        // Remember the current song so the cursor still points at it after add/remove.
        int? currentDeckId = q.DeckCursor >= 0 && q.DeckCursor < q.Deck.Count ? q.Deck[q.DeckCursor] : null;

        var newDeck = q.Deck.Where(eligibleSet.Contains).ToList();
        foreach (var id in eligible)
        {
            if (!newDeck.Contains(id))
            {
                newDeck.Add(id); // newly-eligible -> unplayed tail of this cycle
            }
        }
        q.Deck = newDeck;

        if (currentDeckId is { } cur && q.Deck.IndexOf(cur) is var idx and >= 0)
        {
            q.DeckCursor = idx;
        }
        else
        {
            q.DeckCursor = Math.Clamp(q.DeckCursor, 0, Math.Max(0, q.Deck.Count - 1));
        }
    }

    /// <summary>
    /// The order served to the client: pending player songs, then the current cycle from the cursor. Element 0
    /// is the current song, so a reload resumes it. De-dup (by audio file key) happens downstream.
    /// </summary>
    public static List<int> BuildOrder(TournamentMusicQueue q)
    {
        var order = new List<int>(q.Priority);
        if (q.DeckCursor >= 0 && q.DeckCursor < q.Deck.Count)
        {
            order.AddRange(q.Deck.Skip(q.DeckCursor));
        }
        return order;
    }

    /// <summary>
    /// Records that <paramref name="trackId"/> is now playing, moving the resume anchor to it. A no-op for ids
    /// not in the served order (e.g. a one-shot goal song, or an already-played song).
    /// </summary>
    public static void Advance(TournamentMusicQueue q, int trackId)
    {
        // A pending player song becomes current; earlier (skipped) player songs drop off the front.
        var pi = q.Priority.IndexOf(trackId);
        if (pi >= 0)
        {
            if (pi > 0)
            {
                q.Priority = q.Priority.Skip(pi).ToList();
            }
            q.CurrentTrackId = trackId;
            return;
        }

        // A base-pool song in this cycle's unplayed tail: move the cursor to it, drop remaining player songs.
        var di = q.Deck.IndexOf(trackId);
        if (di >= 0 && di >= q.DeckCursor)
        {
            q.Priority = [];
            q.DeckCursor = di;
            q.CurrentTrackId = trackId;
        }
    }

    /// <summary>
    /// Starts a fresh cycle once the deck is exhausted: reshuffle the eligible catalog and reset to the top,
    /// so no song repeats until the whole catalog has played again.
    /// </summary>
    public static void RollOver(TournamentMusicQueue q, IReadOnlyList<int> eligibleIds, Random rng)
    {
        q.Deck = Shuffle(eligibleIds.Distinct().ToList(), rng);
        q.DeckCursor = 0;
        q.Priority = [];
        q.CurrentTrackId = null;
    }

    /// <summary>
    /// Injects a game's player songs at the front (play-next), replacing any previous game's injected songs,
    /// and resets the resume anchor so the fresh game leads at the front of the queue.
    /// </summary>
    public static void EnqueueNext(TournamentMusicQueue q, IReadOnlyList<int> playerTrackIds)
    {
        q.Priority = playerTrackIds.Distinct().ToList();
        q.CurrentTrackId = null;
    }

    static List<int> Shuffle(List<int> items, Random rng)
    {
        var arr = items.ToList();
        for (var i = arr.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}