using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/teams")]
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TeamsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> Get(int? tournamentId = null)
    {
        return tournamentId is null 
            ? Ok(await _unitOfWork.Teams.GetAllAsync())
            : Ok(await _unitOfWork.Teams.GetAllAsync(tournamentId.Value));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Team?>> Get(int id)
    {
        var result = await _unitOfWork.Teams.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Team entity)
    {
        var result = await _unitOfWork.Teams.AddAsync(entity);
        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Team entity)
    {
        var result = await _unitOfWork.Teams.UpdateAsync(entity);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _unitOfWork.Teams.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
