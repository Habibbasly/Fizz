using FizzBuzz.Api.Configuration;
using FizzBuzz.Api.HealthChecks;
using FizzBuzz.Application;
using FizzBuzz.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

// Logger minimal pour tracer les erreurs de démarrage, remplacé ensuite par la configuration complète.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName));

    builder.Services
        .AddApplication(builder.Configuration)
        .AddInfrastructure();

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();
    builder.Services.AddApiHealthChecks();

    var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
    builder.Services.AddCors(options =>
        options.AddPolicy(CorsOptions.PolicyName, policy => policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .WithMethods("GET")));

    // Derrière un reverse proxy / load balancer : récupère le schéma et l'IP d'origine.
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();

    app.UseForwardedHeaders();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseSerilogRequestLogging(options =>
    {
        // Les health checks sont appelés en boucle par l'orchestrateur : on ne les trace qu'en Verbose.
        options.GetLevel = (httpContext, _, exception) =>
            exception is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
            : httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
            : LogEventLevel.Information;
    });

    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "FizzBuzz API v1");
            options.DocumentTitle = "FizzBuzz API";
        });
    }

    app.UseHttpsRedirection();
    app.UseCors(CorsOptions.PolicyName);
    app.UseAuthorization();

    app.MapControllers();
    app.MapApiHealthChecks();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "L'application s'est arrêtée de manière inattendue");
}
finally
{
    Log.CloseAndFlush();
}

// Exposé pour les tests d'intégration (WebApplicationFactory).
public partial class Program;
