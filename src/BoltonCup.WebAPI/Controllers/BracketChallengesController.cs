using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages bracket challenge queries and payment intents.</summary>
public class BracketChallengesController(
    IBracketChallengeService _bracketChallengeService,
    IBracketChallengeMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of bracket challenges.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<BracketChallengeDto>>> GetBracketChallenges(
        [FromQuery] GetBracketChallengesRequest request)
    {
        var query = _mapper.ToQuery(request);
        var bracketChallenges = await _bracketChallengeService.GetAsync(query);
        return Ok(_mapper.ToDtoList(bracketChallenges));
    }

    /// <summary>Gets a single bracket challenge by its ID.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BracketChallengeSingleDto>> GetBracketChallengeByIdAsync(int id)
    {
        var bracketChallenge = await _bracketChallengeService.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(bracketChallenge));
    }
    
    /// <summary>Creates a Stripe payment intent for a bracket challenge registration.</summary>
    [AllowAnonymous]
    [HttpPost("{id:int}")]
    public async Task<ActionResult<BracketChallengePaymentIntentDto>> CreateBracketChallengePaymentIntent(int id, [FromBody] CreateBracketChallengePaymentIntentRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        var result = await _bracketChallengeService.CreatePaymentIntentAsync(command);
        return _mapper.ToDto(result);
    }
}