using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(
    ApplicationDbContext dbContext) : IOrderRepository
{
    public Task<PagedResult<OrderSummaryModel>> GetAdminOrdersAsync(
        OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = ApplyFilters(
            dbContext.Orders.AsNoTracking(),
            query);

        return GetPageAsync(
            orders,
            query,
            cancellationToken);
    }

    public Task<PagedResult<OrderSummaryModel>> GetCustomerOrdersAsync(
        Guid userId,
        OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId);

        orders = ApplyFilters(orders, query);

        return GetPageAsync(
            orders,
            query,
            cancellationToken);
    }

    public Task<OrderDetailsModel?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId);

        return GetDetailsAsync(query, cancellationToken);
    }

    public Task<OrderDetailsModel?> GetCustomerOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.Id == orderId &&
                order.UserId == userId);

        return GetDetailsAsync(query, cancellationToken);
    }

    private static IQueryable<Order> ApplyFilters(
        IQueryable<Order> orders,
        OrderListQuery query)
    {
        if (query.Status.HasValue)
        {
            orders = orders.Where(
                order => order.Status == query.Status.Value);
        }

        if (query.CreatedFromUtc.HasValue)
        {
            var fromUtc = query.CreatedFromUtc.Value;

            orders = orders.Where(
                order => order.CreatedAtUtc >= fromUtc);
        }

        if (query.CreatedToUtc.HasValue)
        {
            var toUtc = query.CreatedToUtc.Value;

            orders = orders.Where(
                order => order.CreatedAtUtc <= toUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.OrderNumber))
        {
            orders = orders.Where(
                order => order.OrderNumber == query.OrderNumber);
        }

        return orders;
    }

    private static async Task<PagedResult<OrderSummaryModel>> GetPageAsync(
        IQueryable<Order> orders,
        OrderListQuery query,
        CancellationToken cancellationToken)
    {
        var totalCount = await orders.CountAsync(
            cancellationToken);

        var skip = (int)(((long)query.Page - 1) * query.PageSize);

        var items = await orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .ThenByDescending(order => order.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(order => new OrderSummaryModel(
                order.Id,
                order.OrderNumber,
                order.UserId,
                order.Status,
                order.Currency,
                order.GrandTotal,
                order.CreatedAtUtc,
                order.Items.Count(),
                order.Items.Sum(item => (int?)item.Quantity) ?? 0))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSummaryModel>(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }

    private static Task<OrderDetailsModel?> GetDetailsAsync(
        IQueryable<Order> orders,
        CancellationToken cancellationToken)
    {
        return orders
            .Select(order => new OrderDetailsModel(
                order.Id,
                order.OrderNumber,
                order.UserId,
                order.Status,
                order.Currency,
                order.Subtotal,
                order.DiscountTotal,
                order.TaxTotal,
                order.ShippingFee,
                order.GrandTotal,
                new OrderAddressModel(
                    order.ShippingAddress.RecipientName,
                    order.ShippingAddress.RecipientPhone,
                    order.ShippingAddress.AddressLine1,
                    order.ShippingAddress.AddressLine2,
                    order.ShippingAddress.City,
                    order.ShippingAddress.State,
                    order.ShippingAddress.PostalCode,
                    order.ShippingAddress.Country),
                order.CreatedAtUtc,
                order.UpdatedAtUtc,
                order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderItemDetailsModel(
                        item.Id,
                        item.ProductVariantId,
                        item.ProductName,
                        item.Sku,
                        item.ImageUrl,
                        item.PackageQuantity,
                        item.PackageUnit,
                        item.Quantity,
                        item.UnitMrp,
                        item.UnitSellingPrice,
                        item.LineTaxAmount,
                        item.LineDiscountAmount,
                        item.UnitSellingPrice * item.Quantity,
                        item.UnitSellingPrice * item.Quantity
                            - item.LineDiscountAmount
                            + item.LineTaxAmount,
                        item.RequiresPrescription))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdForUpdateAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .FromSqlInterpolated(
                $"""
            SELECT *
            FROM "orders"
            WHERE "Id" = {orderId}
            FOR UPDATE
            """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (order is not null)
        {
            await dbContext.Entry(order)
                .Collection(x => x.Items)
                .LoadAsync(cancellationToken);
        }

        return order;
    }

    public async Task<Order?> GetCustomerOrderForUpdateAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .FromSqlInterpolated(
                $"""
            SELECT *
            FROM "orders"
            WHERE "Id" = {orderId}
              AND "UserId" = {userId}
            FOR UPDATE
            """)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (order is not null)
        {
            await dbContext.Entry(order)
                .Collection(x => x.Items)
                .LoadAsync(cancellationToken);
        }

        return order;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}