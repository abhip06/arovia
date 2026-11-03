using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.Password;

public sealed record ChangePasswordApiRequest(
    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string CurrentPassword,

    [property: Required]
    [property: StringLength(128, MinimumLength = 8)]
    string NewPassword);