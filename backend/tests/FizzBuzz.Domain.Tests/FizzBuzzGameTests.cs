using FizzBuzz.Domain;
using FizzBuzz.Domain.Rules;

namespace FizzBuzz.Domain.Tests;

public class FizzBuzzGameTests
{
    private readonly FizzBuzzGame _game = new([new FizzBuzzRule(3, "Fizz"), new FizzBuzzRule(5, "Buzz")]);

    [Theory]
    [InlineData(1, "1")]
    [InlineData(3, "Fizz")]
    [InlineData(5, "Buzz")]
    [InlineData(15, "FizzBuzz")]
    [InlineData(98, "98")]
    public void Evaluate_ReturnsExpectedValue(int number, string expected)
    {
        Assert.Equal(expected, _game.Evaluate(number));
    }

    [Fact]
    public void Play_ReturnsValuesFromOneToLimitInclusive()
    {
        string[] expected = ["1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz"];

        Assert.Equal(expected, _game.Play(15));
    }

    [Fact]
    public void Play_ThrowsWhenLimitIsLowerThanOne()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _game.Play(0).ToList());
    }

    [Fact]
    public void Rule_RejectsNonPositiveDivisor()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FizzBuzzRule(0, "Zero"));
    }

    [Fact]
    public void Rule_RejectsEmptyWord()
    {
        Assert.Throws<ArgumentException>(() => new FizzBuzzRule(3, " "));
    }
}
