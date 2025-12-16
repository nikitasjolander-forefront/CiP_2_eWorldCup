namespace CiPeWorldCup.Core.Entities;

public sealed record Tournament
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NumberOfPlayers { get; init; }
    public int CurrentRound { get; set; }
    public DateTime StartedAt { get; init; }
    public bool IsCompleted { get; init; }
    public ICollection<Match> Matches { get; init; } = new List<Match>();
    
    // If you need the schedule for business logic, store it as a non-mapped property
    // EF Core will ignore this because it's not a primitive type
    public RoundRobinPairings? Schedule { get; set; }
}
