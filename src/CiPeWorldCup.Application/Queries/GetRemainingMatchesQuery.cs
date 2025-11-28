using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Application.Queries;

public class GetRemainingMatchesQuery
{
    public long TotalPlayers { get; init; }
    public long RoundsPlayed { get; init; }

    public GetRemainingMatchesQuery(long totalPlayers, long roundsPlayed)
    {
        TotalPlayers = totalPlayers;
        RoundsPlayed = roundsPlayed;
    }
}
