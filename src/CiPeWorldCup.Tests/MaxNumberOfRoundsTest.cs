using CiPeWorldCup.Application.Queries;
namespace CiPeWorldCup.Tests;

public class MaxNumberOfRoundsTest
{
    private MaxNumberOfRounds _maxAmountOfRounds;

    [SetUp]
    public void Setup()
    {
        _maxAmountOfRounds = new MaxNumberOfRounds();
    }

    [Test]
    public void MaxNumberOfRoundsIsOneFewerThanAmountOfParticipants()
    {
        var result = _maxAmountOfRounds.CalculateMaxRounds(20);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(19));
    }

    [Test]
    public void MaxNumberOfRoundsShouldFailNotEnoughParticipants()
    {
        var result = _maxAmountOfRounds.CalculateMaxRounds(1);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Not enough participants"));
    }

    [Test]
    public void MaxNumberOfRoundsShouldFailNotAnUnevenNumber()
    {
        var result = _maxAmountOfRounds.CalculateMaxRounds(3);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Not an even number of participants"));
    }

}
