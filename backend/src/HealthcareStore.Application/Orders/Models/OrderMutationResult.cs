namespace HealthcareStore.Application.Orders.Models;

public enum OrderMutationStatus
{
    Succeeded = 1,
    AlreadyCancelled = 2,
    NotFound = 3,
    InvalidTransition = 4,
    InvalidStatus = 5,
    PrescriptionVerificationRequired = 6
}

public sealed record OrderMutationResult(
    OrderMutationStatus Status,
    OrderDetailsModel? Order = null);