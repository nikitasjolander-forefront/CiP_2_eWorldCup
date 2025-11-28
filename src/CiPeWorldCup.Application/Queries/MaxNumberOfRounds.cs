using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Core.Validation;

namespace CiPeWorldCup.Application.Queries;

public class MaxNumberOfRounds
{
    public Result<long> CalculateMaxRounds(long numberOfParticipants)
    {
        var validationResult = ValidateNumberOfParticipants.ValidateParticipants(numberOfParticipants);

        if (!validationResult.IsSuccess)
        {
            return Result<long>.Fail(validationResult.Error);
        }
        return Result<long>.Ok(numberOfParticipants - 1);
    }
}
