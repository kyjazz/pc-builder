namespace PcBuilder.Core.Entities;

/// <summary>A saved PC configuration that belongs to one user.</summary>
public class Build
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BuildItem> Items { get; set; } = new List<BuildItem>();
}
