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
    public required int gm_account_id { get; set; }
    public string? banner_image { get; set; }
    public string? goal_horn_url { get; set; }

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
    public string? playlist_id { get; set; }
    public string? last_played { get; set; }
    public string? last_played_id { get; set; }
    public string HomeTeamName { get; set; }
    public string HomeTeamLogo { get; set; }
    public string HomeTeamNameShort { get; set; }
    public string AwayTeamName { get; set; }
    public string AwayTeamNameShort { get; set; }
    public string AwayTeamLogo { get; set; }
}

public class PlayerProfile : IEquatable<PlayerProfile>
{
    public int id { get; set; }
    public required string name { get; set; }
    public DateTime dob { get; set; }
    public string position { get; set; } = "";
    public int? jersey_number { get; set; }
    public int? account_id { get; set; }
    public int? team_id { get; set; }
    public bool champion { get; set; }
    public int tournament_id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? HighestLevel { get; set; }
    public string? ProfilePicture { get; set; }
    
    public bool IsForward => position == "forward";
    public bool IsDefense => position == "defense";
    public bool IsGoalie => position == "goalie";
    
    
    #region IEquatable
    
    public bool Equals(PlayerProfile? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as PlayerProfile);
    public override int GetHashCode() => id.GetHashCode();
    public static bool operator == (PlayerProfile? left, PlayerProfile? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (PlayerProfile? left, PlayerProfile? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}

public class PlayerGameSummary : BCGame
{
    public int team_id { get; set; }
    public string team_name { get; set; }
    public string team_name_short { get; set; }
    public string team_logo_url { get; set; }
    public int? team_score { get; set; }
    public int opponent_team_id { get; set; }
    public string opponent_name { get; set; }
    public string opponent_name_short { get; set; }
    public string opponent_logo_url { get; set; }
    public int? opponent_team_score { get; set; }
    public int player_id { get; set; }
    public int goals { get; set; }
    public int assists { get; set; }
}



public class GoalieGameSummary : BCGame
{
    public int team_id { get; set; }
    public string team_name { get; set; }
    public string team_name_short { get; set; }
    public string team_logo_url { get; set; }
    public int? team_score { get; set; }
    public int opponent_team_id { get; set; }
    public string opponent_name { get; set; }
    public string opponent_name_short { get; set; }
    public string opponent_logo_url { get; set; }
    public int? opponent_team_score { get; set; }
    public int player_id { get; set; }
    public bool win { get; set; }
}

public class GameGoal
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public int ScorerId { get; set; }
    public int? Assist1Id { get; set; }
    public int? Assist2Id { get; set; }
    public string ScorerName { get; set; }
    public string? Assist1Name { get; set; }
    public string? Assist2Name { get; set; }
    public string ScorerProfilePic { get; set; }
    public string TeamName { get; set; }
    public string TeamLogo { get; set; }
}

public class GamePenalty
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public TimeSpan Time { get; set; }
    public int Period { get; set; }
    public int TeamId { get; set; }
    public string Infraction { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string TeamLogo { get; set; }
}

public class PlayerStatLine
{
    public int player_id { get; set; }
    public string player_name { get; set; } = "";
    public int jersey_number { get; set; }
    public string position { get; set; } = "";
    public int? team_id { get; set; }
    public string profilepicture { get; set; } = "";
    public string? team_name { get; set; }
    public string? team_name_short { get; set; }
    public string? team_logo { get; set; }
    public int goals { get; set; }
    public int assists { get; set; }
    public int tournament_id { get; set; }

    private string GetShortName()
    {
        var names = player_name.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
        var initials = names.SkipLast(1).Select(x => x.ToUpper().First());

        return $"{string.Join("", initials)}. {names.Last()}";
    }

    public string ShortName => GetShortName();
}

public class GoalieStatLine
{
    public int player_id { get; set; }
    public string player_name { get; set; } = "";
    public int jersey_number { get; set; }
    public int? team_id { get; set; }
    public string profilepicture { get; set; } = "";
    public string? team_name { get; set; }
    public string? team_name_short { get; set; }
    public string? team_logo { get; set; }
    public double GAA { get; set; }
    public int games_played { get; set; }
    public int shutouts { get; set; }
    public int tournament_id { get; set; }
    
    private string GetShortName()
    {
        var names = player_name.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
        var initials = names.SkipLast(1).Select(x => x.ToUpper().First());

        return $"{string.Join("", initials)}. {names.Last()}";
    }

    public string ShortName => GetShortName();
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
    
    [Required(ErrorMessage="This is required")] 
    public string JerseySize { get; set; }
    
