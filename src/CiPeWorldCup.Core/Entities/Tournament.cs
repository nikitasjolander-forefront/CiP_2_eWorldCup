using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiPeWorldCup.Core;

namespace CiPeWorldCup.Core.Entities;

public sealed record Tournament(RoundRobinPairings schedule)
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public long CurrentRound { get; set; }
    public DateTime StartedAt { get; init; }
    public bool IsCompleted { get; init; }
    public ICollection<Match> Matches { get; init; } = new List<Match>();
    public RoundRobinPairings Schedule { get; init; } = schedule;
    public long NumberOfRoundsPlayed => CurrentRound - 1;
    public long NumberOfRoundsRemaining => Schedule.NumberOfRounds - NumberOfRoundsPlayed;
    public long NumberOfMatchesRemaining => NumberOfRoundsRemaining * Schedule.MatchesPerRound;
}
