using HealthcareStore.Api.Common.Authorization;
using HealthcareStore.Application.Products.Variants.Media;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ApiRequest =
    HealthcareStore.Api.Admin.Products.Variants.Media.ProductVariantMediaRequest;
using AppRequest =
    HealthcareStore.Application.Products.Variants.Media.ProductVariantMediaRequest;

namespace HealthcareStore.Api.Admin.Products.Variants.Media;

public static class ProductVariantMediaEndpoint
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public static IEndpointRouteBuilder MapProductVariantMediaEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "/api/v1/admin/products/variants/{variantId:guid}/media",
                UploadAsync)
            .Accepts<ApiRequest>("multipart/form-data")
            .Produces<ProductVariantMediaResponse>(
                StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization(
                AuthorizationPolicies.AdminOnly);

        endpoints
            .MapPut(
                "/api/v1/admin/products/variants/{variantId:guid}/media/{mediaId:guid}",
                ReplaceAsync)
            .Accepts<ApiRequest>("multipart/form-data")
            .Produces<ProductVariantMediaResponse>(
                StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery()
            .RequireAuthorization(
                AuthorizationPolicies.AdminOnly);

        endpoints
            .MapDelete(
                "/api/v1/admin/products/variants/{variantId:guid}/media/{mediaId:guid}",
                DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(
                AuthorizationPolicies.AdminOnly);

        return endpoints;
    }

    private static async Task<IResult> UploadAsync(
        Guid variantId,
        [FromForm] ApiRequest request,
        ProductVariantMediaService service,
        CancellationToken cancellationToken)
    {
        var validationErrors =
            Validate(
                variantId,
                request);

        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(
                validationErrors);
        }

        await using var stream =
            request.File!.OpenReadStream();

        var result =
            await service.UploadAsync(
                variantId,
                new AppRequest(
                    stream,
                    Path.GetFileName(request.File.FileName),
                    request.File.ContentType,
                    request.File.Length,
                    request.AltText?.Trim(),
                    request.SortOrder!.Value,
                    request.IsPrimary),
                cancellationToken);

        return Results.Created(
            $"/api/v1/admin/products/variants/{variantId}/media/{result.Id}",
            new ProductVariantMediaResponse(
                result.Id,
                result.AltText,
                result.SortOrder,
                result.IsPrimary,
                result.Url));
    }

    private static async Task<IResult> ReplaceAsync(
        Guid variantId,
        Guid mediaId,
        [FromForm] ApiRequest request,
        ProductVariantMediaService service,
        CancellationToken cancellationToken)
    {
        var validationErrors =
            Validate(
                variantId,
                request,
                mediaId);

        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(
                validationErrors);
        }

        await using var stream =
            request.File!.OpenReadStream();

        var result =
            await service.ReplaceAsync(
                variantId,
                mediaId,
                new AppRequest(
                    stream,
                    Path.GetFileName(request.File.FileName),
                    request.File.ContentType,
                    request.File.Length,
                    request.AltText?.Trim(),
                    request.SortOrder!.Value,
                    request.IsPrimary),
                cancellationToken);

        return Results.Ok(
            new ProductVariantMediaResponse(
                result.Id,
                result.AltText,
                result.SortOrder,
                result.IsPrimary,
                result.Url));
    }

    private static async Task<IResult> DeleteAsync(
        Guid variantId,
        Guid mediaId,
        ProductVariantMediaService service,
        CancellationToken cancellationToken)
    {
        if (variantId == Guid.Empty)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["variantId"] =
                    ["Product variant ID cannot be empty."]
                });
        }

        if (mediaId == Guid.Empty)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["mediaId"] =
                    ["Media ID cannot be empty."]
                });
        }

        await service.DeleteAsync(
            variantId,
            mediaId,
            cancellationToken);

        return Results.NoContent();
    }

    private static Dictionary<string, string[]> Validate(
        Guid variantId,
        ApiRequest request,
        Guid? mediaId = null)
    {
        var errors =
            new Dictionary<string, List<string>>();

        if (variantId == Guid.Empty)
        {
            AddError(
                errors,
                "variantId",
                "Product variant ID cannot be empty.");
        }

        if (mediaId.HasValue &&
            mediaId.Value == Guid.Empty)
        {
            AddError(
                errors,
                "mediaId",
                "Media ID cannot be empty.");
        }

        ValidateObject(
            request,
            errors);

        if (request.File is not null)
        {
            ValidateFile(
                request.File,
                errors);
        }

        return errors.ToDictionary(
            static x => x.Key,
            static x => x.Value.ToArray());
    }

    private static void ValidateObject(
        object instance,
        Dictionary<string, List<string>> errors)
    {
        var validationContext =
            new ValidationContext(instance);

        var results =
            new List<ValidationResult>();

        Validator.TryValidateObject(
            instance,
            validationContext,
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
                    member,
                    result.ErrorMessage ??
                    "The value is invalid.");
            }
        }
    }

    private static void ValidateFile(
        IFormFile file,
        Dictionary<string, List<string>> errors)
    {
        if (file.Length <= 0)
        {
            AddError(
                errors,
                "file",
                "The uploaded image cannot be empty.");
        }

        if (file.Length > ApiRequest.MaxFileSize)
        {
            AddError(
                errors,
                "file",
                "The uploaded image cannot exceed 5 MB.");
        }

        var contentType =
            file.ContentType.Trim()
                .ToLowerInvariant();

        if (!AllowedContentTypes.Contains(
                contentType))
        {
            AddError(
                errors,
                "file",
                "Only JPEG, PNG, and WebP images are supported.");
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(
                extension))
        {
            AddError(
                errors,
                "file",
                "Only .jpg, .jpeg, .png, and .webp files are supported.");
        }
    }

    private static void AddError(
        Dictionary<string, List<string>> errors,
        string key,
        string message)
    {
        if (!errors.TryGetValue(
                key,
                out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}