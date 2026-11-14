using System.Reflection;
using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Tests.Compatibility;

/// <summary>Runs all 14 rules together, the way the API will.</summary>
public class AllRulesTests
{
    private static readonly ICompatibilityRule[] AllRules =
    [
        new CpuSocketRule(),
        new RamMemoryTypeRule(),
        new RamSlotCountRule(),
        new MaxRamCapacityRule(),
        new CaseFormFactorRule(),
        new GpuLengthRule(),
        new PsuCapacityRule(),
        new PsuHeadroomRule(),
        new GpuRecommendedPsuRule(),
        new M2SlotCountRule(),
        new SataPortCountRule(),
        new NoGraphicsOutputRule(),
        new NoStockCoolerRule(),
        new MissingRequiredPartsRule()
    ];

    private static readonly CompatibilityEngine Engine = new(AllRules);

    [Fact]
    public void CoreAssembly_ContainsExactlyRulesR01ToR14()
    {
        // Guards against adding a rule class but forgetting a code, or reusing one.
        var codes = typeof(ICompatibilityRule).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICompatibilityRule).IsAssignableFrom(t))
            .Select(t => (string)t.GetField("Code", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!)
            .Order()
            .ToList();

        Assert.Equal(Enumerable.Range(1, 14).Select(n => $"R{n:00}"), codes);
    }

    [Fact]
    public void TestListCoversEveryRuleInCore()
    {
        var inCore = typeof(ICompatibilityRule).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICompatibilityRule).IsAssignableFrom(t))
            .ToHashSet();

        Assert.Equal(inCore, AllRules.Select(r => r.GetType()).ToHashSet());
    }

    [Fact]
    public void EmptyBuild_OnlyReportsMissingParts()
    {
        var result = Engine.Check(BuildSnapshot.Empty);

        Assert.True(result.IsCompatible);
        Assert.Equal("R14", Assert.Single(result.Issues).RuleCode);
    }

    [Fact]
    public void RealisticValidBuild_IsCompatibleWithOnlyCoolerInfo()
    {
        // Ryzen 7 9700X + MAG B650 Tomahawk WiFi + 32 GB DDR5 + RTX 4070 Super
        // + RM750e + 4000D Airflow + 990 Pro.
        // Power: 65 + 220 + 100 = 385 W, × 1.2 = 462 W; 750 W PSU and GPU wants 650 W: fine.
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(name: "Ryzen 7 9700X", socket: "AM5", tdp: 65,
                hasIntegratedGraphics: true, includesCooler: false),
            Motherboard = TestParts.Motherboard(socket: "AM5", formFactor: FormFactor.ATX,
                memoryType: MemoryType.DDR5, ramSlots: 4, maxRamGb: 192, m2Slots: 3, sataPorts: 6),
            RamKits = [TestParts.RamKit(memoryType: MemoryType.DDR5, moduleCount: 2, moduleSizeGb: 16)],
            Gpu = TestParts.Gpu(tdp: 220, lengthMm: 261, recommendedPsuW: 650),
            Psu = TestParts.Psu(wattage: 750),
            Case = TestParts.Case(maxGpuLengthMm: 360),
            Storages = [TestParts.M2Ssd()]
        };

        var result = Engine.Check(build);

        Assert.True(result.IsCompatible);
        var issue = Assert.Single(result.Issues);
        Assert.Equal("R13", issue.RuleCode);
        Assert.Equal(Severity.Info, issue.Severity);
    }

    [Fact]
    public void BrokenBuild_ReportsErrorsThenWarningsThenInfos()
    {
        // Core i5-12400F (LGA1700, no iGPU, has cooler) on an AM5 Mini-ITX board,
        // a DDR4 kit, 3 NVMe drives in 2 M.2 slots, and no GPU, PSU or case.
        // Expected:
        //   R01 socket (Error), R02 DDR4 on DDR5 (Error), R10 3 > 2 M.2 (Error),
        //   R12 no graphics output (Warning),
        //   R14 PSU and case missing (Info).
        var build = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "LGA1700", hasIntegratedGraphics: false, includesCooler: true),
            Motherboard = TestParts.Motherboard(socket: "AM5", formFactor: FormFactor.MiniITX,
                memoryType: MemoryType.DDR5, ramSlots: 2, maxRamGb: 96, m2Slots: 2, sataPorts: 2),
            RamKits = [TestParts.RamKit(memoryType: MemoryType.DDR4, moduleCount: 2, moduleSizeGb: 16)],
            Storages = [TestParts.M2Ssd(), TestParts.M2Ssd(), TestParts.M2Ssd()]
        };

        var result = Engine.Check(build);

        Assert.False(result.IsCompatible);
        Assert.Equal(["R01", "R02", "R10", "R12", "R14"], result.Issues.Select(i => i.RuleCode));
        Assert.Contains("güç kaynağı, kasa", result.Issues[^1].Message);
    }
}
