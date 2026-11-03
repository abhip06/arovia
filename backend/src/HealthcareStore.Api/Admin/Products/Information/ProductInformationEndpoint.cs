using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Products.Information;
using System.ComponentModel.DataAnnotations;
using ApiRequest =
    HealthcareStore.Api.Admin.Products.Information.ProductInformationRequest;
using AppFaqRequest =
    HealthcareStore.Application.Products.Information.ProductFaqRequest;
using AppRequest =
    HealthcareStore.Application.Products.Information.ProductInformationRequest;

namespace HealthcareStore.Api.Catalog.Products.Information;

public static class ProductInformationEndpoint
{
    public static IEndpointRouteBuilder MapProductInformationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "/api/v1/catalog/products/{productId:guid}/information",
                CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        endpoints
            .MapPut(
                "/api/v1/catalog/products/{productId:guid}/information",
                UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        endpoints
            .MapDelete(
                "/api/v1/catalog/products/{productId:guid}/information",
                DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        Guid productId,
        ApiRequest request,
        ProductInformationService service,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(productId, request);

        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        await service.CreateAsync(
            Map(productId, request),
            cancellationToken);

        return Results.Created(
            $"/api/v1/catalog/products/{productId}/information",
            null);
    }

    private static async Task<IResult> UpdateAsync(
        Guid productId,
        ApiRequest request,
        ProductInformationService service,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(productId, request);

        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        await service.UpdateAsync(
            Map(productId, request),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid productId,
        ProductInformationService service,
        CancellationToken cancellationToken)
    {
        if (productId == Guid.Empty)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["productId"] =
                    ["Product ID cannot be empty."]
                });
        }

        await service.DeleteAsync(
            productId,
            cancellationToken);

        return Results.NoContent();
    }

    private static AppRequest Map(
        Guid productId,
        ApiRequest request)
    {
        return new AppRequest(
            productId,
            request.Description.Trim(),
            request.Highlights
                .Select(static x => x.Trim())
                .ToList(),
            request.Directions.Trim(),
            request.Warnings?
                .Select(static x => x.Trim())
                .ToList()
                ?? [],
            request.StorageInstructions?.Trim(),
            request.SideEffects?
                .Select(static x => x.Trim())
                .ToList()
                ?? [],
            request.HowToUse.Trim(),
            request.Specifications?
                .Where(
                    static x =>
                        !string.IsNullOrWhiteSpace(x.Key) &&
                        !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(
                    static x => x.Key.Trim(),
                    static x => x.Value.Trim())
                ?? new Dictionary<string, string>(),
            request.Faqs?
                .Select(
                    static x => new AppFaqRequest(
                        x.Question.Trim(),
                        x.Answer.Trim()))
                .ToList()
                ?? []);
    }

    private static Dictionary<string, string[]> Validate(
        Guid productId,
        ApiRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        if (productId == Guid.Empty)
        {
            AddError(
                errors,
                "productId",
                "Product ID cannot be empty.");
        }

        ValidateObject(request, errors);

        ValidateStrings(
            request.Highlights,
            "highlights",
            errors);

        ValidateStrings(
            request.Warnings,
            "warnings",
            errors);

        ValidateStrings(
            request.SideEffects,
            "sideEffects",
            errors);

        if (request.Faqs is not null)
        {
            for (var index = 0; index < request.Faqs.Count; index++)
            {
                ValidateObject(
                    request.Faqs[index],
                    errors,
                    $"faqs[{index}]");
            }
        }

        return errors.ToDictionary(
            static x => x.Key,
            static x => x.Value.ToArray());
    }

    private static void ValidateObject(
        object instance,
        Dictionary<string, List<string>> errors,
        string? prefix = null)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            instance,
            context,
            results,
            validateAllProperties: true);

        foreach (var result in results)
        {
            var members =
                result.MemberNames.Any()
                    ? result.MemberNames
                    : ["request"];

            foreach (var member in members)
            {
                AddError(
                    errors,
                    string.IsNullOrWhiteSpace(prefix)
                        ? member
                        : $"{prefix}.{member}",
                    result.ErrorMessage ?? "Invalid value.");
            }
        }
    }

    private static void ValidateStrings(
        IEnumerable<string>? values,
        string fieldName,
        Dictionary<string, List<string>> errors)
    {
        if (values is null)
        {
            return;
        }

        var index = 0;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(
                    errors,
                    $"{fieldName}[{index}]",
                    "Value cannot be empty.");
            }

            index++;
        }
    }

    private static void AddError(
        Dictionary<string, List<string>> errors,
        string key,
        string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}