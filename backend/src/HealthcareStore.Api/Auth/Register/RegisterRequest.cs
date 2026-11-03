using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.Register;

public sealed record RegisterRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string FirstName,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string LastName,

    [property: Required]
    [property: EmailAddress]
    [property: StringLength(320)]
    string Email,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string Password);