using CloudinaryDotNet;
using HealthcareStore.Application.Abstractions.Authentication;
using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Communication;
using HealthcareStore.Application.Abstractions.Documents;
using HealthcareStore.Application.Abstractions.Location;
using HealthcareStore.Application.Abstractions.Media;
using HealthcareStore.Application.Abstractions.Payments;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Application.Checkout;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Infrastructure.Authentication;
using HealthcareStore.Infrastructure.Caching;
using HealthcareStore.Infrastructure.Communication;
using HealthcareStore.Infrastructure.Documents.Receipts;
using HealthcareStore.Infrastructure.Email;
using HealthcareStore.Infrastructure.Location;
using HealthcareStore.Infrastructure.Media.Cloudinary;
using HealthcareStore.Infrastructure.Payments.Razorpay;
using HealthcareStore.Infrastructure.Persistence;
using HealthcareStore.Infrastructure.Persistence.MongoDb;
using HealthcareStore.Infrastructure.Persistence.MongoDb.Repositories;
using HealthcareStore.Infrastructure.Persistence.Repositories;
using HealthcareStore.Infrastructure.Security;
using HealthcareStore.Infrastructure.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HealthcareStore.Infrastructure;

public static class AddDependency
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException(
                "PostgreSQL connection string is not configured.");

        var redisConnectionString =
            configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException(
                "Redis connection string is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "HealthcareStore:";
        });

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1 * 1024 * 1024;
            options.MaximumKeyLength = 1024;

            options.DefaultEntryOptions =
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromSeconds(30)
                };
        });

        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection("MongoDb"))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.DatabaseName),
                "MongoDB database name is required.")
            .ValidateOnStart();

        services.AddSingleton<IMongoClient>(_ =>
        {
            var connectionString =
                configuration.GetConnectionString("MongoDb")
                ?? throw new InvalidOperationException(
                    "MongoDB connection string is not configured.");

            return new MongoClient(connectionString);
        });

        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var client =
                serviceProvider.GetRequiredService<IMongoClient>();

            var options =
                serviceProvider
                    .GetRequiredService<IOptions<MongoDbOptions>>()
                    .Value;

            return client.GetDatabase(options.DatabaseName);
        });

        services.Configure<ShippingOptions>(
            configuration.GetSection(
                "Checkout:Shipping"));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

        services.AddScoped<IUserSessionRepository, UserSessionRepository>();

        services.AddScoped<IExternalIdentityRepository, ExternalIdentityRepository>();

        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

        services.AddScoped<IUserSessionRepository, UserSessionRepository>();

        services.AddScoped<IAddressRepository,  AddressRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IProductDetailRepository, ProductDetailRepository>();

        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        
        services.AddScoped<IBrandRepository, BrandRepository>();

        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();

        services.AddScoped<IProductPriceRepository, ProductPriceRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();

        services.AddScoped<IActiveIngredientRepository, ActiveIngredientRepository>();

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        services.AddScoped<IInventoryBatchRepository, InventoryBatchRepository>();

        services.AddScoped<IWarehouseStockRepository, WarehouseStockRepository>();

        services.AddScoped<IProductInformationRepository, ProductInformationRepository>();

        services.AddScoped<IProductVariantMediaRepository, ProductVariantMediaRepository>();

        services.AddScoped<IWarehouseServiceabilityRepository, WarehouseServiceabilityRepository>();

        services.AddScoped<IInventoryStockMovementRepository, InventoryStockMovementRepository>();

        services.AddScoped<IProductCatalogRepository, ProductCatalogRepository>();

        services.AddScoped<ICartRepository, CartRepository>();

        services.AddScoped<ICartProductReader, CartProductReader>();

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IOrderIdempotencyRepository, OrderIdempotencyRepository>();

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<ICheckoutProductReader, CheckoutProductReader>();

        services.AddScoped<IOrderPrescriptionVerificationReader,
            UnconfiguredOrderPrescriptionVerificationReader>();

        services.AddScoped<IOrderInventoryReservationService, OrderInventoryReservationService>();

        services.AddScoped<ITransactionManager, EfTransactionManager>();

        services.AddScoped<IAccessTokenSessionValidator, AccessTokenSessionValidator>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<ITokenGenerator, TokenGenerator>();

        // Email Configuration
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.VerificationBaseUrl),
                "Email:VerificationBaseUrl must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SmtpHost),
                "Email:SmtpHost must be configured.")
            .Validate(
                options => options.SmtpPort > 0,
                "Email:SmtpPort must be greater than zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SmtpUsername),
                "Email:SmtpUsername must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SmtpPassword),
                "Email:SmtpPassword must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SenderName),
                "Email:SenderName must be configured.")
            .ValidateOnStart();

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        
        // JWT Configuration
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SigningKey),
                "Jwt:SigningKey must be configured.")
            .Validate(
                options => options.SigningKey.Length >= 32,
                "Jwt:SigningKey must contain at least 32 characters.")
            .Validate(
                options => options.AccessTokenLifetimeMinutes > 0,
                "Jwt:AccessTokenLifetimeMinutes must be greater than zero.")
            .ValidateOnStart();

        // Razorpay payment gateway configuration
        services.AddOptions<RazorpayOptions>()
            .Bind(configuration.GetSection(RazorpayOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.KeyId),
                "Razorpay KeyId is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.KeySecret),
                "Razorpay KeySecret is required.")
            .ValidateOnStart();

        services.AddHttpClient<IPaymentGateway, RazorpayPaymentGateway>(
            client =>
            {
                client.BaseAddress =
                    new Uri("https://api.razorpay.com/v1/");

                client.Timeout = TimeSpan.FromSeconds(15);
            });

        // Cloudinary Configuration
        services
            .AddOptions<CloudinaryOptions>()
            .BindConfiguration("Cloudinary")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.CloudName) &&
                    !string.IsNullOrWhiteSpace(options.ApiKey) &&
                    !string.IsNullOrWhiteSpace(options.ApiSecret),
                "Cloudinary credentials are not configured.")
            .ValidateOnStart();

        services.AddSingleton<Cloudinary>(sp =>
        {
            var options =
                sp.GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<CloudinaryOptions>>()
                .Value;

            var account =
                new Account(
                    options.CloudName,
                    options.ApiKey,
                    options.ApiSecret);

            var cloudinary =
                new Cloudinary(account);

            cloudinary.Api.Secure = true;

            return cloudinary;
        });

        services.AddScoped<IProductMediaStorage, CloudinaryProductMediaStorage>();

        services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddSingleton<ICacheService, HybridCacheService>();

        services.AddSingleton<IOrderNumberGenerator, RandomOrderNumberGenerator>();

        // Quest PDF configuration
        services.Configure<OrderReceiptOptions>(
            configuration.GetSection(OrderReceiptOptions.SectionName));

        services.AddScoped<IOrderReceiptPdfGenerator, QuestPdfOrderReceiptPdfGenerator>();

        // IP Geolocation Configuration
        services.AddMemoryCache();

        services.Configure<IpGeolocationOptions>(
            configuration.GetSection(
                IpGeolocationOptions.SectionName));

        services.AddHttpClient<IPincodeResolver, IpApiPincodeResolver>(
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<IpGeolocationOptions>>()
                        .Value;

                client.BaseAddress = new Uri(options.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(options.TimeoutSeconds);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Arovia/1.0");
            });

        return services;
    }
}
