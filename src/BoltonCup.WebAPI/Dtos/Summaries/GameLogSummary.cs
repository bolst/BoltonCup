using BoltonCup.Core;

namespace BoltonCup.WebAPI.Dtos.Summaries;

public record GameLogSummary
{
    public int PlayerId { get; init; }
    public int TeamId { get; init; }
    public int OpponentTeamId { get; init; }
    public int GameId { get; init; }
    public int Goals { get; init; }
    public int Assists { get; init; }
    public int Points => Goals + Assists;
    public double PenaltyMinutes { get; init; }
    public int GoalsAgainst { get; set; }
    public int ShotsAgainst { get; set; }
    public int Saves { get; set; }
    public bool Shutout { get; set; }
    public bool Win { get; set; }
    public PlayerSummary Player { get; init; }

    public GameLogSummary(SkaterGameLog gameLog, Player player, Account? account)
    {
        if (gameLog.PlayerId != player.Id)
            throw new ArgumentException("Player ID does not match GameLog Player ID", nameof(player));
        PlayerId = gameLog.PlayerId;
        TeamId = gameLog.TeamId;
        OpponentTeamId = gameLog.OpponentTeamId;
        GameId = gameLog.GameId;
        Goals = gameLog.Goals;
        Assists = gameLog.Assists;
        PenaltyMinutes = gameLog.PenaltyMinutes;
        GoalsAgainst = 0;
        ShotsAgainst = 0;
        Saves = 0;
        Shutout = false;
        Win = false;
        Player = new PlayerSummary(player, account);
    }
    
    
    public GameLogSummary(GoalieGameLog gameLog, Player player, Account? account)
    {
        if (gameLog.PlayerId != player.Id)
            throw new ArgumentException("Player ID does not match GameLog Player ID", nameof(player));
        PlayerId = gameLog.PlayerId;
        TeamId = gameLog.TeamId;
        OpponentTeamId = gameLog.OpponentTeamId;
        GameId = gameLog.GameId;
        Goals = gameLog.Goals;
        Assists = gameLog.Assists;
        PenaltyMinutes = gameLog.PenaltyMinutes;
        GoalsAgainst = gameLog.GoalsAgainst;
        ShotsAgainst = gameLog.ShotsAgainst;
        Saves = gameLog.Saves;
        Shutout = gameLog.Shutout;
        Win = gameLog.Win;
        Player = new PlayerSummary(player, account);
    }
}