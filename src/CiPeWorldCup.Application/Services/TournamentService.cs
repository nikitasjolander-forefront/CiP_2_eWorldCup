using CiPeWorldCup.Core;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CiPeWorldCup.Application.Services;

public class TournamentService
{
    private readonly CiPeWorldCupDbContext dbContext;

    public TournamentService(CiPeWorldCupDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Result<(Tournament tournament, List<Match> Matches, List<Participant> Participants)>> StartTournamentAsync(string name, int numberOfPlayers)
    {
        // Create participants
        var participantsCount = await GetPlayersForTournamentAsync(numberOfPlayers);
        if (!participantsCount.IsSuccess)
        {
            return Result<(Tournament, List<Match>, List<Participant>)>.Fail(participantsCount.Error);
        }
        var participants = participantsCount.Value!;

        // Create tournament
        var tournamentResult = await CreateTournamentAsync(name, numberOfPlayers);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Tournament, List<Match>, List<Participant>)>.Fail(tournamentResult.Error);
        }
        var tournament = tournamentResult.Value!.Item1;

        // Generate initial matches
        var matchesResult = await GenerateInitialMatchesAsync(tournament, participants);
        if (!matchesResult.IsSuccess)
        {
            return Result<(Tournament, List<Match>, List<Participant>)>.Fail(matchesResult.Error);
        }
        var matches = matchesResult.Value!;

        return Result<(Tournament, List<Match>, List<Participant>)>.Ok((tournament, matches, participants));
    }

    private async Task<Result<List<Participant>>> GetPlayersForTournamentAsync(int numberOfPlayers)
    {
        var participants = await dbContext.Participants
            .Take(numberOfPlayers)
            .ToListAsync();
        return Result<List<Participant>>.Ok(participants);
    }
    
    private async Task<Result<(Tournament, List<Match>, List<Participant>)>> CreateTournamentAsync(string name, int numberOfPlayers)
    {
        var schedule = new RoundRobinPairings(numberOfPlayers);
        var tournament = new Tournament
        {
            Name = name,
            NumberOfPlayers = numberOfPlayers,
            CurrentRound = 1,
            StartedAt = DateTime.UtcNow,
            IsCompleted = false,
            Schedule = schedule  // Set as a property, not constructor parameter
        };
        dbContext.Tournaments.Add(tournament);
        await dbContext.SaveChangesAsync();

        return Result<(Tournament, List<Match>, List<Participant>)>.Ok((tournament, new List<Match>(), new List<Participant>()));
    }

    private async Task<Result<List<Match>>> GenerateInitialMatchesAsync(Tournament tournament, List<Participant> participants)
    {
        var roundRobinPairings = new RoundRobinPairings(participants.Count);
        var roundPairs = roundRobinPairings.GeneratePairings(participants, 1);
        var matches = roundPairs.Value!.Select(pair => new Match
        {
            TournamentId = tournament.Id,
            Round = 1,
            Player1Id = pair.Item1.Id,
            Player2Id = pair.Item2.Id,
            Player1Wins = 0,
            Player2Wins = 0,
            IsCompleted = false,
            WinnerId = null,
        }).ToList();

        dbContext.Matches.AddRange(matches);
        await dbContext.SaveChangesAsync();

        return Result<List<Match>>.Ok(matches);
    }
}
