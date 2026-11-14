namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>
/// R08: the PSU covers the estimate but leaves less than 20% headroom.
/// It only fires between "estimate" and "estimate × 1.2"; below the estimate R07 already
/// reports an Error, and showing a Warning for the same problem would just be noise.
/// </summary>
public class PsuHeadroomRule : ICompatibilityRule
{
    public const string Code = "R08";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Psu is not { } psu || !PowerCalculator.HasPowerConsumers(build))
        {
            yield break;
        }

        var estimated = PowerCalculator.EstimateWattage(build);
        if (psu.Wattage < estimated)
        {
            yield break; // R07's territory.
        }

        if (psu.Wattage >= PowerCalculator.MinimumWattageWithHeadroom(build))
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Warning,
            $"{psu.Name} güç kaynağı ({psu.Wattage} W) tahmini tüketimi ({estimated} W) karşılıyor ama " +
            $"%20'lik güvenlik payı bırakmıyor. Önerilen güç kaynağı: {PowerCalculator.RecommendedPsuWattage(build)} W.");
    }
}
