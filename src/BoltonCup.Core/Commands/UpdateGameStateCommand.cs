namespace BoltonCup.Core.Commands;

public sealed record UpdateGameStateCommand(int GameId, GameState State);
