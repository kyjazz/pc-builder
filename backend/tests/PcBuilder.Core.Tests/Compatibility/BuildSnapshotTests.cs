using PcBuilder.Core.Compatibility;

namespace PcBuilder.Core.Tests.Compatibility;

public class BuildSnapshotTests
{
    [Fact]
    public void EmptySnapshot_HasZeroTotals()
    {
        Assert.Equal(0m, BuildSnapshot.Empty.TotalPrice);
        Assert.Empty(BuildSnapshot.Empty.AllParts);
    }

    [Fact]
    public void TotalPrice_SumsSinglePartsAndEveryUnitInLists()
    {
        var cpu = TestParts.Cpu();
        cpu.Price = 8000m;
        var ram = TestParts.RamKit();
        ram.Price = 4300.50m;
        var ssd = TestParts.M2Ssd();
        ssd.Price = 3000m;

        var build = new BuildSnapshot
        {
            Cpu = cpu,
            RamKits = [ram, ram],        // two kits
            Storages = [ssd, ssd, ssd]   // three drives
        };

        // 8000 + 2 × 4300.50 + 3 × 3000 = 25601.00
        Assert.Equal(25601.00m, build.TotalPrice);
        Assert.Equal(6, build.AllParts.Count());
    }
}
