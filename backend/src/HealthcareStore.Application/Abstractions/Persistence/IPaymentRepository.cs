using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Payments;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdForUpdateAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByProviderOrderIdForUpdateAsync(
        string providerOrderId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttempt?> GetAttemptByIdAsync(
        Guid paymentId,
        Guid paymentAttemptId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttempt?> GetAttemptByIdempotencyKeyForUpdateAsync(
        Guid paymentId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentAttempt>> GetActiveAttemptsForUpdateAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttempt?> GetAttemptByIdForUpdateAsync(
        Guid paymentId,
        Guid paymentAttemptId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttempt?> GetAttemptByProviderPaymentIdForUpdateAsync(
        string providerPaymentId,
        CancellationToken cancellationToken = default);

    Task<bool> ProviderEventExistsAsync(
        string providerEventId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PaymentAttempt attempt,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PaymentEvent paymentEvent,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByProviderOrderIdAsync(
        string providerOrderId,
        CancellationToken cancellationToken = default);

    Task<bool> PaymentEventExistsAsync(
        Guid paymentId,
        string providerPaymentId,
        PaymentEventType eventType,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}