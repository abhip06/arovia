using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(
    ApplicationDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Payment>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                payment => payment.OrderId == orderId,
                cancellationToken);
    }

    public Task<Payment?> GetByOrderIdForUpdateAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Payment>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payments"
                WHERE "OrderId" = {orderId}
                FOR UPDATE
                """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Payment?> GetByProviderOrderIdForUpdateAsync(
        string providerOrderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerOrderId);

        return dbContext.Set<Payment>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payments"
                WHERE "ProviderOrderId" = {providerOrderId}
                FOR UPDATE
                """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<PaymentAttempt?> GetAttemptByIdAsync(
        Guid paymentId,
        Guid paymentAttemptId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<PaymentAttempt>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                attempt =>
                    attempt.PaymentId == paymentId &&
                    attempt.Id == paymentAttemptId,
                cancellationToken);
    }

    public Task<PaymentAttempt?> GetAttemptByIdempotencyKeyForUpdateAsync(
        Guid paymentId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<PaymentAttempt>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payment_attempts"
                WHERE "PaymentId" = {paymentId}
                  AND "IdempotencyKey" = {idempotencyKey}
                FOR UPDATE
                """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentAttempt>>
        GetActiveAttemptsForUpdateAsync(
            Guid paymentId,
            CancellationToken cancellationToken = default)
    {
        var initiated = (int)PaymentAttemptStatus.Initiated;
        var pending = (int)PaymentAttemptStatus.Pending;
        var authorized = (int)PaymentAttemptStatus.Authorized;

        return await dbContext.Set<PaymentAttempt>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payment_attempts"
                WHERE "PaymentId" = {paymentId}
                  AND "Status" IN ({initiated}, {pending}, {authorized})
                ORDER BY "Id"
                FOR UPDATE
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<PaymentAttempt?> GetAttemptByIdForUpdateAsync(
        Guid paymentId,
        Guid paymentAttemptId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<PaymentAttempt>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payment_attempts"
                WHERE "PaymentId" = {paymentId}
                  AND "Id" = {paymentAttemptId}
                FOR UPDATE
                """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<PaymentAttempt?>
        GetAttemptByProviderPaymentIdForUpdateAsync(
            string providerPaymentId,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerPaymentId);

        return dbContext.Set<PaymentAttempt>()
            .FromSqlInterpolated(
                $"""
                SELECT *
                FROM "payment_attempts"
                WHERE "ProviderPaymentId" = {providerPaymentId}
                FOR UPDATE
                """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ProviderEventExistsAsync(
        string providerEventId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerEventId);

        return dbContext.Set<PaymentEvent>()
            .AsNoTracking()
            .AnyAsync(
                paymentEvent =>
                    paymentEvent.ProviderEventId == providerEventId,
                cancellationToken);
    }

    public Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dbContext.Set<Payment>().Add(payment);

        return Task.CompletedTask;
    }

    public Task AddAsync(
        PaymentAttempt attempt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dbContext.Set<PaymentAttempt>().Add(attempt);

        return Task.CompletedTask;
    }

    public Task AddAsync(
        PaymentEvent paymentEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dbContext.Set<PaymentEvent>().Add(paymentEvent);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Payment?> GetByProviderOrderIdAsync(
        string providerOrderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerOrderId);

        return dbContext.Set<Payment>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                payment => payment.ProviderOrderId == providerOrderId,
                cancellationToken);
    }

    public Task<bool> PaymentEventExistsAsync(
        Guid paymentId,
        string providerPaymentId,
        PaymentEventType eventType,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<PaymentEvent>()
            .AsNoTracking()
            .AnyAsync(
                paymentEvent =>
                    paymentEvent.PaymentId == paymentId &&
                    paymentEvent.ProviderPaymentId == providerPaymentId &&
                    paymentEvent.EventType == eventType,
                cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = checked((page - 1) * pageSize);

        var query = dbContext.Payments
            .AsNoTracking();

        var totalCount = await query.CountAsync(
            cancellationToken);

        var payments = await query
            .OrderByDescending(payment => payment.CreatedAtUtc)
            .ThenByDescending(payment => payment.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Payment>(
            payments,
            page,
            pageSize,
            totalCount);
    }
}