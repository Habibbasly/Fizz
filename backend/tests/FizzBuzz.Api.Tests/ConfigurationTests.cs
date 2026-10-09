using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.Api.Tests;

public class ConfigurationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [InlineData(10, HttpStatusCode.OK)]
    [InlineData(11, HttpStatusCode.BadRequest)]
    public async Task MaxLimit_IsReadFromConfiguration(int limit, HttpStatusCode expected)
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseSetting("FizzBuzz:MaxLimit", "10")).CreateClient();

        var response = await client.GetAsync($"/api/fizzbuzz?int1=3&int2=5&limit={limit}&str1=Fizz&str2=Buzz");

        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("2000000")]
    public void InvalidMaxLimit_PreventsStartup(string maxLimit)
    {
        // ValidateOnStart : une configuration hors bornes doit empêcher l'application de démarrer.
        var invalidFactory = factory.WithWebHostBuilder(builder => builder.UseSetting("FizzBuzz:MaxLimit", maxLimit));

        Assert.ThrowsAny<Exception>(() => invalidFactory.CreateClient());
    }
}
