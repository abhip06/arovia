using HealthcareStore.Domain.Users;
using System.ComponentModel.DataAnnotations;

namespace HealthcareStore.Api.Users.Update;

public sealed record UpdateCurrentUserApiRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string FirstName,

    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string LastName,

    [property: StringLength(20)]
    string? Phone,

    Gender? Gender,

    DateOnly? DateOfBirth);