namespace FizzBuzz.Api.Configuration;

/// <summary>Section "Cors" de appsettings.{Environment}.json.</summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "Frontend";

    /// <summary>Origines du front autorisées à appeler l'API (ex : https://fizzbuzz.example.com).</summary>
    public string[] AllowedOrigins { get; init; } = [];
}
