namespace BoltonCup.Core;

/// <summary>A franchise with its title and season counts, for list views. Not persisted.</summary>
public sealed record FranchiseSummary(Franchise Franchise, int TitleCount, int SeasonCount);

/// <summary>
/// A franchise's full history. <see cref="AllTime"/> is the sum of <see cref="Seasons"/>, so a game between two
/// teams of this franchise counts once per side (see <see cref="IntraFranchiseGames"/>). Not persisted.
/// </summary>
public sealed record FranchiseDetail
{
    public required Franchise Franchise { get; init; }
    public required IReadOnlyList<Account> Owners { get; init; }
    public required IReadOnlyList<Tournament> Titles { get; init; }
    public required FranchiseRecord AllTime { get; init; }
    public required int IntraFranchiseGames { get; init; }
    public required IReadOnlyList<FranchiseSeason> Seasons { get; init; }
    public required IReadOnlyList<FranchiseSkaterLeader> SkaterLeaders { get; init; }
    public required IReadOnlyList<FranchiseGoalieLeader> GoalieLeaders { get; init; }
}

/// <summary>
/// Win/loss totals over completed games. <see cref="Losses"/> includes OT/SO losses; a forfeit (completed 0–0)
/// is a tie.
/// </summary>
public sealed record FranchiseRecord(int GamesPlayed, int Wins, int Losses, int Ties, int GoalsFor, int GoalsAgainst)
{
    public static FranchiseRecord Empty { get; } = new FranchiseRecord(0, 0, 0, 0, 0, 0);

    public FranchiseRecord Add(FranchiseRecord other) => new(
        GamesPlayed + other.GamesPlayed,
        Wins + other.Wins,
        Losses + other.Losses,
        Ties + other.Ties,
        GoalsFor + other.GoalsFor,
        GoalsAgainst + other.GoalsAgainst);
}

/// <summary>One season (team row) of a franchise, with its as-played brand on <see cref="Team"/>.</summary>
public sealed record FranchiseSeason(
    Team Team,
    Tournament? Tournament,
    FranchiseRecord Record,
    IReadOnlyList<Account> GeneralManagers,
    bool IsChampion);

/// <summary>A skater's totals across every season played for a franchise.</summary>
public sealed record FranchiseSkaterLeader(
    int AccountId,
    int PlayerId,
    string FirstName,
    string LastName,
    string? ProfilePicture,
    int Seasons,
    int GamesPlayed,
    int Goals,
    int Assists,
    int Points);

/// <summary>
/// A goalie's totals across every season played for a franchise. <see cref="GoalsAgainstAverage"/> is goals
/// against per game played, unweighted by minutes.
/// </summary>
public sealed record FranchiseGoalieLeader(
    int AccountId,
    int PlayerId,
    string FirstName,
    string LastName,
    string? ProfilePicture,
    int Seasons,
    int GamesPlayed,
    int Wins,
    int Shutouts,
    int Saves,
    int ShotsAgainst,
    double SavePercentage,
    double GoalsAgainstAverage);