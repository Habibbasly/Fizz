using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FizzBuzz.Api.HealthChecks;

public static class HealthCheckExtensions
{
    private const string LiveTag = "live";

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        // Ajouter ici les vérifications de dépendances (base de données, API externes...) :
        // elles seront prises en compte par /health/ready.
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag]);

        return services;
    }

    /// <summary>
    /// /health/live  : le processus répond (liveness probe).
    /// /health/ready : l'application et ses dépendances sont prêtes (readiness probe).
    /// </summary>
    public static IEndpointRouteBuilder MapApiHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(LiveTag),
            ResponseWriter = WriteJsonResponse,
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteJsonResponse,
        });

        return endpoints;
    }

    private static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonSerializerOptions.Web));
    }
}
