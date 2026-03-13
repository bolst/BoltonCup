using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public record TeamBriefDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? NameShort { get; set; }
    public string? Abbreviation { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }

    public TeamBriefDto(Team team)
    {
        Id = team.Id;
        Name = team.Name;
        NameShort = team.NameShort;
        Abbreviation = team.Abbreviation;
        LogoUrl = team.LogoUrl;
        BannerUrl = team.BannerUrl;
    }
}

public record TeamInGameDto : TeamBriefDto
{
    public int Goals { get; set; }

    public TeamInGameDto(Team team, ICollection<Goal> goals) : base(team)
    {
        Goals = goals.Count(g => g.TeamId == team.Id);
    }
}
