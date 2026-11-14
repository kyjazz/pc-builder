using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class M2SlotCountRuleTests
{
    private readonly M2SlotCountRule _rule = new();

    [Fact]
    public void M2DrivesExactlyFillSlots_ReturnsNoIssue()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(m2Slots: 2),
            Storages = [TestParts.M2Ssd(), TestParts.M2Ssd()]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void TooManyM2Drives_ReturnsR10Error()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "B550M DS3H", m2Slots: 2),
            Storages = [TestParts.M2Ssd(), TestParts.M2Ssd(), TestParts.M2Ssd()]
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R10", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Equal("3 adet M.2 depolama seçildi, B550M DS3H anakartında ise 2 M.2 yuvası var.", issue.Message);
    }

    [Fact]
    public void SataDrives_DoNotUseM2Slots()
    {
        // One M.2 slot, one NVMe + two SATA drives: M.2 count is 1, fine.
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(m2Slots: 1),
            Storages = [TestParts.M2Ssd(), TestParts.SataSsd(), TestParts.Hdd()]
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void SameDriveListedThreeTimes_CountsThree()
    {
        var ssd = TestParts.M2Ssd();
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(m2Slots: 2),
            Storages = [ssd, ssd, ssd]
        };

        Assert.StartsWith("3 adet", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Storages = [TestParts.M2Ssd(), TestParts.M2Ssd(), TestParts.M2Ssd()] };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoStorage_ReturnsNoIssue()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard(m2Slots: 0) };

        Assert.Empty(_rule.Check(build));
    }
}
