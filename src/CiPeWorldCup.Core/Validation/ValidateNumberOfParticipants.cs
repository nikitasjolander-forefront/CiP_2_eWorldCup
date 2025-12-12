using CiPeWorldCup.Core.RailwayErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.Validation;
public class ValidateNumberOfParticipants
{
    public static Result<int> ValidateParticipants(int numberOfParticipants)
    {
        if (numberOfParticipants < 2)
        {
            return Result<int>.Fail("Not enough participants");
        }

        if (numberOfParticipants % 2 != 0)
        {
            return Result<int>.Fail("Not an even number of participants");
        }

        return Result<int>.Ok(numberOfParticipants);
    }


}
