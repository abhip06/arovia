using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.ResendEmailVerification;

public sealed record ResendEmailVerificationRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(320)]
    string Email);