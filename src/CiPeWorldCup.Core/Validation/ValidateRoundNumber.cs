using CiPeWorldCup.Core.RailwayErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Validation;
public class ValidateRoundNumber
{
    public static Result<long> ValidateRound(long roundNumber, long numberOfParticipants)
    {
        if (roundNumber < 1)
        {
            return Result<long>.Fail("Round number must be at least 1");
        }
        if (roundNumber > numberOfParticipants - 1)
        {
            return Result<long>.Fail($"Round number cannot exceed {numberOfParticipants - 1}");
        }
        return Result<long>.Ok(roundNumber);
    }

}
