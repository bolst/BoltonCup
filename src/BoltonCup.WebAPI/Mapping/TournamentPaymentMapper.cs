using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps tournament payment intent objects to DTOs and commands.</summary>
public interface ITournamentPaymentMapper
{
    /// <summary>Maps a <see cref="TournamentPaymentIntent"/> to a <see cref="TournamentPaymentIntentDto"/>.</summary>
    TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent);

    /// <summary>Maps a <see cref="CreateTournamentPaymentIntentRequest"/> to a <see cref="CreateTournamentPaymentIntentCommand"/>.</summary>
    CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request);
}

/// <summary>Maps tournament payment intent objects to DTOs and commands.</summary>
public class TournamentPaymentMapper : ITournamentPaymentMapper
{
    /// <inheritdoc/>
    public TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent)
    {
        return new TournamentPaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }
    
    /// <inheritdoc/>
    public CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request)
    {
        return new CreateTournamentPaymentIntentCommand(
            AccountId: accountId,
            TournamentId: tournamentId,
            Position: request.Position
        );
    }
}
