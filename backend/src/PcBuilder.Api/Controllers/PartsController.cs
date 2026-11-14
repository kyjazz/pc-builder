using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.Api.Dtos;
using PcBuilder.Api.Services;
using PcBuilder.Core.Enums;

namespace PcBuilder.Api.Controllers;

[ApiController]
[Route("api/parts")]
[Produces("application/json")]
public class PartsController(IPartService partService) : ControllerBase
{
    /// <summary>Lists active parts. Example: /api/parts?category=Gpu&amp;search=rtx&amp;sort=PriceAsc</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PartSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PartSummaryDto>>> GetParts(
        [FromQuery] PartListQuery query, CancellationToken cancellationToken)
    {
        var parts = await partService.GetPartsAsync(query, cancellationToken);
        return Ok(parts);
    }

    /// <summary>Returns one part with its category-specific specs.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PartDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartDetailDto>> GetPart(int id, CancellationToken cancellationToken)
    {
        var part = await partService.GetPartAsync(id, cancellationToken);

        // NotFound() is turned into a ProblemDetails body by [ApiController].
        return part is null ? NotFound() : Ok(part);
    }

    /// <summary>
    /// Lists parts of one category that can be added to the current selection without a new Error.
    /// Used by the mobile "Sadece uyumlular" filter. POST because the selection travels in the body.
    /// Example: POST /api/parts/compatible?category=Gpu with { "items": [{ "partId": 12 }] }
    /// </summary>
    [HttpPost("compatible")]
    [ProducesResponseType<IReadOnlyList<PartSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PartSummaryDto>>> GetCompatibleParts(
        [FromQuery, Required(ErrorMessage = "Kategori belirtilmeli.")] PartCategory? category,
        CompatibilityCheckRequest request,
        CancellationToken cancellationToken)
    {
        var result = await partService.GetCompatiblePartsAsync(category!.Value, request.Items, cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToValidationProblem(result.Errors);
        }

        return Ok(result.Value);
    }
}
