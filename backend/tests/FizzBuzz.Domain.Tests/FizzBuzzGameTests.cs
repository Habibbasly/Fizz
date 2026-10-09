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
    [InlineData(30, "FizzBuzz")]
    [InlineData(98, "98")]
    public void Evaluate_ReturnsExpectedValue(int number, string expected)
    {
        Assert.Equal(expected, _game.Evaluate(number));
    }

    [Fact]
    public void Evaluate_ConcatenatesWordsInRuleOrder()
    {
        var reversed = new FizzBuzzGame([new FizzBuzzRule(5, "Buzz"), new FizzBuzzRule(3, "Fizz")]);

        Assert.Equal("BuzzFizz", reversed.Evaluate(15));
    }

    [Fact]
    public void Evaluate_SupportsMoreThanTwoRules()
    {
        var game = new FizzBuzzGame([new FizzBuzzRule(3, "Fizz"), new FizzBuzzRule(5, "Buzz"), new FizzBuzzRule(7, "Bazz")]);

        Assert.Equal("FizzBuzzBazz", game.Evaluate(105));
        Assert.Equal("FizzBazz", game.Evaluate(21));
    }

    [Fact]
    public void Evaluate_ReturnsNumberWhenNoRules()
    {
        var game = new FizzBuzzGame([]);

        Assert.Equal("15", game.Evaluate(15));
    }

    [Fact]
    public void Constructor_RejectsNullRules()
    {
        Assert.Throws<ArgumentNullException>(() => new FizzBuzzGame(null!));
    }

    [Fact]
    public void Constructor_CopiesRules()
    {
        var rules = new List<FizzBuzzRule> { new(3, "Fizz") };
        var game = new FizzBuzzGame(rules);

        rules.Add(new FizzBuzzRule(5, "Buzz"));

        Assert.Equal("5", game.Evaluate(5));
    }

    [Fact]
    public void Play_ReturnsValuesFromOneToLimitInclusive()
    {
        string[] expected = ["1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz"];

        Assert.Equal(expected, _game.Play(15));
    }

    [Fact]
    public void Play_WithLimitOne_ReturnsSingleValue()
    {
        Assert.Equal(["1"], _game.Play(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Play_ThrowsWhenLimitIsLowerThanOne(int limit)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _game.Play(limit).ToList());
        Assert.Equal("limit", ex.ParamName);
    }
}
