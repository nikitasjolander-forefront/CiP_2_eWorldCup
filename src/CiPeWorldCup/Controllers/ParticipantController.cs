using CiPeWorldCup.API.DTO;
using CiPeWorldCup.Application.Services;
using CiPeWorldCup.Application.Queries;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CiPeWorldCup.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantController : ControllerBase
    {
        private readonly ParticipantService _participantService;
        private readonly IMatchService _matchService;

        public ParticipantController(ParticipantService participantService, IMatchService matchService)
        {
            _participantService = participantService;
            _matchService = matchService;
        }

        [HttpPost("participants")]
        public async Task<ActionResult<ParticipantDTO>> AddParticipant([FromBody] ParticipantDTO dto)
        {
            var participant = new Participant(dto.Name, 0);
            
            var createdParticipant = await _participantService.AddParticipantAsync(participant);
            
            var responseDTO = new ParticipantDTO
            {
                Name = createdParticipant.Name
            };
            
            return CreatedAtAction(
                nameof(GetParticipantById), 
                new { id = createdParticipant.Id }, 
                responseDTO);
        }

        [HttpGet("participants")]
        public async Task<ActionResult<IEnumerable<ParticipantDTO>>> GetAllParticipants()
        {
            var participants = await _participantService.GetAllParticipantsAsync();
            
            var dtos = participants.Select(p => new ParticipantDTO
            {
                Id = p.Id,
                Name = p.Name
            });
            
            return Ok(dtos);
        }

        [HttpGet("participants/{id}")]
        public async Task<ActionResult<ParticipantDTO>> GetParticipantById(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            
            var dto = new ParticipantDTO
            {
                Id = participant.Id,
                Name = participant.Name
            };
            
            return Ok(dto);
        }

        [HttpDelete("participants/{id}")]
        public async Task<ActionResult> RemoveParticipant(int id)
        {
            var existingParticipant = await _participantService.GetParticipantByIdAsync(id);
            if (existingParticipant == null)
            {
                return NotFound();
            }
            
            await _participantService.RemoveParticipantByIdAsync(id);
            return NoContent();
        }
        
        [HttpGet("participants/{id}/schedule")]
        public async Task<ActionResult<IEnumerable<MatchDTO>>> GetScheduleForParticipant(int id)
        {
            var query = new GetPlayerScheduleQuery(id);
            
            var allParticipants = await _participantService.GetAllParticipantsAsync();
            
            var scheduleResult = await _matchService.GetScheduleForParticipantAsync(
                query.ParticipantId, 
                allParticipants.ToList());
            
            if (!scheduleResult.IsSuccess)
            {
                return BadRequest(scheduleResult.Error);
            }

            var dtos = scheduleResult.Value.Select(m => new MatchDTO
            {
                Id = m.Id,
                RoundNumber = m.RoundNumber,
                ParticipantIds = m.ParticipantIds
            });

            return Ok(dtos);
        }

        [HttpGet("player/{i}/round/{d}")]
        public async Task<IActionResult> GetMatchesForParticipantInRound(int i, int d)
        {
            var participants = await _participantService.GetAllParticipantsAsync();
            var participant = participants.FirstOrDefault(p => p.Id == i);
            if (participant == null)
            {
                return NotFound($"Participant with ID {i} not found");
            }
            var matchesResult = await _matchService.GetMatchesInRound(participants.ToList(), d);
            if (!matchesResult.IsSuccess)
            {
                return BadRequest(matchesResult.Error);
            }
            var matchesForParticipant = matchesResult.Value
                .Where(m => m.ParticipantIds.Contains(i))
                .ToList();
            return Ok(matchesForParticipant);
        }
    }
}
