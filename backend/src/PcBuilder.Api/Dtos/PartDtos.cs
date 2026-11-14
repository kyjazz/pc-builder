using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PcBuilder.Core.Enums;

namespace PcBuilder.Api.Dtos;

public enum PartSort
{
    Name,
    PriceAsc,
    PriceDesc
}

/// <summary>Query string of GET /api/parts. Invalid values produce a 400 ProblemDetails automatically.</summary>
public class PartListQuery
{
    public PartCategory? Category { get; init; }

    [MaxLength(100)]
    public string? Search { get; init; }

    public PartSort Sort { get; init; } = PartSort.Name;
}

/// <summary>One row in the parts list. Only the fields a list screen needs.</summary>
public record PartSummaryDto(
    int Id,
    string Name,
    string Brand,
    PartCategory Category,
    decimal Price,
    int Tdp,
    string? ImageUrl);

/// <summary>
/// Full part details. Common fields sit at the top level; category-specific fields
/// sit under "specs", whose shape is decided by "category".
/// </summary>
public record PartDetailDto(
    int Id,
    string Name,
    string Brand,
    PartCategory Category,
    decimal Price,
    int Tdp,
    string? ImageUrl,
    bool IsActive,
    PartSpecsDto Specs);

// The attributes tell System.Text.Json (and OpenAPI) which concrete types can appear
// behind the abstract "Specs" property, so their fields are serialized.
[JsonDerivedType(typeof(CpuSpecsDto))]
[JsonDerivedType(typeof(MotherboardSpecsDto))]
[JsonDerivedType(typeof(RamKitSpecsDto))]
[JsonDerivedType(typeof(GpuSpecsDto))]
[JsonDerivedType(typeof(PsuSpecsDto))]
[JsonDerivedType(typeof(CaseSpecsDto))]
[JsonDerivedType(typeof(StorageSpecsDto))]
public abstract record PartSpecsDto;

public sealed record CpuSpecsDto(
    string Socket,
    int Cores,
    int Threads,
    double BaseClockGhz,
    double BoostClockGhz,
    bool HasIntegratedGraphics,
    bool IncludesCooler) : PartSpecsDto;

public sealed record MotherboardSpecsDto(
    string Socket,
    string Chipset,
    FormFactor FormFactor,
    MemoryType MemoryType,
    int RamSlots,
    int MaxRamGb,
    int M2Slots,
    int SataPorts) : PartSpecsDto;

public sealed record RamKitSpecsDto(
    MemoryType MemoryType,
    int ModuleCount,
    int ModuleSizeGb,
    int TotalCapacityGb,
    int SpeedMhz) : PartSpecsDto;

public sealed record GpuSpecsDto(
    string GpuChip,
    int VramGb,
    int LengthMm,
    int RecommendedPsuW) : PartSpecsDto;

public sealed record PsuSpecsDto(
    int Wattage,
    string EfficiencyRating,
    bool IsModular) : PartSpecsDto;

/// <summary>Flags are turned into a plain list (["ATX", "MicroATX"]) so the mobile app doesn't do bit math.</summary>
public sealed record CaseSpecsDto(
    IReadOnlyList<FormFactor> SupportedFormFactors,
    int MaxGpuLengthMm,
    int MaxCoolerHeightMm) : PartSpecsDto;

public sealed record StorageSpecsDto(
    StorageType StorageType,
    StorageInterface Interface,
    int CapacityGb) : PartSpecsDto;
