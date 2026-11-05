using HealthcareStore.Api.Admin.ActiveIngredients.Status;
using HealthcareStore.Api.Admin.Catalog.ActiveIngredients.Create;
using HealthcareStore.Api.Admin.Catalog.Brands.Create;
using HealthcareStore.Api.Admin.Catalog.Brands.Delete;
using HealthcareStore.Api.Admin.Catalog.Categories.Create;
using HealthcareStore.Api.Admin.Catalog.Manufacturers.Create;
using HealthcareStore.Api.Admin.Catalog.Manufacturers.Delete;
using HealthcareStore.Api.Admin.Categories.Status;
using HealthcareStore.Api.Admin.Products.AssignCategory;
using HealthcareStore.Api.Admin.Products.Create;
using HealthcareStore.Api.Admin.Products.Ingredients.Add;
using HealthcareStore.Api.Admin.Products.Ingredients.Remove;
using HealthcareStore.Api.Admin.Products.Prices.Create;
using HealthcareStore.Api.Admin.Products.Prices.Delete;
using HealthcareStore.Api.Admin.Products.Prices.Get;
using HealthcareStore.Api.Admin.Products.RemoveCategory;
using HealthcareStore.Api.Admin.Products.Status;
using HealthcareStore.Api.Admin.Products.Update;
using HealthcareStore.Api.Admin.Products.Variants.Create;
using HealthcareStore.Api.Admin.Products.Variants.Media;
using HealthcareStore.Api.Admin.Products.Variants.Status;
using HealthcareStore.Api.Admin.Warehouses.Create;
using HealthcareStore.Api.Admin.Warehouses.Get;
using HealthcareStore.Api.Admin.Warehouses.Serviceable;
using HealthcareStore.Api.Admin.Warehouses.Status;
using HealthcareStore.Api.Auth.Google;
using HealthcareStore.Api.Auth.Login;
using HealthcareStore.Api.Auth.Logout;
using HealthcareStore.Api.Auth.Password;
using HealthcareStore.Api.Auth.RefreshToken;
using HealthcareStore.Api.Auth.Register;
using HealthcareStore.Api.Auth.ResendEmailVerification;
using HealthcareStore.Api.Auth.VerifyEmail;
using HealthcareStore.Api.Carts;
using HealthcareStore.Api.Catalog.Brands;
using HealthcareStore.Api.Catalog.Categories;
using HealthcareStore.Api.Catalog.Get;
using HealthcareStore.Api.Catalog.Get.Detail;
using HealthcareStore.Api.Catalog.Manufacturers;
using HealthcareStore.Api.Catalog.Products.Information;
using HealthcareStore.Api.Checkout;
using HealthcareStore.Api.Common.Authentication;
using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Api.Common.Exceptions;
using HealthcareStore.Api.Common.RateLimiting;
using HealthcareStore.Api.Endpoints.Orders;
using HealthcareStore.Api.Inventory.Batches.Create;
using HealthcareStore.Api.Inventory.Batches.Get;
using HealthcareStore.Api.Inventory.Batches.Status;
using HealthcareStore.Api.Inventory.Movements.Get;
using HealthcareStore.Api.Inventory.Stock.Adjust;
using HealthcareStore.Api.Inventory.Stock.Get;
using HealthcareStore.Api.Inventory.Stock.Receive;
using HealthcareStore.Api.Inventory.Stock.Transfer;
using HealthcareStore.Api.Location;
using HealthcareStore.Api.Orders;
using HealthcareStore.Api.Orders.Receipts;
using HealthcareStore.Api.Payments;
using HealthcareStore.Api.Users.Addresses;
using HealthcareStore.Api.Users.ChangeRole;
using HealthcareStore.Api.Users.GetAllUsers;
using HealthcareStore.Api.Users.GetCurrentUser;
using HealthcareStore.Api.Users.UpdateCurrentUser;
using HealthcareStore.Application;
using HealthcareStore.Application.Abstractions.Security;
using HealthcareStore.Domain.Users;
using HealthcareStore.Infrastructure;
using HealthcareStore.Infrastructure.Persistence;
using HealthcareStore.Infrastructure.Persistence.MongoDb;
using HealthcareStore.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            context.HttpContext.Request.Path;

        if (context.ProblemDetails.Status == StatusCodes.Status400BadRequest)
        {
            context.ProblemDetails.Title = "Validation failed.";
        }
    };
});

