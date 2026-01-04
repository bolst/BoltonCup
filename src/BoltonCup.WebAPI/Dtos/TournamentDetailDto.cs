using BoltonCup.Infrastructure.Data.Entities;

namespace BoltonCup.WebAPI.Dtos;

public sealed record TournamentDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? WinningTeamId { get; set; }
    public bool IsActive { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public bool IsPaymentOpen { get; set; }
    public int? SkaterLimit { get; set; }
    public int? GoalieLimit { get; set; }
    
    public List<TournamentGameDetail> Games { get; set; }
    public List<TournamentTeamDetail> Teams { get; set; }

    public sealed record TournamentGameDetail
    {
        public int Id { get; set; }
        public DateTime GameTime { get; set; }
        public string? GameType { get; set; }
        public string? Venue { get; set; }
        public string? Rink { get; set; }
        public int? HomeTeamId { get; set; }
        public int? AwayTeamId { get; set; }
    }

    public sealed record TournamentTeamDetail
    {
        public int Id { get; set; }
        public string Name  { get; set; }
        public string NameShort  { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string PrimaryHex { get; set; }
        public string SecondaryHex { get; set; }
        public string? TertiaryHex { get; set; }
    }
}


public static class TournamentDetailDtoExtensions
{
    public static TournamentDetailDto ToTournamentDetailDto(this Tournament entity)
    {
        return new TournamentDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            WinningTeamId = entity.WinningTeamId,
            IsActive = entity.IsActive,
            IsRegistrationOpen = entity.IsRegistrationOpen,
            IsPaymentOpen = entity.IsPaymentOpen,
            SkaterLimit = entity.SkaterLimit,
            GoalieLimit = entity.GoalieLimit,
            Games = entity.Games
                .Select(g => new TournamentDetailDto.TournamentGameDetail
                {
                    Id = g.Id,
                    GameTime = g.GameTime,
                    GameType = g.GameType,
                    Venue = g.Venue,
                    Rink = g.Rink,
                    HomeTeamId = g.HomeTeamId,
                    AwayTeamId = g.AwayTeamId
                })
                .OrderBy(g => g.GameTime)
                .ToList(),
            Teams = entity.Teams
                .Select(e => new TournamentDetailDto.TournamentTeamDetail
                {
                    Id = e.Id,
                    Name =  e.Name,
                    NameShort = e.NameShort,
                    LogoUrl = e.LogoUrl,
                    BannerUrl = e.BannerUrl,
                    PrimaryHex = e.PrimaryColorHex,
                    SecondaryHex = e.SecondaryColorHex,
                    TertiaryHex = e.TertiaryColorHex,
                })
                .OrderBy(t => t.Name)
                .ToList()
        };
    }

}