    public bool IsForward => Position == "forward";
    public bool IsDefense => Position == "defense";
    public bool IsGoalie => Position == "goalie";
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
    public required int id { get; set; }
    public required string Roles { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public DateTime? Birthday { get; set; }
    public required string Position { get; set; }
    public required string HighestLevel { get; set; }
    public required string ProfilePicture { get; set; }
    public bool Payed { get; set; }
    public required bool IsActive { get; set; }
    public int? preferred_number1 { get; set; }
    public int? preferred_number2 { get; set; }
    public int? preferred_number3 { get; set; }
    public string? preferred_beer { get; set; }
    public Guid PCKey { get; set; }
    public bool Editable { get; set; }
    public string? Message { get; set; }
    public string? SongRequest { get; set; }
    public string? SongRequestId { get; set; }
    public int? preferred_teammate1 { get; set; }
    public int? preferred_teammate2 { get; set; }


    public string FullName => $"{FirstName} {LastName}";
    public bool IsForward => Position == "forward";
    public bool IsDefense => Position == "defense";
    public bool IsGoalie => Position == "goalie";

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
    public bool registration_open { get; set; }
    public bool payment_open { get; set; }
    public string? payment_link { get; set; }
    public string? player_payment_link { get; set; }
    public string? goalie_payment_link { get; set; }
    public int player_limit { get; set; }
    public int goalie_limit { get; set; }

    public bool Equals(BCTournament? other) => other is not null && other.tournament_id == tournament_id;
    public override bool Equals(object? obj) => Equals(obj as BCTournament);
    public override int GetHashCode() => tournament_id.GetHashCode();
}






public class BCDraftPick
{
    public int id { get; set; }
    public required int draft_id { get; set; }
    public required int round { get; set; }
    public required int pick { get; set; }
    public int player_id { get; set; }
}

public class BCDraftOrder
{
    public required int id { get; set; }
    public required int draft_id { get; set; }
    public required int order { get; set; }
    public required int team_id { get; set; }
}

public class BCDraftPickDetail : BCDraftPick
{
    public required string Name { get; set; }
    public required string Birthday { get; set; }
    public required int TeamId { get; set; }
    public required string Position { get; set; }
    public string? ProfilePicture { get; set; }
    public required string TeamName { get; set; }
    public required string TeamNameShort { get; set; }
    public required string TeamLogo { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    
}


public class BCSponsor
{
    public int id { get; set; }
    public required string name { get; set; }
    public bool is_active { get; set; }
    public string site_url { get; set; }
    public string logo_url { get; set; }
}



public class BCAvailability : IEquatable<BCAvailability>
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int GameId { get; set; }
    public string? Availability { get; set; }
    public DateTime GameDate { get; set; }
    public int TeamId { get; set; }
    public int OpponentId { get; set; }
    public string TeamName { get; set; }
    public string OpponentName { get; set; }
    public string TeamLogo { get; set; }
    public string OpponentLogo { get; set; }
    public string GameType { get; set; }
    
    #region IEquatable
    
    public bool Equals(BCAvailability? other) => other is not null && other.Id == Id;
    public override bool Equals(object? obj) => Equals(obj as BCAvailability);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator == (BCAvailability? left, BCAvailability? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (BCAvailability? left, BCAvailability? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}


public class BCRefreshToken
{
    public required int id { get; set; }
    public required DateTime created_at { get; set; }
    public required string token { get; set; }
    public required Guid local_id { get; set; }
}


public class BCSong : IEquatable<BCSong>
{
    public int id { get; set; }
    public string spotify_id { get; set; }
    public string name { get; set; }
    public int account_id { get; set; }
    public string album_cover { get; set; }
    
    #region IEquatable
    
    public bool Equals(BCSong? other) => other is not null && other.id == id;
    public override bool Equals(object? obj) => Equals(obj as BCSong);
    public override int GetHashCode() => id.GetHashCode();
    public static bool operator == (BCSong? left, BCSong? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (BCSong? left, BCSong? right) => left is null ? right is not null : !left.Equals(right);

    #endregion

}

public class GoalEntry
{
    public required BCGame Game { get; set; }
    public required BCTeam Team { get; set; }
    public required PlayerProfile Scorer { get; set; }
    public PlayerProfile? Assist1 { get; set; }
    public PlayerProfile? Assist2 { get; set; }
    public required int Period { get; set; }
    public required TimeSpan Time { get; set; }
}


public class PenaltyEntry
{
    public required BCGame Game { get; set; }
    public required BCTeam Team { get; set; }
    public required PlayerProfile Player { get; set; }
    public required string Infraction { get; set; }
    public required int DurationMins { get; set; }
    public required int Period { get; set; }
    public required TimeSpan Time { get; set; }
}