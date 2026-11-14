using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

/// <summary>A retail RAM kit, e.g. 2 x 16 GB.</summary>
public class RamKit : Part
{
    public RamKit() : base(PartCategory.Ram) { }

    public MemoryType MemoryType { get; set; }
    public int ModuleCount { get; set; }
    public int ModuleSizeGb { get; set; }
    public int SpeedMhz { get; set; }

    /// <summary>Calculated, not stored (EF Core ignores get-only properties).</summary>
    public int TotalCapacityGb => ModuleCount * ModuleSizeGb;
}
