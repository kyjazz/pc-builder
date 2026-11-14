using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Gpu : Part
{
    public Gpu() : base(PartCategory.Gpu) { }

    /// <summary>e.g. "GeForce RTX 4070". Brand holds the board maker (ASUS, MSI...).</summary>
    public string GpuChip { get; set; } = string.Empty;
    public int VramGb { get; set; }
    public int LengthMm { get; set; }
    public int RecommendedPsuW { get; set; }
}
