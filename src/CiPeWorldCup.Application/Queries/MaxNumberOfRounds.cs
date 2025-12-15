using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Application.Queries;

public class MaxNumberOfRounds
{
    public Result<int> CalculateMaxRounds(int numberOfParticipants)
    {
        var validationResult = ValidateNumberOfParticipants.ValidateParticipants(numberOfParticipants);

        if (!validationResult.IsSuccess)
        {
            return Result<int>.Fail(validationResult.Error);
        }
        return Result<int>.Ok(numberOfParticipants - 1);
    }
}
