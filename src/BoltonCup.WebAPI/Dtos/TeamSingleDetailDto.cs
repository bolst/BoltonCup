using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record TeamSingleDetailDto : TeamDetailDto, IMappable<Team, TeamSingleDetailDto>
{
    private List<TeamGameSummary> _homeGames { get; init; } = [];
    private List<TeamGameSummary> _awayGames { get; init; } = [];
    
    
    public List<PlayerSummary> Players { get; init; } = [];
    
    public List<TeamGameSummary> Games => 
        _homeGames.Concat(_awayGames).OrderBy(g => g.GameTime).ToList();
    
    
    static Expression<Func<Team, TeamSingleDetailDto>> IMappable<Team, TeamSingleDetailDto>.Projection =>
        team => new TeamSingleDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            TournamentId = team.Tournament.Id,
            TournamentName = team.Tournament.Name,
            LogoUrl = team.LogoUrl,
            BannerUrl = team.BannerUrl,
            PrimaryColorHex = team.PrimaryColorHex,
            SecondaryColorHex = team.SecondaryColorHex,
            TertiaryColorHex = team.TertiaryColorHex,
            GoalSongUrl = team.GoalSongUrl,
            PenaltySongUrl = team.PenaltySongUrl,
            GmAccountId = team.GmAccountId,
            GmFirstName = team.GeneralManager!.FirstName,
            GmLastName = team.GeneralManager.LastName,
            GmProfilePicture = team.GeneralManager.ProfilePicture,
            Players = team.Players
                .Select(p => new PlayerSummary
                {
                    Id = p.Id,
                    AccountId = p.AccountId,
                    Position = p.Position,
                    JerseyNumber = p.JerseyNumber,
                    FirstName = p.Account!.FirstName,
                    LastName = p.Account.LastName,
                    Birthday = p.Account.Birthday,
                    ProfilePicture = p.Account.ProfilePicture,
                })
                .ToList(),
            _homeGames = team.HomeGames
                .Select(game => new TeamGameSummary
                {
                    Id = game.Id,
                    TournamentId = game.TournamentId,
                    TournamentName = game.Tournament.Name,
                    GameTime = game.GameTime,
                    GameType = game.GameType,
                    Venue = game.Venue, 
                    Rink = game.Rink,
                    IsHome = true,
                    GoalsFor = game.Goals.Count(g => g.TeamId == game.HomeTeamId),
                    GoalsAgainst = game.Goals.Count(g => g.TeamId == game.AwayTeamId),
                    Opponent = game.AwayTeam == null ? null : new TeamSummary(game.AwayTeam)
                })
                .ToList(),            
            _awayGames = team.AwayGames
                .Select(game => new TeamGameSummary
                {
                    Id = game.Id,
                    TournamentId = game.TournamentId,
                    TournamentName = game.Tournament.Name,
                    GameTime = game.GameTime,
                    GameType = game.GameType,
                    Venue = game.Venue, 
                    Rink = game.Rink,
                    IsHome = false,
                    GoalsFor = game.Goals.Count(g => g.TeamId == game.AwayTeamId),
                    GoalsAgainst = game.Goals.Count(g => g.TeamId == game.HomeTeamId),
                    Opponent = game.HomeTeam == null ? null : new TeamSummary(game.HomeTeam)
                })
                .ToList(),
        };


    public record TeamGameSummary : GameSummary
    {
        public required bool IsHome { get; init; }
        public required int GoalsFor { get; init; }
        public required int GoalsAgainst { get; init; }
        public required TeamSummary? Opponent { get; init; }
    }
}

