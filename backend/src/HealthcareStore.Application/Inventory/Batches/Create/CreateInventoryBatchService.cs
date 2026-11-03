using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Inventory.Batches.Create;

public sealed class CreateInventoryBatchService(
    IProductVariantRepository variantRepository,
    IInventoryBatchRepository batchRepository,
    ICacheService cache)
{
    public async Task<CreateInventoryBatchResponse> CreateAsync(
        CreateInventoryBatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProductVariantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variant ID cannot be empty.",
                nameof(request.ProductVariantId));
        }

        if (request.SellableUntil.HasValue &&
            request.ExpiryDate.HasValue &&
            request.SellableUntil >
            request.ExpiryDate)
        {
            throw new ArgumentException(
                "Sellable-until date cannot be later than expiry date.");
        }

        var variantStatus =
            await variantRepository.GetStatusAsync(
                request.ProductVariantId,
                cancellationToken) ?? throw new NotFoundException(
                    "The specified product variant was not found.");

        if (variantStatus == ProductVariantStatus.Discontinued)
        {
            throw new ConflictException(
                "A batch cannot be created for a discontinued product variant.");
        }

        var batch = InventoryBatch.Create(
            request.ProductVariantId,
            request.BatchNumber,
            request.ExpiryDate,
            request.SellableUntil,
            request.Mrp);

        var result =
            await batchRepository.TryCreateAsync(
                batch,
                cancellationToken);

        if (result ==
            InventoryBatchInsertResult.DuplicateBatchNumber)
        {
            throw new ConflictException(
                "This batch number already exists for the product variant.");
        }

        await cache.RemoveByTagAsync(
            CacheTags.Batches,
            cancellationToken);

        return new CreateInventoryBatchResponse(
            batch.Id,
            batch.ProductVariantId,
            batch.BatchNumber,
            batch.ExpiryDate,
            batch.SellableUntil,
            batch.Mrp,
            batch.Status);
    }
}