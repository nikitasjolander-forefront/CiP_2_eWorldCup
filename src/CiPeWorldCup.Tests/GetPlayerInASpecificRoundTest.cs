using CiPeWorldCup.Application.Queries;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Infrastructure.Data;
using CiPeWorldCup.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CiPeWorldCup.Tests;

public class GetASpecificRoundTest
{
    private GetPairInASpecificRound _getSpecificRound;
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

        _getSpecificRound = new GetPairInASpecificRound();
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

    [Test]
    public void GetCorrectPairForSpecificRoundTest()
    {
        var result = _getSpecificRound.GetPlayersInRound(_participants, 10, 4, 2);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Item1.Name, Is.EqualTo("Charlie"));
        Assert.That(result.Value.Item2.Name, Is.EqualTo("Fiona"));
    }

    [Test]
    public void GetPlayersInRound_ReturnsFailure_WhenMatchNumberTooHigh()
    {
        var result = _getSpecificRound.GetPlayersInRound(_participants, 10, 99, 2);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("Invalid match number"));
    }

    [Test]
    public void GetPlayersInRound_ReturnsFailure_WhenRoundNumberInvalid()
    {
        var result = _getSpecificRound.GetPlayersInRound(_participants, 10, 1, 0);

        Assert.That(result.IsSuccess, Is.False);
    }
}