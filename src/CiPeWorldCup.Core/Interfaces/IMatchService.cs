using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.RailwayErrorHandling;

namespace CiPeWorldCup.Core.Interfaces;

public interface IMatchService
{
    Task<Result<List<Match>>> GetMatchesInRound(List<Participant> participants, int d);
    
    Task<Result<List<Match>>> GetScheduleForParticipantAsync(int participantId, List<Participant> allParticipants);
    
    Tournament Create(long numberOfParticipants);
    
    long GetRemainingMatchesCount(long totalPlayers, long roundsPlayed);
}
