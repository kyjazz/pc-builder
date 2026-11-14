using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Data;
using PcBuilder.Api.Dtos;
using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Api.Services;

/// <summary>Either a ready snapshot or user-facing validation errors (field name -> messages).</summary>
public record SnapshotLoadResult(BuildSnapshot? Snapshot, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool Succeeded => Snapshot is not null;

    public static SnapshotLoadResult Success(BuildSnapshot snapshot) => new(snapshot, new Dictionary<string, string[]>());

    public static SnapshotLoadResult Failure(string field, params string[] messages) =>
        new(null, new Dictionary<string, string[]> { [field] = messages });
}

/// <summary>
/// Turns a list of (partId, quantity) into a BuildSnapshot the compatibility engine can read.
/// This is the bridge between the database and the database-free rules in PcBuilder.Core.
/// Also used later by the "compatible parts" and saved-build endpoints.
/// </summary>
public class BuildSnapshotLoader(AppDbContext db)
{
    // A PC has exactly one of these; only RAM kits and drives can repeat.
    private static readonly HashSet<PartCategory> SingleSlotCategories =
        [PartCategory.Cpu, PartCategory.Motherboard, PartCategory.Gpu, PartCategory.Psu, PartCategory.Case];

    private const string ItemsField = "items";

    public async Task<SnapshotLoadResult> LoadAsync(
        IEnumerable<BuildItemRequest> items, CancellationToken cancellationToken)
    {
        // The same part sent twice is treated as one line with the quantities added up.
        var quantities = items
            .GroupBy(i => i.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var ids = quantities.Keys.ToList();

        // Inactive parts are allowed: a saved build may contain a discontinued part.
        // One query; TPT returns each row as its concrete type (Cpu, Gpu, ...).
        var parts = await db.Parts.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var unknownIds = ids.Where(id => !parts.ContainsKey(id)).Order().ToList();
        if (unknownIds.Count > 0)
        {
            return SnapshotLoadResult.Failure(ItemsField, $"Parça bulunamadı: {string.Join(", ", unknownIds)}.");
        }

        var tooMany = quantities
            .GroupBy(q => parts[q.Key].Category)
            .Where(g => SingleSlotCategories.Contains(g.Key) && g.Sum(q => q.Value) > 1)
            .Select(g => g.Key)
            .Order()
            .Select(category => $"Bir sistemde yalnızca bir {DisplayNames.Of(category)} olabilir.")
            .ToArray();

        if (tooMany.Length > 0)
        {
            return SnapshotLoadResult.Failure(ItemsField, tooMany);
        }

        return SnapshotLoadResult.Success(ToSnapshot(quantities, parts));
    }

    private static BuildSnapshot ToSnapshot(Dictionary<int, int> quantities, Dictionary<int, Part> parts)
    {
        Cpu? cpu = null;
        Motherboard? motherboard = null;
        Gpu? gpu = null;
        Psu? psu = null;
        Case? pcCase = null;
        var ramKits = new List<RamKit>();
        var storages = new List<Storage>();

        foreach (var (id, quantity) in quantities)
        {
            switch (parts[id])
            {
                case Cpu c: cpu = c; break;
                case Motherboard m: motherboard = m; break;
                case Gpu g: gpu = g; break;
                case Psu p: psu = p; break;
                case Case c: pcCase = c; break;
                // BuildSnapshot lists one entry per physical unit, so quantity 2 = listed twice.
                case RamKit r: ramKits.AddRange(Enumerable.Repeat(r, quantity)); break;
                case Storage s: storages.AddRange(Enumerable.Repeat(s, quantity)); break;
                default: throw new NotSupportedException($"Unknown part type {parts[id].GetType().Name}.");
            }
        }

        return new BuildSnapshot
        {
            Cpu = cpu,
            Motherboard = motherboard,
            Gpu = gpu,
            Psu = psu,
            Case = pcCase,
            RamKits = ramKits,
            Storages = storages
        };
    }
}
