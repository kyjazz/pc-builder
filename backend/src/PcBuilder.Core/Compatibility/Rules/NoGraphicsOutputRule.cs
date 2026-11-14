namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>
/// R12: no graphics card and the CPU has no integrated graphics, so nothing can drive a monitor.
/// A Warning, not an Error: the build is still a valid PC (e.g. a headless server).
/// </summary>
public class NoGraphicsOutputRule : ICompatibilityRule
{
    public const string Code = "R12";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        if (build.Cpu is not { } cpu || build.Gpu is not null || cpu.HasIntegratedGraphics)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Warning,
            $"{cpu.Name} işlemcisinde dahili grafik yok ve ekran kartı seçilmedi. " +
            "Monitörden görüntü alabilmek için bir ekran kartı ekleyin.");
    }
}
