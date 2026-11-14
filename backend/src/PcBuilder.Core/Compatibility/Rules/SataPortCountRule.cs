namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R11: the number of SATA drives (SSD + HDD) must not exceed the motherboard's SATA ports.</summary>
public class SataPortCountRule : ICompatibilityRule
{
    public const string Code = "R11";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Motherboard is not { } motherboard)
        {
            yield break;
        }

        var drives = build.SataDriveCount;
        if (drives <= motherboard.SataPorts)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{drives} adet SATA depolama seçildi, {motherboard.Name} anakartında ise " +
            $"{motherboard.SataPorts} SATA portu var.");
    }
}
