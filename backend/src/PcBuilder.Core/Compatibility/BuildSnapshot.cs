using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Compatibility;

/// <summary>
/// The user's current part selection, already loaded from the database.
/// Rules only read this object, so they never touch EF Core or SQL and are trivial to unit test.
/// </summary>
public record BuildSnapshot
{
    public Cpu? Cpu { get; init; }
    public Motherboard? Motherboard { get; init; }
    public Gpu? Gpu { get; init; }
    public Psu? Psu { get; init; }
    public Case? Case { get; init; }

    /// <summary>One entry per physical kit. Two identical kits = the same RamKit listed twice.</summary>
    public IReadOnlyList<RamKit> RamKits { get; init; } = [];

    /// <summary>One entry per physical drive. Three identical SSDs = the same Storage listed three times.</summary>
    public IReadOnlyList<Storage> Storages { get; init; } = [];

    /// <summary>Total RAM sticks across every kit (each needs its own slot).</summary>
    public int TotalRamModules => RamKits.Sum(kit => kit.ModuleCount);

    /// <summary>Total RAM capacity across every kit.</summary>
    public int TotalRamGb => RamKits.Sum(kit => kit.TotalCapacityGb);

    /// <summary>Drives that need an M.2 slot on the motherboard.</summary>
    public int M2DriveCount => Storages.Count(drive => drive.Interface == StorageInterface.M2);

    /// <summary>Drives that need a SATA port (SATA SSDs and HDDs).</summary>
    public int SataDriveCount => Storages.Count(drive => drive.Interface == StorageInterface.Sata);

    /// <summary>Every selected part, one entry per physical unit.</summary>
    public IEnumerable<Part> AllParts =>
        new Part?[] { Cpu, Motherboard, Gpu, Psu, Case }
            .OfType<Part>()
            .Concat(RamKits)
            .Concat(Storages);

    /// <summary>Sum of all part prices; repeated kits/drives are counted once per unit.</summary>
    public decimal TotalPrice => AllParts.Sum(part => part.Price);

    /// <summary>
    /// Removes the selected part of a single-slot category (CPU, motherboard, GPU, PSU, case)
    /// so a candidate can take its place. RAM and storage are left as they are, because
    /// candidates in those categories are added next to what is already selected.
    /// </summary>
    public BuildSnapshot Without(PartCategory category) => category switch
    {
        PartCategory.Cpu => this with { Cpu = null },
        PartCategory.Motherboard => this with { Motherboard = null },
        PartCategory.Gpu => this with { Gpu = null },
        PartCategory.Psu => this with { Psu = null },
        PartCategory.Case => this with { Case = null },
        _ => this
    };

    /// <summary>
    /// Returns a copy with the part added: single-slot parts replace the current one,
    /// a RAM kit or drive is appended as one more unit. The original snapshot is unchanged.
    /// </summary>
    public BuildSnapshot With(Part part) => part switch
    {
        Cpu cpu => this with { Cpu = cpu },
        Motherboard motherboard => this with { Motherboard = motherboard },
        Gpu gpu => this with { Gpu = gpu },
        Psu psu => this with { Psu = psu },
        Case pcCase => this with { Case = pcCase },
        RamKit kit => this with { RamKits = [.. RamKits, kit] },
        Storage drive => this with { Storages = [.. Storages, drive] },
        _ => throw new NotSupportedException($"Unknown part type {part.GetType().Name}.")
    };

    /// <summary>A completely empty selection.</summary>
    public static BuildSnapshot Empty { get; } = new();
}
