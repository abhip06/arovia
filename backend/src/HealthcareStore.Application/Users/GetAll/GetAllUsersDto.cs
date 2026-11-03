using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.GetAll;

public sealed record GetAllUsersDto(
    IReadOnlyList<UserDto> Users,
    int TotalCount);

public sealed record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    bool IsEmailVerified);