using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;

namespace BoltonCup.WebAPI.Dtos;

public record TournamentDetailDto : IMappable<Tournament, TournamentDetailDto>
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

    Expression<Func<Tournament, TournamentDetailDto>> IMappable<Tournament, TournamentDetailDto>.Projection =>
        tournament => new TournamentDetailDto 
        {
            Id = tournament.Id,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            WinningTeamId = tournament.WinningTeamId,
            IsActive = tournament.IsActive,
            IsRegistrationOpen = tournament.IsRegistrationOpen,
            IsPaymentOpen = tournament.IsPaymentOpen,
            SkaterLimit = tournament.SkaterLimit,
            GoalieLimit = tournament.GoalieLimit,
        };
}
