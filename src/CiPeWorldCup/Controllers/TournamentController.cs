using CiPeWorldCup.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost("start")] //POST	/tournament/start Startar ny turnering.Skapar par för runda 1 baserat på din round-robin-funktion.Body: { "name": "Alice", "players": 8 }.
    //    
     //GET	/tournament/status Returnerar aktuell runda, din nästa motståndare och scoreboard, samt information om delrundor i matchen, t.ex. "round": 1 of 3, "playerWins": 1, "opponentWins": 0. Även status för om övriga matcher i rundan är färdigspelade(bäst av 3) kan ingå.
     //POST    /tournament/play Du gör ditt drag (rock, paper, scissors). Backend avgör resultatet för delrundan, uppdaterar poäng och sparar resultatet för denna delrunda i matchen. Returnerar status för matchens delrundor och aktuell ställning.
     //POST    /tournament/advance Simulerar alla övriga/automatiska matcher i den pågående rundan som bäst av 3 (tills någon har 2 delrundevinster), uppdaterar scoreboard och sätter upp nästa runda via round-robin-logiken först när samtliga matcher i rundan är färdigspelade.
     //GET	/tournament/final Returnerar slutresultatet och vinnare när alla rundor är spelade.
}
