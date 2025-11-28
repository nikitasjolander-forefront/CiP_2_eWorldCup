using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Tests;

public class GetMatchesForSpecificRound
{
    public Result<long> GetAllMatchesForSpecificRound(long numberOfParticipants, int roundNumber)
    {
        var validationResult = ValidateNumberOfParticipants.ValidateParticipants(numberOfParticipants);
        if (!validationResult.IsSuccess)
        {
            return Result<long>.Fail(validationResult.Error);
        }
        var roundValidation = ValidateRoundNumber.ValidateRound(roundNumber, (int)numberOfParticipants);
        if (!roundValidation.IsSuccess)
        {
            return Result<long>.Fail(roundValidation.Error);
        }
        long matchesPerRound = numberOfParticipants / 2;
        return Result<long>.Ok(matchesPerRound);
    }
}