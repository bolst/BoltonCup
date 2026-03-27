namespace BoltonCup.WebAPI.Mapping;

public record CreateTournamentPaymentIntentRequest
{
    public bool IsGoalie { get; set; }
}