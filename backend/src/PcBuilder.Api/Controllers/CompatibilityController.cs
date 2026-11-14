using Microsoft.AspNetCore.Mvc;
using PcBuilder.Api.Dtos;
using PcBuilder.Api.Services;
using PcBuilder.Core.Compatibility;

namespace PcBuilder.Api.Controllers;

[ApiController]
[Route("api/compatibility")]
[Produces("application/json")]
public class CompatibilityController(BuildSnapshotLoader snapshotLoader, CompatibilityEngine engine) : ControllerBase
{
    /// <summary>
    /// Checks a part selection against all compatibility rules.
    /// Returns issues (most serious first), total price and power figures.
    /// </summary>
    [HttpPost("check")]
    [ProducesResponseType<CompatibilityCheckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompatibilityCheckResponse>> Check(
        CompatibilityCheckRequest request, CancellationToken cancellationToken)
    {
        var load = await snapshotLoader.LoadAsync(request.Items, cancellationToken);
        if (!load.Succeeded)
        {
            return this.ToValidationProblem(load.Errors);
        }

        var snapshot = load.Value!;
        return Ok(engine.Check(snapshot).ToResponse(snapshot));
    }
}
