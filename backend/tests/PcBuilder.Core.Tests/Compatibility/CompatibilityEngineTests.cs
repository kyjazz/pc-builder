using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;

namespace PcBuilder.Core.Tests.Compatibility;

public class CompatibilityEngineTests
{
    /// <summary>A rule that always reports the issues it was given. Lets us test the engine without real rules.</summary>
    private sealed class FixedRule(params CompatibilityIssue[] issues) : ICompatibilityRule
    {
        public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build) => issues;
    }

    private static CompatibilityIssue Issue(string code, Severity severity) => new(code, severity, $"{code} message");

    [Fact]
    public void NoRules_IsCompatibleWithNoIssues()
    {
        var result = new CompatibilityEngine([]).Check(BuildSnapshot.Empty);

        Assert.Empty(result.Issues);
        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void CollectsIssuesFromEveryRule()
    {
        var engine = new CompatibilityEngine(
        [
            new FixedRule(Issue("R01", Severity.Error)),
            new FixedRule(),
            new FixedRule(Issue("R12", Severity.Warning), Issue("R13", Severity.Info))
        ]);

        Assert.Equal(3, engine.Check(BuildSnapshot.Empty).Issues.Count);
    }

    [Fact]
    public void SortsBySeverityDescendingThenByRuleCode()
    {
        var engine = new CompatibilityEngine(
        [
            new FixedRule(Issue("R13", Severity.Info)),
            new FixedRule(Issue("R08", Severity.Warning)),
            new FixedRule(Issue("R06", Severity.Error)),
            new FixedRule(Issue("R01", Severity.Error))
        ]);

        var codes = engine.Check(BuildSnapshot.Empty).Issues.Select(i => i.RuleCode);

        Assert.Equal(["R01", "R06", "R08", "R13"], codes);
    }

    [Fact]
    public void WarningsAndInfos_DoNotMakeBuildIncompatible()
    {
        var engine = new CompatibilityEngine(
        [
            new FixedRule(Issue("R08", Severity.Warning), Issue("R13", Severity.Info))
        ]);

        Assert.True(engine.Check(BuildSnapshot.Empty).IsCompatible);
    }

    [Fact]
    public void AnyError_MakesBuildIncompatible()
    {
        var engine = new CompatibilityEngine(
        [
            new FixedRule(Issue("R13", Severity.Info)),
            new FixedRule(Issue("R01", Severity.Error))
        ]);

        Assert.False(engine.Check(BuildSnapshot.Empty).IsCompatible);
    }

    [Fact]
    public void WithRealRule_SocketMismatchIsReported()
    {
        var engine = new CompatibilityEngine([new CpuSocketRule()]);
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "LGA1700"),
            Motherboard = TestParts.Motherboard(socket: "AM5")
        };

        var result = engine.Check(build);

        Assert.False(result.IsCompatible);
        Assert.Equal("R01", Assert.Single(result.Issues).RuleCode);
    }

    [Fact]
    public void WithRealRules_ReportsEveryBrokenRuleInCodeOrder()
    {
        var engine = new CompatibilityEngine(
        [
            // Registered out of order on purpose: the engine must sort, not rely on registration order.
            new CaseFormFactorRule(),
            new MaxRamCapacityRule(),
            new RamSlotCountRule(),
            new RamMemoryTypeRule(),
            new CpuSocketRule()
        ]);

        // AM5 CPU on an AM5 Mini-ITX board (R01 ok), but:
        // DDR4 kit on DDR5 board (R02), 4 sticks in 2 slots (R03), 128 GB > 96 GB (R04),
        // Mini-ITX board is fine in this case, so R05 must NOT fire.
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "AM5"),
            Motherboard = TestParts.Motherboard(
                socket: "AM5",
                formFactor: Enums.FormFactor.MiniITX,
                memoryType: Enums.MemoryType.DDR5,
                ramSlots: 2,
                maxRamGb: 96),
            RamKits = [TestParts.RamKit(memoryType: Enums.MemoryType.DDR4, moduleCount: 4, moduleSizeGb: 32)],
            Case = TestParts.Case(supportedFormFactors: Enums.FormFactorSupport.MiniITX)
        };

        var result = engine.Check(build);

        Assert.False(result.IsCompatible);
        Assert.Equal(["R02", "R03", "R04"], result.Issues.Select(i => i.RuleCode));
    }
}
