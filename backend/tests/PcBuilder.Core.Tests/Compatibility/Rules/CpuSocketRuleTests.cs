using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class CpuSocketRuleTests
{
    private readonly CpuSocketRule _rule = new();

    [Fact]
    public void SameSocket_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "AM5"),
            Motherboard = TestParts.Motherboard(socket: "AM5")
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void DifferentSocket_ReturnsOneR01Error()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(name: "Ryzen 5 5600", socket: "AM4"),
            Motherboard = TestParts.Motherboard(name: "MAG B650 Tomahawk WiFi", socket: "AM5")
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R01", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        // The message must name both parts and both sockets so the user knows what to change.
        Assert.Contains("Ryzen 5 5600", issue.Message);
        Assert.Contains("MAG B650 Tomahawk WiFi", issue.Message);
        Assert.Contains("AM4", issue.Message);
        Assert.Contains("AM5", issue.Message);
    }

    [Theory]
    [InlineData("LGA1700", "LGA1851")] // Intel 12-14th gen vs Core Ultra 200
    [InlineData("AM5", "AM4")]
    [InlineData("LGA1851", "AM5")]     // Intel vs AMD
    public void AnySocketMismatch_IsError(string cpuSocket, string motherboardSocket)
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: cpuSocket),
            Motherboard = TestParts.Motherboard(socket: motherboardSocket)
        };

        Assert.Equal(Severity.Error, Assert.Single(_rule.Check(build)).Severity);
    }

    [Fact]
    public void SocketComparison_IgnoresCase()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "lga1700"),
            Motherboard = TestParts.Motherboard(socket: "LGA1700")
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(socket: "AM4") };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoCpu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard(socket: "AM5") };

        Assert.Empty(_rule.Check(build));
    }
}
