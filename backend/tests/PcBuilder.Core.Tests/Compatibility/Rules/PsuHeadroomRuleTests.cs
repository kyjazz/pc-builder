using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class PsuHeadroomRuleTests
{
    private readonly PsuHeadroomRule _rule = new();

    // CPU 65 W + GPU 200 W + 100 W = 365 W estimate.
    // 365 × 1.2 = 438 W minimum with headroom; recommended = 450 W.
    private static BuildSnapshot BuildWithPsu(int psuWattage) => new()
    {
        Cpu = TestParts.Cpu(tdp: 65),
        Gpu = TestParts.Gpu(tdp: 200),
        Psu = TestParts.Psu(name: "Test PSU", wattage: psuWattage)
    };

    [Theory]
    [InlineData(450)]
    [InlineData(438)] // exactly estimate × 1.2: enough headroom
    public void EnoughHeadroom_ReturnsNoIssue(int wattage)
    {
        Assert.Empty(_rule.Check(BuildWithPsu(wattage)));
    }

    [Theory]
    [InlineData(365)] // exactly the estimate: works, but zero headroom
    [InlineData(400)]
    [InlineData(437)] // one watt short of 438
    public void BetweenEstimateAndHeadroom_ReturnsWarning(int wattage)
    {
        var issue = Assert.Single(_rule.Check(BuildWithPsu(wattage)));

        Assert.Equal("R08", issue.RuleCode);
        Assert.Equal(Severity.Warning, issue.Severity);
    }

    [Fact]
    public void Warning_MessageShowsPsuEstimateAndRecommendation()
    {
        var issue = Assert.Single(_rule.Check(BuildWithPsu(400)));

        Assert.Equal(
            "Test PSU güç kaynağı (400 W) tahmini tüketimi (365 W) karşılıyor ama " +
            "%20'lik güvenlik payı bırakmıyor. Önerilen güç kaynağı: 450 W.",
            issue.Message);
    }

    [Fact]
    public void PsuBelowEstimate_IsLeftToR07()
    {
        // 300 W < 365 W: R07 reports an Error; R08 must stay quiet to avoid a duplicate.
        Assert.Empty(_rule.Check(BuildWithPsu(300)));
    }

    [Fact]
    public void NoPsu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(), Gpu = TestParts.Gpu() };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void PsuWithoutCpuOrGpu_RuleIsSkipped()
    {
        // Estimate would be 100 W, minimum 120 W; a 110 W PSU must not trigger a warning.
        var build = new BuildSnapshot { Psu = TestParts.Psu(wattage: 110) };

        Assert.Empty(_rule.Check(build));
    }
}
