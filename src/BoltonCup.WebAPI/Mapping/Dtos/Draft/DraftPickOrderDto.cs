namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing an entry in the draft pick order.</summary>
public record DraftPickOrderDto
{
    /// <summary>Gets or sets the pick number in the overall draft order.</summary>
    public required int Pick { get; set; }
    /// <summary>Gets or sets the team assigned to this pick slot.</summary>
    public required TeamBriefDto Team { get; set; }
}