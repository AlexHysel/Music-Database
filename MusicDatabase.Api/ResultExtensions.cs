using Microsoft.AspNetCore.Mvc;
using MusicDatabase.Contracts;

namespace MusicDatabase.Api;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        => result.IsSuccess ? controller.Ok(result.Value) : result.Error!.ToProblem(controller);

    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        => result.IsSuccess ? controller.NoContent() : result.Error!.ToProblem(controller);

    private static IActionResult ToProblem(this Error error, ControllerBase controller)
    {
        int status = error.Type switch
        {
            ErrorType.Validation    => StatusCodes.Status400BadRequest,
            ErrorType.NotFound      => StatusCodes.Status404NotFound,
            ErrorType.Conflict      => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized  => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden     => StatusCodes.Status403Forbidden,
            _                       => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(title: error.Code, detail: error.Message, statusCode: status);
    }
}