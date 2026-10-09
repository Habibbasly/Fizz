using FizzBuzz.Domain;
using FizzBuzz.Domain.Rules;

namespace FizzBuzz.Application.FizzBuzz;

/// <summary>
/// Cas d'usage : construit le jeu à partir de la requête et le joue.
/// La requête doit avoir été validée au préalable par <see cref="IFizzBuzzRequestValidator"/>.
/// </summary>
public sealed class FizzBuzzService : IFizzBuzzService
{
    public IReadOnlyList<string> Generate(FizzBuzzRequest request)
    {
        // L'ordre des règles garantit que les multiples de int1 ET int2 donnent "str1str2".
        var game = new FizzBuzzGame(
        [
            new FizzBuzzRule(request.Int1, request.Str1),
            new FizzBuzzRule(request.Int2, request.Str2),
        ]);

        return game.Play(request.Limit).ToList();
    }
}
