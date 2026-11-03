namespace HealthcareStore.Application.Carts.Models;

public enum CartMutationStatus
{
    Applied,
    Replayed,
    NotFound,
    IdempotencyConflict,
    LimitExceeded
}

public sealed record CartMutationResult(
    CartMutationStatus Status);