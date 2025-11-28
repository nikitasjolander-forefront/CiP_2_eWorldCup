using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Entities;

public class Match
{
    public int Id { get; set; }
    public long TournamentId { get; init; }
    public int RoundNumber { get; set; }
    public int Player1Id { get; init; }
    public int Player2Id { get; init; }
    public int Player1Wins { get; init; }
    public int Player2Wins { get; init; }
    public bool IsCompleted { get; init; }
    public int? WinnerId { get; init; }
    public IEnumerable<long> ParticipantIds { get; set; } = new List<long>();
}
