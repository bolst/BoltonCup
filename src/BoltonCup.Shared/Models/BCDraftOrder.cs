namespace BoltonCup.Shared.Data;

public class BCDraftOrder
{
    public required int id { get; set; }
    public required int draft_id { get; set; }
    public required int order { get; set; }
    public required int team_id { get; set; }
}