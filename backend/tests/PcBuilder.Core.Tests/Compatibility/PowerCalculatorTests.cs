using PcBuilder.Core.Compatibility;

namespace PcBuilder.Core.Tests.Compatibility;

public class PowerCalculatorTests
{
    [Fact]
    public void EstimateWattage_AddsCpuGpuAndBaseSystem()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(tdp: 120),
            Gpu = TestParts.Gpu(tdp: 300)
        };

        Assert.Equal(120 + 300 + 100, PowerCalculator.EstimateWattage(build));
    }

    [Fact]
    public void EstimateWattage_MissingCpuAndGpu_CountAsZero()
    {
        Assert.Equal(100, PowerCalculator.EstimateWattage(BuildSnapshot.Empty));
    }

    [Fact]
    public void EstimateWattage_CpuOnly_IgnoresMissingGpu()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(tdp: 65) };

        Assert.Equal(165, PowerCalculator.EstimateWattage(build));
    }

    [Theory]
    // cpu + gpu + 100 = estimate -> × 1.2 -> rounded up to 50
    [InlineData(65, 85, 300)]    // 250 × 1.2 = 300.0  -> exactly on a step, must NOT round up to 350
    [InlineData(65, 125, 350)]   // 290 × 1.2 = 348.0  -> 350
    [InlineData(65, 136, 400)]   // 301 × 1.2 = 361.2  -> 400
    [InlineData(120, 575, 1000)] // 795 × 1.2 = 954.0  -> 1000 (9800X3D + RTX 5090)
    public void RecommendedPsuWattage_AddsHeadroomAndRoundsUpTo50(int cpuTdp, int gpuTdp, int expected)
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(tdp: cpuTdp),
            Gpu = TestParts.Gpu(tdp: gpuTdp)
        };

        Assert.Equal(expected, PowerCalculator.RecommendedPsuWattage(build));
    }
}
