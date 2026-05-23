namespace BoltonCup.WebAPI.Mapping;


public record PlayerStatLeadersDto
{
    public required string Title { get; init; }
    public IEnumerable<PlayerStatDto> Leaders { get; init; } = [];
}


public record PlayerStatDto
{
    public int PlayerId { get; init; }
    public int AccountId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Position { get; init; }
    public int? JerseyNumber { get; init; }
    public DateTime Birthday { get; init; }
    public string? ProfilePicture { get; init; }
    public int TeamId { get; init; }
    public string? TeamName { get; init; }
    public string? TeamLogoUrl { get; init; }
    public string? TeamAbbreviation { get; init; }
    public required double StatValue { get; init; }
    public required string StatString { get; init; }

    public string FullName => FirstName + " " + LastName;
    public string JerseyNumberLabel => JerseyNumber.HasValue ? $"#{JerseyNumber}" : string.Empty;
}