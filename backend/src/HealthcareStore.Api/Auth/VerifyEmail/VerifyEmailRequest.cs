using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Auth.VerifyEmail;

public sealed record VerifyEmailRequest([property: Required] string Token);