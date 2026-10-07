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
    public void Generate_AcceptsConfiguredMaxLimit()
    {
        Assert.Equal(MaxLimit, _service.Generate(new FizzBuzzRequest(3, 5, MaxLimit, "Fizz", "Buzz")).Count);
    }

    [Theory]
    [InlineData(0, 5, 15, "Fizz", "Buzz")]
    [InlineData(3, -1, 15, "Fizz", "Buzz")]
    [InlineData(3, 5, 0, "Fizz", "Buzz")]
    [InlineData(3, 5, MaxLimit + 1, "Fizz", "Buzz")]
    [InlineData(3, 5, 15, "", "Buzz")]
    [InlineData(3, 5, 15, "Fizz", " ")]
    public void Generate_RejectsInvalidParameters(int int1, int int2, int limit, string str1, string str2)
    {
        Assert.ThrowsAny<ArgumentException>(() => _service.Generate(new FizzBuzzRequest(int1, int2, limit, str1, str2)));
    }
}
