using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PcBuilder.Core.Entities;

namespace PcBuilder.Api.Data.Seed;

/// <summary>Fills the Parts tables from parts.json when the database has no parts yet.</summary>
public static class DbSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        // "DDR5" -> MemoryType.DDR5, "ATX, MicroATX" -> FormFactorSupport.ATX | MicroATX
        Converters = { new JsonStringEnumConverter() },
        // A typo like "socke" fails loudly instead of silently leaving the field empty.
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static async Task SeedPartsAsync(
        AppDbContext db, string seedFilePath, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Parts.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Parts table already has data, seeding skipped.");
            return;
        }

        await using var stream = File.OpenRead(seedFilePath);
        var seed = await JsonSerializer.DeserializeAsync<PartSeedFile>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"Seed file '{seedFilePath}' is empty.");

        var parts = seed.AllParts().ToList();
        db.Parts.AddRange(parts);

        // SaveChanges runs in a single transaction: either every part is inserted or none.
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} parts from {File}.", parts.Count, Path.GetFileName(seedFilePath));
    }

    /// <summary>Shape of parts.json: one array per category.</summary>
    private sealed class PartSeedFile
    {
        public List<Cpu> Cpus { get; init; } = [];
        public List<Motherboard> Motherboards { get; init; } = [];
        public List<RamKit> RamKits { get; init; } = [];
        public List<Gpu> Gpus { get; init; } = [];
        public List<Psu> Psus { get; init; } = [];
        public List<Case> Cases { get; init; } = [];
        public List<Storage> Storages { get; init; } = [];

        public IEnumerable<Part> AllParts() =>
            Cpus.Cast<Part>()
                .Concat(Motherboards)
                .Concat(RamKits)
                .Concat(Gpus)
                .Concat(Psus)
                .Concat(Cases)
                .Concat(Storages);
    }
}
