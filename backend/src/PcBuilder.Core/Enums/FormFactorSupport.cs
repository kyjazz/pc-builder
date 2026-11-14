namespace PcBuilder.Core.Enums;

/// <summary>
/// A case can support several form factors at once, so values are combined as bit flags.
/// Example: ATX | MicroATX | MiniITX = 7.
/// </summary>
[Flags]
public enum FormFactorSupport
{
    None = 0,
    ATX = 1,
    MicroATX = 2,
    MiniITX = 4
}
