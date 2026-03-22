namespace BoltonCup.WebAPI.Mapping;

public record PlayerDto
{
    public required int Id { get; init; }
    public int? AccountId { get; init; }
    public string? Position { get; init; }
    public int? JerseyNumber { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? Birthday { get; init; }
    public string? ProfilePicture { get; init; }
    public string? BannerPicture { get; init; }
    public string? PreferredBeer { get; init; }
    public required TournamentBriefDto Tournament { get; init; }
    public TeamBriefDto? Team { get; init; }

    public string FullName => FirstName + " " + LastName;
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber.Value}" : string.Empty;
    public bool IsGoalie => Position == "goalie";
}

