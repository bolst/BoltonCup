namespace BoltonCup.Shared.Data;

public class BCDraft
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public bool Snake { get; set; }
    public string State { get; set; }
    public string TournamentName { get; set; }
    public bool IsCurrentTournament { get; set; }
    public int PlayerLimit { get; set; }
    public int GoalieLimit { get; set; }
    public int NumTeams { get; set; }
    public int Rounds { get; set; }
    public int PicksPerRound { get; set; }
}