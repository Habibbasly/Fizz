using FizzBuzz.Domain;
using FizzBuzz.Domain.Rules;
using Microsoft.Extensions.Options;

namespace FizzBuzz.Application.FizzBuzz;

public sealed class FizzBuzzService(IOptions<FizzBuzzOptions> options) : IFizzBuzzService
{
    private readonly int _maxLimit = options.Value.MaxLimit;

    public IReadOnlyList<string> Generate(FizzBuzzRequest request)
    {
        if (request.Limit > _maxLimit)
            throw new ArgumentOutOfRangeException(nameof(request.Limit), $"La limite ne peut pas dépasser {_maxLimit}.");

        // L'ordre des règles garantit que les multiples de int1 ET int2 donnent "str1str2".
        var game = new FizzBuzzGame(
        [
            new FizzBuzzRule(request.Int1, request.Str1),
            new FizzBuzzRule(request.Int2, request.Str2),
        ]);

        return game.Play(request.Limit).ToList();
    }
}
