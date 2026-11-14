using Microsoft.EntityFrameworkCore;
using PcBuilder.Core.Entities;
using PcBuilder.Core.Enums;

namespace PcBuilder.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Cpu> Cpus => Set<Cpu>();
    public DbSet<Motherboard> Motherboards => Set<Motherboard>();
    public DbSet<RamKit> RamKits => Set<RamKit>();
    public DbSet<Gpu> Gpus => Set<Gpu>();
    public DbSet<Psu> Psus => Set<Psu>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<Storage> Storages => Set<Storage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<BuildItem> BuildItems => Set<BuildItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Store single-value enums as readable text ("DDR5", "Cpu") instead of 0/1/2.
        // Reordering enum members later can't silently corrupt existing rows.
        configurationBuilder.Properties<PartCategory>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<MemoryType>().HaveConversion<string>().HaveMaxLength(10);
        configurationBuilder.Properties<FormFactor>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<StorageType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<StorageInterface>().HaveConversion<string>().HaveMaxLength(10);
        // FormFactorSupport is a [Flags] enum (ATX | MicroATX = 3), so it stays an int.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureParts(modelBuilder);
        ConfigureUsersAndBuilds(modelBuilder);
    }

    private static void ConfigureParts(ModelBuilder modelBuilder)
    {
        // TPT: Parts holds the shared columns, each subclass table holds its own
        // columns and uses the same Id as both primary key and foreign key to Parts.
        modelBuilder.Entity<Part>(part =>
        {
            part.UseTptMappingStrategy();
            part.ToTable("Parts");

            part.Property(p => p.Name).HasMaxLength(200).IsRequired();
            part.Property(p => p.Brand).HasMaxLength(100).IsRequired();
            part.Property(p => p.Price).HasPrecision(10, 2);
            part.Property(p => p.ImageUrl).HasMaxLength(500);

            // The parts list endpoint filters by these on every request.
            part.HasIndex(p => new { p.Category, p.IsActive });
        });

        modelBuilder.Entity<Cpu>(cpu =>
        {
            cpu.ToTable("Cpus");
            cpu.Property(c => c.Socket).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Motherboard>(mb =>
        {
            mb.ToTable("Motherboards");
            mb.Property(m => m.Socket).HasMaxLength(20).IsRequired();
            mb.Property(m => m.Chipset).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<RamKit>().ToTable("RamKits");

        modelBuilder.Entity<Gpu>(gpu =>
        {
            gpu.ToTable("Gpus");
            gpu.Property(g => g.GpuChip).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Psu>(psu =>
        {
            psu.ToTable("Psus");
            psu.Property(p => p.EfficiencyRating).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Case>().ToTable("Cases");
        modelBuilder.Entity<Storage>().ToTable("Storages");
    }

    private static void ConfigureUsersAndBuilds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Email).HasMaxLength(256).IsRequired();
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.PasswordHash).IsRequired();
            user.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();

            // Deleting a user deletes their builds.
            user.HasMany(u => u.Builds)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Build>(build =>
        {
            build.Property(b => b.Name).HasMaxLength(100).IsRequired();

            // Deleting a build deletes its items.
            build.HasMany(b => b.Items)
                .WithOne(i => i.Build)
                .HasForeignKey(i => i.BuildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BuildItem>(item =>
        {
            // Parts are never hard-deleted, and a part used in a build must not be deletable.
            item.HasOne(i => i.Part)
                .WithMany()
                .HasForeignKey(i => i.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            // The same part appears once per build; use Quantity for more.
            item.HasIndex(i => new { i.BuildId, i.PartId }).IsUnique();
        });
    }
}
