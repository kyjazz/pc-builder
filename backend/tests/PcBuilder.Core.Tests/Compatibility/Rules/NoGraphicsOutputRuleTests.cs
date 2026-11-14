using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class NoGraphicsOutputRuleTests
{
    private readonly NoGraphicsOutputRule _rule = new();

    [Fact]
    public void NoGpuAndNoIntegratedGraphics_ReturnsR12Warning()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(name: "Core i5-12400F", hasIntegratedGraphics: false)
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R12", issue.RuleCode);
        Assert.Equal(Severity.Warning, issue.Severity);
        Assert.Equal(
            "Core i5-12400F işlemcisinde dahili grafik yok ve ekran kartı seçilmedi. " +
            "Monitörden görüntü alabilmek için bir ekran kartı ekleyin.",
            issue.Message);
    }

    [Fact]
    public void NoGpuButIntegratedGraphics_ReturnsNoIssue()
    {
        var build = new BuildSnapshot { Cpu = TestParts.Cpu(hasIntegratedGraphics: true) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void GpuSelected_ReturnsNoIssue_EvenWithoutIntegratedGraphics()
    {
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(hasIntegratedGraphics: false),
            Gpu = TestParts.Gpu()
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoCpu_RuleIsSkipped()
    {
        Assert.Empty(_rule.Check(BuildSnapshot.Empty));
    }
}
