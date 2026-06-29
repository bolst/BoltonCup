using Sdk = BoltonCup.Sdk;

namespace BoltonCup.Draft.Services;

/// <summary>
/// Shared reorder/visibility logic for custom-ranking player lists, used by both the
/// list and grid editor views. All operations mutate the supplied master list in place
/// and keep ranks contiguous (1-based) via <see cref="Renumber"/>.
/// </summary>
public static class RankingReorder
{
    public static IReadOnlyList<Sdk.CustomRankingPlayerDto> Visible(
        IReadOnlyList<Sdk.CustomRankingPlayerDto> players, string? positionFilter)
        => positionFilter is null
            ? players
            : players.Where(p => p.Player.Position == positionFilter).ToList();

    public static bool IsFirstVisible(
        IReadOnlyList<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
        => Visible(players, positionFilter).FirstOrDefault() == player;

    public static bool IsLastVisible(
        IReadOnlyList<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
        => Visible(players, positionFilter).LastOrDefault() == player;

    /// <summary>Moves <paramref name="item"/> to <paramref name="visibleTarget"/> within the filtered view.</summary>
    public static void ReorderToVisibleIndex(
        List<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto item, int visibleTarget, string? positionFilter)
    {
        var visible = Visible(players, positionFilter).Where(p => p != item).ToList();
        var target = Math.Clamp(visibleTarget, 0, visible.Count);

        players.Remove(item);
        if (target >= visible.Count)
        {
            if (visible.Count == 0)
                players.Add(item);
            else
                players.Insert(players.IndexOf(visible[^1]) + 1, item);
        }
        else
        {
            players.Insert(players.IndexOf(visible[target]), item);
        }

        Renumber(players);
    }

    public static void MoveUp(
        List<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
    {
        var visible = Visible(players, positionFilter);
        var vi = visible.ToList().IndexOf(player);
        if (vi <= 0)
            return;

        var prev = visible[vi - 1];
        players.Remove(player);
        players.Insert(players.IndexOf(prev), player);
        Renumber(players);
    }

    public static void MoveDown(
        List<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
    {
        var visible = Visible(players, positionFilter);
        var vi = visible.ToList().IndexOf(player);
        if (vi < 0 || vi >= visible.Count - 1)
            return;

        var next = visible[vi + 1];
        players.Remove(player);
        players.Insert(players.IndexOf(next) + 1, player);
        Renumber(players);
    }

    public static void MoveToTop(
        List<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
    {
        var first = Visible(players, positionFilter).FirstOrDefault();
        if (first is null || first == player)
            return;

        players.Remove(player);
        players.Insert(players.IndexOf(first), player);
        Renumber(players);
    }

    public static void MoveToBottom(
        List<Sdk.CustomRankingPlayerDto> players, Sdk.CustomRankingPlayerDto player, string? positionFilter)
    {
        var last = Visible(players, positionFilter).LastOrDefault();
        if (last is null || last == player)
            return;

        players.Remove(player);
        players.Insert(players.IndexOf(last) + 1, player);
        Renumber(players);
    }

    public static void Renumber(List<Sdk.CustomRankingPlayerDto> players)
    {
        for (var i = 0; i < players.Count; i++)
            players[i].Rank = i + 1;
    }
}
