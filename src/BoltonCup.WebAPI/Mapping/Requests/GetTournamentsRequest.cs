namespace BoltonCup.WebAPI.Mapping;

public record GetTournamentsRequest : RequestBase
{
    public bool? RegistrationOpen { get; set; }
}