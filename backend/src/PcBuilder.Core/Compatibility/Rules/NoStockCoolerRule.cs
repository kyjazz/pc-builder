namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R13: the CPU box has no cooler, so the user must buy one separately.</summary>
public class NoStockCoolerRule : ICompatibilityRule
{
    public const string Code = "R13";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Cpu is not { } cpu || cpu.IncludesCooler)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Info,
            $"{cpu.Name} kutusundan soğutucu çıkmıyor. Ayrıca bir işlemci soğutucusu almanız gerekecek.");
    }
}
