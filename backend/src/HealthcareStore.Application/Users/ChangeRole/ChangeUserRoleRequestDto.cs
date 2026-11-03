using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Users.ChangeRole;

public sealed record ChangeUserRoleRequestDto(
    Guid UserId,
    UserRole Role);