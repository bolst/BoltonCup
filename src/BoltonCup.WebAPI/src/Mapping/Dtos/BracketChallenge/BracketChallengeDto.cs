namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a bracket challenge event.</summary>
public record BracketChallengeDto
{
    /// <summary>Gets the bracket challenge ID.</summary>
    public int Id { get; init; }
    /// <summary>Gets the bracket challenge title.</summary>
    public string? Title { get; init; }
    /// <summary>Gets the external link for the bracket challenge.</summary>
    public string? Link { get; init; }
    /// <summary>Gets the entry fee for the bracket challenge.</summary>
    public decimal? Fee { get; init; }
    /// <summary>Gets whether the bracket challenge is currently open for registration.</summary>
    public bool IsOpen { get; init; }
    /// <summary>Gets the URL of the bracket challenge logo.</summary>
    public string? Logo { get; init; }
    /// <summary>Gets the date when registration closes.</summary>
    public DateTime? CloseDate { get; init; }
}