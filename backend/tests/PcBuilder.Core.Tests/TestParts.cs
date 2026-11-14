using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Tests;

/// <summary>
/// Builds parts with sensible defaults so each test only spells out the fields it cares about.
/// Example: TestParts.Cpu(socket: "AM4") instead of setting ten properties.
/// </summary>
internal static class TestParts
{
    public static Cpu Cpu(
        string name = "Test CPU",
        string socket = "AM5",
        int tdp = 65,
        bool hasIntegratedGraphics = false,
        bool includesCooler = false) => new()
    {
        Name = name,
        Brand = "Test",
        Socket = socket,
        Tdp = tdp,
        Cores = 6,
        Threads = 12,
        BaseClockGhz = 3.5,
        BoostClockGhz = 5.0,
        HasIntegratedGraphics = hasIntegratedGraphics,
        IncludesCooler = includesCooler
    };

    public static Motherboard Motherboard(
        string name = "Test Motherboard",
        string socket = "AM5",
        FormFactor formFactor = FormFactor.ATX,
        MemoryType memoryType = MemoryType.DDR5,
        int ramSlots = 4,
        int maxRamGb = 128,
        int m2Slots = 2,
        int sataPorts = 4) => new()
    {
        Name = name,
        Brand = "Test",
        Socket = socket,
        Chipset = "Test",
        FormFactor = formFactor,
        MemoryType = memoryType,
        RamSlots = ramSlots,
        MaxRamGb = maxRamGb,
        M2Slots = m2Slots,
        SataPorts = sataPorts
    };

    public static Gpu Gpu(
        string name = "Test GPU",
        int tdp = 200,
        int lengthMm = 250,
        int recommendedPsuW = 650) => new()
    {
        Name = name,
        Brand = "Test",
        GpuChip = "Test Chip",
        VramGb = 8,
        Tdp = tdp,
        LengthMm = lengthMm,
        RecommendedPsuW = recommendedPsuW
    };
}
