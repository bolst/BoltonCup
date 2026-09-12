namespace BoltonCup.Core.Commands;

public record CreateTournamentPaymentIntentCommand(
    int AccountId,
    int TournamentId,
    string Position
)
{
    public bool IsGoalie => Position.Equals(Values.Position.Goalie, StringComparison.OrdinalIgnoreCase);
}