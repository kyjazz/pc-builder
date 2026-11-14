namespace PcBuilder.Core.Compatibility;

/// <summary>
/// One compatibility check. Each rule is a small, independent class so it can be
/// tested on its own and new rules can be added without touching existing ones.
/// </summary>
public interface ICompatibilityRule
{
    /// <summary>
    /// Returns the problems found, or nothing if the build passes.
    /// If a part the rule needs is not selected yet, the rule returns nothing (skipped).
    /// </summary>
    IEnumerable<CompatibilityIssue> Check(BuildSnapshot build);
}
