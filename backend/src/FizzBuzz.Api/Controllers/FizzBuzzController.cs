using FizzBuzz.Application.FizzBuzz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FizzBuzz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FizzBuzzController(IFizzBuzzService fizzBuzzService, ILogger<FizzBuzzController> logger) : ControllerBase
{
    /// <summary>GET api/fizzbuzz?int1=3&amp;int2=5&amp;limit=15&amp;str1=Fizz&amp;str2=Buzz</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<string>> Get(
        [FromQuery, BindRequired] int int1,
        [FromQuery, BindRequired] int int2,
        [FromQuery, BindRequired] int limit,
        [FromQuery, BindRequired] string str1,
        [FromQuery, BindRequired] string str2)
    {
        try
        {
            return Ok(fizzBuzzService.Generate(new FizzBuzzRequest(int1, int2, limit, str1, str2)));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Requête FizzBuzz invalide (int1={Int1}, int2={Int2}, limit={Limit}) : {Reason}",
                int1, int2, limit, ex.Message);
            return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
