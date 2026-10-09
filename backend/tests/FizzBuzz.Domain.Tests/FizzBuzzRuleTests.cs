using FizzBuzz.Domain.Rules;

namespace FizzBuzz.Domain.Tests;

public class FizzBuzzRuleTests
{
    [Theory]
    [InlineData(3, 3)]
    [InlineData(3, 9)]
    [InlineData(3, 0)]
    [InlineData(3, -6)]
    [InlineData(1, 7)]
    public void AppliesTo_ReturnsTrueForMultiples(int divisor, int number)
    {
        Assert.True(new FizzBuzzRule(divisor, "Fizz").AppliesTo(number));
    }

    [Theory]
    [InlineData(3, 1)]
    [InlineData(3, 10)]
    [InlineData(5, -7)]
    public void AppliesTo_ReturnsFalseForNonMultiples(int divisor, int number)
    {
        Assert.False(new FizzBuzzRule(divisor, "Fizz").AppliesTo(number));
    }

    [Fact]
    public void Constructor_ExposesDivisorAndWord()
    {
        var rule = new FizzBuzzRule(3, "Fizz");

        Assert.Equal(3, rule.Divisor);
        Assert.Equal("Fizz", rule.Word);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_RejectsNonPositiveDivisor(int divisor)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new FizzBuzzRule(divisor, "Fizz"));
        Assert.Equal("divisor", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsEmptyWord(string? word)
    {
        var ex = Assert.Throws<ArgumentException>(() => new FizzBuzzRule(3, word!));
        Assert.Equal("word", ex.ParamName);
    }

    [Fact]
    public void Rules_WithSameValues_AreEqual()
    {
        Assert.Equal(new FizzBuzzRule(3, "Fizz"), new FizzBuzzRule(3, "Fizz"));
    }
}
