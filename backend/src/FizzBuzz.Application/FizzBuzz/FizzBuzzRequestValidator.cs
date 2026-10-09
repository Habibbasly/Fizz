using Microsoft.Extensions.Options;

namespace FizzBuzz.Application.FizzBuzz;

/// <summary>
/// Valide les entrées utilisateur avant d'appeler le domaine : une saisie invalide est un cas attendu,
/// elle produit des messages d'erreur et non une exception. Les gardes du domaine restent en place
/// pour protéger ses invariants contre une erreur de programmation.
/// </summary>
public sealed class FizzBuzzRequestValidator(IOptions<FizzBuzzOptions> options) : IFizzBuzzRequestValidator
{
    private readonly int _maxLimit = options.Value.MaxLimit;

    public IReadOnlyDictionary<string, string[]> Validate(FizzBuzzRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Int1 <= 0)
            errors["int1"] = ["Le diviseur doit être strictement positif."];
        if (request.Int2 <= 0)
            errors["int2"] = ["Le diviseur doit être strictement positif."];

        if (request.Limit < 1)
            errors["limit"] = ["La limite doit être supérieure ou égale à 1."];
        else if (request.Limit > _maxLimit)
            errors["limit"] = [$"La limite ne peut pas dépasser {_maxLimit}."];

        if (string.IsNullOrWhiteSpace(request.Str1))
            errors["str1"] = ["Le texte ne peut pas être vide."];
        if (string.IsNullOrWhiteSpace(request.Str2))
            errors["str2"] = ["Le texte ne peut pas être vide."];

        return errors;
    }
}
