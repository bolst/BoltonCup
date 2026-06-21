namespace BoltonCup.Core.Exceptions;

public sealed class PlayerLockedException(Player player)
    : BoltonCupException(GetExceptionMessage(player))
{
    static string GetExceptionMessage(Player player)
    {
        var playerName = player.Account == null
            ? $"Player {player.Id}"
            : player.Account.FirstName + " " + player.Account.LastName;
        return $"{playerName} is already part of a trade.";
    }
}
