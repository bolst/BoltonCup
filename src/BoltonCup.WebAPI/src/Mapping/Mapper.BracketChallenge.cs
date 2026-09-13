using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using Event = BoltonCup.Core.BracketChallenge.Event;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- BracketChallenge ----------

    public GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request) => new GetBracketChallengesQuery
    {
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Event> bracketChallenges) => bracketChallenges.ProjectTo(challenge => new BracketChallengeDto
    {
        Id = challenge.Id,
        Title = challenge.Title,
        Link = challenge.Link,
        Fee = challenge.Fee,
        IsOpen = challenge.IsOpen,
        Logo = _urlResolver.GetFullUrl(challenge.Logo),
        CloseDate = challenge.RegistrationCloseDate
    });

    public BracketChallengeSingleDto? ToDto(Event? challenge)
    {
        if (challenge is null)
        {
            return null;
        }

        return new BracketChallengeSingleDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Link = challenge.Link,
            Fee = challenge.Fee,
            IsOpen = challenge.IsOpen,
            Logo = _urlResolver.GetFullUrl(challenge.Logo),
            CloseDate = challenge.RegistrationCloseDate,
            TOSMarkdown = challenge.TermsOfServiceMarkdownContent
        };
    }

    public BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent) => new BracketChallengePaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );

    public CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request) => new CreateBracketChallengePaymentIntentCommand(
            Name: request.Name,
            Email: request.Email,
            AgreedToTOS: request.AgreedToTOS,
            BracketChallengeId: bracketChallengeId
        );
}
