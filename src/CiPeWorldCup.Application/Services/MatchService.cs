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
                Round = roundNumber,
                Player1Id = player1.Id,
                Player2Id = player2.Id
            });
        }

        return Result<List<Match>>.Ok(matches);
    }

    public Tournament Create(int numberOfParticipants)
    {
        var tournament = new Tournament
        {
            NumberOfPlayers = numberOfParticipants,
            Schedule = new RoundRobinPairings(numberOfParticipants)
        };
        return tournament;
    }

    public int GetRemainingMatchesCount(int totalPlayers, int roundsPlayed)
    {
        // Calculate the total number of matches in a round-robin tournament
        int totalMatches = totalPlayers * (totalPlayers - 1) / 2;
        // Calculate the number of matches played so far
        var roundRobin = new RoundRobinPairings(totalPlayers);
        int matchesPerRound = roundRobin.MatchesPerRound;
        int matchesPlayed = matchesPerRound * roundsPlayed;
        return totalMatches - matchesPlayed;
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

        for (int round = 1; round <= roundRobin.NumberOfRounds; round++)
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
                        Round = round,
                        Player1Id = player1.Id,
                        Player2Id = player2.Id
                    });
                    break;
                }
            }
        }

        return Result<List<Match>>.Ok(schedule);
    }
}
