namespace BoltonCup.WebAPI.Mapping;

public record CreateDraftRequest
{
    public int TournamentId { get; set; }
}