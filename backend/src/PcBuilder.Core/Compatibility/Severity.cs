namespace PcBuilder.Core.Compatibility;

/// <summary>Ordered from least to most serious, so "Severity >= Warning" reads naturally.</summary>
public enum Severity
{
    /// <summary>Good to know, nothing is wrong (e.g. the CPU box has no cooler).</summary>
    Info,

    /// <summary>The build works but is risky or sub-optimal (e.g. little PSU headroom).</summary>
    Warning,

    /// <summary>The build cannot work (e.g. CPU and motherboard sockets differ).</summary>
    Error
}
