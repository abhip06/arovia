namespace HealthcareStore.Infrastructure.Communication;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string VerificationBaseUrl { get; init; } = string.Empty;

    public string PasswordResetBaseUrl { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; }

    public string SmtpUsername { get; init; } = string.Empty;

    public string SmtpPassword { get; init; } = string.Empty;

    public string SenderName { get; init; } = string.Empty;
}