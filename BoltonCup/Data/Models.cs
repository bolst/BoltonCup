using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Data;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string PrimaryHex { get; set; }
    public string SecondaryHex { get; set; }
    public string? TertiaryHex { get; set; } = "";
    public string? LogoUrl { get; set; } = "";
}

public class ScheduledGame
{
    public int GameId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public DateTime Date { get; set; }
    public string GameType { get; set; }
    public string Location { get; set; }
    public string Rink { get; set; }
}

public class TeamPlayer
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string Position { get; set; }
    public int JerseyNumber { get; set; }
    public int PlayerId { get; set; }
    public int TeamId { get; set; }

}

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string PreferredBeer { get; set; }
}

public class PlayerProfile
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string PreferredBeer { get; set; }
    public string Position { get; set; }
    public int JerseyNumber { get; set; }
    public int CurrentTeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int PIMs { get; set; }
}

public class PlayerGameSummary
{
    public int GameId { get; set; }
    public int TeamId { get; set; }
    public int OpponentTeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public DateTime GameDate { get; set; }
}
public class GoalieGameSummary
{
    public int GameId { get; set; }
    public int TeamId { get; set; }
    public int OpponentTeamId { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public DateTime GameDate { get; set; }
}

public class GameGoal
{
    public int GameId { get; set; }
    public int ScorerJersey { get; set; }
    public int? Assist1Jersey { get; set; }
    public int? Assist2Jersey { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public int ScorerId { get; set; }
    public int? Assist1Id { get; set; }
    public int? Assist2Id { get; set; }
    public string ScorerName { get; set; } = "";
    public string? Assist1Name { get; set; }
    public string? Assist2Name { get; set; }
}

public class GamePenalty
{
    public int GameId { get; set; }
    public int PlayerJersey { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public string Infraction { get; set; } = "";
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
}

public class PlayerStatline
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public int PlayerNumber { get; set; }
    public string Position { get; set; } = "";
    public int TeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
}

public class GoalieStatline
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public int PlayerNumber { get; set; }
    public int TeamId { get; set; }
    public double GAA { get; set; }
    public int Shutouts { get; set; }
}

public class GameScore
{
    public int GameId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
}

public class PlayerProfilePicture
{
    public int PlayerId { get; set; }
    public string Source { get; set; } = "spotlight/default.jpg";
}