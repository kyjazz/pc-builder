namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>
/// R09: the PSU is below what the graphics card maker recommends.
/// This is independent of our own estimate: manufacturers account for power spikes
/// that a simple TDP sum doesn't, so their advice is worth showing on its own.
/// </summary>
public class GpuRecommendedPsuRule : ICompatibilityRule
{
    public const string Code = "R09";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Gpu is not { } gpu || build.Psu is not { } psu)
        {
            yield break;
        }

        if (psu.Wattage >= gpu.RecommendedPsuW)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Warning,
            $"{gpu.Name} ekran kartı için üretici en az {gpu.RecommendedPsuW} W güç kaynağı öneriyor, " +
            $"seçilen {psu.Name} ise {psu.Wattage} W.");
    }
}
