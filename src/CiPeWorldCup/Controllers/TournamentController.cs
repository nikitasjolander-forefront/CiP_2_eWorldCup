using CiPeWorldCup.API.DTO;
using CiPeWorldCup.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CiPeWorldCup.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentController : ControllerBase
{
    private readonly TournamentService _tournamentService;
    public TournamentController(TournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }
    [HttpPost("start")] //POST	/tournament/start Startar ny turnering.Skapar par för runda 1 baserat på din round-robin-funktion.
    public async Task<IActionResult> StartTournament([FromBody] StartTournamentDto request)
    {
        var result = await _tournamentService.StartTournamentAsync(request.Name!, request.Participants);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        var (tournament, matches, participants) = result.Value!;
        return Ok(new
        {
            tournamentId = tournament.Id,
            name = tournament.Name,
            numberOfPlayers = participants.Count,
            currentRound = tournament.CurrentRound,
            matches = matches.Select(m => new
            {
                matchId = m.Id,
                round = m.Round,
                player1 = participants.First(p => p.Id == m.Player1Id).Name,
                player2 = participants.First(p => p.Id == m.Player2Id).Name

            })
        });
    }

    //GET	/tournament/status Returnerar aktuell runda, din nästa motståndare och scoreboard, samt information om delrundor i matchen, t.ex. "round": 1 of 3, "playerWins": 1, "opponentWins": 0. Även status för om övriga matcher i rundan är färdigspelade(bäst av 3) kan ingå.
    [HttpGet("status")]
    public async Task<IActionResult> GetTournamentStatus([FromQuery] int tournamentId, [FromQuery] int participantId)
    {
        var result = await _tournamentService.GetTournamentStatusAsync(tournamentId, participantId);

        if (!result.IsSuccess)
        {

            return BadRequest(new { message = result.Error });
        }
        var (tournament, currentMatch, participant, opponent, isPlayer1, allMatchesInRound) = result.Value!;
        var playerWins = isPlayer1 ? currentMatch.Player1Wins : currentMatch.Player2Wins;
        var opponentWins = isPlayer1 ? currentMatch.Player2Wins : currentMatch.Player1Wins;
        const int bestOf = 3;

        var otherMatchesCompleted = allMatchesInRound
            .Where(m => m.Id != currentMatch.Id)
            .All(m => m.IsCompleted);

        return Ok(new
        {
            tournamentId = tournament.Id,
            tournamentName = tournament.Name,
            currentRound = tournament.CurrentRound,
            maxRounds = tournament.NumberOfPlayers - 1,
            player = new
            {
                id = participant.Id,
                name = participant.Name

            },
            opponent = new
            {
                id = opponent.Id,
                name = opponent.Name
            },
            match = new
            {
                matchId = currentMatch.Id,
                round = $"{tournament.CurrentRound} of {tournament.NumberOfPlayers - 1}",
                playerWins,
                opponentWins,
                bestOf,
                isCompleted = currentMatch.IsCompleted,
                winner = currentMatch.WinnerId.HasValue
                        ? (currentMatch.WinnerId == participantId ? participant.Name : opponent.Name)
                        : null
            },
            roundStatus = new
            {

                allMatchesCompleted = allMatchesInRound.All(m => m.IsCompleted),
                otherMatchesCompleted,
                completedMatches = allMatchesInRound.Count(m => m.IsCompleted),
                totalMatches = allMatchesInRound.Count
            }
        });
    }

    //POST    /tournament/play Du gör ditt drag (rock, paper, scissors). Backend avgör resultatet för delrundan, uppdaterar poäng och sparar resultatet för denna delrunda i matchen. Returnerar status för matchens delrundor och aktuell ställning.
    [HttpPost("play")]
    public async Task<IActionResult> Playround([FromBody] PlayRoundDto request)
    {
        var result = await _tournamentService.PlayRoundAsync(request.TournamentId, request.ParticipantId, request.Move);
        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "TOURNAMENT_NOT_FOUND" => NotFound(new { message = result.Error }),
                "MATCH_NOT_FOUND" => NotFound(new { message = result.Error }),
                "PLAYER_NOT_FOUND" => NotFound(new { message = result.Error }),
                "TOURNAMENT_COMPLETED" => BadRequest(new { message = result.Error }),
                "MATCH_COMPLETED" => BadRequest(new { message = result.Error }),
                "INVALID_MOVE" => BadRequest(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error })
            };
        }

        var (updatedMatch, player, opponent, isPlayer1, playerMove, opponentMove, gameResult) = result.Value!;
        string[] moveNames = { "Rock", "Paper", "Scissors" };

        return Ok(new
        {
            matchId = updatedMatch.Id,
            playerMove = moveNames[playerMove],
            opponentMove = moveNames[opponentMove],
            roundResult = gameResult == 1 ? "win" : gameResult == -1 ? "loss" : "draw",
            playerWins = isPlayer1 ? updatedMatch.Player1Wins : updatedMatch.Player2Wins,
            opponentWins = isPlayer1 ? updatedMatch.Player2Wins : updatedMatch.Player1Wins,
            isMatchCompleted = updatedMatch.IsCompleted,
            matchWinner = updatedMatch.WinnerId.HasValue
                    ? (updatedMatch.WinnerId == request.ParticipantId ? player.Name : opponent.Name)
                        : null

        });
    }
    //POST    /tournament/advance Simulerar alla övriga/automatiska matcher i den pågående rundan som bäst av 3 (tills någon har 2 delrundevinster), uppdaterar scoreboard och sätter upp nästa runda via round-robin-logiken först när samtliga matcher i rundan är färdigspelade.
    [HttpPost("advance")]
    public async Task<IActionResult> AdvanceRound([FromBody] AdvanceRoundDTO request)
    {
        var result = await _tournamentService.AdvanceRoundAsync(request.TournamentId);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "TOURNAMENT_NOT_FOUND" => NotFound(new { message = result.Error }),
                "TOURNAMENT_COMPLETED" => BadRequest(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error })
            };
        }
        var (tournament, newMatches, players, isCompleted) = result.Value!;

        if (isCompleted)
        {
            return Ok(new
            {
                message = "Tournament completed!",
                tournamentId = tournament.Id,
                isCompleted = true,
                currentRound = tournament.CurrentRound
            });
        }

        var maxRounds = tournament.NumberOfPlayers - 1;

        return Ok(new
        {
            message = "Round advanced successfully.",
            tournamentId = tournament.Id,
            currentRound = tournament.CurrentRound,

            maxRounds = maxRounds,
            newMatches = newMatches!.Select(m => new
            {
                matchId = m.Id,
                player1 = players!.First(p => p.Id == m.Player1Id).Name,
                player2 = players!.First(p => p.Id == m.Player2Id).Name
            })
        });
    }

    //GET	/tournament/final Returnerar slutresultatet och vinnare när alla rundor är spelade.
    [HttpGet("final")]
    public async Task<IActionResult> GetFinalResults([FromQuery] int tournamentId)
    {
        var result = await _tournamentService.GetFinalResultsAsync(tournamentId);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "TOURNAMENT_NOT_FOUND" => NotFound(new { message = result.Error }),
                "TOURNAMENT_NOT_COMPLETED" => BadRequest(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error })
            };
        }

        var (tournament, scoreboard) = result.Value!;
        var winner = scoreboard.FirstOrDefault();

        return Ok(new
        {
            tournamentId = tournament.Id,
            tournamentName = tournament.Name,
            isCompleted = tournament.IsCompleted,
            totalRounds = tournament.NumberOfPlayers - 1,
            winner = winner.Rank > 0 ? new
            {
                playerId = winner.PlayerId,
                playerName = winner.PlayerName,
                wins = winner.Wins,
                losses = winner.Losses
            } : null,
            scoreboard = scoreboard.Select(s => new
            {
                rank = s.Rank,
                playerId = s.PlayerId,
                playerName = s.PlayerName,
                wins = s.Wins,
                losses = s.Losses
            })
        });
    }
}
