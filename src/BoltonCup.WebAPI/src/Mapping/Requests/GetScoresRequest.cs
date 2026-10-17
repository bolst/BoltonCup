namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve one day of scores.</summary>
public record GetScoresRequest
{
    /// <summary>Gets or sets the Eastern-time day to show. When omitted, the API picks today, else the next game day, else the latest past game day.</summary>
    public DateOnly? Date { get; set; }
}