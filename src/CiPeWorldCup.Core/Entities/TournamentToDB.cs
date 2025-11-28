using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Entities;

public sealed record TournamentToDB
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NumberOfPlayers { get; init; }
    public long CurrentRound { get; set; }
    public DateTime StartedAt { get; init; }
    public bool IsCompleted { get; init; }
    public ICollection<Match> Matches { get; init; } = new List<Match>();
}
