using System.ComponentModel.DataAnnotations;
using PcBuilder.Core.Compatibility;

namespace PcBuilder.Api.Dtos;

/// <summary>One selected part. Same shape as a BuildItems row, so saved builds can be checked as-is.</summary>
public class BuildItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Parça kimliği pozitif bir sayı olmalı.")]
    public int PartId { get; init; }

    /// <summary>Only RAM kits and drives may be more than 1.</summary>
    [Range(1, 10, ErrorMessage = "Adet 1 ile 10 arasında olmalı.")]
    public int Quantity { get; init; } = 1;
}

/// <summary>Body of POST /api/compatibility/check and /api/parts/compatible. The server works out each part's category.</summary>
public class CompatibilityCheckRequest
{
    [Required(ErrorMessage = "Parça listesi gönderilmeli.")]
    [MaxLength(30, ErrorMessage = "Bir sistemde en fazla 30 kalem olabilir.")]
    public List<BuildItemRequest> Items { get; init; } = [];
}

public record CompatibilityIssueDto(string RuleCode, Severity Severity, string Message);

public record CompatibilityCheckResponse(
    IReadOnlyList<CompatibilityIssueDto> Issues,
    decimal TotalPrice,
    int EstimatedWattage,
    int RecommendedPsuWattage,
    bool IsCompatible);

public static class CompatibilityMappings
{
    public static CompatibilityCheckResponse ToResponse(this CompatibilityResult result, BuildSnapshot snapshot) => new(
        result.Issues.Select(i => new CompatibilityIssueDto(i.RuleCode, i.Severity, i.Message)).ToList(),
        snapshot.TotalPrice,
        PowerCalculator.EstimateWattage(snapshot),
        PowerCalculator.RecommendedPsuWattage(snapshot),
        result.IsCompatible);
}
