using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R05: the case must accept the motherboard's form factor.</summary>
public class CaseFormFactorRule : ICompatibilityRule
{
    public const string Code = "R05";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Case is not { } pcCase || build.Motherboard is not { } motherboard)
        {
            yield break;
        }

        if (pcCase.Supports(motherboard.FormFactor))
        {
            yield break;
        }

        var supported = Enum.GetValues<FormFactor>()
            .Where(pcCase.Supports)
            .Select(DisplayNames.Of)
            .ToList();

        var supportedText = supported.Count == 0
            ? "hiçbir anakart boyutunu desteklemiyor"
            : $"sadece {string.Join(", ", supported)} anakartları destekliyor";

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{motherboard.Name} anakartı {DisplayNames.Of(motherboard.FormFactor)} boyutunda, " +
            $"{pcCase.Name} kasası ise {supportedText}.");
    }
}
