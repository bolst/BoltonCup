using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;

namespace BoltonCup.WebAPI.Mapping;

public interface IBracketChallengeMapper
{
    GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request);
    IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Core.BracketChallenge.Event> bracketChallenges);
    BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent);
    CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request);
}

public class BracketChallengeMapper(IAssetUrlResolver _urlResolver) : IBracketChallengeMapper
{
    public GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request)
    {
        return new GetBracketChallengesQuery
        {
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }

    public IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Core.BracketChallenge.Event> bracketChallenges)
    {
        return bracketChallenges.ProjectTo(challenge => new BracketChallengeDto(
            Id: challenge.Id,
            Title: challenge.Title,
            Link: challenge.Link,
            Fee: challenge.Fee,
            IsOpen: challenge.IsOpen,
            Logo: _urlResolver.GetFullUrl(challenge.Logo)
        ));
    }
    
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
