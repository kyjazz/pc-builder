using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Data;
using PcBuilder.Api.Dtos;

namespace PcBuilder.Api.Services;

public interface IPartService
{
    Task<IReadOnlyList<PartSummaryDto>> GetPartsAsync(PartListQuery query, CancellationToken cancellationToken);
    Task<PartDetailDto?> GetPartAsync(int id, CancellationToken cancellationToken);
}

public class PartService(AppDbContext db) : IPartService
{
    public async Task<IReadOnlyList<PartSummaryDto>> GetPartsAsync(
        PartListQuery query, CancellationToken cancellationToken)
    {
        // Read-only query: no change tracking needed.
        var parts = db.Parts.AsNoTracking().Where(p => p.IsActive);

        if (query.Category is { } category)
        {
            parts = parts.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            parts = parts.Where(p => p.Name.Contains(term) || p.Brand.Contains(term));
        }

        // Id as the last tie-breaker keeps the order stable between requests.
        parts = query.Sort switch
        {
            PartSort.PriceAsc => parts.OrderBy(p => p.Price).ThenBy(p => p.Name).ThenBy(p => p.Id),
            PartSort.PriceDesc => parts.OrderByDescending(p => p.Price).ThenBy(p => p.Name).ThenBy(p => p.Id),
            _ => parts.OrderBy(p => p.Name).ThenBy(p => p.Id)
        };

        // Project in SQL: only the summary columns are selected, not whole entities.
        return await parts
            .Select(p => new PartSummaryDto(p.Id, p.Name, p.Brand, p.Category, p.Price, p.Tdp, p.ImageUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<PartDetailDto?> GetPartAsync(int id, CancellationToken cancellationToken)
    {
        // Inactive parts are still returned here: a saved build may reference one
        // and must be able to show it. The list endpoint hides them.
        var part = await db.Parts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return part?.ToDetailDto();
    }
}
