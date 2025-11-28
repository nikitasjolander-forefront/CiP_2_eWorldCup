using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Core.Entities;

public class TournamentRules(long numberOfParticipants)
{
    public long NumberOfParticipants { get; } = numberOfParticipants;
    public long NumberOfRounds => NumberOfParticipants - 1;
    public long MatchesPerRound => NumberOfParticipants / 2;

    public static Result<TournamentRules> Create(long numberOfParticipants)
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
