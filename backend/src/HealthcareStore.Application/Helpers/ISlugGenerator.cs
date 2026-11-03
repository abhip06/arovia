namespace HealthcareStore.Application.Helpers;

public interface ISlugGenerator
{
    string Generate(string value);

    string AppendSuffix(
        string baseSlug,
        int suffix);
}