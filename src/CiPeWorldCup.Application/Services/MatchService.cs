using CiPeWorldCup.Core;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.Interfaces;
using CiPeWorldCup.Core.RailwayErrorHandling;

namespace CiPeWorldCup.Application.Services;

public class MatchService : IMatchService
{//TODO: Remove logic from here to core layer somewhere
    public async Task<Result<List<Match>>> GetMatchesInRound(List<Participant> participants, int roundNumber)
    {
        var roundRobin = new RoundRobinPairings(participants.Count);
        var pairingsResult = roundRobin.GeneratePairings(participants, roundNumber);

        if (!pairingsResult.IsSuccess)
        {
            return Result<List<Match>>.Fail(pairingsResult.Error);
        }

        var matches = new List<Match>();
        int matchId = 1;

        foreach (var (player1, player2) in pairingsResult.Value)
        {
            matches.Add(new Match
            {
                Id = matchId++,
                RoundNumber = roundNumber,
                ParticipantIds = new[] { (long)player1.Id, (long)player2.Id }
            });
        }

        return Result<List<Match>>.Ok(matches);
    }

    public Tournament Create(long numberOfParticipants)
    {
        var schedule = new RoundRobinPairings(numberOfParticipants);
        return new Tournament(schedule);
    }

    public long GetRemainingMatchesCount(long totalPlayers, long roundsPlayed)
    {
        var tournament = new Tournament(new RoundRobinPairings(totalPlayers));
        tournament.CurrentRound = roundsPlayed + 1;
        return tournament.NumberOfMatchesRemaining;
    }

    public async Task<Result<List<Match>>> GetScheduleForParticipantAsync(int participantId, List<Participant> allParticipants)
    {
        var participant = allParticipants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
        {
            return Result<List<Match>>.Fail($"Participant with ID {participantId} not found");
        }

        var roundRobin = new RoundRobinPairings(allParticipants.Count);
        var schedule = new List<Match>();
        int matchIdCounter = 1;

        for (long round = 1; round <= roundRobin.NumberOfRounds; round++)
        {
            var pairingsResult = roundRobin.GeneratePairings(allParticipants, round);
            
            if (!pairingsResult.IsSuccess)
            {
                return Result<List<Match>>.Fail(pairingsResult.Error);
            }

            foreach (var (player1, player2) in pairingsResult.Value)
            {
                if (player1.Id == participantId || player2.Id == participantId)
                {
                    schedule.Add(new Match
                    {
                        Id = matchIdCounter++,
                        RoundNumber = (int)round,
                        ParticipantIds = new[] { (long)player1.Id, (long)player2.Id }
                    });
                    break;
                }
            }
        }

        return Result<List<Match>>.Ok(schedule);
    }
}
