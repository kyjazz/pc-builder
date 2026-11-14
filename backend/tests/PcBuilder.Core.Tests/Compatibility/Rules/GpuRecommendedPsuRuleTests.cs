using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class GpuRecommendedPsuRuleTests
{
    private readonly GpuRecommendedPsuRule _rule = new();

    [Theory]
    [InlineData(650, 650)] // exactly the recommendation
    [InlineData(650, 850)]
    public void PsuMeetsRecommendation_ReturnsNoIssue(int recommended, int psuWattage)
    {
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(recommendedPsuW: recommended),
            Psu = TestParts.Psu(wattage: psuWattage)
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void PsuBelowRecommendation_ReturnsR09Warning()
    {
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(name: "TUF Gaming GeForce RTX 5080 16GB", recommendedPsuW: 850),
            Psu = TestParts.Psu(name: "RM750e", wattage: 750)
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R09", issue.RuleCode);
        Assert.Equal(Severity.Warning, issue.Severity);
        Assert.Equal(
            "TUF Gaming GeForce RTX 5080 16GB ekran kartı için üretici en az 850 W güç kaynağı öneriyor, " +
            "seçilen RM750e ise 750 W.",
            issue.Message);
    }

    [Fact]
    public void WarnsEvenWhenOurEstimateIsHappy()
    {
        // RTX 4060 (115 W) + 65 W CPU + 100 = 280 W estimate, × 1.2 = 336 W.
        // A 450 W PSU passes R07 and R08, but the card maker asks for 550 W.
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(tdp: 65),
            Gpu = TestParts.Gpu(tdp: 115, recommendedPsuW: 550),
            Psu = TestParts.Psu(wattage: 450)
        };

        Assert.Empty(new PsuCapacityRule().Check(build));
        Assert.Empty(new PsuHeadroomRule().Check(build));
        Assert.Single(_rule.Check(build));
    }

    [Fact]
    public void NoGpu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(), Psu = TestParts.Psu(wattage: 300) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoPsu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Gpu = TestParts.Gpu(recommendedPsuW: 1000) };

        Assert.Empty(_rule.Check(build));
    }
}
