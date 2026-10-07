namespace FizzBuzz.Application.FizzBuzz;

public interface IFizzBuzzService
{
    IReadOnlyList<string> Generate(FizzBuzzRequest request);
}
