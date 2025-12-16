namespace CiPeWorldCup.API.DTO;

public class PlayRoundDto
{
    public int TournamentId { get; set; }
    public int ParticipantId { get; set; }
    public string Move { get; set; } = string.Empty;
}