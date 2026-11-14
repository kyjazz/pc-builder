using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

/// <summary>
/// Common fields for every part. Mapped with TPT: this class becomes the Parts table,
/// each subclass gets its own table that shares the same Id.
/// </summary>
public abstract class Part
{
    protected Part(PartCategory category)
    {
        Category = category;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;

    /// <summary>Set by the subclass constructor so it always matches the actual type.</summary>
    public PartCategory Category { get; private set; }

    public decimal Price { get; set; }

    /// <summary>Thermal design power in watts. Used by the power calculator.</summary>
    public int Tdp { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>Parts are never deleted; they are deactivated so old builds stay valid.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
