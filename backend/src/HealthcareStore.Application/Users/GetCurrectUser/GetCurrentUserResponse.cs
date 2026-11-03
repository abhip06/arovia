using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Gender? Gender,
    DateOnly? DateOfBirth,
    string Role,
    bool IsEmailVerified);