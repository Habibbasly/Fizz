using System.Net;
using FizzBuzz.Api.Observability;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace FizzBuzz.Api.Tests;

public class ApplicationInsightsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    // Chaîne factice : point d'ingestion local injoignable, aucune donnée ne quitte la machine.
    private const string FakeConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://127.0.0.1:1/";

    [Fact]
    public void WithoutConnectionString_TelemetryIsDisabled()
    {
        Assert.Null(factory.Services.GetService<TracerProvider>());
    }

    [Fact]
    public async Task WithConnectionString_TelemetryIsEnabledAndApiStillWorks()
    {
        using var enabled = factory.WithWebHostBuilder(builder =>
            builder.UseSetting(ApplicationInsightsExtensions.ConnectionStringKey, FakeConnectionString));
        var client = enabled.CreateClient();

        Assert.NotNull(enabled.Services.GetService<TracerProvider>());

        var api = await client.GetAsync("/api/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz");
        var health = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, api.StatusCode);
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }
}
