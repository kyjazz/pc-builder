namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R01: the CPU must fit the motherboard socket.</summary>
public class CpuSocketRule : ICompatibilityRule
{
    public const string Code = "R01";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Cpu is not { } cpu || build.Motherboard is not { } motherboard)
        {
            yield break; // Needs both parts; skipped until they are selected.
        }

        if (string.Equals(cpu.Socket, motherboard.Socket, StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{cpu.Name} işlemcisi {cpu.Socket} soketli, {motherboard.Name} anakartı ise {motherboard.Socket} soketli. " +
            "İşlemci bu anakarta takılamaz.");
    }
}
