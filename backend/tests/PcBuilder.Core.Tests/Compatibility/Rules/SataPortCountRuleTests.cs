using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class SataPortCountRuleTests
{
    private readonly SataPortCountRule _rule = new();

    [Fact]
    public void SataDrivesExactlyFillPorts_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(sataPorts: 2),
            Storages = [TestParts.SataSsd(), TestParts.Hdd()]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void TooManySataDrives_ReturnsR11Error()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "ROG Strix B650E-I Gaming WiFi", sataPorts: 2),
            Storages = [TestParts.SataSsd(), TestParts.Hdd(), TestParts.Hdd()]
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R11", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Equal(
            "3 adet SATA depolama seçildi, ROG Strix B650E-I Gaming WiFi anakartında ise 2 SATA portu var.",
            issue.Message);
    }

    [Fact]
    public void HddAndSataSsd_BothCountAsSata()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(sataPorts: 1),
            Storages = [TestParts.SataSsd(), TestParts.Hdd()]
        };

        Assert.StartsWith("2 adet", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void M2Drives_DoNotUseSataPorts()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(sataPorts: 0),
            Storages = [TestParts.M2Ssd(), TestParts.M2Ssd()]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Storages = [TestParts.Hdd(), TestParts.Hdd(), TestParts.Hdd()] };

        Assert.Empty(_rule.Check(build));
    }
}
