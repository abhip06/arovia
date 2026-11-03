using HealthcareStore.Domain.Users;

namespace HealthcareStore.Api.Users.ChangeRole;

public sealed record ChangeUserRoleRequest(UserRole Role);