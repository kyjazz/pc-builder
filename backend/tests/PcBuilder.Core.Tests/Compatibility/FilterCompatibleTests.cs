using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;
using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Tests.Compatibility;

public class FilterCompatibleTests
{
    private static readonly CompatibilityEngine Engine = new(
    [
        new CpuSocketRule(), new RamMemoryTypeRule(), new RamSlotCountRule(), new MaxRamCapacityRule(),
        new CaseFormFactorRule(), new GpuLengthRule(), new PsuCapacityRule(), new PsuHeadroomRule(),
        new GpuRecommendedPsuRule(), new M2SlotCountRule(), new SataPortCountRule(),
        new NoGraphicsOutputRule(), new NoStockCoolerRule(), new MissingRequiredPartsRule()
    ]);

    private static IEnumerable<string> Names(IEnumerable<Part> parts) => parts.Select(p => p.Name);

    [Fact]
    public void EmptySelection_EveryCandidateIsCompatible()
    {
        Cpu[] cpus = [TestParts.Cpu(name: "A", socket: "AM4"), TestParts.Cpu(name: "B", socket: "LGA1851")];

        var result = Engine.FilterCompatible(BuildSnapshot.Empty, PartCategory.Cpu, cpus);

        Assert.Equal(["A", "B"], Names(result));
    }

    [Fact]
    public void Cpu_OnlySocketMatchesAreKept()
    {
        var selection = new BuildSnapshot { Motherboard = TestParts.Motherboard(socket: "AM5") };
        Cpu[] cpus =
        [
            TestParts.Cpu(name: "Ryzen 5 5600", socket: "AM4"),
            TestParts.Cpu(name: "Ryzen 7 9700X", socket: "AM5"),
            TestParts.Cpu(name: "Core i5-14600K", socket: "LGA1700")
        ];

        var result = Engine.FilterCompatible(selection, PartCategory.Cpu, cpus);

        Assert.Equal(["Ryzen 7 9700X"], Names(result));
    }

    [Fact]
    public void SingleSlotCategory_CurrentPartIsReplacedNotKept()
    {
        // The selected AM4 CPU clashes with the AM5 board (R01), but while browsing CPUs
        // it is being replaced, so AM5 candidates must still be offered.
        var selection = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(name: "Selected AM4", socket: "AM4"),
            Motherboard = TestParts.Motherboard(socket: "AM5")
        };
        Cpu[] cpus = [TestParts.Cpu(name: "AM5 Candidate", socket: "AM5")];

