namespace BoltonCup.Core;

/// <summary>One Eastern-time day of games, plus the index of every game day. Not persisted.</summary>
public sealed record GameScores(
    DateOnly Date,
    DateOnly? PreviousDate,
    DateOnly? NextDate,
    IReadOnlyList<GameDay> Days,
    IReadOnlyList<Game> Games);

/// <summary>A tournament's games on one Eastern-time day.</summary>
public sealed record GameDay(DateOnly Date, int TournamentId, string TournamentName, string? TournamentLogo, int GameCount);