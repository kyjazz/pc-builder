using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Entities;

public class Storage : Part
{
    public Storage() : base(PartCategory.Storage) { }

    public StorageType StorageType { get; set; }
    public StorageInterface Interface { get; set; }
    public int CapacityGb { get; set; }
}
