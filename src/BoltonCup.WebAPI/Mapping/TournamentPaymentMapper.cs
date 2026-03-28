using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

public interface ITournamentPaymentMapper
{
    TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent);
    CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request);
}

public class TournamentPaymentMapper : ITournamentPaymentMapper
{
    public TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent)
    {
        return new TournamentPaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }
    
    public CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request)
    {
        return new CreateTournamentPaymentIntentCommand(
            AccountId: accountId,
            TournamentId: tournamentId,
            Position: request.Position
        );
    }
}
