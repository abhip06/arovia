using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Admin.Warehouses.Get;
using HealthcareStore.Application.Auth.AuthSession;
using HealthcareStore.Application.Auth.ExternalLogin;
using HealthcareStore.Application.Auth.Login;
using HealthcareStore.Application.Auth.Logout;
using HealthcareStore.Application.Auth.Password;
using HealthcareStore.Application.Auth.RefreshToken;
using HealthcareStore.Application.Auth.Register;
using HealthcareStore.Application.Auth.ResendEmailVerification;
using HealthcareStore.Application.Auth.VerifyEmail;
using HealthcareStore.Application.Caching;
using HealthcareStore.Application.Carts;
using HealthcareStore.Application.Catalog.ActiveIngredients.Create;
using HealthcareStore.Application.Catalog.ActiveIngredients.Status;
using HealthcareStore.Application.Catalog.Brands.Create;
using HealthcareStore.Application.Catalog.Brands.Delete;
using HealthcareStore.Application.Catalog.Brands.Get;
using HealthcareStore.Application.Catalog.Categories.Create;
using HealthcareStore.Application.Catalog.Categories.Get;
using HealthcareStore.Application.Catalog.Categories.Status;
using HealthcareStore.Application.Catalog.Manufacturers.Create;
using HealthcareStore.Application.Catalog.Manufacturers.Delete;
using HealthcareStore.Application.Catalog.Manufacturers.Get;
using HealthcareStore.Application.Checkout;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Inventory.Batches.Create;
using HealthcareStore.Application.Inventory.Batches.Get;
using HealthcareStore.Application.Inventory.Batches.Status;
using HealthcareStore.Application.Inventory.Movements.Get;
using HealthcareStore.Application.Inventory.Stock.Adjust;
using HealthcareStore.Application.Inventory.Stock.Get;
using HealthcareStore.Application.Inventory.Stock.Receive;
using HealthcareStore.Application.Inventory.Stock.Transfer;
using HealthcareStore.Application.Inventory.Warehouses.Create;
using HealthcareStore.Application.Inventory.Warehouses.Serviceable;
using HealthcareStore.Application.Inventory.Warehouses.Status;
using HealthcareStore.Application.Orders;
using HealthcareStore.Application.Orders.Receipts;
using HealthcareStore.Application.Payments;
using HealthcareStore.Application.Products.AddIngredient;
using HealthcareStore.Application.Products.AssignCategory;
using HealthcareStore.Application.Products.Create;
using HealthcareStore.Application.Products.Get;
using HealthcareStore.Application.Products.Get.Detail;
using HealthcareStore.Application.Products.Information;
using HealthcareStore.Application.Products.Prices.Create;
using HealthcareStore.Application.Products.Prices.Delete;
using HealthcareStore.Application.Products.Prices.Get;
using HealthcareStore.Application.Products.RemoveCategory;
using HealthcareStore.Application.Products.RemoveIngredient;
using HealthcareStore.Application.Products.Status;
using HealthcareStore.Application.Products.Update;
using HealthcareStore.Application.Products.Variants.Create;
using HealthcareStore.Application.Products.Variants.Media;
using HealthcareStore.Application.Products.Variants.Status;
using HealthcareStore.Application.Users.Addresses;
using HealthcareStore.Application.Users.ChangeRole;
using HealthcareStore.Application.Users.GetAll;
using HealthcareStore.Application.Users.GetCurrentUser;
using HealthcareStore.Application.Users.Update;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareStore.Application;

public static class AddDependency
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserService>();
        services.AddScoped<VerifyEmailService>();
        services.AddScoped<ResendEmailVerificationService>();
        services.AddScoped<LoginUserService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<LogoutUserService>();
        services.AddScoped<ExternalLoginUserService>();
        services.AddScoped<IAuthenticationSessionService, AuthenticationSessionService>();
        services.AddScoped<LinkGoogleAccountService>();
        services.AddScoped<ResetPasswordService>();
        services.AddScoped<RequestPasswordResetService>();
        services.AddScoped<ChangePasswordService>();

        services.AddScoped<GetCurrentUserService>();
        services.AddScoped<UpdateCurrentUserService>();
        services.AddScoped<AddressService>();
        services.AddScoped<CartService>();

        services.AddScoped<CreateProductService>();
        services.AddScoped<CreateProductVariantService>();
        services.AddScoped<CreateProductPriceService>();
        services.AddScoped<CreateManufacturerService>();
        services.AddScoped<CreateBrandService>();
        services.AddScoped<CreateCategoryService>();
        services.AddScoped<CreateWarehouseService>();
        services.AddScoped<CreateInventoryBatchService>();
        services.AddScoped<CreateActiveIngredientService>();
        services.AddScoped<CreateOrderService>();

        services.AddScoped<AddProductIngredientService>();
        services.AddScoped<ReceiveStockService>();
        services.AddScoped<AdjustStockService>();
        services.AddScoped<TransferStockService>();

        services.AddScoped<GetProductPriceHistoryService>();
        services.AddScoped<GetWarehouseStockService>();
        services.AddScoped<GetInventoryMovementHistoryService>();
        services.AddScoped<GetBrandsService>();
        services.AddScoped<GetManufacturersService>();
        services.AddScoped<GetCategoriesService>();
        services.AddScoped<GetWarehousesService>();
        services.AddScoped<GetInventoryBatchesService>();
        services.AddScoped<ProductCatalogService>();
        services.AddScoped<GetProductDetailService>();
        services.AddScoped<GetOrdersService>();
        services.AddScoped<GetAllUsersService>();
        services.AddScoped<GetPaymentHistoryService>();

        services.AddScoped<AssignProductCategoryService>();
        services.AddScoped<RemoveProductCategoryService>();

        services.AddScoped<UpdateProductService>();
        services.AddScoped<ProductStatusService>();
        services.AddScoped<ProductVariantStatusService>();
        services.AddScoped<WarehouseStatusService>();
        services.AddScoped<InventoryBatchStatusService>();
        services.AddScoped<CategoryStatusService>();
        services.AddScoped<ActiveIngredientStatusService>();
        services.AddScoped<ChangeUserRoleService>();

        services.AddScoped<ChangeOrderStatusService>();
        services.AddScoped<CancelOrderService>();

        services.AddScoped<RemoveProductIngredientService>();
        services.AddScoped<DeleteProductPriceService>();
        services.AddScoped<DeleteBrandService>();
        services.AddScoped<DeleteManufacturerService>();

        services.AddScoped<ProductInformationService>();
        services.AddScoped<ProductVariantMediaService>();

        services.AddScoped<ManageWarehouseServiceabilityService>();

        services.AddScoped<CacheVerificationService>();

        services.AddScoped<CheckoutService>();

        services.AddScoped<CreatePaymentSessionService>();
        services.AddScoped<VerifyRazorpayPaymentService>();
        services.AddScoped<SendOrderPaymentConfirmationEmailService>();

        services.AddScoped<OrderReceiptService>();

        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddScoped<IShippingFeeCalculator, FixedShippingFeeCalculator>();

        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        services.AddSingleton<ISkuGenerator, SkuGenerator>();

        services.AddScoped<IPincodeContextResolver, PincodeContextResolver>();

        return services;
    }
}
