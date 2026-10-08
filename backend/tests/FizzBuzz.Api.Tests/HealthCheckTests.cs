using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FizzBuzz.Api.Tests;

public class HealthCheckTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthChecks_ReturnHealthy(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
        Assert.Contains(json.RootElement.GetProperty("checks").EnumerateArray(),
            check => check.GetProperty("name").GetString() == "self");
    }

    [Fact]
    public async Task FailingDependency_MakesReadyUnhealthy_ButKeepsLiveHealthy()
    {
        // Une dépendance en panne (sans le tag "live") ne doit faire échouer que la readiness probe :
        // l'orchestrateur arrête d'envoyer du trafic sans redémarrer le processus.
        var client = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHealthChecks().AddCheck("database", () => HealthCheckResult.Unhealthy("Base indisponible"))))
            .CreateClient();

        var ready = await client.GetAsync("/health/ready");
        var live = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        using var json = JsonDocument.Parse(await ready.Content.ReadAsStringAsync());
        Assert.Equal("Unhealthy", json.RootElement.GetProperty("status").GetString());

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
    }
}
