using System.ComponentModel.DataAnnotations;

namespace FizzBuzz.Application.FizzBuzz;

/// <summary>Section "FizzBuzz" de appsettings.{Environment}.json.</summary>
public sealed class FizzBuzzOptions
{
    public const string SectionName = "FizzBuzz";

    /// <summary>Valeur maximale acceptée pour le paramètre "limit".</summary>
    [Range(1, 1_000_000)]
    public int MaxLimit { get; init; } = 10_000;
}
