using FizzBuzz.Application.FizzBuzz;
using Microsoft.Extensions.Options;

namespace FizzBuzz.Application.Tests;

public class FizzBuzzRequestValidatorTests
{
    private const int MaxLimit = 100;

    private readonly FizzBuzzRequestValidator _validator = new(Options.Create(new FizzBuzzOptions { MaxLimit = MaxLimit }));

    [Theory]
    [InlineData(3, 5, 15, "Fizz", "Buzz")]
    [InlineData(1, 1, 1, "a", "b")]
    [InlineData(3, 5, MaxLimit, "Fizz", "Buzz")]
    public void Validate_AcceptsValidRequest(int int1, int int2, int limit, string str1, string str2)
    {
        Assert.Empty(_validator.Validate(new FizzBuzzRequest(int1, int2, limit, str1, str2)));
    }

    [Theory]
    [InlineData(0, 5, 15, "Fizz", "Buzz", "int1")]
    [InlineData(-3, 5, 15, "Fizz", "Buzz", "int1")]
    [InlineData(3, 0, 15, "Fizz", "Buzz", "int2")]
    [InlineData(3, 5, 0, "Fizz", "Buzz", "limit")]
    [InlineData(3, 5, -10, "Fizz", "Buzz", "limit")]
    [InlineData(3, 5, MaxLimit + 1, "Fizz", "Buzz", "limit")]
    [InlineData(3, 5, 15, "", "Buzz", "str1")]
    [InlineData(3, 5, 15, "Fizz", " ", "str2")]
    [InlineData(3, 5, 15, null, "Buzz", "str1")]
    public void Validate_ReportsInvalidParameter(int int1, int int2, int limit, string? str1, string? str2, string field)
    {
        var errors = _validator.Validate(new FizzBuzzRequest(int1, int2, limit, str1!, str2!));

        Assert.Equal([field], errors.Keys);
    }

    [Fact]
    public void Validate_MentionsConfiguredMaxLimit()
    {
        var errors = _validator.Validate(new FizzBuzzRequest(3, 5, MaxLimit + 1, "Fizz", "Buzz"));

        Assert.Contains(MaxLimit.ToString(), Assert.Single(errors["limit"]));
    }

    [Fact]
    public void Validate_ReportsEveryInvalidParameterAtOnce()
    {
        var errors = _validator.Validate(new FizzBuzzRequest(0, -1, 0, "", " "));

        Assert.Equal(["int1", "int2", "limit", "str1", "str2"], errors.Keys.Order());
    }
}
