using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class BracketChallengesController(
    IBracketChallengeService _bracketChallengeService, 
    IBracketChallengeMapper _mapper
) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<BracketChallengeDto>>> GetBracketChallenges(
        [FromQuery] GetBracketChallengesRequest request)
    {
        var query = _mapper.ToQuery(request);
        var bracketChallenges = await _bracketChallengeService.GetAsync(query);
        return Ok(_mapper.ToDtoList(bracketChallenges));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BracketChallengeSingleDto>> GetBracketChallengeByIdAsync(int id)
    {
        var bracketChallenge = await _bracketChallengeService.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(bracketChallenge));
    }
    
    [AllowAnonymous]
    [HttpPost("{id:int}")]
    public async Task<ActionResult<BracketChallengePaymentIntentDto>> CreateBracketChallengePaymentIntent(int id, [FromBody] CreateBracketChallengePaymentIntentRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        var result = await _bracketChallengeService.CreatePaymentIntentAsync(command);
        return _mapper.ToDto(result);
    }
}