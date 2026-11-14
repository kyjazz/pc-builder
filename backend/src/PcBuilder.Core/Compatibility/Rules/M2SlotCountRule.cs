namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R10: the number of M.2 drives must not exceed the motherboard's M.2 slots.</summary>
public class M2SlotCountRule : ICompatibilityRule
{
    public const string Code = "R10";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Motherboard is not { } motherboard)
        {
            yield break;
        }

        var drives = build.M2DriveCount;
        if (drives <= motherboard.M2Slots)
        {
            yield break; // Also covers "no M.2 drive selected" (0 <= slots).
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{drives} adet M.2 depolama seçildi, {motherboard.Name} anakartında ise " +
            $"{motherboard.M2Slots} M.2 yuvası var.");
    }
}
