using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiPeWorldCup.Core;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.RailwayErrorHandling;

namespace CiPeWorldCup.Application.Queries;

public class GetPairInASpecificRound
{
    public Result<(Participant, Participant)> GetPlayersInRound(List<Participant> players, int numberOfParticipants, int matchNumber, int roundNumber)
    {
        var roundRobin = new RoundRobinPairings(numberOfParticipants);
        var pairingsResult = roundRobin.GeneratePairings(players, roundNumber);
        if (!pairingsResult.IsSuccess)
        {
            return Result<(Participant, Participant)>.Fail(pairingsResult.Error);
        }
        var pairings = pairingsResult.Value;
        if (matchNumber < 1 || matchNumber > pairings.Count)
        {
            return Result<(Participant, Participant)>.Fail($"Invalid match number. Must be between 1 and {pairings.Count}");
        }
        var (player1, player2) = pairings[matchNumber - 1];
        return Result<(Participant, Participant)>.Ok((player1, player2));
    }
}
