using System.Security.Claims;

using ErrorOr;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using SpartanFitness.Api.Common.Http;
using SpartanFitness.Domain.Common.Authentication;

namespace SpartanFitness.Api.Controllers;

/// <summary>
/// Base classs of all controllers, which inherits from ControllerBase.
/// </summary>
[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        HttpContext.Items[HttpContextItemKeys.Errors] = errors;

        return Problem(errors[0]);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    private IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(statusCode: statusCode, title: error.Description);
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(
                error.Code,
                error.Description);
        }

        return ValidationProblem(modelStateDictionary);
    }

    protected static class Authorization
    {
        public static bool IsAdmin(HttpContext context)
            => context.User.IsInRole(RoleTypes.Administrator);
        public static bool UserIdMatchesClaim(HttpContext context, string userId)
            => context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value == userId;

        public static string GetUserIdFromClaims(HttpContext context)
            => context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
    }
}