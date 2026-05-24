using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="Player"/> entities to DTOs and queries.</summary>
public interface IPlayerMapper
{
    /// <summary>Maps a <see cref="GetPlayersRequest"/> to a <see cref="GetPlayersQuery"/>.</summary>
    GetPlayersQuery ToQuery(GetPlayersRequest request);

    /// <summary>Maps a paged list of <see cref="Player"/> entities to a paged list of <see cref="PlayerDto"/>.</summary>
    IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players);

    /// <summary>Maps a <see cref="Player"/> to a <see cref="PlayerSingleDto"/>, or returns <see langword="null"/> if the player is null.</summary>
    PlayerSingleDto? ToDto(Player? player);
}

/// <summary>Maps <see cref="Player"/> entities to DTOs and queries.</summary>
public class PlayerMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IPlayerMapper
{
    /// <inheritdoc/>
    public GetPlayersQuery ToQuery(GetPlayersRequest request)
    {
        return new GetPlayersQuery
        {
            TournamentId = request.TournamentId,
            TeamId = request.TeamId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    /// <inheritdoc/>
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
    
    
    /// <inheritdoc/>
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
            Height = player.Account.HeightFeet is null ? null : $"{player.Account.HeightFeet}'{player.Account.HeightInches}",
            Weight = player.Account.Weight,
            Tournament = _briefMapper.ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : _briefMapper.ToTeamBriefDto(player.Team),
            TournamentStats = _briefMapper.ToPlayerTournamentStatsDto(player),
            GameByGame = _briefMapper.ToPlayerGameByGameDtos(player),            
        };
    }
}
