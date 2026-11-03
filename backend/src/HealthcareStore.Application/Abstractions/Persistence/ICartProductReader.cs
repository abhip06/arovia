using HealthcareStore.Application.Carts.Models;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface ICartProductReader
{
    Task<IReadOnlyList<CartProductReadModel>> GetForCartAsync(
        IReadOnlyCollection<Guid> productVariantIds,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}