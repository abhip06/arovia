using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Information;

public sealed class ProductInformationService(
    IProductRepository productRepository,
    IProductInformationRepository productInformationRepository)
{
    public async Task CreateAsync(
        ProductInformationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(request));
        }

        if (!await productRepository.ExistsByIdAsync(
                request.ProductId,
                cancellationToken))
        {
            throw new NotFoundException(
                "The specified product was not found.");
        }

        var existing =
            await productInformationRepository.GetAsync(
                request.ProductId,
                cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(
                "Product information already exists for the specified product.");
        }

        var information =
            ProductInformation.Create(
                request.ProductId,
                request.Description,
                request.Highlights,
                request.Directions,
                request.Warnings,
                request.StorageInstructions,
                request.SideEffects,
                request.HowToUse,
                request.Specifications,
                request.Faqs.Select(
                    static x => new ProductFaq(
                        x.Question,
                        x.Answer)));

        await productInformationRepository.CreateAsync(
            information,
            cancellationToken);
    }

    public async Task UpdateAsync(
        ProductInformationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(request));
        }

        var information =
            await productInformationRepository.GetAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new NotFoundException(
                "Product information was not found for the specified product.");

        information.Update(
            request.Description,
            request.Highlights,
            request.Directions,
            request.Warnings,
            request.StorageInstructions,
            request.SideEffects,
            request.HowToUse,
            request.Specifications,
            request.Faqs.Select(
                static x => new ProductFaq(
                    x.Question,
                    x.Answer)));

        var updated =
            await productInformationRepository.UpdateAsync(
                information,
                cancellationToken);

        if (!updated)
        {
            throw new NotFoundException(
                "Product information was not found for the specified product.");
        }
    }

    public async Task DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));
        }

        var deleted =
            await productInformationRepository.DeleteAsync(
                productId,
                cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException(
                "Product information was not found for the specified product.");
        }
    }
}