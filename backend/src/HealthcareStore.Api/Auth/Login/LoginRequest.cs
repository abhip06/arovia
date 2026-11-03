using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.Login;

public sealed record LoginRequest(
    [property: Required]
    [property: EmailAddress]
    [property: StringLength(320)]
    string Email,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password);