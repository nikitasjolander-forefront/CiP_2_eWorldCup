using CiPeWorldCup.Application.Queries;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Infrastructure;
using CiPeWorldCup.Infrastructure.Data;
using CiPeWorldCup.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


namespace CiPeWorldCup.Tests;

public class GetMatchesForASpecificRoundTest
{
    private GetMatchesForSpecificRound _getMatchesForSpecificRound;
    private ParticipantRepository _repository;
    private CiPeWorldCupDbContext _context;
    private List<Participant> _participants;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CiPeWorldCupDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CiPeWorldCupTest;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        _context = new CiPeWorldCupDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _getMatchesForSpecificRound = new GetMatchesForSpecificRound();
        _repository = new ParticipantRepository(_context);
        _participants = new List<Participant>
        {
            new Participant("Alice", 1),
            new Participant("Bob", 2),
            new Participant("Charlie", 3),
            new Participant("Diana", 4),
            new Participant("Ethan", 5),
            new Participant("Fiona", 6),
            new Participant("George", 7),
            new Participant("Hannah", 8),
            new Participant("Isaac", 9),
            new Participant("Julia", 10)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
