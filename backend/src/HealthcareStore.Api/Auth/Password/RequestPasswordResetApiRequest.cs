using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.Password;

public sealed record RequestPasswordResetApiRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(320)]
    string Email);