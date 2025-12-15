using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Tests;

public class GetMatchesForSpecificRound
{
    public Result<int> GetAllMatchesForSpecificRound(int numberOfParticipants, int roundNumber)
    {
        var validationResult = ValidateNumberOfParticipants.ValidateParticipants(numberOfParticipants);
        if (!validationResult.IsSuccess)
        {
            return Result<int>.Fail(validationResult.Error);
        }
        var roundValidation = ValidateRoundNumber.ValidateRound(roundNumber, (int)numberOfParticipants);
        if (!roundValidation.IsSuccess)
        {
            return Result<int>.Fail(roundValidation.Error);
        }
        int matchesPerRound = numberOfParticipants / 2;
        return Result<int>.Ok(matchesPerRound);
    }
}