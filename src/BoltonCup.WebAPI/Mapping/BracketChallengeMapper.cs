using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps bracket challenge domain models to DTOs and commands.</summary>
public interface IBracketChallengeMapper
{
    /// <summary>Maps a <see cref="GetBracketChallengesRequest"/> to a <see cref="GetBracketChallengesQuery"/>.</summary>
    GetBracketChallengesQuery ToQuery(GetBracketChallengesRequest request);
    /// <summary>Maps a paged list of bracket challenge events to a paged list of <see cref="BracketChallengeDto"/>.</summary>
    IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Core.BracketChallenge.Event> bracketChallenges);
    /// <summary>Maps a bracket challenge event to a <see cref="BracketChallengeSingleDto"/>.</summary>
    BracketChallengeSingleDto? ToDto(Core.BracketChallenge.Event? bracketChallenge);
    /// <summary>Maps a <see cref="BracketChallengePaymentIntent"/> to a <see cref="BracketChallengePaymentIntentDto"/>.</summary>
    BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent);
    /// <summary>Maps a request to a <see cref="CreateBracketChallengePaymentIntentCommand"/>.</summary>
    CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request);
}

/// <summary>Maps bracket challenge domain models to DTOs and commands.</summary>
public class BracketChallengeMapper(IAssetUrlResolver _urlResolver) : IBracketChallengeMapper
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public IPagedList<BracketChallengeDto> ToDtoList(IPagedList<Core.BracketChallenge.Event> bracketChallenges)
    {
        return bracketChallenges.ProjectTo(challenge => new BracketChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Link = challenge.Link,
            Fee = challenge.Fee,
            IsOpen = challenge.IsOpen,
            Logo = _urlResolver.GetFullUrl(challenge.Logo),
            CloseDate = challenge.RegistrationCloseDate
        });
    }

    /// <inheritdoc/>
    public BracketChallengeSingleDto? ToDto(Core.BracketChallenge.Event? challenge)
    {
        if (challenge is null)
            return null;
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
    
    /// <inheritdoc/>
    public BracketChallengePaymentIntentDto ToDto(BracketChallengePaymentIntent paymentIntent)
    {
        return new BracketChallengePaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );
    }
    
    /// <inheritdoc/>
    public CreateBracketChallengePaymentIntentCommand ToCommand(int bracketChallengeId, CreateBracketChallengePaymentIntentRequest request)
    {
        return new CreateBracketChallengePaymentIntentCommand(
            Name: request.Name,
            Email: request.Email,
            AgreedToTOS: request.AgreedToTOS,
            BracketChallengeId: bracketChallengeId
        );
    }
}
