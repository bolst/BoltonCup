namespace BoltonCup.Shared.Data;

public class BCDraftPick
{
    public int id { get; set; }
    public required int draft_id { get; set; }
    public required int round { get; set; }
    public required int pick { get; set; }
    public int player_id { get; set; }
    public int OverallPick(int picksPerRound) => (round - 1) * picksPerRound + pick;
}