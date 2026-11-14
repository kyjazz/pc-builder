using PcBuilder.Core.Compatibility;

namespace PcBuilder.Api.Services;

public static class CompatibilityServiceCollectionExtensions
{
    /// <summary>
    /// Registers every ICompatibilityRule found in PcBuilder.Core plus the engine.
    /// New rule = new class in Core/Compatibility/Rules; nothing to change here.
    /// (AllRulesTests in the test project guards that the rules are R01..R14.)
    /// </summary>
    public static IServiceCollection AddCompatibilityEngine(this IServiceCollection services)
    {
        var ruleTypes = typeof(ICompatibilityRule).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICompatibilityRule).IsAssignableFrom(t));

        // Rules hold no state, so one shared instance each is safe.
        foreach (var ruleType in ruleTypes)
        {
            services.AddSingleton(typeof(ICompatibilityRule), ruleType);
        }

        // DI hands the engine every registered rule as IEnumerable<ICompatibilityRule>.
        services.AddSingleton<CompatibilityEngine>();

        // Uses AppDbContext (scoped), so it must be scoped too.
        services.AddScoped<BuildSnapshotLoader>();

        return services;
    }
}
