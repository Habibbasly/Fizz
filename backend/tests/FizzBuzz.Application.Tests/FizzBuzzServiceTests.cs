using FizzBuzz.Application.FizzBuzz;
using Microsoft.Extensions.Options;

namespace FizzBuzz.Application.Tests;

public class FizzBuzzServiceTests
{
    private const int MaxLimit = 100;

    private readonly FizzBuzzService _service = new(Options.Create(new FizzBuzzOptions { MaxLimit = MaxLimit }));

    [Fact]
    public void Generate_MatchesSpecificationExample()
    {
        var result = _service.Generate(new FizzBuzzRequest(3, 5, 15, "Fizz", "Buzz"));

        string[] expected = ["1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz"];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_UsesCustomParameters()
    {
        var result = _service.Generate(new FizzBuzzRequest(2, 7, 14, "foo", "bar"));

        Assert.Equal("foo", result[1]);     // 2
        Assert.Equal("bar", result[6]);     // 7
        Assert.Equal("foobar", result[13]); // 14
        Assert.Equal("13", result[12]);
    }

    [Fact]
    public void Generate_ConcatenatesStr1ThenStr2WhenIntegersAreEqual()
    {
        var result = _service.Generate(new FizzBuzzRequest(3, 3, 3, "a", "b"));

        Assert.Equal("ab", result[2]);
    }

    [Fact]
    public void Generate_ConcatenatesStr1ThenStr2EvenWhenInt1IsGreater()
    {
        var result = _service.Generate(new FizzBuzzRequest(5, 3, 15, "Buzz", "Fizz"));

        Assert.Equal("BuzzFizz", result[14]);
    }

    [Fact]
    public void Generate_WithDivisorOne_ReplacesEveryNumber()
    {
        var result = _service.Generate(new FizzBuzzRequest(1, 100, 5, "x", "y"));

        Assert.All(result, value => Assert.Equal("x", value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(MaxLimit)]
    public void Generate_ReturnsOneValuePerNumberUpToLimit(int limit)
    {
        Assert.Equal(limit, _service.Generate(new FizzBuzzRequest(3, 5, limit, "Fizz", "Buzz")).Count);
    }

    [Fact]
    public void Generate_RejectsLimitAboveConfiguredMax()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.Generate(new FizzBuzzRequest(3, 5, MaxLimit + 1, "Fizz", "Buzz")));

        Assert.Contains(MaxLimit.ToString(), ex.Message);
    }

    [Theory]
    [InlineData(0, 5, 15)]
    [InlineData(3, -1, 15)]
    [InlineData(3, 5, 0)]
    [InlineData(3, 5, -10)]
    public void Generate_RejectsOutOfRangeNumbers(int int1, int int2, int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Generate(new FizzBuzzRequest(int1, int2, limit, "Fizz", "Buzz")));
    }

    [Theory]
    [InlineData("", "Buzz")]
    [InlineData("Fizz", " ")]
    [InlineData(null, "Buzz")]
    [InlineData("Fizz", null)]
    public void Generate_RejectsEmptyStrings(string? str1, string? str2)
    {
        Assert.Throws<ArgumentException>(() => _service.Generate(new FizzBuzzRequest(3, 5, 15, str1!, str2!)));
    }
}
