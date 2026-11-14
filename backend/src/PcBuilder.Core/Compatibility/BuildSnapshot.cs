using PcBuilder.Core.Entities;

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

    /// <summary>A completely empty selection.</summary>
    public static BuildSnapshot Empty { get; } = new();
}
