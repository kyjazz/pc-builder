using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Psu : Part
{
    public Psu() : base(PartCategory.Psu) { }

    public int Wattage { get; set; }

    /// <summary>e.g. "80+ Bronze", "80+ Gold". Display-only, so a string is enough.</summary>
    public string EfficiencyRating { get; set; } = string.Empty;
    public bool IsModular { get; set; }
}
