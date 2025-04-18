using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BoltonCup.Shared.Data;

public class BCTeam : IEquatable<BCTeam>
{
    public required int id { get; set; }
    public required string name { get; set; }
    public string name_short { get; set; } = "";
    public required string primary_color_hex { get; set; }
    public required string secondary_color_hex { get; set; }
    public string? tertiary_color_hex { get; set; } = "";
    public string? logo_url { get; set; } = "";
    public required int tournament_id { get; set; }

    public bool Equals(BCTeam? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as BCTeam);
    public override int GetHashCode() => id.GetHashCode();
}

public class BCGame
{
    public int id { get; set; }
    public int home_team_id { get; set; }
    public int away_team_id { get; set; }
    public DateTime date { get; set; }
    public string type { get; set; } = "";
    public string location { get; set; } = "";
    public string rink { get; set; } = "";
    public int tournament_id { get; set; }
    public int home_score { get; set; }
    public int away_score { get; set; }
    public required string state { get; set; }
}

public class BCTeamPlayer
{
    public required string name { get; set; }
    public DateTime dob { get; set; }
    public string position { get; set; } = "";
    public int jersey_number { get; set; }
    public int player_id { get; set; }
    public int team_id { get; set; }

}

public class PlayerProfile
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string PreferredBeer { get; set; } = "";
    public string Position { get; set; } = "";
    public int JerseyNumber { get; set; }
    public int CurrentTeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int PIMs { get; set; }
    public bool IsWinner { get; set; }
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

public class PlayerStatLine
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public int PlayerNumber { get; set; }
    public string Position { get; set; } = "";
    public int TeamId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int TournamentId { get; set; }
}

public class GoalieStatLine
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public int PlayerNumber { get; set; }
    public int TeamId { get; set; }
    public double GAA { get; set; }
    public int Shutouts { get; set; }
    public int TournamentId { get; set; }
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

public class RegisterFormModel
{
    [Required(ErrorMessage="First name is required")] 
    public string FirstName { get; set; }
        
    [Required(ErrorMessage="Last name is required")] 
    public string LastName { get; set; }
        
    [Required(ErrorMessage="Email is required")] 
    public string Email { get; set; }
        
    [Required(ErrorMessage="Birthday is required")] 
    public DateTime? Birthday { get; set; }
        
    [Required(ErrorMessage="Position is required")] 
    public string Position { get; set; }
        
    [Required(ErrorMessage="This is required")] 
    public string HighestLevel { get; set; }
}

public class LoginFormModel
{
    [Required(ErrorMessage="Email is required")] 
    public string Email { get; set; }

    [Required(ErrorMessage="Password is required")] 
    public string Password { get; set; }
}

public class BCAccount
{
    public required string Roles { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateTime? Birthday { get; set; }
    public required string Position { get; set; }
    public required string HighestLevel { get; set; }
    public required string ProfilePicture { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        IEnumerable<Claim> claims =
        [
            new(ClaimTypes.Name, FullName),
            new(ClaimTypes.Email, Email)
        ];

        foreach (var role in Roles.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            claims = claims.Append(new Claim(ClaimTypes.Role, role));
        }

        return new(new ClaimsIdentity(claims, "BoltonCup"));
    }
}

public class BCTournament : IEquatable<BCTournament>
{
    public required int tournament_id { get; set; }
    public DateTime? start_date { get; set; }
    public DateTime? end_date { get; set; }
    public int? winning_team_id { get; set; }
    public required string name { get; set; }
    
    public bool Equals(BCTournament? other) => other is not null && other.tournament_id == tournament_id;
    public override bool Equals(object? obj) => Equals(obj as BCTournament);
    public override int GetHashCode() => tournament_id.GetHashCode();
}
