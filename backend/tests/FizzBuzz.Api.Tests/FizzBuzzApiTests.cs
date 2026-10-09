using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.Api.Tests;

public class FizzBuzzApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_ReturnsSpecificationExample()
    {
        var result = await _client.GetFromJsonAsync<string[]>("/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz");

        string[] expected = ["1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz"];
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Get_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_WithLimitOne_ReturnsSingleValue()
    {
        var result = await _client.GetFromJsonAsync<string[]>("/api/fizzbuzz?int1=3&int2=5&limit=1&str1=Fizz&str2=Buzz");

        Assert.Equal(["1"], result!);
    }

    [Fact]
    public async Task Get_WithLimitEqualToMaxLimit_ReturnsFullSequence()
    {
        var result = await _client.GetFromJsonAsync<string[]>("/api/fizzbuzz?int1=3&int2=5&limit=10000&str1=Fizz&str2=Buzz");

        Assert.NotNull(result);
        Assert.Equal(10_000, result.Length);
        Assert.Equal("Buzz", result[^1]);
    }

    [Fact]
    public async Task Get_WithSameDivisors_ConcatenatesWords()
    {
        var result = await _client.GetFromJsonAsync<string[]>("/api/fizzbuzz?int1=2&int2=2&limit=4&str1=Fizz&str2=Buzz");

        Assert.Equal(["1", "FizzBuzz", "3", "FizzBuzz"], result!);
    }

    [Fact]
    public async Task Get_PreservesUrlEncodedStrings()
    {
        // "A&B" et "été" encodés dans l'URL doivent ressortir intacts dans le JSON.
        var result = await _client.GetFromJsonAsync<string[]>("/api/fizzbuzz?int1=1&int2=2&limit=2&str1=A%26B&str2=%C3%A9t%C3%A9");

        Assert.Equal(["A&B", "A&Bété"], result!);
    }

    [Theory]
    [InlineData("int1=3&int2=5&str1=Fizz&str2=Buzz")]                // limit manquant
    [InlineData("int2=5&limit=15&str1=Fizz&str2=Buzz")]              // int1 manquant
    [InlineData("int1=3&int2=5&limit=15&str2=Buzz")]                 // str1 manquant
    [InlineData("int1=abc&int2=5&limit=15&str1=Fizz&str2=Buzz")]     // int1 non numérique
    [InlineData("int1=3&int2=5&limit=1.5&str1=Fizz&str2=Buzz")]      // limit non entier
    [InlineData("int1=0&int2=5&limit=15&str1=Fizz&str2=Buzz")]       // diviseur nul
    [InlineData("int1=-3&int2=5&limit=15&str1=Fizz&str2=Buzz")]      // diviseur négatif
    [InlineData("int1=3&int2=5&limit=15&str1=&str2=Buzz")]           // chaîne vide
    [InlineData("int1=3&int2=5&limit=15&str1=%20&str2=Buzz")]        // chaîne blanche
    [InlineData("int1=3&int2=5&limit=0&str1=Fizz&str2=Buzz")]        // limit nul
    [InlineData("int1=3&int2=5&limit=-1&str1=Fizz&str2=Buzz")]       // limit négatif
    [InlineData("int1=3&int2=5&limit=10001&str1=Fizz&str2=Buzz")]    // juste au-delà de MaxLimit
    [InlineData("int1=3&int2=5&limit=99999999&str1=Fizz&str2=Buzz")] // très au-delà de MaxLimit
    public async Task Get_ReturnsBadRequestForInvalidParameters(string query)
    {
        var response = await _client.GetAsync($"/api/fizzbuzz?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_ReturnsValidationErrorsPerParameter()
    {
        var response = await _client.GetAsync("/api/fizzbuzz?int1=0&int2=5&limit=10001&str1=Fizz&str2=Buzz");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(["int1", "limit"], problem.Errors.Keys.Order());
        Assert.Contains("strictement positif", Assert.Single(problem.Errors["int1"]));
        Assert.Contains("10000", Assert.Single(problem.Errors["limit"]));
    }
}
