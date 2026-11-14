namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R03: the total number of RAM sticks must fit in the motherboard's slots.</summary>
public class RamSlotCountRule : ICompatibilityRule
{
    public const string Code = "R03";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Motherboard is not { } motherboard || build.RamKits.Count == 0)
        {
            yield break;
        }

        var modules = build.TotalRamModules;
        if (modules <= motherboard.RamSlots)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"Seçilen RAM'ler toplam {modules} modül, {motherboard.Name} anakartında ise " +
            $"{motherboard.RamSlots} RAM yuvası var.");
    }
}
