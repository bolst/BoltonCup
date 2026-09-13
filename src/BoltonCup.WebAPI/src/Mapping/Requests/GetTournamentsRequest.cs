namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to retrieve a paged list of tournaments with optional filters.</summary>
public record GetTournamentsRequest : RequestBase
{
    /// <summary>Gets or sets an optional filter to return only tournaments with open registration.</summary>
    public bool? RegistrationOpen { get; set; }
}