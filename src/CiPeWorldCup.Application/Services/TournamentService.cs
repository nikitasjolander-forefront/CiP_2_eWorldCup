using CiPeWorldCup.Core;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.RailwayErrorHandling;
using CiPeWorldCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

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

    public async Task<Result<(Tournament Tournament, Match CurrentMatch, Participant Participant, Participant Opponent, bool IsPlayer1, List<Match> AllMatchesInRound)>>
            GetTournamentStatusAsync(int tournamentId, int participantId)
    {
        // Get tournament with matches
        var tournamentResult = await GetTournamentWithMatchesAsync(tournamentId);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Tournament, Match, Participant, Participant, bool, List<Match>)>.Fail(tournamentResult.Error!);
        }
        var tournament = tournamentResult.Value!;

        // Get participant
        var participantResult = await GetPlayerAsync(participantId);
        if (!participantResult.IsSuccess)
        {
            return Result<(Tournament, Match, Participant, Participant, bool, List<Match>)>.Fail(participantResult.Error!);
        }
        var participant = participantResult.Value!;

        // Find current match and opponent
        var matchResult = GetCurrentMatchForPlayer(tournament, participantId);
        if (!matchResult.IsSuccess)
        {
            return Result<(Tournament, Match, Participant, Participant, bool, List<Match>)>.Fail(matchResult.Error!);
        }
        var (currentMatch, isPlayer1) = matchResult.Value!;

        // Get opponent
        var opponentResult = await GetOpponentAsync(currentMatch, isPlayer1);
        if (!opponentResult.IsSuccess)
        {
            return Result<(Tournament, Match, Participant, Participant, bool, List<Match>)>.Fail(opponentResult.Error!);
        }
        var opponent = opponentResult.Value!;

        // Get all matches in current round
        var allMatchesInRound = GetMatchesInRound(tournament, tournament.CurrentRound);

        return Result<(Tournament, Match, Participant, Participant, bool, List<Match>)>.Ok(
            (tournament, currentMatch, participant, opponent, isPlayer1, allMatchesInRound));
    }

    private async Task<Result<Tournament>> GetTournamentWithMatchesAsync(int tournamentId)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return Result<Tournament>.Fail("Tournament not found.");
        }

        return Result<Tournament>.Ok(tournament);
    }

    private async Task<Result<Participant>> GetPlayerAsync(int participantId)
    {
        var participant = await dbContext.Participants.FindAsync(participantId);
        if (participant == null)
        {
            return Result<Participant>.Fail("Participant not found.");
        }

        return Result<Participant>.Ok(participant);
    }

    private Result<(Match Match, bool IsPlayer1)> GetCurrentMatchForPlayer(Tournament tournament, int playerId)
    {
        var currentMatch = tournament.Matches
            .FirstOrDefault(m => m.Round == tournament.CurrentRound &&
                                (m.Player1Id == playerId || m.Player2Id == playerId));

        if (currentMatch == null)
        {
            return Result<(Match, bool)>.Fail("No match found for player in current round.");
        }

        var isPlayer1 = currentMatch.Player1Id == playerId;
        return Result<(Match, bool)>.Ok((currentMatch, isPlayer1));
    }

    private async Task<Result<Participant>> GetOpponentAsync(Match match, bool isPlayer1)
    {
        var opponentId = isPlayer1 ? match.Player2Id : match.Player1Id;
        var opponent = await dbContext.Participants.FindAsync(opponentId);

        if (opponent == null)
        {
            return Result<Participant>.Fail("Opponent not found.");
        }

        return Result<Participant>.Ok(opponent);
    }

    private List<Match> GetMatchesInRound(Tournament tournament, int round)
    {
        return tournament.Matches
            .Where(m => m.Round == round)
            .ToList();
    }

    public async Task<Result<(Match UpdatedMatch, Participant Participant, Participant Opponent, bool IsPlayer1, int PlayerMove, int OpponentMove, int GameResult)>>
        PlayMoveAsync(int tournamentId, int playerId, string moveString)
    {
        // Validate tournament
        var tournamentResult = await ValidateTournamentForPlayAsync(tournamentId);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(tournamentResult.Error!);
        }
        var tournament = tournamentResult.Value!;

        // Validate and get current match
        var matchResult = ValidateAndGetCurrentMatch(tournament, playerId);
        if (!matchResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(matchResult.Error!);
        }
        var (currentMatch, isPlayer1) = matchResult.Value!;

        // Parse and validate move
        var moveResult = ParsePlayerMove(moveString);
        if (!moveResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(moveResult.Error!);
        }
        var playerMove = moveResult.Value;

        // Execute game round
        var gameResult = ExecuteGameRound(playerMove, out var opponentMove);

        // Update match
        var updatedMatch = UpdateMatchWithGameResult(currentMatch, isPlayer1, gameResult);

        // Save changes
        dbContext.Entry(currentMatch).State = EntityState.Detached;
        dbContext.Matches.Update(updatedMatch);
        await dbContext.SaveChangesAsync();

        // Get player details
        var playersResult = await GetPlayersForMatchAsync(playerId, updatedMatch, isPlayer1);
        if (!playersResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(playersResult.Error!);
        }
        var (player, opponent) = playersResult.Value!;

        return Result<(Match, Participant, Participant, bool, int, int, int)>.Ok(
            (updatedMatch, player, opponent, isPlayer1, playerMove, opponentMove, gameResult));
    }

    private async Task<Result<Tournament>> ValidateTournamentForPlayAsync(int tournamentId)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return Result<Tournament>.Fail("Tournament not found.");
        }

        if (tournament.IsCompleted)
        {
            return Result<Tournament>.Fail("Tournament is already completed.");
        }

        return Result<Tournament>.Ok(tournament);
    }

    private Result<(Match Match, bool IsPlayer1)> ValidateAndGetCurrentMatch(Tournament tournament, int playerId)
    {
        var currentMatch = tournament.Matches
            .FirstOrDefault(m => m.Round == tournament.CurrentRound &&
                                (m.Player1Id == playerId || m.Player2Id == playerId));

        if (currentMatch == null)
        {
            return Result<(Match, bool)>.Fail("No match found for player in current round.");
        }

        if (currentMatch.IsCompleted)
        {
            return Result<(Match, bool)>.Fail("Match is already completed.");
        }

        var isPlayer1 = currentMatch.Player1Id == playerId;
        return Result<(Match, bool)>.Ok((currentMatch, isPlayer1));
    }

    private Result<int> ParsePlayerMove(string moveString)
    {
        int playerMove = moveString.ToLower() switch
        {
            "rock" => 0,
            "paper" => 1,
            "scissors" => 2,
            _ => -1
        };

        if (playerMove == -1)
        {
            return Result<int>.Fail("Invalid move. Use 'rock', 'paper', or 'scissors'.");
        }

        return Result<int>.Ok(playerMove);
    }

    private int ExecuteGameRound(int playerMove, out int opponentMove)
    {
        var random = new Random();
        opponentMove = random.Next(0, 3);
        return DetermineWinner(playerMove, opponentMove);
    }

    private Match UpdateMatchWithGameResult(Match match, bool isPlayer1, int gameResult)
    {
        var updatedMatch = match;

        // Update scores based on game result
        if (gameResult == 1)
        {
            updatedMatch = isPlayer1
                ? updatedMatch with { Player1Wins = updatedMatch.Player1Wins + 1 }
                : updatedMatch with { Player2Wins = updatedMatch.Player2Wins + 1 };
        }
        else if (gameResult == -1)
        {
            updatedMatch = isPlayer1
                ? updatedMatch with { Player2Wins = updatedMatch.Player2Wins + 1 }
                : updatedMatch with { Player1Wins = updatedMatch.Player1Wins + 1 };
        }

        // Check if match is completed (best of 3 - first to 2 wins)
        if (updatedMatch.Player1Wins == 2 || updatedMatch.Player2Wins == 2)
        {
            updatedMatch = updatedMatch with
            {
                IsCompleted = true,
                WinnerId = updatedMatch.Player1Wins == 2 ? updatedMatch.Player1Id : updatedMatch.Player2Id
            };
        }

        return updatedMatch;
    }

    private async Task<Result<(Participant Player, Participant Opponent)>> GetPlayersForMatchAsync(int playerId, Match match, bool isPlayer1)
    {
        var player = await dbContext.Participants.FindAsync(playerId);
        var opponentId = isPlayer1 ? match.Player2Id : match.Player1Id;
        var opponent = await dbContext.Participants.FindAsync(opponentId);

        if (player == null || opponent == null)
        {
            return Result<(Participant, Participant)>.Fail("Player or opponent not found.");
        }

        return Result<(Participant, Participant)>.Ok((player, opponent));
    }

    private async Task<Result<Tournament>> ValidateTournamentForAdvanceAsync(int tournamentId)
    {
        var tournament = await dbContext.Tournaments
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return Result<Tournament>.Fail("Tournament not found.");
        }

        if (tournament.IsCompleted)
        {
            return Result<Tournament>.Fail("Tournament is already completed.");
        }

        return Result<Tournament>.Ok(tournament);
    }

    private async Task SimulateIncompleteMatchesAsync(int tournamentId, int currentRound)
    {
        var currentRoundMatches = await dbContext.Matches
            .Where(m => m.TournamentId == tournamentId && m.Round == currentRound)
            .ToListAsync();

        var random = new Random();
        var updatedMatches = new List<Match>();

        foreach (var match in currentRoundMatches.Where(m => !m.IsCompleted))
        {
            var entry = dbContext.Entry(match);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
            }

            var updatedMatch = SimulateMatch(match, random);
            updatedMatches.Add(updatedMatch);
        }

        if (updatedMatches.Any())
        {
            dbContext.Matches.UpdateRange(updatedMatches);
            await dbContext.SaveChangesAsync();
        }
    }

    private Match SimulateMatch(Match match, Random random)
    {
        var updatedMatch = match;

        while (updatedMatch.Player1Wins < 2 && updatedMatch.Player2Wins < 2)
        {
            var move1 = random.Next(0, 3);
            var move2 = random.Next(0, 3);
            var result = DetermineWinner(move1, move2);

            if (result == 1)
                updatedMatch = updatedMatch with { Player1Wins = updatedMatch.Player1Wins + 1 };
            else if (result == -1)
                updatedMatch = updatedMatch with { Player2Wins = updatedMatch.Player2Wins + 1 };
        }

        return updatedMatch with
        {
            IsCompleted = true,
            WinnerId = updatedMatch.Player1Wins == 2 ? updatedMatch.Player1Id : updatedMatch.Player2Id
        };
    }

    private async Task<Tournament> CompleteTournamentAsync(Tournament tournament)
    {
        var completedTournament = tournament with { IsCompleted = true };
        dbContext.Tournaments.Update(completedTournament);
        await dbContext.SaveChangesAsync();
        return completedTournament;
    }

    private async Task<Result<(Tournament Tournament, List<Match> NewMatches, List<Participant> Players)>> AdvanceToNextRoundAsync(Tournament tournament)
    {
        var nextRound = tournament.CurrentRound + 1;
        var advancedTournament = tournament with { CurrentRound = nextRound };

        var allPlayers = await dbContext.Participants
            .Take(tournament.NumberOfPlayers)
            .ToListAsync();

        var roundRobinPairings = new RoundRobinPairings(allPlayers.Count);
        var pairingResult = roundRobinPairings.GeneratePairings(allPlayers, nextRound);

        if (!pairingResult.IsSuccess)
        {
            return Result<(Tournament, List<Match>, List<Participant>)>.Fail(pairingResult.Error!);
        }

        var newMatches = pairingResult.Value!.Select(pair => new Match
        {
            TournamentId = tournament.Id,
            Round = nextRound,
            Player1Id = pair.Item1.Id,
            Player2Id = pair.Item2.Id,
            Player1Wins = 0,
            Player2Wins = 0,
            IsCompleted = false,
            WinnerId = null
        }).ToList();

        dbContext.Matches.AddRange(newMatches);
        dbContext.Tournaments.Update(advancedTournament);
        await dbContext.SaveChangesAsync();

        return Result<(Tournament, List<Match>, List<Participant>)>.Ok((advancedTournament, newMatches, allPlayers));
    }

    public async Task<Result<(Tournament Tournament, List<(int Rank, int PlayerId, string PlayerName, int Wins, int Losses)> Scoreboard)>>
    GetFinalResultsAsync(int tournamentId)
    {
        var tournamentResult = await GetCompletedTournamentAsync(tournamentId);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Tournament, List<(int, int, string, int, int)>)>.Fail(tournamentResult.Error!);
        }
        var tournament = tournamentResult.Value!;

        var scoreboard = await CalculateScoreboardAsync(tournament);

        return Result<(Tournament, List<(int, int, string, int, int)>)>.Ok((tournament, scoreboard));
    }

    private async Task<Result<Tournament>> GetCompletedTournamentAsync(int tournamentId)
    {
        var tournament = await dbContext.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null)
        {
            return Result<Tournament>.Fail("Tournament not found.");
        }

        if (!tournament.IsCompleted)
        {
            return Result<Tournament>.Fail("Tournament is not yet completed.");
        }

        return Result<Tournament>.Ok(tournament);
    }

    private async Task<List<(int Rank, int PlayerId, string PlayerName, int Wins, int Losses)>> CalculateScoreboardAsync(Tournament tournament)
    {
        var playerStats = new Dictionary<int, (string Name, int Wins, int Losses)>();
        var allPlayers = await dbContext.Participants
            .Where(p => p.Id <= tournament.NumberOfPlayers)
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        foreach (var match in tournament.Matches.Where(m => m.IsCompleted))
        {
            UpdatePlayerStats(playerStats, allPlayers, match);
        }

        return playerStats
            .OrderByDescending(p => p.Value.Wins)
            .ThenBy(p => p.Value.Losses)
            .Select((p, index) => (
                Rank: index + 1,
                PlayerId: p.Key,
                PlayerName: p.Value.Name,
                Wins: p.Value.Wins,
                Losses: p.Value.Losses
            ))
            .ToList();
    }

    private void UpdatePlayerStats(Dictionary<int, (string Name, int Wins, int Losses)> playerStats,
                                   Dictionary<int, string> allPlayers,
                                   Match match)
    {
        var player1Id = match.Player1Id;
        var player2Id = match.Player2Id;

        if (!playerStats.ContainsKey(player1Id))
            playerStats[player1Id] = (allPlayers[player1Id], 0, 0);
        if (!playerStats.ContainsKey(player2Id))
            playerStats[player2Id] = (allPlayers[player2Id], 0, 0);

        if (match.WinnerId == player1Id)
        {
            playerStats[player1Id] = (playerStats[player1Id].Name, playerStats[player1Id].Wins + 1, playerStats[player1Id].Losses);
            playerStats[player2Id] = (playerStats[player2Id].Name, playerStats[player2Id].Wins, playerStats[player2Id].Losses + 1);
        }
        else
        {
            playerStats[player2Id] = (playerStats[player2Id].Name, playerStats[player2Id].Wins + 1, playerStats[player2Id].Losses);
            playerStats[player1Id] = (playerStats[player1Id].Name, playerStats[player1Id].Wins, playerStats[player1Id].Losses + 1);
        }
    }

    private static int DetermineWinner(int move1, int move2)
    {
        if (move1 == move2) return 0; // Draw

        return (move1, move2) switch
        {
            (0, 2) => 1,  // Rock beats Scissors
            (1, 0) => 1,  // Paper beats Rock
            (2, 1) => 1,  // Scissors beats Paper
            _ => -1
        };
    }

    public async Task<Result<(Match UpdatedMatch, Participant Participant, Participant Opponent, bool IsPlayer1, int PlayerMove, int OpponentMove, int GameResult)>>
            PlayRoundAsync(int tournamentId, int playerId, string moveString)
    {
        // Validate tournament
        var tournamentResult = await ValidateTournamentForPlayAsync(tournamentId);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(tournamentResult.Error!);
        }
        var tournament = tournamentResult.Value!;

        // Validate and get current match
        var matchResult = ValidateAndGetCurrentMatch(tournament, playerId);
        if (!matchResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(matchResult.Error!);
        }
        var (currentMatch, isPlayer1) = matchResult.Value!;

        // Parse and validate move
        var moveResult = ParsePlayerMove(moveString);
        if (!moveResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(moveResult.Error!);
        }
        var playerMove = moveResult.Value;

        // Execute game round
        var gameResult = ExecuteGameRound(playerMove, out var opponentMove);

        // Update match
        var updatedMatch = UpdateMatchWithGameResult(currentMatch, isPlayer1, gameResult);

        // Save changes
        dbContext.Entry(currentMatch).State = EntityState.Detached;
        dbContext.Matches.Update(updatedMatch);
        await dbContext.SaveChangesAsync();

        // Get player details
        var playersResult = await GetPlayersForMatchAsync(playerId, updatedMatch, isPlayer1);
        if (!playersResult.IsSuccess)
        {
            return Result<(Match, Participant, Participant, bool, int, int, int)>.Fail(playersResult.Error!);
        }
        var (player, opponent) = playersResult.Value!;

        return Result<(Match, Participant, Participant, bool, int, int, int)>.Ok(
            (updatedMatch, player, opponent, isPlayer1, playerMove, opponentMove, gameResult));
    }

    public async Task<Result<(Tournament Tournament, List<Match>? NewMatches, List<Participant>? Participants, bool IsCompleted)>>
                   AdvanceRoundAsync(int tournamentId)
    {
        // Validate tournament
        var tournamentResult = await ValidateTournamentForAdvanceAsync(tournamentId);
        if (!tournamentResult.IsSuccess)
        {
            return Result<(Tournament, List<Match>?, List<Participant>?, bool)>.Fail(tournamentResult.Error!);
        }
        var tournament = tournamentResult.Value!;

        // Simulate incomplete matches
        await SimulateIncompleteMatchesAsync(tournamentId, tournament.CurrentRound);

        // Check if tournament is complete
        var maxRounds = tournament.NumberOfPlayers - 1;
        if (tournament.CurrentRound >= maxRounds)
        {
            var completedTournament = await CompleteTournamentAsync(tournament);
            return Result<(Tournament, List<Match>?, List<Participant>?, bool)>.Ok(
                (completedTournament, null, null, true));
        }

        // Advance to next round
        var nextRoundResult = await AdvanceToNextRoundAsync(tournament);
        if (!nextRoundResult.IsSuccess)
        {
            return Result<(Tournament, List<Match>?, List<Participant>?, bool)>.Fail(nextRoundResult.Error!);
        }

        var (advancedTournament, newMatches, players) = nextRoundResult.Value!;
        return Result<(Tournament, List<Match>?, List<Participant>?, bool)>.Ok(
            (advancedTournament, newMatches, players, false));
    }
    //}
}
