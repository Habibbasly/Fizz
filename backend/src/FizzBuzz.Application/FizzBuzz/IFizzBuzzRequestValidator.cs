namespace FizzBuzz.Application.FizzBuzz;

public interface IFizzBuzzRequestValidator
{
    /// <summary>
    /// Valide les paramètres saisis par l'utilisateur.
    /// </summary>
    /// <returns>Les messages d'erreur par paramètre (int1, int2, limit, str1, str2) ; vide si la requête est valide.</returns>
    IReadOnlyDictionary<string, string[]> Validate(FizzBuzzRequest request);
}
