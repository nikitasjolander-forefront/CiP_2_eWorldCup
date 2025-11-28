using CiPeWorldCup.Application.Queries;
using CiPeWorldCup.Application.Services;
using CiPeWorldCup.Core.Interfaces;
using CiPeWorldCup.Core.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CiPeWorldCup.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly ParticipantService _participantService;

    public MatchController(IMatchService matchService, ParticipantService participantService)
    {
        _matchService = matchService;
        _participantService = participantService;
    }


    [HttpGet] //    GET	/rounds/:d	Returnerar alla matcher i runda d (1 ≤ d ≤ n−1).
    [Route("rounds/{roundNumber}")]
    public async Task<IActionResult> GetMatchesInRound(int roundNumber)
    {
        var participants = await _participantService.GetAllParticipantsAsync();
        var matchesResult = await _matchService.GetMatchesInRound(participants.ToList(), roundNumber);
        if (!matchesResult.IsSuccess)
        {
            return BadRequest(matchesResult.Error);
        }
        return Ok(matchesResult.Value);
    }
    [HttpGet] //    GET	/rounds/max?n=	Returnerar max antal rundor för n deltagare (n−1).
    [Route("rounds/max")]
    public IActionResult GetMaxRounds(int n)
    {
        var participantValidation = ValidateNumberOfParticipants.ValidateParticipants(n);
        if (!participantValidation.IsSuccess)
        {
            return BadRequest(participantValidation.Error);
        }
        var maxRounds = n - 1;
        return Ok(maxRounds);
    }

    [HttpGet("remaining")]
    public ActionResult<long> GetRemainingMatches(
        [FromQuery(Name = "n")] long totalPlayers, 
        [FromQuery(Name = "D")] long roundsPlayed)
    {
        var query = new GetRemainingMatchesQuery(totalPlayers, roundsPlayed);
        
        var remaining = _matchService.GetRemainingMatchesCount(query.TotalPlayers, query.RoundsPlayed);
        return Ok(remaining);
    }

    [HttpGet("match")] // /api/match/match?n=10&i=4&d=2
    public async Task<IActionResult> GetMatchForPlayerInRound(
        [FromQuery(Name = "n")] long n, 
        [FromQuery(Name = "i")] long i, 
        [FromQuery(Name = "d")] long d)
    {
        var participants = await _participantService.GetAllParticipantsAsync();
        var participantsList = participants.ToList();
        
        var participant = participantsList.FirstOrDefault(p => p.Id == i);
        if (participant == null)
        {
            return NotFound($"Participant with ID {i} not found");
        }
        
        var matchesResult = await _matchService.GetMatchesInRound(participantsList, (int)d);
        if (!matchesResult.IsSuccess)
        {
            return BadRequest(matchesResult.Error);
        }
        
        var match = matchesResult.Value
            .FirstOrDefault(m => m.ParticipantIds.Contains(i));
        
        if (match == null)
        {
            return NotFound($"No match found for participant {i} in round {d}");
        }
        
        return Ok(match);
    }
}
