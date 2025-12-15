using CiPeWorldCup.Core.RailwayErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Validation;
public class ValidateRoundNumber
{
    public static Result<int> ValidateRound(int roundNumber, int numberOfParticipants)
    {
        if (roundNumber < 1)
        {
            return Result<int>.Fail("Round number must be at least 1");
        }
        if (roundNumber > numberOfParticipants - 1)
        {
            return Result<int>.Fail($"Round number cannot exceed {numberOfParticipants - 1}");
        }
        return Result<int>.Ok(roundNumber);
    }

}
