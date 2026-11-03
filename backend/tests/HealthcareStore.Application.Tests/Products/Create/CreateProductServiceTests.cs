using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Products.Create;
using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;

namespace HealthcareStore.Application.Tests.Products.Create;

public sealed class CreateProductServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateProduct()
    {
        var products = new FakeProductRepository();

        var service = CreateService(products);

        var request = new CreateProductRequest(
            "Paracetamol   500 MG Tablets",
            ProductType.Medicine,
            Guid.NewGuid(),
            Guid.NewGuid(),
            false,
            "300490",
            12.00m,
            true,
            7);

        var result = await service.CreateAsync(
            request,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(
            "Paracetamol   500 MG Tablets",
            result.Name);
        Assert.Equal(
            "paracetamol-500-mg-tablets",
            result.Slug);
        Assert.Equal(
            ProductType.Medicine,
            result.ProductType);
        Assert.Equal(
            ProductStatus.Draft,
            result.Status);

        Assert.Single(products.CreatedProducts);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateCatalogIdentity()
    {
        var products = new FakeProductRepository
        {
            IdentityExists = true
        };

        var service = CreateService(products);

        var request = CreateRequest();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Equal(
            "This product already exists in the catalog.",
            exception.Message);

        Assert.Empty(products.CreatedProducts);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInactiveManufacturer()
    {
        var products = new FakeProductRepository();

        var manufacturers = new FakeManufacturerRepository
        {
            ExistsActive = false
        };

        var service = CreateService(
            products,
            manufacturers);

        var request = CreateRequest();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Empty(products.CreatedProducts);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInactiveBrand()
    {
        var products = new FakeProductRepository();

        var brands = new FakeBrandRepository
        {
            ExistsActive = false
        };

        var service = CreateService(
            products,
            brandRepository: brands);

        var request = CreateRequest(
            brandId: Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Empty(products.CreatedProducts);
    }

    [Fact]
    public async Task CreateAsync_ShouldRetryWhenSlugAlreadyExists()
    {
        var products = new FakeProductRepository();

        products.Results.Enqueue(
            ProductInsertResult.DuplicateSlug);

        products.Results.Enqueue(
            ProductInsertResult.Created);

        var service = CreateService(products);

        var request = CreateRequest();

        var result = await service.CreateAsync(
            request,
            CancellationToken.None);

        Assert.Equal(
            "paracetamol-500-mg-tablets-2",
            result.Slug);

        Assert.Equal(
            2,
            products.CreatedProducts.Count);

        Assert.Equal(
            "paracetamol-500-mg-tablets",
            products.CreatedProducts[0].Slug);

        Assert.Equal(
            "paracetamol-500-mg-tablets-2",
            products.CreatedProducts[1].Slug);
    }

    [Fact]
    public async Task CreateAsync_ShouldTranslateDatabaseCatalogIdentityViolation()
    {
        var products = new FakeProductRepository();

        products.Results.Enqueue(
            ProductInsertResult.DuplicateCatalogIdentity);

        var service = CreateService(products);

        var request = CreateRequest();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                request,
                CancellationToken.None));

        Assert.Equal(
            "This product already exists in the catalog.",
            exception.Message);
    }

    private static CreateProductService CreateService(
        FakeProductRepository products,
        FakeManufacturerRepository? manufacturerRepository = null,
        FakeBrandRepository? brandRepository = null)
    {
        return new CreateProductService(
            products,
            manufacturerRepository ?? new FakeManufacturerRepository(),
            brandRepository ?? new FakeBrandRepository(),
            new FakeSlugGenerator(),
            new FakeCacheService());
    }

    private static CreateProductRequest CreateRequest(
        Guid? manufacturerId = null,
        Guid? brandId = null)
    {
        return new CreateProductRequest(
            "Paracetamol 500 MG Tablets",
            ProductType.Medicine,
            manufacturerId ?? Guid.NewGuid(),
            brandId,
            false,
            "300490",
            12.00m,
            true,
            7);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public bool IdentityExists { get; init; }

        public Queue<ProductInsertResult> Results { get; } = new();

        public List<Product> CreatedProducts { get; } = [];

        public Task<bool> IdentityExistsAsync(
            ProductType productType,
            Guid manufacturerId,
            Guid? brandId,
            string normalizedName,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityExists);
        }

        public Task<ProductInsertResult> TryCreateAsync(
            Product product,
            CancellationToken cancellationToken)
        {
            CreatedProducts.Add(product);

            var result = Results.Count > 0
                ? Results.Dequeue()
                : ProductInsertResult.Created;

            return Task.FromResult(result);
        }

        public Task<ProductVariantParentInfo?> GetForVariantCreationAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ProductVariantParentInfo?>(null);
        }

        public Task<ProductVariantPriceInfo?> GetForPriceCreationAsync(
            Guid variantId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ProductVariantPriceInfo?>(null);
        }

        public Task<Product?> GetWithIngredientsAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Product?>(null);
        }

        public void AddIngredient(ProductIngredient ingredient)
        {
            return;
        }

        public void RemoveIngredient(ProductIngredient ingredient)
        {
            return;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<Product?> GetForUpdateAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Product?>(null);
        }

        public Task<bool> IdentityExistsForAnotherProductAsync(
            ProductType productType,
            Guid manufacturerId,
            Guid? brandId,
            string normalizedName,
            Guid productId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<ProductUpdateResult> TryUpdateAsync(
            Product product,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                ProductUpdateResult.Updated);
        }

        public Task<Product?> GetForIngredientRemovalAsync(
            Guid productId,
            Guid productIngredientId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Product?> GetForCategoryAssignmentAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ExistsByIdAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeManufacturerRepository
        : IManufacturerRepository
    {
        public bool ExistsActive { get; init; } = true;

        public Task<bool> ExistsActiveAsync(
            Guid manufacturerId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistsActive);
        }

        public Task<ManufacturerInsertResult> TryCreateAsync(
            Manufacturer manufacturer,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                ManufacturerInsertResult.Created);
        }

        public Task<IReadOnlyList<ManufacturerReadModel>> GetAllForReadAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ManufacturerReadModel>>([]);
        }

        public Task<Manufacturer?> GetForDeleteAsync(
            Guid manufacturerId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<ManufacturerDeleteResult> TryDeleteAsync(
            Manufacturer manufacturer,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeBrandRepository
    : IBrandRepository
    {
        public bool ExistsActive { get; init; } = true;

        public Task<bool> ExistsActiveAsync(
            Guid brandId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistsActive);
        }

        public Task<BrandInsertResult> TryCreateAsync(
            Brand brand,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                BrandInsertResult.Created);
        }

        public Task<IReadOnlyList<BrandReadModel>> GetAllForReadAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<BrandReadModel>>([]);
        }

        public Task<Brand?> GetForDeleteAsync(
            Guid brandId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<BrandDeleteResult> TryDeleteAsync(
            Brand brand,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeSlugGenerator : ISlugGenerator
    {
        public string Generate(string value)
        {
            return "paracetamol-500-mg-tablets";
        }

        public string AppendSuffix(
            string baseSlug,
            int suffix)
        {
            return $"{baseSlug}-{suffix}";
        }
    }

    private sealed class FakeCacheService : ICacheService
    {
        public ValueTask<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, ValueTask<T>> factory,
            TimeSpan expiration,
            TimeSpan? localCacheExpiration = null,
            IEnumerable<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            return factory(cancellationToken);
        }

        public ValueTask RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveByTagAsync(
            string tag,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}