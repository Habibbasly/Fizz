namespace FizzBuzz.Domain.Rules;

/// <summary>
/// Règle métier : si un nombre est divisible par <see cref="Divisor"/>, on affiche <see cref="Word"/>.
/// </summary>
public sealed record FizzBuzzRule
{
    public int Divisor { get; }
    public string Word { get; }

    public FizzBuzzRule(int divisor, string word)
    {
        if (divisor <= 0)
            throw new ArgumentOutOfRangeException(nameof(divisor), "Le diviseur doit être strictement positif.");
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Le mot ne peut pas être vide.", nameof(word));

        Divisor = divisor;
        Word = word;
    }

    public bool AppliesTo(int number) => number % Divisor == 0;
}
