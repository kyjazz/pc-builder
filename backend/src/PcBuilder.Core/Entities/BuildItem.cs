namespace PcBuilder.Core.Entities;

/// <summary>Join row between a Build and a Part. Quantity covers e.g. two RAM kits or three SSDs.</summary>
public class BuildItem
{
    public int Id { get; set; }

    public int BuildId { get; set; }
    public Build Build { get; set; } = null!;

    public int PartId { get; set; }
    public Part Part { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}
