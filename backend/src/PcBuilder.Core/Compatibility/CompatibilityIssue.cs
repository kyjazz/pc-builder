namespace PcBuilder.Core.Compatibility;

/// <summary>One problem found by a rule. Message is Turkish and shown to the user as-is.</summary>
public record CompatibilityIssue(string RuleCode, Severity Severity, string Message);
