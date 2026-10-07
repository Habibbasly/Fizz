using System.Net;
using System.Net.Http.Json;
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

    [Theory]
    [InlineData("int1=3&int2=5&str1=Fizz&str2=Buzz")]               // limit manquant
    [InlineData("int1=0&int2=5&limit=15&str1=Fizz&str2=Buzz")]      // diviseur nul
    [InlineData("int1=3&int2=5&limit=15&str1=&str2=Buzz")]          // chaîne vide
    [InlineData("int1=3&int2=5&limit=99999999&str1=Fizz&str2=Buzz")] // au-delà de MaxLimit
    public async Task Get_ReturnsBadRequestForInvalidParameters(string query)
    {
        var response = await _client.GetAsync($"/api/fizzbuzz?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthChecks_ReturnHealthy(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"Healthy\"", await response.Content.ReadAsStringAsync());
    }
}
