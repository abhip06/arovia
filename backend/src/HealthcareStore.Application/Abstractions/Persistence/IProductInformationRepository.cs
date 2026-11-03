using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IProductInformationRepository
{
    Task<ProductInformation?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task CreateAsync(
        ProductInformation productInformation,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        ProductInformation productInformation,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken);
}