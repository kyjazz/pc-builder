using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class RamMemoryTypeRuleTests
{
    private readonly RamMemoryTypeRule _rule = new();

    [Theory]
    [InlineData(MemoryType.DDR4)]
    [InlineData(MemoryType.DDR5)]
    public void SameMemoryType_ReturnsNoIssue(MemoryType type)
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(memoryType: type),
            RamKits = [TestParts.RamKit(memoryType: type)]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void Ddr4KitOnDdr5Board_ReturnsR02Error()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "MAG B650 Tomahawk WiFi", memoryType: MemoryType.DDR5),
            RamKits = [TestParts.RamKit(name: "Vengeance LPX 32GB", memoryType: MemoryType.DDR4)]
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R02", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Contains("Vengeance LPX 32GB", issue.Message);
        Assert.Contains("MAG B650 Tomahawk WiFi", issue.Message);
        Assert.Contains("DDR4", issue.Message);
        Assert.Contains("DDR5", issue.Message);
    }

    [Fact]
    public void Ddr5KitOnDdr4Board_ReturnsError()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(memoryType: MemoryType.DDR4),
            RamKits = [TestParts.RamKit(memoryType: MemoryType.DDR5)]
        };

        Assert.Equal(Severity.Error, Assert.Single(_rule.Check(build)).Severity);
    }

    [Fact]
    public void MixedKits_OnlyTheWrongOneIsReported()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(memoryType: MemoryType.DDR5),
            RamKits =
            [
                TestParts.RamKit(name: "Good DDR5 Kit", memoryType: MemoryType.DDR5),
                TestParts.RamKit(name: "Wrong DDR4 Kit", memoryType: MemoryType.DDR4)
            ]
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Contains("Wrong DDR4 Kit", issue.Message);
        Assert.DoesNotContain("Good DDR5 Kit", issue.Message);
    }

    [Fact]
    public void SameWrongKitTwice_IsReportedOnce()
    {
        var kit = TestParts.RamKit(memoryType: MemoryType.DDR4);
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(memoryType: MemoryType.DDR5),
            RamKits = [kit, kit]
        };

        Assert.Single(_rule.Check(build));
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { RamKits = [TestParts.RamKit(memoryType: MemoryType.DDR4)] };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoRam_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard() };

        Assert.Empty(_rule.Check(build));
    }
}
