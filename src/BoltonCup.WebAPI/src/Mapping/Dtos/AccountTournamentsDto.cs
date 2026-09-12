namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a tournament associated with an account.</summary>
public sealed record AccountTournamentDto
{
    /// <summary>Gets the tournament summary.</summary>
    public required TournamentBriefDto Tournament { get; init; }
    /// <summary>Gets the team the account participated on, if any.</summary>
    public TeamBriefDto? Team { get; init; }
}