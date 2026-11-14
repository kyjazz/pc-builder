using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Motherboard : Part
{
    public Motherboard() : base(PartCategory.Motherboard) { }

    public string Socket { get; set; } = string.Empty;
    public string Chipset { get; set; } = string.Empty;
    public FormFactor FormFactor { get; set; }
    public MemoryType MemoryType { get; set; }
    public int RamSlots { get; set; }
    public int MaxRamGb { get; set; }
    public int M2Slots { get; set; }
    public int SataPorts { get; set; }
}
