namespace CiPeWorldCup.API.DTO;

public class MatchDTO
{
    public int Id { get; set; }
    public int RoundNumber { get; set; }
    public IEnumerable<long> ParticipantIds { get; set; } = new List<long>();
}