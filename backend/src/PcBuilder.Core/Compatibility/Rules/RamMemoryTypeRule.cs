namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R02: every RAM kit must match the motherboard's memory type (DDR4 vs DDR5).</summary>
public class RamMemoryTypeRule : ICompatibilityRule
{
    public const string Code = "R02";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Motherboard is not { } motherboard || build.RamKits.Count == 0)
        {
            yield break;
        }

        // Distinct: two identical kits are the same object listed twice; report the problem once.
        var mismatchedKits = build.RamKits
            .Where(kit => kit.MemoryType != motherboard.MemoryType)
            .Distinct();

        foreach (var kit in mismatchedKits)
        {
            yield return new CompatibilityIssue(
                Code,
                Severity.Error,
                $"{kit.Name} {kit.MemoryType} bellek, {motherboard.Name} anakartı ise sadece " +
                $"{motherboard.MemoryType} destekliyor. DDR4 ve DDR5 birbirinin yuvasına takılamaz.");
        }
    }
}
