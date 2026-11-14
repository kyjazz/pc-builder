namespace PcBuilder.Core.Compatibility;

/// <summary>Outcome of running every rule against a build.</summary>
public record CompatibilityResult(IReadOnlyList<CompatibilityIssue> Issues)
{
    /// <summary>Warnings and infos don't block a build; only errors do.</summary>
    public bool IsCompatible => Issues.All(i => i.Severity != Severity.Error);
}

/// <summary>
/// Runs all registered rules and returns their issues, most serious first.
/// The engine itself knows no rule by name: the API registers rules in DI and they
/// arrive here as IEnumerable&lt;ICompatibilityRule&gt;. Adding a rule = adding a class.
/// </summary>
public class CompatibilityEngine(IEnumerable<ICompatibilityRule> rules)
{
    private readonly IReadOnlyList<ICompatibilityRule> _rules = rules.ToList();

    public CompatibilityResult Check(BuildSnapshot build)
    {
        ArgumentNullException.ThrowIfNull(build);

        var issues = _rules
            .SelectMany(rule => rule.Check(build))
            .OrderByDescending(issue => issue.Severity)
            // Same severity: R01 before R02, so the order is predictable for the UI and tests.
            .ThenBy(issue => issue.RuleCode, StringComparer.Ordinal)
            .ToList();

        return new CompatibilityResult(issues);
    }
}
