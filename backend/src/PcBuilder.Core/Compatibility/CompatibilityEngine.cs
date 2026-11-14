using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

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

    /// <summary>
    /// Keeps the candidates that can be added to the selection without causing a new Error.
    /// <list type="bullet">
    /// <item>Single-slot categories: the candidate replaces the currently selected part.</item>
    /// <item>RAM and storage: the candidate is added next to the selected ones.</item>
    /// <item>Errors the selection already had (from other parts) don't count against a candidate;
    /// otherwise one unrelated mistake would empty every list.</item>
    /// <item>Warnings and infos never exclude a candidate.</item>
    /// </list>
    /// </summary>
    public IReadOnlyList<Part> FilterCompatible(
        BuildSnapshot selection, PartCategory category, IEnumerable<Part> candidates)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(candidates);

        var baseline = selection.Without(category);
        var existingErrors = ErrorRuleCodes(Check(baseline));

        return candidates
            .Where(candidate =>
            {
                if (candidate.Category != category)
                {
                    throw new ArgumentException(
                        $"Candidate {candidate.Name} is {candidate.Category}, expected {category}.", nameof(candidates));
                }

                return ErrorRuleCodes(Check(baseline.With(candidate))).IsSubsetOf(existingErrors);
            })
            .ToList();
    }

    private static HashSet<string> ErrorRuleCodes(CompatibilityResult result) =>
        result.Issues
            .Where(issue => issue.Severity == Severity.Error)
            .Select(issue => issue.RuleCode)
            .ToHashSet();
}
