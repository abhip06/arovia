using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Catalog;

namespace HealthcareStore.Application.Catalog.ActiveIngredients.Create;

public sealed class CreateActiveIngredientService(
    IActiveIngredientRepository repository,
    ISlugGenerator slugGenerator)
{
    private const int MaxSlugAttempts = 100;

    public async Task<CreateActiveIngredientResponse> CreateAsync(
        CreateActiveIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var baseSlug = slugGenerator.Generate(request.Name);

        for (var attempt = 1; attempt <= MaxSlugAttempts; attempt++)
        {
            var slug = attempt == 1
                ? baseSlug
                : slugGenerator.AppendSuffix(
                    baseSlug,
                    attempt);

            var activeIngredient =
                ActiveIngredient.Create(
                    request.Name,
                    slug);

            var result =
                await repository.TryCreateAsync(
                    activeIngredient,
                    cancellationToken);

            if (result == ActiveIngredientInsertResult.Created)
            {
                return new CreateActiveIngredientResponse(
                    activeIngredient.Id,
                    activeIngredient.Name,
                    activeIngredient.Slug,
                    activeIngredient.Status);
            }
        }

        throw new ConflictException(
            "Unable to generate a unique active ingredient slug.");
    }
}