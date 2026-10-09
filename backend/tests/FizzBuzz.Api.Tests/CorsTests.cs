using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.Api.Tests;

public class CorsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private const string FizzBuzzUrl = "/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz";

    // WebApplicationFactory démarre en Development : origine autorisée = http://localhost:4200 (appsettings.Development.json).
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AllowedOrigin_ReceivesCorsHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, FizzBuzzUrl);
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("http://localhost:4200", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task UnknownOrigin_DoesNotReceiveCorsHeader()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, FizzBuzzUrl);
        request.Headers.Add("Origin", "https://evil.example.com");

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_ForGet_IsAllowed()
    {
        var response = await _client.SendAsync(Preflight("http://localhost:4200", "GET"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:4200", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Contains("GET", response.Headers.GetValues("Access-Control-Allow-Methods"));
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("DELETE")]
    public async Task Preflight_ForOtherMethods_IsRejected(string method)
    {
        var response = await _client.SendAsync(Preflight("http://localhost:4200", method));

        // ASP.NET Core renvoie la liste des méthodes autorisées : c'est le navigateur qui bloque la requête.
        Assert.DoesNotContain(method, response.Headers.GetValues("Access-Control-Allow-Methods"));
    }

    [Fact]
    public async Task DevelopmentOrigin_IsRejectedInProduction()
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production")).CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, FizzBuzzUrl);
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static HttpRequestMessage Preflight(string origin, string method)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/fizzbuzz");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        return request;
    }
}
