namespace HealthcareStore.Application.Helpers;

public interface ISkuGenerator
{
    string Generate(
        string productName,
        decimal packageQuantity,
        string packageUnit,
        decimal? netQuantity,
        string? netQuantityUnit);

    string AppendSuffix(
        string baseSku,
        int suffix);
}