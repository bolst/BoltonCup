using BoltonCup.Core.BracketChallenge;

namespace BoltonCup.WebAPI.Mapping;

public interface IBracketChallengeMapper
{
    BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent);
    CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request);
}

public class BracketChallengeMapper : IBracketChallengeMapper
{
    public BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent)
    {
        return new BracketChallengePaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }
    
    public CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request)
    {
        return new CreateBracketChallengePaymentIntentCommand(
            Name: request.Name,
            Email: request.Email,
            BracketChallengeId: bracketChallengeId
        );
    }
}