        Assert.Equal(["AM5 Candidate"], Names(Engine.FilterCompatible(selection, PartCategory.Cpu, cpus)));
    }

    [Fact]
    public void Gpu_OnlyCardsThatFitTheCaseAreKept()
    {
        var selection = new BuildSnapshot { Case = TestParts.Case(maxGpuLengthMm: 330) };
        Gpu[] gpus =
        [
            TestParts.Gpu(name: "Short", lengthMm: 300),
            TestParts.Gpu(name: "Exact", lengthMm: 330),
            TestParts.Gpu(name: "Long", lengthMm: 358)
        ];

        var result = Engine.FilterCompatible(selection, PartCategory.Gpu, gpus);

        Assert.Equal(["Short", "Exact"], Names(result));
    }

    [Fact]
    public void ExistingUnrelatedError_DoesNotEmptyTheList()
    {
        // Selection already has R01 (socket). Browsing GPUs must still show the ones that fit the case.
        var selection = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(socket: "AM4"),
            Motherboard = TestParts.Motherboard(socket: "AM5"),
            Case = TestParts.Case(maxGpuLengthMm: 330)
        };
        Gpu[] gpus = [TestParts.Gpu(name: "Fits", lengthMm: 300), TestParts.Gpu(name: "Too long", lengthMm: 358)];

        Assert.Equal(["Fits"], Names(Engine.FilterCompatible(selection, PartCategory.Gpu, gpus)));
    }

    [Fact]
    public void Psu_ErrorExcludes_WarningDoesNot()
    {
        // 65 + 200 + 100 = 365 W estimate, × 1.2 = 438 W; GPU maker asks for 400 W.
        var selection = new BuildSnapshot
        {
            Cpu = TestParts.Cpu(tdp: 65),
            Gpu = TestParts.Gpu(tdp: 200, recommendedPsuW: 400)
        };
        Psu[] psus =
        [
            TestParts.Psu(name: "300 W", wattage: 300), // R07 Error   -> excluded
            TestParts.Psu(name: "400 W", wattage: 400), // R08 Warning -> kept
            TestParts.Psu(name: "550 W", wattage: 550)  // fine        -> kept
        ];

        var result = Engine.FilterCompatible(selection, PartCategory.Psu, psus);

        Assert.Equal(["400 W", "550 W"], Names(result));
    }

    [Fact]
    public void Ram_CandidateIsAddedNextToSelectedKits()
    {
        // 2-slot board already holds a 2-stick kit; any further kit would need more slots (R03).
        var selection = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(ramSlots: 2, memoryType: MemoryType.DDR5),
            RamKits = [TestParts.RamKit(moduleCount: 2)]
        };
        RamKit[] kits = [TestParts.RamKit(name: "Another 2x16", moduleCount: 2)];

        Assert.Empty(Engine.FilterCompatible(selection, PartCategory.Ram, kits));
    }

    [Fact]
    public void Ram_WrongMemoryTypeIsExcluded()
    {
        var selection = new BuildSnapshot { Motherboard = TestParts.Motherboard(memoryType: MemoryType.DDR4) };
        RamKit[] kits =
        [
            TestParts.RamKit(name: "DDR4 kit", memoryType: MemoryType.DDR4),
            TestParts.RamKit(name: "DDR5 kit", memoryType: MemoryType.DDR5)
        ];

        Assert.Equal(["DDR4 kit"], Names(Engine.FilterCompatible(selection, PartCategory.Ram, kits)));
    }

    [Fact]
    public void Storage_M2CandidateExcludedWhenSlotsAreFull_SataStillOffered()
    {
        var selection = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(m2Slots: 1, sataPorts: 4),
            Storages = [TestParts.M2Ssd()]
        };
        Storage[] drives = [TestParts.M2Ssd(name: "Second NVMe"), TestParts.Hdd(name: "HDD")];

        Assert.Equal(["HDD"], Names(Engine.FilterCompatible(selection, PartCategory.Storage, drives)));
    }

    [Fact]
    public void CandidateOfWrongCategory_Throws()
    {
        Part[] wrong = [TestParts.Gpu()];

        Assert.Throws<ArgumentException>(() => Engine.FilterCompatible(BuildSnapshot.Empty, PartCategory.Cpu, wrong));
    }

    [Fact]
    public void With_ReplacesSingleSlotAndAppendsMultiSlot_WithoutChangingOriginal()
    {
        var original = new BuildSnapshot { Cpu = TestParts.Cpu(name: "Old"), RamKits = [TestParts.RamKit()] };

        var changed = original.With(TestParts.Cpu(name: "New")).With(TestParts.RamKit());

        Assert.Equal("New", changed.Cpu!.Name);
        Assert.Equal(2, changed.RamKits.Count);
        Assert.Equal("Old", original.Cpu!.Name);   // records + `with` = original untouched
        Assert.Single(original.RamKits);
    }

    [Fact]
    public void Without_ClearsSingleSlotButKeepsRamAndStorage()
    {
        var selection = new BuildSnapshot
        {
            Gpu = TestParts.Gpu(),
            RamKits = [TestParts.RamKit()],
            Storages = [TestParts.M2Ssd()]
        };

        Assert.Null(selection.Without(PartCategory.Gpu).Gpu);
        Assert.Single(selection.Without(PartCategory.Ram).RamKits);
        Assert.Single(selection.Without(PartCategory.Storage).Storages);
    }
}
