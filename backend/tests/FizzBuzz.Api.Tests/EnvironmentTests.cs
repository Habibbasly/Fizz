using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.Api.Tests;

public class EnvironmentTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient CreateClient(string environment, string baseAddress = "http://localhost") =>
        factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment))
            .CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri(baseAddress) });

    [Theory]
    [InlineData("Development", "/openapi/v1.json", HttpStatusCode.OK)]
    [InlineData("Development", "/swagger/index.html", HttpStatusCode.OK)]
    [InlineData("Preprod", "/openapi/v1.json", HttpStatusCode.OK)]
    [InlineData("Production", "/openapi/v1.json", HttpStatusCode.NotFound)]
    [InlineData("Production", "/swagger/index.html", HttpStatusCode.NotFound)]
    public async Task ApiDocumentation_IsOnlyExposedOutsideProduction(string environment, string url, HttpStatusCode expected)
    {
        var response = await CreateClient(environment).GetAsync(url);

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("Preprod")]
    [InlineData("Production")]
    public async Task Hsts_IsSentOutsideDevelopment(string environment)
    {
        // HSTS n'est émis que sur HTTPS et jamais pour localhost : on appelle donc un hôte HTTPS fictif.
        var response = await CreateClient(environment, "https://fizzbuzz.test").GetAsync("/health/live");

        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Hsts_IsNotSentInDevelopment()
    {
        var response = await CreateClient("Development", "https://fizzbuzz.test").GetAsync("/health/live");

        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }
}
