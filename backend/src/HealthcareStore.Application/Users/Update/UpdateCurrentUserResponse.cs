using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.Update;

public sealed record UpdateCurrentUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Gender? Gender,
    DateOnly? DateOfBirth,
    string Role,
    bool IsEmailVerified);