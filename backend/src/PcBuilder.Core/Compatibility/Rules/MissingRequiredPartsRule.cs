using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Compatibility.Rules;

/// <summary>
/// R14: lists the categories a working PC needs but the build doesn't have yet.
/// The graphics card is not required: CPUs with integrated graphics work without one,
/// and R12 already warns when a GPU really is needed.
/// One Info listing everything missing, rather than one Info per category, keeps the UI short.
/// </summary>
public class MissingRequiredPartsRule : ICompatibilityRule
{
    public const string Code = "R14";

    public IEnumerable<CompatibilityIssue> Check(BuildSnapshot build)
    {
        var missing = MissingCategories(build).Select(DisplayNames.Of).ToList();
        if (missing.Count == 0)
        {
            yield break;
        }

        yield return new CompatibilityIssue(
            Code,
            Severity.Info,
            $"Sistem henüz tamamlanmadı. Eksik parçalar: {string.Join(", ", missing)}.");
    }

    // Order follows the PartCategory enum, so the message order is stable.
    private static IEnumerable<PartCategory> MissingCategories(BuildSnapshot build)
    {
        if (build.Cpu is null) yield return PartCategory.Cpu;
        if (build.Motherboard is null) yield return PartCategory.Motherboard;
        if (build.RamKits.Count == 0) yield return PartCategory.Ram;
        if (build.Psu is null) yield return PartCategory.Psu;
        if (build.Case is null) yield return PartCategory.Case;
        if (build.Storages.Count == 0) yield return PartCategory.Storage;
    }
}
