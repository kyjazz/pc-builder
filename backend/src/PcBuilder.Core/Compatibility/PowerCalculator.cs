namespace PcBuilder.Core.Compatibility;

/// <summary>
/// Rough power budget. Pure functions with no state, so a static class is enough.
/// </summary>
public static class PowerCalculator
{
    /// <summary>Motherboard, RAM, drives and fans are not modelled; this flat amount covers them.</summary>
    public const int BaseSystemWatts = 100;

    /// <summary>20% headroom on top of the estimate.</summary>
    public const decimal HeadroomFactor = 1.2m;

    /// <summary>PSUs are sold in 50 W steps, so the recommendation is rounded up to one.</summary>
    public const int PsuStepWatts = 50;

    /// <summary>CPU TDP + GPU TDP + 100 W. Missing parts count as 0 W.</summary>
    public static int EstimateWattage(BuildSnapshot build)
    {
        ArgumentNullException.ThrowIfNull(build);
        return (build.Cpu?.Tdp ?? 0) + (build.Gpu?.Tdp ?? 0) + BaseSystemWatts;
    }

    /// <summary>
    /// True when a CPU or GPU is selected. Without either, the estimate is just the flat
    /// 100 W and judging a PSU against it would be meaningless, so PSU rules skip.
    /// </summary>
    public static bool HasPowerConsumers(BuildSnapshot build) =>
        build.Cpu is not null || build.Gpu is not null;

    /// <summary>Estimate × 1.2, not rounded. A PSU below this has less than 20% headroom.</summary>
    public static decimal MinimumWattageWithHeadroom(BuildSnapshot build) =>
        EstimateWattage(build) * HeadroomFactor;

    /// <summary>Estimate × 1.2, rounded up to the next multiple of 50 W.</summary>
    public static int RecommendedPsuWattage(BuildSnapshot build) =>
        RoundUpToPsuStep(MinimumWattageWithHeadroom(build));

    // decimal, not double: 1.2 has no exact binary representation, so double results
    // can land a hair above a 50 W boundary and round up one step too far. decimal stores
    // 1.2 exactly, so a 250 W estimate is exactly 300 W, not "approximately" 300 W.
    private static int RoundUpToPsuStep(decimal watts) =>
        (int)(Math.Ceiling(watts / PsuStepWatts) * PsuStepWatts);
}
