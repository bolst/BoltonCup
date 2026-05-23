namespace BoltonCup.WebAPI.Mapping;

public record CreateDraftRequest
{
    public required int TournamentId { get; set; }
    public required string Title { get; set; }
}