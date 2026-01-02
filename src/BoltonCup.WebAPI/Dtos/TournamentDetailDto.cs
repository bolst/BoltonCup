using BoltonCup.WebAPI.Data.Entities;

namespace BoltonCup.WebAPI.Dtos;

public record TournamentDetailDto
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
    
    public List<GameDetail> Games { get; set; }

    public record GameDetail
    {
        public int Id { get; set; }
        public DateTime GameTime { get; set; }
        public string? GameType { get; set; }
        public string? Venue { get; set; }
        public string? Rink { get; set; }
        public TeamDetail? HomeTeam { get; set; }
        public TeamDetail? AwayTeam { get; set; }
    }

    public record TeamDetail
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
                .Select(g => new TournamentDetailDto.GameDetail
                {
                    Id = g.Id,
                    GameTime = g.GameTime,
                    GameType = g.GameType,
                    Venue = g.Venue,
                    Rink = g.Rink,
                    HomeTeam = g.HomeTeam?.ToTeamDetailDto(),
                    AwayTeam = g.AwayTeam?.ToTeamDetailDto()
                })
                .OrderBy(g => g.GameTime)
                .ToList(),
        };
    }


    private static TournamentDetailDto.TeamDetail ToTeamDetailDto(this Team entity)
    {
        return new TournamentDetailDto.TeamDetail
        {
            Id = entity.Id,
            Name =  entity.Name,
            NameShort = entity.NameShort,
            LogoUrl = entity.LogoUrl,
            BannerUrl = entity.BannerUrl,
            PrimaryHex = entity.PrimaryColorHex,
            SecondaryHex = entity.SecondaryColorHex,
            TertiaryHex = entity.TertiaryColorHex,
        };
    }
}