using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class RamSlotCountRuleTests
{
    private readonly RamSlotCountRule _rule = new();

    [Fact]
    public void ModulesFewerThanSlots_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(ramSlots: 4),
            RamKits = [TestParts.RamKit(moduleCount: 2)]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void ModulesExactlyFillSlots_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(ramSlots: 4),
            RamKits = [TestParts.RamKit(moduleCount: 2), TestParts.RamKit(moduleCount: 2)]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void FourStickKitOnTwoSlotItxBoard_ReturnsR03Error()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "ROG Strix B650E-I Gaming WiFi", ramSlots: 2),
            RamKits = [TestParts.RamKit(moduleCount: 4)]
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R03", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Contains("4 modül", issue.Message);
        Assert.Contains("2 RAM yuvası", issue.Message);
        Assert.Contains("ROG Strix B650E-I Gaming WiFi", issue.Message);
    }

    [Fact]
    public void SameKitTwice_CountsModulesTwice()
    {
        // Two 2-stick kits = 4 sticks; a 2-slot board can't hold them.
        var kit = TestParts.RamKit(moduleCount: 2);
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(ramSlots: 2),
            RamKits = [kit, kit]
        };

        Assert.Contains("4 modül", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { RamKits = [TestParts.RamKit(moduleCount: 8)] };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoRam_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard(ramSlots: 2) };

        Assert.Empty(_rule.Check(build));
    }
}
