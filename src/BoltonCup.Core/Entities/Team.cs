namespace BoltonCup.Core;

public class Team : EntityBase
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string NameShort { get; set; }
    public required string Abbreviation { get; set; }
    public int? TournamentId { get; set; }
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }

    /// <summary>The library track played when this team scores. Null until a GM picks one.</summary>
    public int? GoalSongTrackId { get; set; }

    /// <summary>The library track played when this team wins. Null until a GM picks one.</summary>
    public int? WinSongTrackId { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public ICollection<Account> GeneralManagers { get; set; } = [];
    public TournamentMusicTrack? GoalSongTrack { get; set; }
    public TournamentMusicTrack? WinSongTrack { get; set; }
    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Game> HomeGames { get; set; } = [];
    public ICollection<Game> AwayGames { get; set; } = [];
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<Penalty> Penalties { get; set; } = [];
    public ICollection<SkaterStat> SkaterGameLogs { get; set; } = [];
    public ICollection<GoalieStat> GoalieGameLogs { get; set; } = [];

    public override string ToString() => Name;
}

public class TeamComparer : IEqualityComparer<Team>
{
    public bool Equals(Team? item1, Team? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Team item) => item.Id;
}