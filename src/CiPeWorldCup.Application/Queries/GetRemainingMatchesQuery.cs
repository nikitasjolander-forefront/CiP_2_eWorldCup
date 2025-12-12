using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Application.Queries;

public class GetRemainingMatchesQuery
{
    public int TotalPlayers { get; init; }
    public int RoundsPlayed { get; init; }

    public GetRemainingMatchesQuery(int totalPlayers, int roundsPlayed)
    {
        TotalPlayers = totalPlayers;
        RoundsPlayed = roundsPlayed;
    }
}
