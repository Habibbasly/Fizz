using FizzBuzz.Domain.Rules;

namespace FizzBuzz.Domain;

/// <summary>
/// Cœur du domaine : applique un ensemble de règles à un nombre.
/// Les mots des règles qui s'appliquent sont concaténés dans l'ordre des règles.
/// </summary>
public sealed class FizzBuzzGame
{
    private readonly IReadOnlyList<FizzBuzzRule> _rules;

    public FizzBuzzGame(IEnumerable<FizzBuzzRule> rules)
    {
        _rules = rules?.ToList() ?? throw new ArgumentNullException(nameof(rules));
    }

    public string Evaluate(int number)
    {
        var result = string.Concat(_rules.Where(r => r.AppliesTo(number)).Select(r => r.Word));
        return result.Length > 0 ? result : number.ToString();
    }

    /// <summary>Retourne les valeurs de 1 à <paramref name="limit"/> (inclus).</summary>
    public IEnumerable<string> Play(int limit)
    {
        if (limit < 1)
            throw new ArgumentOutOfRangeException(nameof(limit), "La limite doit être supérieure ou égale à 1.");

        for (var i = 1; i <= limit; i++)
            yield return Evaluate(i);
    }
}
