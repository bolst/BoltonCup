namespace BoltonCup.Core.Exceptions;

public sealed class PlayerNotTradeableException(Player player)
    : BoltonCupException(GetExceptionMessage(player))
{
    static string GetExceptionMessage(Player player)
    {
        var playerName = player.Account == null
            ? $"Player {player.Id}"
            : player.Account.FirstName + " " + player.Account.LastName;
        return $"{playerName} cannot be traded.";
    }
}
