using System.Net;
using System.Text.Json;
using FizzBuzz.Application.FizzBuzz;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FizzBuzz.Api.Tests;

public class ErrorHandlingTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UnknownRoute_ReturnsNotFoundProblem()
    {
        var response = await factory.CreateClient().GetAsync("/api/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task UnsupportedMethod_ReturnsMethodNotAllowed(string method)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz");

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task UnhandledException_ReturnsProblemWithoutStackTrace(string environment)
    {
        var client = factory.WithWebHostBuilder(builder => builder
                .UseEnvironment(environment)
                .ConfigureTestServices(services => services.AddScoped<IFizzBuzzService, ThrowingFizzBuzzService>()))
            .CreateClient();

        var response = await client.GetAsync("/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(body);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain(ThrowingFizzBuzzService.SecretMessage, body);
        Assert.DoesNotContain(nameof(ThrowingFizzBuzzService), body);
    }

    private sealed class ThrowingFizzBuzzService : IFizzBuzzService
    {
        public const string SecretMessage = "détail interne qui ne doit pas fuiter";

        public IReadOnlyList<string> Generate(FizzBuzzRequest request) => throw new InvalidOperationException(SecretMessage);
    }
}
