using BoltonCup.Core;

namespace BoltonCup.WebAPI.Dtos.Summaries;

public record TeamSummary
{
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamNameShort { get; set; }
    public string? TeamAbbreviation { get; set; }
    public string? TeamLogoUrl { get; set; }
    public string? TeamBannerUrl { get; set; }

    public TeamSummary(Team team)
    {
        TeamId = team.Id;
        TeamName = team.Name;
        TeamNameShort = team.NameShort;
        TeamAbbreviation = team.Abbreviation;
        TeamLogoUrl = team.LogoUrl;
        TeamBannerUrl = team.BannerUrl;
    }
}