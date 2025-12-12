using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Core.Entities;

public class TournamentRules(int numberOfParticipants)
{
    public int NumberOfParticipants { get; } = numberOfParticipants;
    public int NumberOfRounds => NumberOfParticipants - 1;
    public int MatchesPerRound => NumberOfParticipants / 2;

    public static Result<TournamentRules> Create(int numberOfParticipants)
    {
        var validationParticipant = ValidateNumberOfParticipants.ValidateParticipants(numberOfParticipants);
        if (!validationParticipant.IsSuccess)
        {
            return Result<TournamentRules>.Fail(validationParticipant.Error);
        }

        //var validationRounds = ValidateRoundNumber.ValidateRound(roundNumber, numberOfParticipants);



        return Result<TournamentRules>.Ok(new TournamentRules(numberOfParticipants));
    }
}
