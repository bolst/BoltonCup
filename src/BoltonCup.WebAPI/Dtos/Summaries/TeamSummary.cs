using BoltonCup.Core;

namespace BoltonCup.WebAPI.Dtos.Summaries;

public record TeamSummary
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? NameShort { get; set; }
    public string? Abbreviation { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }

    public TeamSummary(Team team)
    {
        Id = team.Id;
        Name = team.Name;
        NameShort = team.NameShort;
        Abbreviation = team.Abbreviation;
        LogoUrl = team.LogoUrl;
        BannerUrl = team.BannerUrl;
    }
}