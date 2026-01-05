using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;

namespace BoltonCup.WebAPI.Dtos;

public record PlayerSingleDetailDto : PlayerDetailDto, IMappable<Player, PlayerSingleDetailDto>
{
    static Expression<Func<Player, PlayerSingleDetailDto>> IMappable<Player, PlayerSingleDetailDto>.Projection =>
        player => new PlayerSingleDetailDto
        {
            Id = player.Id, 
            TournamentId = player.TournamentId, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            PreferredBeer = player.Account.PreferredBeer, 
            TournamentName = player.Tournament.Name, 
            TeamName = player.Team!.Name, 
            TeamNameShort = player.Team.NameShort, 
            TeamAbbreviation = player.Team.Abbreviation, 
            TeamLogoUrl = player.Team.LogoUrl, 
            TeamBannerUrl = player.Team.BannerUrl, 
            TeamPrimaryHex = player.Team.PrimaryColorHex, 
            TeamSecondaryHex = player.Team.SecondaryColorHex, 
            TeamTertiaryHex = player.Team.TertiaryColorHex,
        };
}