builder.Services.AddValidation();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Dependency Injection
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication configuration - JWT, Google OAuth
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =
            builder.Configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>() ?? throw new InvalidOperationException(
                    "JWT configuration is missing.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SigningKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Email
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var sessionIdValue =
                    context.Principal?
                        .FindFirstValue(
                            JwtRegisteredClaimNames.Sid);

                if (!Guid.TryParse(
                        sessionIdValue,
                        out var sessionId))
                {
                    context.Fail(
                        "Invalid authentication session.");

                    return;
                }

                var sessionValidator =
                    context.HttpContext.RequestServices
                        .GetRequiredService<
                            IAccessTokenSessionValidator>();

                var isActive =
                    await sessionValidator.IsActiveAsync(
                        sessionId,
                        context.HttpContext.RequestAborted);

                if (!isActive)
                {
                    context.Fail(
                        "Authentication session is no longer active.");
                }
            }
        };
    })
    .AddCookie(
        ExternalAuthenticationDefaults.Scheme,
        options =>
        {
            options.Cookie.Name =
                "healthcare_store_external";

            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy =
                CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(5);

            options.SlidingExpiration = false;
        })
    .AddGoogle(
    ExternalAuthenticationDefaults.GoogleScheme,
    options =>
    {
        options.ClientId =
            builder.Configuration[
                "Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException(
                "Google ClientId is missing.");

        options.ClientSecret =
            builder.Configuration[
                "Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException(
                "Google ClientSecret is missing.");

        options.CallbackPath =
            "/api/v1/auth/google/callback";

        options.SignInScheme =
            ExternalAuthenticationDefaults.Scheme;

        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");

        options.ClaimActions.MapJsonKey(
            "email_verified",
            "email_verified",
            "boolean");
    });

// Authorization configuration
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(UserRole.Admin.ToString());
        })
    .AddPolicy(
        AuthorizationPolicies.CustomerOnly,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(UserRole.Customer.ToString());
        })
    .AddPolicy(
        AuthorizationPolicies.AdminOrWarehouseStaff,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(UserRole.Admin.ToString(), UserRole.WarehouseStaff.ToString());
        });

// Cookie options configuration
builder.Services
    .AddOptions<RefreshTokenCookieOptions>()
    .Bind(builder.Configuration.GetSection(
        RefreshTokenCookieOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Name),
        "Refresh token cookie name must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Path),
        "Refresh token cookie path must be configured.")
    .ValidateOnStart();

// Sengleton Cookie DI
builder.Services.AddSingleton<RefreshTokenCookie>();

// Json configuration - Seriealization / Deserialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// CORS Policy
builder.Services.AddCors(options =>
{
    var allowedOrigins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
        ?? throw new InvalidOperationException(
            "CORS allowed origins are not configured.");

    if (allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException(
            "At least one CORS allowed origin must be configured.");
    }

    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .WithMethods(
                    "GET",
                    "POST",
                    "PUT",
                    "PATCH",
                    "DELETE",
                    "OPTIONS")
                .WithHeaders(
                    "Authorization",
                    "Content-Type",
                    "Idempotency-Key")
                .AllowCredentials();
        });
});

// Rate Limiting Config
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    RateLimitPolicies.AddPolicies(options);
});

// Forwarded Headers Configuration
var knownProxyValue =
    builder.Configuration["ForwardedHeaders:KnownProxy"];

IPAddress? knownProxyAddress = null;

