using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class GameWriteService(BoltonCupDbContext _dbContext) : IGameWriteService
{
    public async Task UpdateStateAsync(UpdateGameStateCommand command, CancellationToken cancellationToken = default)
    {
        var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), command.GameId);

        game.GameState = command.State;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddGoalAsync(CreateGoalCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        var goal = new Goal
        {
            GameId = command.GameId,
            TeamId = command.TeamId,
            Period = command.Period,
            PeriodLabel = command.PeriodLabel,
            PeriodTimeRemaining = command.PeriodTimeRemaining,
            GoalPlayerId = command.GoalPlayerId,
            Assist1PlayerId = command.Assist1PlayerId,
            Assist2PlayerId = command.Assist2PlayerId,
            Notes = command.Notes,
        };
        _dbContext.Goals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return goal.Id;
    }

    public async Task UpdateGoalAsync(UpdateGoalCommand command, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == command.GoalId && g.GameId == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Goal), command.GoalId);

        goal.TeamId = command.TeamId;
        goal.Period = command.Period;
        goal.PeriodLabel = command.PeriodLabel;
        goal.PeriodTimeRemaining = command.PeriodTimeRemaining;
        goal.GoalPlayerId = command.GoalPlayerId;
        goal.Assist1PlayerId = command.Assist1PlayerId;
        goal.Assist2PlayerId = command.Assist2PlayerId;
        goal.Notes = command.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGoalAsync(int gameId, int goalId, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.GameId == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Goal), goalId);

        _dbContext.Goals.Remove(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddPenaltyAsync(CreatePenaltyCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        var penalty = new Penalty
        {
            GameId = command.GameId,
            TeamId = command.TeamId,
            Period = command.Period,
            PeriodLabel = command.PeriodLabel,
            PeriodTimeRemaining = command.PeriodTimeRemaining,
            PlayerId = command.PlayerId,
            InfractionName = command.InfractionName,
            DurationMinutes = command.DurationMinutes,
            Notes = command.Notes,
        };
        _dbContext.Penalties.Add(penalty);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return penalty.Id;
    }

    public async Task UpdatePenaltyAsync(UpdatePenaltyCommand command, CancellationToken cancellationToken = default)
    {
        var penalty = await _dbContext.Penalties
            .FirstOrDefaultAsync(p => p.Id == command.PenaltyId && p.GameId == command.GameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Penalty), command.PenaltyId);

        penalty.TeamId = command.TeamId;
        penalty.Period = command.Period;
        penalty.PeriodLabel = command.PeriodLabel;
        penalty.PeriodTimeRemaining = command.PeriodTimeRemaining;
        penalty.PlayerId = command.PlayerId;
        penalty.InfractionName = command.InfractionName;
        penalty.DurationMinutes = command.DurationMinutes;
        penalty.Notes = command.Notes;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePenaltyAsync(int gameId, int penaltyId, CancellationToken cancellationToken = default)
    {
        var penalty = await _dbContext.Penalties
            .FirstOrDefaultAsync(p => p.Id == penaltyId && p.GameId == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Penalty), penaltyId);

        _dbContext.Penalties.Remove(penalty);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStarsAsync(SetGameStarsCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureGameExistsAsync(command.GameId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existing = await _dbContext.GameStars
            .Where(s => s.GameId == command.GameId)
            .ToListAsync(cancellationToken);
        _dbContext.GameStars.RemoveRange(existing);

        var newStars = new List<GameStar>();
        if (command.FirstStarPlayerId.HasValue)
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 1, PlayerId = command.FirstStarPlayerId.Value });
        if (command.SecondStarPlayerId.HasValue)
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 2, PlayerId = command.SecondStarPlayerId.Value });
        if (command.ThirdStarPlayerId.HasValue)
            newStars.Add(new GameStar { GameId = command.GameId, StarRank = 3, PlayerId = command.ThirdStarPlayerId.Value });

        if (newStars.Count > 0)
            _dbContext.GameStars.AddRange(newStars);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task EnsureGameExistsAsync(int gameId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Games.AnyAsync(g => g.Id == gameId, cancellationToken);
        if (!exists)
            throw new EntityNotFoundException(nameof(Game), gameId);
    }
}
