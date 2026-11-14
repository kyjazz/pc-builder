using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class MissingRequiredPartsRuleTests
{
    private readonly MissingRequiredPartsRule _rule = new();

    /// <summary>Every required category filled, no GPU.</summary>
    private static BuildSnapshot CompleteBuild() => new()
    {
        Cpu = TestParts.Cpu(),
        Motherboard = TestParts.Motherboard(),
        RamKits = [TestParts.RamKit()],
        Psu = TestParts.Psu(),
        Case = TestParts.Case(),
        Storages = [TestParts.M2Ssd()]
    };

    [Fact]
    public void EmptyBuild_ListsEveryRequiredCategoryInOrder()
    {
        var issue = Assert.Single(_rule.Check(BuildSnapshot.Empty));

        Assert.Equal("R14", issue.RuleCode);
        Assert.Equal(Severity.Info, issue.Severity);
        Assert.Equal(
            "Sistem henüz tamamlanmadı. Eksik parçalar: işlemci, anakart, RAM, güç kaynağı, kasa, depolama.",
            issue.Message);
    }

    [Fact]
    public void CompleteBuildWithoutGpu_ReturnsNoIssue()
    {
        // GPU is optional: integrated graphics may be enough, and R12 covers the case where it isn't.
        Assert.Empty(_rule.Check(CompleteBuild()));
    }

    [Fact]
    public void OnlyPsuMissing_ListsOnlyPsu()
    {
        var build = CompleteBuild() with { Psu = null };

        Assert.Equal(
            "Sistem henüz tamamlanmadı. Eksik parçalar: güç kaynağı.",
            Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void EmptyRamAndStorageLists_CountAsMissing()
    {
        var build = CompleteBuild() with { RamKits = [], Storages = [] };

        Assert.EndsWith("Eksik parçalar: RAM, depolama.", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void GpuIsNeverListedAsMissing()
    {
        Assert.DoesNotContain("ekran kartı", Assert.Single(_rule.Check(BuildSnapshot.Empty)).Message);
    }
}
