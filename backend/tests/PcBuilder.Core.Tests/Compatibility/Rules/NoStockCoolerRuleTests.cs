using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class NoStockCoolerRuleTests
{
    private readonly NoStockCoolerRule _rule = new();

    [Fact]
    public void CpuWithoutCooler_ReturnsR13Info()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(name: "Ryzen 7 9800X3D", includesCooler: false) };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R13", issue.RuleCode);
        Assert.Equal(Severity.Info, issue.Severity);
        Assert.Equal(
            "Ryzen 7 9800X3D kutusundan soğutucu çıkmıyor. Ayrıca bir işlemci soğutucusu almanız gerekecek.",
            issue.Message);
    }

    [Fact]
    public void CpuWithCooler_ReturnsNoIssue()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(includesCooler: true) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoCpu_RuleIsSkipped()
    {
        Assert.Empty(_rule.Check(BuildSnapshot.Empty));
    }
}
