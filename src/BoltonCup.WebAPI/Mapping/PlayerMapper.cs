using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IPlayerMapper
{
    IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players);
    PlayerSingleDto? ToDto(Player? player);
}

public class PlayerMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IPlayerMapper
{
    public IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players)
    {
        return players.ProjectTo(player => new PlayerDto
        {
            Id = player.Id, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar), 
            BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
            PreferredBeer = player.Account.PreferredBeer, 
            Tournament = _briefMapper.ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : _briefMapper.ToTeamBriefDto(player.Team),
        });
    }    
    
    
    public PlayerSingleDto? ToDto(Player? player)
    {
        if (player is null)
            return null;
        
        return new PlayerSingleDto
        { 
            Id = player.Id, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar), 
            BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
            PreferredBeer = player.Account.PreferredBeer, 
            Tournament = _briefMapper.ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : _briefMapper.ToTeamBriefDto(player.Team),
            TournamentStats = _briefMapper.ToPlayerTournamentStatsDto(player),
            GameByGame = _briefMapper.ToPlayerGameByGameDtos(player),            
        };
    }
}
