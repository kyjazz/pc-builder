using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Compatibility;

/// <summary>Human-readable names used inside Turkish rule messages.</summary>
internal static class DisplayNames
{
    public static string Of(FormFactor formFactor) => formFactor switch
    {
        FormFactor.ATX => "ATX",
        FormFactor.MicroATX => "Micro-ATX",
        FormFactor.MiniITX => "Mini-ITX",
        _ => formFactor.ToString()
    };
}
