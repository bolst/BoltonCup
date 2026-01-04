using BoltonCup.Infrastructure.Data.Entities;

namespace BoltonCup.WebAPI.Dtos;

public sealed record TeamDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameShort { get; set; }
    public string Abbreviation { get; set; }
    public int? TournamentId { get; set; }
    public string? TournamentName { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string PrimaryColorHex { get; set; }
    public string SecondaryColorHex { get; set; }
    public string? TertiaryColorHex { get; set; }
    public string? GoalSongUrl { get; set; }
    public string? PenaltySongUrl { get; set; }
    
    public int? GmAccountId { get; set; }
    public string? GmName { get; set; }
    public string? GmProfilePicture  { get; set; }

    public List<TeamPlayerDetail> Players { get; set; }
    public List<TeamGameDetail> Games { get; set; }

    public sealed record TeamPlayerDetail
    {
        public int Id  { get; set; }
        public int? AccountId {  get; set; }
        public string? Position { get; set; }
        public int? JerseyNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? ProfilePicture { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public sealed record TeamGameDetail
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public DateTime GameTime { get; set; }
        public string? GameType { get; set; }
        public string? Venue { get; set; } 
        public string? Rink { get; set; }
        public bool IsHomeGame { get; set; }
        public int? OpponentTeamId { get; set; }
        public string? OpponentTeamName { get; set; }
        public string? OpponentTeamNameShort { get; set; }
        public string? OpponentTeamAbbreviation { get; set; }
        public string? OpponentTeamLogoUrl { get; set; }
    }
}


public static class TeamDetailDtoExtensions
{
    public static TeamDetailDto ToTeamDetailDto(this Team entity)
    {
        return new TeamDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            NameShort = entity.NameShort,
            Abbreviation = entity.Abbreviation,
            TournamentId = entity.Tournament.Id,
            TournamentName = entity.Tournament.Name,
            LogoUrl = entity.LogoUrl,
            BannerUrl = entity.BannerUrl,
            PrimaryColorHex = entity.PrimaryColorHex,
            SecondaryColorHex = entity.SecondaryColorHex,
            TertiaryColorHex = entity.TertiaryColorHex,
            GoalSongUrl = entity.GoalSongUrl,
            PenaltySongUrl = entity.PenaltySongUrl,
            GmAccountId = entity.GmAccountId,
            GmName = entity.GeneralManager?.FirstName +  " " + entity.GeneralManager?.LastName,
            GmProfilePicture = entity.GeneralManager?.ProfilePicture,
            Players = entity.Players.Select(p => new TeamDetailDto.TeamPlayerDetail 
                {
                    Id = p.Id, 
                    AccountId = p.AccountId,
                    Position = p.Position,
                    JerseyNumber = p.JerseyNumber,
                    Birthday = p.Account?.Birthday,
                    ProfilePicture = p.Account?.ProfilePicture,
                    FirstName = p.Account?.FirstName,
                    LastName = p.Account?.LastName,
                })
                .ToList(),
            Games = entity.HomeGames.Concat(entity.AwayGames).Select(g =>
                {
                    var isHome = entity.Id == g.HomeTeamId;
                    var opponent = isHome ? g.AwayTeam : g.HomeTeam;
                    return new TeamDetailDto.TeamGameDetail
                    {
                        Id = g.Id,
                        TournamentId = g.TournamentId,
                        GameTime = g.GameTime,
                        GameType = g.GameType,
                        Venue = g.Venue, 
                        Rink = g.Rink,
                        IsHomeGame = isHome,
                        OpponentTeamId = opponent?.Id,
                        OpponentTeamName = opponent?.Name,
                        OpponentTeamNameShort = opponent?.NameShort,
                        OpponentTeamAbbreviation = opponent?.Abbreviation,
                        OpponentTeamLogoUrl = opponent?.LogoUrl,
                    };
                })
                .ToList(),
        };
    }
}