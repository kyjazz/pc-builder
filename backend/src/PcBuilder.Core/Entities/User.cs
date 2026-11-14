namespace PcBuilder.Core.Entities;

public class User
{
    public int Id { get; set; }

    /// <summary>Unique index is configured in AppDbContext.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Produced by PasswordHasher&lt;User&gt;. The plain password is never stored.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Build> Builds { get; set; } = new List<Build>();
}
