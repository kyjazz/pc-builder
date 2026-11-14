using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class GpuLengthRuleTests
{
    private readonly GpuLengthRule _rule = new();

    [Theory]
    [InlineData(250, 360)]
    [InlineData(360, 360)] // exactly the limit still fits
    public void GpuFits_ReturnsNoIssue(int gpuLength, int caseMax)
    {
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(lengthMm: gpuLength),
            Case = TestParts.Case(maxGpuLengthMm: caseMax)
        };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void GpuTooLong_ReturnsR06Error()
    {
        // ROG Astral RTX 5090 (358 mm) in a Lian Li A4-H2O (322 mm).
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(name: "ROG Astral GeForce RTX 5090 32GB", lengthMm: 358),
            Case = TestParts.Case(name: "A4-H2O", maxGpuLengthMm: 322)
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R06", issue.RuleCode);
        Assert.Equal(Severity.Error, issue.Severity);
        Assert.Equal(
            "ROG Astral GeForce RTX 5090 32GB ekran kartı 358 mm uzunluğunda, A4-H2O kasasına ise " +
            "en fazla 322 mm ekran kartı sığıyor.",
            issue.Message);
    }

    [Fact]
    public void OneMillimetreTooLong_IsStillError()
    {
        var build = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(lengthMm: 361),
            Case = TestParts.Case(maxGpuLengthMm: 360)
        };

        Assert.Single(_rule.Check(build));
    }

    [Fact]
    public void NoCase_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Gpu = TestParts.Gpu(lengthMm: 400) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoGpu_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Case = TestParts.Case(maxGpuLengthMm: 200) };

        Assert.Empty(_rule.Check(build));
    }
}
