namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing an info guide.</summary>
public record InfoGuideDto
{
    /// <summary>Gets the unique identifier of the info guide.</summary>
    public required Guid Id { get; init; }
    /// <summary>Gets or sets the title of the info guide.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the ID of the tournament this info guide is associated with.</summary>
    public int? TournamentId { get; set; }
    /// <summary>Gets the tournament this info guide is associated with.</summary>
    public TournamentBriefDto? Tournament { get; init; }
}