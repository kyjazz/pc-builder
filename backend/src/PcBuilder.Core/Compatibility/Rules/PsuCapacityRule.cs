namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R07: the PSU must deliver at least the estimated system power draw.</summary>
public class PsuCapacityRule : ICompatibilityRule
{
    public const string Code = "R07";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Psu is not { } psu || !PowerCalculator.HasPowerConsumers(build))
        {
            yield break;
        }

        var estimated = PowerCalculator.EstimateWattage(build);
        if (psu.Wattage >= estimated)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{psu.Name} güç kaynağı {psu.Wattage} W, sistemin tahmini tüketimi ise {estimated} W. " +
            $"En az {PowerCalculator.RecommendedPsuWattage(build)} W'lık bir güç kaynağı seçin.");
    }
}
