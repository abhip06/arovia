using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<PagedResult<OrderSummaryModel>> GetAdminOrdersAsync(
       OrderListQuery query,
       CancellationToken cancellationToken = default);

    Task<PagedResult<OrderSummaryModel>> GetCustomerOrdersAsync(
        Guid userId,
        OrderListQuery query,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsModel?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsModel?> GetCustomerOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdForUpdateAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<Order?> GetCustomerOrderForUpdateAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
