using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Case : Part
{
    public Case() : base(PartCategory.Case) { }

    public FormFactorSupport SupportedFormFactors { get; set; }
    public int MaxGpuLengthMm { get; set; }
    public int MaxCoolerHeightMm { get; set; }

    /// <summary>Converts the single motherboard form factor to its flag and checks it.</summary>
    public bool Supports(FormFactor formFactor)
    {
        var flag = formFactor switch
        {
            FormFactor.ATX => FormFactorSupport.ATX,
            FormFactor.MicroATX => FormFactorSupport.MicroATX,
            FormFactor.MiniITX => FormFactorSupport.MiniITX,
            _ => FormFactorSupport.None
        };

        return flag != FormFactorSupport.None && SupportedFormFactors.HasFlag(flag);
    }
}
