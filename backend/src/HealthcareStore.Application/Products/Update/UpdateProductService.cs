using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Products.Update;

public sealed class UpdateProductService(
    IProductRepository productRepository,
    IManufacturerRepository manufacturerRepository,
    IBrandRepository brandRepository)
{
    public async Task<UpdateProductResponse> UpdateAsync(
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product =
            await productRepository.GetForUpdateAsync(
                request.ProductId,
                cancellationToken)
            ?? throw new NotFoundException(
                "The specified product was not found.");

        if (!await manufacturerRepository.ExistsActiveAsync(
                request.ManufacturerId,
                cancellationToken))
        {
            throw new NotFoundException(
                "The specified manufacturer was not found or is inactive.");
        }

        if (request.BrandId.HasValue &&
            !await brandRepository.ExistsActiveAsync(
                request.BrandId.Value,
                cancellationToken))
        {
            throw new NotFoundException(
                "The specified brand was not found or is inactive.");
        }

        var normalizedName =
            Product.NormalizeName(request.Name);

        if (await productRepository.IdentityExistsForAnotherProductAsync(
                product.ProductType,
                request.ManufacturerId,
                request.BrandId,
                normalizedName,
                product.Id,
                cancellationToken))
        {
            throw new ConflictException(
                "Another product with the same catalog identity already exists.");
        }

        product.UpdateDetails(
            request.Name,
            request.ManufacturerId,
            request.BrandId,
            request.RequiresPrescription,
            request.HsnCode,
            request.TaxRate,
            request.Returnable,
            request.ReturnWindowDays);

        var result =
            await productRepository.TryUpdateAsync(
                product,
                cancellationToken);

        if (result ==
            ProductUpdateResult.DuplicateCatalogIdentity)
        {
            throw new ConflictException(
                "Another product with the same catalog identity already exists.");
        }

        return new UpdateProductResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.ProductType,
            product.Status,
            product.ManufacturerId,
            product.BrandId,
            product.RequiresPrescription,
            product.HsnCode,
            product.TaxRate,
            product.Returnable,
            product.ReturnWindowDays);
    }
}