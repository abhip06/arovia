using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Application.Orders.Models;

public sealed record OrderListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public OrderStatus? Status { get; init; }

    public DateTimeOffset? CreatedFromUtc { get; init; }
    public DateTimeOffset? CreatedToUtc { get; init; }

    public string? OrderNumber { get; init; }

    public bool TryNormalize(
        out OrderListQuery normalized,
        out string? error)
    {
        normalized = this;
        error = null;

        if (Page < 1)
        {
            error = "Page must be greater than zero.";
            return false;
        }

        if (PageSize is < 1 or > 50)
        {
            error = "Page size must be between 1 and 50.";
            return false;
        }

        if (((long)Page - 1) * PageSize > int.MaxValue)
        {
            error = "The requested page is too large.";
            return false;
        }

        if (Status is { } status &&
            !Enum.IsDefined<OrderStatus>(status))
        {
            error = "The supplied order status is invalid.";
            return false;
        }

        var fromUtc = CreatedFromUtc?.ToUniversalTime();
        var toUtc = CreatedToUtc?.ToUniversalTime();

        if (fromUtc.HasValue &&
            toUtc.HasValue &&
            fromUtc.Value > toUtc.Value)
        {
            error = "The start date must not be later than the end date.";
            return false;
        }

        if (OrderNumber is { Length: > 40 })
        {
            error = "Order number cannot exceed 40 characters.";
            return false;
        }

        normalized = this with
        {
            CreatedFromUtc = fromUtc,
            CreatedToUtc = toUtc,
            OrderNumber = string.IsNullOrWhiteSpace(OrderNumber)
                ? null
                : OrderNumber.Trim().ToUpperInvariant()
        };

        return true;
    }

    public OrderListQuery NormalizeAndValidate()
    {
        if (!TryNormalize(out var normalized, out var error))
        {
            throw new ArgumentException(error);
        }

        return normalized;
    }
}