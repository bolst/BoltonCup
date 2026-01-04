using BoltonCup.Core;

namespace BoltonCup.WebAPI.Dtos;


public record PlayerDetailDto
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int? AccountId { get; set; }
    public string? Position { get; set; }
    public int? JerseyNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string? ProfilePicture { get; set; }
    public string? PreferredBeer { get; set; }
    public string TournamentName { get; set; }
    public string? TeamName { get; set; }
    public string? TeamNameShort { get; set; }
    public string? TeamAbbreviation { get; set; }
    public string? TeamLogoUrl { get; set; }
    public string? TeamBannerUrl { get; set; }
    public string? TeamPrimaryHex { get; set; }
    public string? TeamSecondaryHex { get; set; }
    public string? TeamTertiaryHex { get; set; }
}


public sealed record SinglePlayerDetailDto : PlayerDetailDto
{
    
    public ICollection<PlayerGameSummary> Games { get; set; }

    public sealed record PlayerGameSummary
    {
        public int Id { get; set; }
        public DateTime GameTime { get; set; }
        public string? GameType { get; set; }
        public string? Venue { get; set; } 
        public string? Rink { get; set; }
        public bool IsHomeGame { get; set; }
        public int? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        public string? OpponentTeamNameShort { get; set; }
        public string? OpponentTeamAbbreviation { get; set; }
        public string? OpponentTeamLogoUrl { get; set; }
        
        public int TotalTeamGoals { get; set; }
        public int TotalOpponentGoals { get; set; }
        public int TotalPlayerGoals { get; set; }
        public int TotalPlayerAssists { get; set; }
    }
    
}


public static class PlayerDetailDtoExtensions
{
    public static PlayerDetailDto ToPlayerDetailDto(this Player entity)
    {
        return new PlayerDetailDto
        {
            Id = entity.Id, 
            TournamentId = entity.TournamentId, 
            AccountId = entity.AccountId, 
            Position = entity.Position, 
            JerseyNumber = entity.JerseyNumber, 
            FirstName = entity.Account?.FirstName, 
            LastName = entity.Account?.LastName, 
            Birthday = entity.Account?.Birthday, 
            ProfilePicture = entity.Account?.ProfilePicture, 
            PreferredBeer = entity.Account?.PreferredBeer, 
            TournamentName = entity.Tournament.Name, 
            TeamName = entity.Team?.Name, 
            TeamNameShort = entity.Team?.NameShort, 
            TeamAbbreviation = entity.Team?.Abbreviation, 
            TeamLogoUrl = entity.Team?.LogoUrl, 
            TeamBannerUrl = entity.Team?.BannerUrl, 
            TeamPrimaryHex = entity.Team?.PrimaryColorHex, 
            TeamSecondaryHex = entity.Team?.SecondaryColorHex, 
            TeamTertiaryHex = entity.Team?.TertiaryColorHex,
        };
    }
    
    public static SinglePlayerDetailDto ToSinglePlayerDetailDto(this Player entity)
    {
        var games = entity.Team is not null
            ? entity.Team.HomeGames.Concat(entity.Team.AwayGames)
            : [];
        return new SinglePlayerDetailDto
        {
            Id = entity.Id, 
            TournamentId = entity.TournamentId, 
            AccountId = entity.AccountId, 
            Position = entity.Position, 
            JerseyNumber = entity.JerseyNumber, 
            FirstName = entity.Account?.FirstName, 
            LastName = entity.Account?.LastName, 
            Birthday = entity.Account?.Birthday, 
            ProfilePicture = entity.Account?.ProfilePicture, 
            PreferredBeer = entity.Account?.PreferredBeer, 
            TournamentName = entity.Tournament.Name, 
            TeamName = entity.Team?.Name, 
            TeamNameShort = entity.Team?.NameShort, 
            TeamAbbreviation = entity.Team?.Abbreviation, 
            TeamLogoUrl = entity.Team?.LogoUrl, 
            TeamBannerUrl = entity.Team?.BannerUrl, 
            TeamPrimaryHex = entity.Team?.PrimaryColorHex, 
            TeamSecondaryHex = entity.Team?.SecondaryColorHex, 
            TeamTertiaryHex = entity.Team?.TertiaryColorHex,
            Games = games.Select(g => 
            { 
                var isHome = entity.TeamId == g.HomeTeamId;
                var opponent = isHome ? g.AwayTeam : g.HomeTeam;
                return new SinglePlayerDetailDto.PlayerGameSummary
                {
                    Id = g.Id,
                    GameTime = g.GameTime,
                    GameType = g.GameType,
                    Venue = g.Venue,
                    Rink = g.Rink,
                    IsHomeGame = isHome,
                    OpponentTeamId = opponent?.Id,
                    OpponentTeamName = opponent?.Name,
                    OpponentTeamNameShort = opponent?.NameShort,
                    OpponentTeamAbbreviation = opponent?.Abbreviation,
                    OpponentTeamLogoUrl = opponent?.LogoUrl,
                    
                    TotalTeamGoals = g.Goals.Count(x => x.TeamId != opponent?.Id),
                    TotalOpponentGoals = g.Goals.Count(x => x.TeamId == opponent?.Id),
                    TotalPlayerGoals = g.Goals.Count(x => x.GoalPlayerId == entity.Id),
                    TotalPlayerAssists = g.Goals.Count(x => x.Assist1PlayerId == entity.Id || x.Assist2PlayerId == entity.Id),
                };
            }).ToList(),
        };
    }
}