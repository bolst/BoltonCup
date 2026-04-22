namespace BoltonCup.WebAPI.Mapping;

public record DraftPlayerRequest
{
    public int PlayerId { get; set; }
    public int TeamId { get; set; }
    public int OverallPick { get; set; }
}