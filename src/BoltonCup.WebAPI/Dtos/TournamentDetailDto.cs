using BoltonCup.WebAPI.Data.Entities;

namespace BoltonCup.WebAPI.Dtos;

public class TournamentDetailDto
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

    public List<PlayerDetailDto> Players { get; set; } = [];
    public List<TeamDetailDto> Teams { get; set; } = [];
}


public class PlayerDetailDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}


public class TeamDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}


public static class TournamentDetailDtoExtensions
{
    public static TournamentDetailDto ToTournamentDetailDto(this Tournament entity)
    {
        return new TournamentDetailDto()
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
            Players = entity.Players.Select(e => e.ToPlayerDetailDto()).ToList(),
            Teams = entity.Teams.Select(e => e.ToTeamDetailDto()).ToList()
        };
    }

    public static PlayerDetailDto ToPlayerDetailDto(this Player player)
    {
        return new PlayerDetailDto()
        {
            FirstName = player.Account.FirstName,
            LastName = player.Account.LastName,
        };
    }

    public static TeamDetailDto ToTeamDetailDto(this Team team)
    {
        return new TeamDetailDto()
        {
            Id = team.Id,
            Name = team.Name,
        };
    }
}