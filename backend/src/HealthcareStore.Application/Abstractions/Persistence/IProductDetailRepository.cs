using HealthcareStore.Application.Products.Get.Detail;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductDetailRepository
{
    Task<ProductDetailBaseReadModel?> GetAsync(
        string productSlug,
        string sku,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
