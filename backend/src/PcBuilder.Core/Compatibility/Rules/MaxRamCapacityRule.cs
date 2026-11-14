namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R04: total RAM capacity must not exceed what the motherboard supports.</summary>
public class MaxRamCapacityRule : ICompatibilityRule
{
    public const string Code = "R04";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Motherboard is not { } motherboard || build.RamKits.Count == 0)
        {
            yield break;
        }

        var totalGb = build.TotalRamGb;
        if (totalGb <= motherboard.MaxRamGb)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"Seçilen RAM'ler toplam {totalGb} GB, {motherboard.Name} anakartı ise en fazla " +
            $"{motherboard.MaxRamGb} GB destekliyor.");
    }
}
