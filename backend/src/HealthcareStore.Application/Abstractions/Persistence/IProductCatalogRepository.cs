using HealthcareStore.Application.Products.Get;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductCatalogRepository
{
    Task<(IReadOnlyList<ProductCatalogListingRow> Items, int TotalCount)>
        GetPageAsync(
            ProductCatalogFilter filter,
            int skip,
            int pageSize,
            DateTimeOffset now,
            CancellationToken cancellationToken = default);
}