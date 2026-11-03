using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.Update;

public sealed record UpdateCurrentUserRequest(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Phone,
    Gender? Gender,
    DateOnly? DateOfBirth);