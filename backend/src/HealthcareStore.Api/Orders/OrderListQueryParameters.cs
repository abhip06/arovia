using HealthcareStore.Application.Orders.Models;
using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Api.Orders;

public sealed record OrderListQueryParameters
{
    public int? Page { get; init; }

    public int? PageSize { get; init; }

    public OrderStatus? Status { get; init; }

    public DateTimeOffset? CreatedFromUtc { get; init; }

    public DateTimeOffset? CreatedToUtc { get; init; }

    public string? OrderNumber { get; init; }

    public bool TryNormalize(
        out OrderListQuery normalized,
        out string? error)
    {
        var query = new OrderListQuery
        {
            Page = Page ?? 1,
            PageSize = PageSize ?? 20,
            Status = Status,
            CreatedFromUtc = CreatedFromUtc,
            CreatedToUtc = CreatedToUtc,
            OrderNumber = OrderNumber
        };

        return query.TryNormalize(
            out normalized,
            out error);
    }
}