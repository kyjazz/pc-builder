using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class PsuCapacityRuleTests
{
    private readonly PsuCapacityRule _rule = new();

    // CPU 65 W + GPU 200 W + 100 W = 365 W estimate; recommended = 438 -> 450 W.
    private static BuildSnapshot BuildWithPsu(int psuWattage) => new()
    {
        Cpu = TestParts.Cpu(tdp: 65),
        Gpu = TestParts.Gpu(tdp: 200),
        Psu = TestParts.Psu(name: "Test PSU", wattage: psuWattage)
    };

    [Theory]
    [InlineData(450)]
    [InlineData(365)] // exactly the estimate: enough, R08 will warn about headroom instead
    public void PsuCoversEstimate_ReturnsNoIssue(int wattage)
    {
        Assert.Empty(_rule.Check(BuildWithPsu(wattage)));
    }

    [Fact]
    public void PsuBelowEstimate_ReturnsR07ErrorWithNumbers()
    {
        var issue = Assert.Single(_rule.Check(BuildWithPsu(300)));

        Assert.Equal("R07", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Equal(
            "Test PSU güç kaynağı 300 W, sistemin tahmini tüketimi ise 365 W. " +
            "En az 450 W'lık bir güç kaynağı seçin.",
            issue.Message);
    }

    [Fact]
    public void CpuOnly_EstimateUsesCpu()
    {
        // 125 + 100 = 225 W > 200 W
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(tdp: 125),
            Psu = TestParts.Psu(wattage: 200)
        };

        Assert.Single(_rule.Check(build));
    }

    [Fact]
    public void GpuOnly_EstimateUsesGpu()
    {
        // RTX 5090: 575 + 100 = 675 W > 650 W
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(tdp: 575),
            Psu = TestParts.Psu(wattage: 650)
        };

        Assert.Single(_rule.Check(build));
    }

    [Fact]
    public void NoPsu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(tdp: 170), Gpu = TestParts.Gpu(tdp: 575) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void PsuWithoutCpuOrGpu_RuleIsSkipped()
    {
        // Nothing draws power yet (estimate 0 W), so there is nothing to judge this PSU against.
        var build = new BuildSnapshot { Psu = TestParts.Psu(wattage: 50) };

        Assert.Empty(_rule.Check(build));
    }
}
