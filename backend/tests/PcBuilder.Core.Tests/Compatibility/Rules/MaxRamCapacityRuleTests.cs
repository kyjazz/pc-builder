using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class MaxRamCapacityRuleTests
{
    private readonly MaxRamCapacityRule _rule = new();

    [Fact]
    public void BelowMax_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(maxRamGb: 128),
            RamKits = [TestParts.RamKit(moduleCount: 2, moduleSizeGb: 16)] // 32 GB
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void ExactlyAtMax_ReturnsNoIssue()
    {
        // Crucial Pro 2x48 GB on a 96 GB Mini-ITX board.
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(maxRamGb: 96, ramSlots: 2),
            RamKits = [TestParts.RamKit(moduleCount: 2, moduleSizeGb: 48)]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void AboveMax_ReturnsR04Error()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "PRO B760M-A WiFi DDR4", maxRamGb: 128),
            RamKits = [TestParts.RamKit(moduleCount: 4, moduleSizeGb: 48)] // 192 GB
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R04", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Contains("192 GB", issue.Message);
        Assert.Contains("128 GB", issue.Message);
        Assert.Contains("PRO B760M-A WiFi DDR4", issue.Message);
    }

    [Fact]
    public void CapacityIsSummedAcrossKits()
    {
        // 2x32 + 2x32 = 128 GB on a 96 GB board.
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(maxRamGb: 96),
            RamKits =
            [
                TestParts.RamKit(moduleCount: 2, moduleSizeGb: 32),
                TestParts.RamKit(moduleCount: 2, moduleSizeGb: 32)
            ]
        };

        Assert.Contains("128 GB", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { RamKits = [TestParts.RamKit(moduleCount: 2, moduleSizeGb: 64)] };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoRam_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard(maxRamGb: 32) };

        Assert.Empty(_rule.Check(build));
    }
}
