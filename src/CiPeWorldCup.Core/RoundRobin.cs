using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Core;

public class RoundRobinPairings : TournamentRules
{
    public RoundRobinPairings(long numberOfParticipants) : base(numberOfParticipants)
    {
    }

    public Result<List<(Participant, Participant)>> GeneratePairings(List<Participant> players, long roundNumber) 
    {
        var participantValidation = ValidateNumberOfParticipants.ValidateParticipants(players.Count);
        if (!participantValidation.IsSuccess)
        {
            return Result<List<(Participant, Participant)>>.Fail(participantValidation.Error);
        }
        
        var roundValidation = ValidateRoundNumber.ValidateRound(roundNumber, players.Count);
        if (!roundValidation.IsSuccess)
        {
            return Result<List<(Participant, Participant)>>.Fail(roundValidation.Error);
        }

        var pairings = new List<(Participant, Participant)>();
        long numberOfPlayers = NumberOfParticipants;
        long halfSize = numberOfPlayers / 2;

        var rotatedPlayers = RotatePlayersForRound(players, roundNumber);

        for (var i = 0; i < halfSize; i++)
        {
            var player1 = rotatedPlayers[i];
            var player2 = rotatedPlayers[(int)(numberOfPlayers - 1 - i)];
            pairings.Add((player1, player2));
        }

        return Result<List<(Participant, Participant)>>.Ok(pairings);
    }

    private List<Participant> RotatePlayersForRound(List<Participant> players, long roundNumber)
    {
        var rotated = new List<Participant> { players[0] }; 

        var offset = roundNumber - 1;
        for (var i = 1; i < players.Count; i++)
        {
            var index = (int)((i - offset - 1 + players.Count - 1) % (players.Count - 1) + 1);
            rotated.Add(players[index]);
        }

        return rotated;
    }
}