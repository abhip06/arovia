namespace HealthcareStore.Application.Products.Information;

public sealed record ProductInformationRequest(
    Guid ProductId,
    string Description,
    IReadOnlyList<string> Highlights,
    string Directions,
    IReadOnlyList<string> Warnings,
    string? StorageInstructions,
    IReadOnlyList<string> SideEffects,
    string HowToUse,
    IReadOnlyDictionary<string, string> Specifications,
    IReadOnlyList<ProductFaqRequest> Faqs);

public sealed record ProductFaqRequest(
    string Question,
    string Answer);