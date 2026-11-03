using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.Password;

public sealed record ResetPasswordApiRequest(
    [property: Required]
    string Token,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string NewPassword);