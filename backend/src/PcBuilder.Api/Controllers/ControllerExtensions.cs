using Microsoft.AspNetCore.Mvc;

namespace PcBuilder.Api.Controllers;

public static class ControllerExtensions
{
    /// <summary>
    /// Turns service validation errors into a 400 ValidationProblemDetails, with the same
    /// shape (and Turkish title) as the automatic [ApiController] validation responses.
    /// </summary>
    public static ActionResult ToValidationProblem(
        this ControllerBase controller, IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
        {
            foreach (var message in messages)
            {
                controller.ModelState.AddModelError(field, message);
            }
        }

        return controller.ValidationProblem();
    }
}
