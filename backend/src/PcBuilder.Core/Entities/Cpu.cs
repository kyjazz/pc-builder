using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Cpu : Part
{
    public Cpu() : base(PartCategory.Cpu) { }

    /// <summary>AM4, AM5, LGA1700, LGA1851.</summary>
    public string Socket { get; set; } = string.Empty;
    public int Cores { get; set; }
    public int Threads { get; set; }
    public double BaseClockGhz { get; set; }
    public double BoostClockGhz { get; set; }
    public bool HasIntegratedGraphics { get; set; }
    public bool IncludesCooler { get; set; }
}
