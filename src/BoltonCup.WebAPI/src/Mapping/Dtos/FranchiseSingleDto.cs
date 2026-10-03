namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single franchise: owners, titles, all-time record, season history and leaders.</summary>
public sealed record FranchiseSingleDto : FranchiseBriefDto
{
    /// <summary>Gets the URL of the franchise banner image.</summary>
    public string? BannerUrl { get; init; }
    /// <summary>Gets the franchise's owners.</summary>
    public required List<FranchiseOwnerDto> Owners { get; init; } = [];
    /// <summary>Gets the tournaments won by any team of this franchise, oldest first.</summary>
    public required List<TournamentBriefDto> Titles { get; init; } = [];
    /// <summary>
    /// Gets the all-time record: the sum of <see cref="Seasons"/>. A game between two teams of this franchise counts
    /// once for each side.
    /// </summary>
    public required FranchiseRecordDto AllTime { get; init; }
    /// <summary>Gets the number of completed games played between two teams of this franchise.</summary>
    public required int IntraFranchiseGames { get; init; }
    /// <summary>Gets the franchise's seasons, newest first, each with its as-played team brand.</summary>
    public required List<FranchiseSeasonDto> Seasons { get; init; } = [];
    /// <summary>Gets the top skaters across all seasons, sorted by points, then goals, then fewest games played.</summary>
    public required List<FranchiseSkaterLeaderDto> SkaterLeaders { get; init; } = [];
    /// <summary>Gets the top goalies (at least two games played), sorted by wins, then goals-against average.</summary>
    public required List<FranchiseGoalieLeaderDto> GoalieLeaders { get; init; } = [];
}