if (!string.IsNullOrWhiteSpace(knownProxyValue))
{
    if (!IPAddress.TryParse(
            knownProxyValue,
            out knownProxyAddress))
    {
        throw new InvalidOperationException(
            "Forwarded proxy address is invalid.");
    }
}
else if (builder.Environment.IsProduction())
{
    throw new InvalidOperationException(
        "ForwardedHeaders:KnownProxy must be configured in Production.");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.ForwardLimit = 1;

    if (knownProxyAddress is not null)
    {
        options.KnownProxies.Add(knownProxyAddress);
    }
});

// App Initialization
var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.OpenConnectionAsync();
    await dbContext.Database.CloseConnectionAsync();
}

// Middleware Pipeline
app.UseForwardedHeaders();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// MongoDb Index Init
using (var scope = app.Services.CreateScope())
{
    var database =
        scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

    await MongoDbIndexInitializer.InitializeAsync(database);
}

// Endpoints
app.MapRegisterEndpoint();
app.MapVerifyEmailEndpoint();
app.MapResendEmailVerificationEndpoint();

app.MapLoginEndpoint();

app.MapRefreshTokenEndpoint();

app.MapLogoutEndpoint();

app.MapGoogleLoginEndpoint();
app.MapGoogleCompleteEndpoint();
app.MapGoogleLinkEndpoint();

app.MapGetCurrentUserEndpoint();
app.MapGetAllUsersEndpoint();
app.MapUpdateCurrentUserEndpoint();
app.MapChangeUserRoleEndpoint();
app.MapAddressEndpoints();

app.MapChangePasswordEndpoint();
app.MapRequestPasswordResetEndpoint();
app.MapResetPasswordEndpoint();

app.MapCreateManufacturerEndpoint();
app.MapDeleteManufacturerEndpoint();
app.MapGetManufacturersEndpoint();

app.MapCreateBrandEndpoint();
app.MapDeleteBrandEndpoint();
app.MapGetBrandsEndpoint();

app.MapCreateProductEndpoint();
app.MapUpdateProductEndpoint();
app.MapProductStatusEndpoints();
app.MapProductInformationEndpoints();

app.MapCreateProductVariantEndpoint();
app.MapProductVariantStatusEndpoints();
app.MapProductVariantMediaEndpoints();

app.MapCreateProductPriceEndpoint();
app.MapGetProductPriceHistoryEndpoint();
app.MapDeleteProductPriceEndpoint();

app.MapCreateCategoryEndpoint();
app.MapAssignProductCategoryEndpoint();
app.MapRemoveProductCategoryEndpoint();
app.MapCategoryStatusEndpoints();
app.MapGetCategoriesEndpoint();

app.MapCreateActiveIngredientEndpoint();
app.MapAddProductIngredientEndpoint();
app.MapRemoveProductIngredientEndpoint();
app.MapActiveIngredientStatusEndpoints();

app.MapCreateWarehouseEndpoint();
app.MapWarehouseStatusEndpoints();
app.MapGetWarehousesEndpoint();

app.MapReceiveStockEndpoint();
app.MapAdjustStockEndpoint();
app.MapTransferStockEndpoint();
app.MapGetWarehouseStockEndpoint();

app.MapCreateInventoryBatchEndpoint();
app.MapInventoryBatchStatusEndpoints();
app.MapGetInventoryBatchesEndpoint();

app.MapProductCatalogEndpoints();
app.MapProductDetailEndpoints();

app.MapCheckoutEndpoints();

app.MapWarehouseServiceabilityEndpoints();

app.MapGetInventoryMovementHistoryEndpoint();

app.MapCartEndpoints();

app.MapOrderQueryEndpoints();
app.MapCreateOrderEndpoint();
app.MapOrderMutationEndpoints();

app.MapPaymentEndpoints();

app.MapCustomerOrderReceipt();

app.MapLocationEndpoints();

// Start the Application
app.Run();
