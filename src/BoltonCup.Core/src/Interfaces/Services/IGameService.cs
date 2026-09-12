using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IGameService
{
    Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateStateAsync(UpdateGameStateCommand command, CancellationToken cancellationToken = default);

    Task<int> AddGoalAsync(CreateGoalCommand command, CancellationToken cancellationToken = default);
    Task UpdateGoalAsync(UpdateGoalCommand command, CancellationToken cancellationToken = default);
    Task DeleteGoalAsync(int gameId, int goalId, CancellationToken cancellationToken = default);

    Task<int> AddPenaltyAsync(CreatePenaltyCommand command, CancellationToken cancellationToken = default);
    Task UpdatePenaltyAsync(UpdatePenaltyCommand command, CancellationToken cancellationToken = default);
    Task DeletePenaltyAsync(int gameId, int penaltyId, CancellationToken cancellationToken = default);

    Task SetStarsAsync(SetGameStarsCommand command, CancellationToken cancellationToken = default);
}