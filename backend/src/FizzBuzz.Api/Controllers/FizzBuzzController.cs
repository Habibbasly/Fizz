using FizzBuzz.Application.FizzBuzz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FizzBuzz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FizzBuzzController(
    IFizzBuzzRequestValidator validator,
    IFizzBuzzService fizzBuzzService,
    ILogger<FizzBuzzController> logger) : ControllerBase
{
    /// <summary>Génère la séquence FizzBuzz de 1 à <paramref name="limit"/>.</summary>
    /// <remarks>Exemple : GET api/fizzbuzz?int1=3&amp;int2=5&amp;limit=15&amp;str1=Fizz&amp;str2=Buzz</remarks>
    /// <param name="int1">Les multiples de ce nombre sont remplacés par <paramref name="str1"/>.</param>
    /// <param name="int2">Les multiples de ce nombre sont remplacés par <paramref name="str2"/>.</param>
    /// <param name="limit">Dernier nombre de la séquence.</param>
    /// <param name="str1">Texte affiché pour les multiples de <paramref name="int1"/>.</param>
    /// <param name="str2">Texte affiché pour les multiples de <paramref name="int2"/>.</param>
    /// <response code="200">La séquence générée.</response>
    /// <response code="400">Paramètres invalides : le champ "errors" détaille les erreurs par paramètre.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public ActionResult<IReadOnlyList<string>> Get(
        [FromQuery, BindRequired] int int1,
        [FromQuery, BindRequired] int int2,
        [FromQuery, BindRequired] int limit,
        [FromQuery, BindRequired] string str1,
        [FromQuery, BindRequired] string str2)
    {
        var request = new FizzBuzzRequest(int1, int2, limit, str1, str2);

        var errors = validator.Validate(request);
        if (errors.Count > 0)
        {
            logger.LogWarning("Requête FizzBuzz invalide (int1={Int1}, int2={Int2}, limit={Limit}) : {@Errors}",
                int1, int2, limit, errors);

            // Même format (ValidationProblemDetails) que les erreurs de binding renvoyées automatiquement par [ApiController].
            foreach (var (field, messages) in errors)
                foreach (var message in messages)
                    ModelState.AddModelError(field, message);

            return ValidationProblem(ModelState);
        }

        return Ok(fizzBuzzService.Generate(request));
    }
}
