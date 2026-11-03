using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.RefreshToken;

public sealed record RefreshTokenRequest(
    [property: Required]
    string RefreshToken);