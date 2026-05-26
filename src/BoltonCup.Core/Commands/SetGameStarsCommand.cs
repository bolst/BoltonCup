namespace BoltonCup.Core.Commands;

public sealed record SetGameStarsCommand(
    int GameId,
    int? FirstStarPlayerId,
    int? SecondStarPlayerId,
    int? ThirdStarPlayerId
);
