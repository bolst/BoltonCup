using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;

namespace BoltonCup.WebAPI.Dtos;

public sealed record TeamDetailDto : IMappable<Team, TeamDetailDto>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string NameShort { get; set; }
    public required string Abbreviation { get; set; }
    public int? TournamentId { get; set; }
    public string? TournamentName { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public required string PrimaryColorHex { get; set; }
    public required string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    public string? GoalSongUrl { get; set; }
    public string? PenaltySongUrl { get; set; }
    
    public int? GmAccountId { get; set; }
    public string? GmFirstName { get; set; }
    public string? GmLastName { get; set; }
    public string? GmProfilePicture  { get; set; }


    static Expression<Func<Team, TeamDetailDto>> IMappable<Team, TeamDetailDto>.Projection =>
        team => new TeamDetailDto
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
        };
}

