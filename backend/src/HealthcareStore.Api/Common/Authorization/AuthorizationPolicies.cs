namespace HealthcareStore.Api.Common.Authorization;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CustomerOnly = "CustomerOnly";
    public const string AdminOrWarehouseStaff = "AdminOrWarehouseStaff";
}