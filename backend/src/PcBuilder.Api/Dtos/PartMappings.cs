using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Api.Dtos;

/// <summary>Entity -> DTO conversion. Entities never leave the API directly.</summary>
public static class PartMappings
{
    /// <summary>
    /// In-memory version of the list projection in PartService.GetPartsAsync,
    /// for parts that are already loaded (e.g. after compatibility filtering).
    /// </summary>
    public static PartSummaryDto ToSummaryDto(this Part part) => new(
        part.Id, part.Name, part.Brand, part.Category, part.Price, part.Tdp, part.ImageUrl);

    public static PartDetailDto ToDetailDto(this Part part) => new(
        part.Id,
        part.Name,
        part.Brand,
        part.Category,
        part.Price,
        part.Tdp,
        part.ImageUrl,
        part.IsActive,
        part.ToSpecsDto());

    private static PartSpecsDto ToSpecsDto(this Part part) => part switch
    {
        Cpu c => new CpuSpecsDto(
            c.Socket, c.Cores, c.Threads, c.BaseClockGhz, c.BoostClockGhz,
            c.HasIntegratedGraphics, c.IncludesCooler),

        Motherboard m => new MotherboardSpecsDto(
            m.Socket, m.Chipset, m.FormFactor, m.MemoryType,
            m.RamSlots, m.MaxRamGb, m.M2Slots, m.SataPorts),

        RamKit r => new RamKitSpecsDto(
            r.MemoryType, r.ModuleCount, r.ModuleSizeGb, r.TotalCapacityGb, r.SpeedMhz),

        Gpu g => new GpuSpecsDto(g.GpuChip, g.VramGb, g.LengthMm, g.RecommendedPsuW),

        Psu p => new PsuSpecsDto(p.Wattage, p.EfficiencyRating, p.IsModular),

        Case c => new CaseSpecsDto(
            Enum.GetValues<FormFactor>().Where(c.Supports).ToList(),
            c.MaxGpuLengthMm, c.MaxCoolerHeightMm),

        Storage s => new StorageSpecsDto(s.StorageType, s.Interface, s.CapacityGb),

        // Adding a new Part subclass without a mapping fails loudly here.
        _ => throw new NotSupportedException($"No specs mapping for {part.GetType().Name}.")
    };
}
