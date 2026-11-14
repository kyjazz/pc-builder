namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>R06: the graphics card must physically fit in the case.</summary>
public class GpuLengthRule : ICompatibilityRule
{
    public const string Code = "R06";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Gpu is not { } gpu || build.Case is not { } pcCase)
        {
            yield break;
        }

        if (gpu.LengthMm <= pcCase.MaxGpuLengthMm)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Error,
            $"{gpu.Name} ekran kartı {gpu.LengthMm} mm uzunluğunda, {pcCase.Name} kasasına ise " +
            $"en fazla {pcCase.MaxGpuLengthMm} mm ekran kartı sığıyor.");
    }
}
