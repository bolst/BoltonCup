using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;

namespace BoltonCup.WebAPI.Dtos;

public record TournamentSingleDetailDto : TournamentDetailDto, IMappable<Tournament, TournamentSingleDetailDto>
{
    static Expression<Func<Tournament, TournamentSingleDetailDto>> IMappable<Tournament, TournamentSingleDetailDto>.Projection =>
        tournament => new TournamentSingleDetailDto
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