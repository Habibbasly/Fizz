using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace FizzBuzz.Api.Observability;

public static class ApplicationInsightsExtensions
{
    /// <summary>Variable standard lue par le SDK Azure Monitor.</summary>
    public const string ConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    /// <summary>
    /// Envoie logs, requêtes HTTP, dépendances, exceptions et métriques vers Application Insights (OpenTelemetry).
    /// Activé uniquement si <see cref="ConnectionStringKey"/> est renseignée : sans elle, les logs restent sur la console.
    /// </summary>
    /// <returns><c>true</c> si Application Insights est activé.</returns>
    public static bool AddApplicationInsights(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration[ConnectionStringKey];
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        // Serilog relaie ses logs vers les providers enregistrés (writeToProviders) : on retire les providers
        // par défaut (console...) pour ne garder que celui d'OpenTelemetry et éviter les doublons.
        builder.Logging.ClearProviders();

        builder.Services.AddOpenTelemetry().UseAzureMonitor(options => options.ConnectionString = connectionString);

        // Comme pour les logs de requêtes, les health checks appelés en boucle ne sont pas tracés.
        builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
            options.Filter = context => !context.Request.Path.StartsWithSegments("/health"));

        return true;
    }
}
