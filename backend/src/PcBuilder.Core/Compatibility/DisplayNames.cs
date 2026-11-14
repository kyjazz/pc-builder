using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Compatibility;

/// <summary>Human-readable Turkish names used in rule messages and API validation messages.</summary>
public static class DisplayNames
{
    public static string Of(FormFactor formFactor) => formFactor switch
    {
        FormFactor.ATX => "ATX",
        FormFactor.MicroATX => "Micro-ATX",
        FormFactor.MiniITX => "Mini-ITX",
        _ => formFactor.ToString()
    };

    public static string Of(PartCategory category) => category switch
    {
        PartCategory.Cpu => "işlemci",
        PartCategory.Motherboard => "anakart",
        PartCategory.Ram => "RAM",
        PartCategory.Gpu => "ekran kartı",
        PartCategory.Psu => "güç kaynağı",
        PartCategory.Case => "kasa",
        PartCategory.Storage => "depolama",
        _ => category.ToString()
    };
}
