using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record TeamDetailDto : IMappable<Team, TeamDetailDto>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string NameShort { get; init; }
    public required string Abbreviation { get; init; }
    public TournamentSummary? Tournament { get; init; }
    public string? LogoUrl { get; init; }
    public string? BannerUrl { get; init; }
    public required string PrimaryColorHex { get; init; }
    public required string SecondaryColorHex { get; init; }
    public string? TertiaryColorHex { get; init; }
    public string? GoalSongUrl { get; init; }
    public string? PenaltySongUrl { get; init; }
    
    public int? GmAccountId { get; init; }
    public string? GmFirstName { get; init; }
    public string? GmLastName { get; init; }
    public string? GmProfilePicture  { get; init; }


    static Expression<Func<Team, TeamDetailDto>> IMappable<Team, TeamDetailDto>.Projection =>
        team => new TeamDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            Tournament = new TournamentSummary(team.Tournament),
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

