using HealthcareStore.Application.Abstractions.Caching;
using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Common.Caching;
using HealthcareStore.Application.Common.Exceptions;
using HealthcareStore.Application.Common.Pagination;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Payments.Models;
using HealthcareStore.Domain.Users;

namespace HealthcareStore.Application.Payments;

public sealed class GetPaymentHistoryService(
    IPaymentRepository paymentRepository,
    IUserRepository userRepository,
    ICacheService cache)
{
    private static readonly TimeSpan CacheTtl =
        TimeSpan.FromMinutes(2);

    private const int pageSize = PaginationDefaults.PaymentPageSize;

    public async Task<PagedResult<PaymentHistoryItemDto>> GetPageAsync(
        Guid adminUserId,
        int page,
        CancellationToken cancellationToken = default)
    {
        if (adminUserId == Guid.Empty)
        {
            throw new ForbiddenException(
                "You are not allowed to perform this operation.");
        }

        var admin = await userRepository.FindByIdAsync(
            adminUserId,
            cancellationToken);

        if (admin is null || admin.Role != UserRole.Admin)
        {
            throw new ForbiddenException(
                "You are not allowed to perform this operation.");
        }

        if (page < 1 ||
            (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be a valid positive number.");
        }

        if (pageSize < 1 ||
            pageSize > PaginationDefaults.MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Page size must be between 1 and {PaginationDefaults.MaximumPageSize}.");
        }

        var fingerprint = CacheKeyFingerprint.Create(
            page,
            pageSize);

        var cacheKey = CacheKeys.Payments(fingerprint);

        return await cache.GetOrCreateAsync<
            PagedResult<PaymentHistoryItemDto>>(
            cacheKey,
            LoadPageAsync,
            CacheTtl,
            null,
            [CacheTags.Payments],
            cancellationToken);

        async ValueTask<PagedResult<PaymentHistoryItemDto>> LoadPageAsync(
            CancellationToken ct)
        {
            var result = await paymentRepository.GetPageAsync(
                page,
                pageSize,
                ct);

            var items = result.Items
                .Select(payment => new PaymentHistoryItemDto(
                    payment.Id,
                    payment.OrderId,
                    payment.Status.ToString(),
                    payment.CreatedAtUtc,
                    payment.UpdatedAtUtc))
                .ToArray();

            return new PagedResult<PaymentHistoryItemDto>(
                items,
                result.Page,
                result.PageSize,
                result.TotalCount);
        }
    }
}