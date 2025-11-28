using CiPeWorldCup.Core;
using CiPeWorldCup.Core.Entities;
using CiPeWorldCup.Core.Validation;
using System.Numerics;

namespace CiPeWorldCup.Tests;

public class RoundRobinPairingsTest
{
    private RoundRobinPairings _roundRobinPairings;
    private List<Participant> _participants;



    //    Indata:
    //numberOfPlayers = 6
    //roundNumber = 2

    //Möjlig utdata:
    //Alice vs Charlie
    //Bob vs Fiona
    //Diana vs Ethan

    [SetUp]
    public void Setup()
    {
        _roundRobinPairings = new RoundRobinPairings(_participants.Count); // Pass numberOfParticipants as required
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

    [Test]
    public void RoundRobinPairingsAreHalfAmountOfParticipantsTester()
    {
        var result = _roundRobinPairings.GeneratePairings(_participants, 2);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(5));
    }

    [Test]
    public void GeneratePairings_FirstPlayerStaysFixed_AcrossAllRounds()
    {

        var round1 = _roundRobinPairings.GeneratePairings(_participants, 1);
        var round2 = _roundRobinPairings.GeneratePairings(_participants, 2);
        var round3 = _roundRobinPairings.GeneratePairings(_participants, 3);

        Assert.That(round1.Value[0].Item1.Name, Is.EqualTo("Alice"));
        Assert.That(round2.Value[0].Item1.Name, Is.EqualTo("Alice"));
        Assert.That(round3.Value[0].Item1.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public void GeneratePairings_Round1_ReturnsExpectedPairings()
    {
        var result = _roundRobinPairings.GeneratePairings(_participants, 1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value[0], Is.EqualTo((_participants[0], _participants[9]))); // Alice vs Julia
            Assert.That(result.Value[1], Is.EqualTo((_participants[1], _participants[8]))); // Bob vs Isaac
            Assert.That(result.Value[2], Is.EqualTo((_participants[2], _participants[7]))); // Charlie vs Hannah
            Assert.That(result.Value[3], Is.EqualTo((_participants[3], _participants[6]))); // Diana vs George
            Assert.That(result.Value[4], Is.EqualTo((_participants[4], _participants[5]))); // Ethan vs Fiona
        });
    }

    [Test]
    public void GeneratePairings_EachParticipantAppearsOnce_PerRound()
    {
        var result = _roundRobinPairings.GeneratePairings(_participants, 2);

        var allParticipants = new List<Participant>();
        foreach (var (player1, player2) in result.Value)
        {
            allParticipants.Add(player1);
            allParticipants.Add(player2);
        }

        Assert.That(allParticipants.Count, Is.EqualTo(10));
        Assert.That(allParticipants.Distinct().Count(), Is.EqualTo(10));
    }

    [Test]
    public void GeneratePairings_NoPlayerPlaysThemselves()
    {
        var result = _roundRobinPairings.GeneratePairings(_participants, 3);

        foreach (var (player1, player2) in result.Value)
        {
            Assert.That(player1.Id, Is.Not.EqualTo(player2.Id));
        }
    }

    [Test]
    public void GeneratePairings_DifferentRounds_ProduceDifferentPairings()
    {
        var round1 = _roundRobinPairings.GeneratePairings(_participants, 1);
        var round2 = _roundRobinPairings.GeneratePairings(_participants, 2);

        var hasDifference = false;
        for (int i = 0; i < round1.Value.Count; i++)
        {
            if (round1.Value[i] != round2.Value[i])
            {
                hasDifference = true;
                break;
            }
        }

        Assert.That(hasDifference, Is.True);
    }
}