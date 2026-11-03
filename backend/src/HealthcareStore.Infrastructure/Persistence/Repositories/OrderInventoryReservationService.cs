using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Domain.Orders;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class OrderInventoryReservationService(
    IWarehouseStockRepository warehouseStockRepository,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : IOrderInventoryReservationService
{
    public async Task<OrderInventoryReservationResult> ReserveForOrderAsync(
        Order order,
        string pincode,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentException.ThrowIfNullOrWhiteSpace(pincode);

        var candidates = await warehouseStockRepository.GetReservationCandidatesAsync(
            order.Items
                .Select(item => item.ProductVariantId)
                .Distinct()
                .ToArray(),
            pincode,
            today,
            cancellationToken);

        if (candidates.Count == 0)
        {
            return OrderInventoryReservationResult.InsufficientStock;
        }

        var plan = BuildPlan(order.Items, candidates);

        if (plan is null || plan.Count == 0)
        {
            return OrderInventoryReservationResult.InsufficientStock;
        }

        var allocationsByStock = plan
            .GroupBy(allocation => allocation.Candidate.WarehouseStockId)
            .OrderBy(group => group.Key)
            .ToArray();

        var lockedStocks = new Dictionary<Guid, WarehouseStock>();

        // Acquire every required row lock before changing any quantities.
        foreach (var group in allocationsByStock)
        {
            var candidate = group.First().Candidate;
            var requiredQuantity = group.Sum(allocation => allocation.Quantity);

            var stock = await warehouseStockRepository.GetForUpdateAsync(
                candidate.InventoryBatchId,
                candidate.WarehouseId,
                cancellationToken);

            if (stock is null ||
                stock.AvailableQuantity < requiredQuantity)
            {
                return OrderInventoryReservationResult.InsufficientStock;
            }

            lockedStocks.Add(stock.Id, stock);
        }

        // All required rows are locked and validated. Now reserve them.
        foreach (var group in allocationsByStock)
        {
            var stock = lockedStocks[group.Key];
            var quantity = group.Sum(allocation => allocation.Quantity);

            stock.Reserve(quantity);
        }

        var now = timeProvider.GetUtcNow();

        foreach (var allocation in plan)
        {
            dbContext.OrderItemStockAllocations.Add(
                OrderItemStockAllocation.Create(
                    orderItemId: allocation.OrderItem.Id,
                    warehouseStockId: allocation.Candidate.WarehouseStockId,
                    quantity: allocation.Quantity,
                    createdAtUtc: now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return OrderInventoryReservationResult.Reserved;
    }

    private static List<PlannedAllocation>? BuildPlan(
        IReadOnlyCollection<OrderItem> orderItems,
        IReadOnlyList<WarehouseStockReservationCandidate> candidates)
    {
        var requiredByVariant = orderItems
            .GroupBy(item => item.ProductVariantId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.Quantity));

        // Prefer a single warehouse if it has sufficient stock for every
        // variant in the order.
        var preferredWarehouse = candidates
            .GroupBy(candidate => candidate.WarehouseId)
            .Where(warehouse => requiredByVariant.All(required =>
                warehouse
                    .Where(candidate =>
                        candidate.ProductVariantId == required.Key)
                    .Sum(candidate => (long)candidate.AvailableQuantity)
                >= required.Value))
            .OrderBy(warehouse => warehouse.Count())
            .ThenBy(warehouse => warehouse.Key)
            .Select(warehouse => (Guid?)warehouse.Key)
            .FirstOrDefault();

        var eligibleCandidates = preferredWarehouse.HasValue
            ? candidates
                .Where(candidate =>
                    candidate.WarehouseId == preferredWarehouse.Value)
                .ToArray()
            : candidates.ToArray();

        var remainingByStock = eligibleCandidates.ToDictionary(
            candidate => candidate.WarehouseStockId,
            candidate => candidate.AvailableQuantity);

        var allocations = new List<PlannedAllocation>();

        foreach (var item in orderItems.OrderBy(item => item.ProductVariantId))
        {
            var remainingQuantity = item.Quantity;

            var itemCandidates = eligibleCandidates
                .Where(candidate =>
                    candidate.ProductVariantId == item.ProductVariantId)
                .OrderBy(candidate => candidate.ExpiryDate is null)
                .ThenBy(candidate => candidate.ExpiryDate)
                .ThenBy(candidate => candidate.SellableUntil is null)
                .ThenBy(candidate => candidate.SellableUntil)
                .ThenBy(candidate => candidate.ReceivedAt)
                .ThenBy(candidate => candidate.BatchCreatedAt)
                .ThenBy(candidate => candidate.WarehouseId)
                .ThenBy(candidate => candidate.WarehouseStockId);

            foreach (var candidate in itemCandidates)
            {
                if (remainingQuantity == 0)
                {
                    break;
                }

                var available = remainingByStock[candidate.WarehouseStockId];

                if (available <= 0)
                {
                    continue;
                }

                var allocated = Math.Min(remainingQuantity, available);

                allocations.Add(
                    new PlannedAllocation(item, candidate, allocated));

                remainingByStock[candidate.WarehouseStockId] -= allocated;
                remainingQuantity -= allocated;
            }

            if (remainingQuantity > 0)
            {
                return null;
            }
        }

        return allocations;
    }

    public async Task ReleaseForOrderAsync(
    Order order,
    DateTimeOffset utcNow,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureUtc(utcNow, nameof(utcNow));

        var (allocations, stocks) = await LockOrderAllocationsAsync(
            order,
            cancellationToken);

        foreach (var group in allocations.GroupBy(
            allocation => allocation.WarehouseStockId))
        {
            var quantity = GetAllocationQuantity(group);

            stocks[group.Key].ReleaseReservation(quantity);
        }

        foreach (var allocation in allocations)
        {
            allocation.Release(utcNow);
        }
    }

    public async Task FulfillForOrderAsync(
        Order order,
        Guid performedByUserId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (performedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Performing user ID cannot be empty.",
                nameof(performedByUserId));
        }

        EnsureUtc(utcNow, nameof(utcNow));

        var (allocations, stocks) = await LockOrderAllocationsAsync(
            order,
            cancellationToken);

        foreach (var group in allocations.GroupBy(
            allocation => allocation.WarehouseStockId))
        {
            var quantity = GetAllocationQuantity(group);
            var stock = stocks[group.Key];

            stock.FulfillReserved(quantity);

            dbContext.InventoryStockMovements.Add(
                InventoryStockMovement.Create(
                    warehouseStockId: stock.Id,
                    movementType: InventoryMovementType.Sold,
                    quantityDelta: -quantity,
                    createdByUserId: performedByUserId,
                    referenceType: "Order",
                    referenceId: order.Id,
                    note: $"Order {order.OrderNumber} shipped."));
        }

        foreach (var allocation in allocations)
        {
            allocation.Fulfill(utcNow);
        }
    }

    private async Task<(
        List<OrderItemStockAllocation> Allocations,
        Dictionary<Guid, WarehouseStock> Stocks)>
        LockOrderAllocationsAsync(
            Order order,
            CancellationToken cancellationToken)
    {
        var items = order.Items.ToDictionary(item => item.Id);

        if (items.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot change inventory allocations for an order without items.");
        }

        var itemIds = items.Keys.ToArray();

        IQueryable<OrderItemStockAllocation> query =
            dbContext.OrderItemStockAllocations;

        query = query
            .Where(allocation =>
                itemIds.Contains(allocation.OrderItemId))
            .OrderBy(allocation => allocation.WarehouseStockId)
            .ThenBy(allocation => allocation.Id);

        var allocations = await
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync<OrderItemStockAllocation>(
                    query,
                    cancellationToken);

        if (allocations.Count == 0)
        {
            throw new InvalidOperationException(
                $"Order {order.OrderNumber} has no stock allocations.");
        }

        if (allocations.Any(allocation =>
            allocation.Status != OrderItemStockAllocationStatus.Reserved))
        {
            throw new InvalidOperationException(
                $"Order {order.OrderNumber} contains a non-reserved allocation.");
        }

        foreach (var item in order.Items)
        {
            var allocatedQuantity = allocations
                .Where(allocation => allocation.OrderItemId == item.Id)
                .Sum(allocation => (long)allocation.Quantity);

            if (allocatedQuantity != item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Stock allocations do not match order item {item.Id}.");
            }
        }

        var stocks = new Dictionary<Guid, WarehouseStock>();

        foreach (var group in allocations.GroupBy(
            allocation => allocation.WarehouseStockId)
            .OrderBy(group => group.Key))
        {
            var stock = await warehouseStockRepository.GetForUpdateByIdAsync(
                group.Key,
                cancellationToken);

            if (stock is null)
            {
                throw new InvalidOperationException(
                    $"Warehouse stock {group.Key} no longer exists.");
            }

            var quantity = GetAllocationQuantity(group);

            if (stock.QuantityReserved < quantity)
            {
                throw new InvalidOperationException(
                    $"Reserved stock is insufficient for allocation {group.Key}.");
            }

            if (stock.QuantityOnHand < quantity)
            {
                throw new InvalidOperationException(
                    $"Quantity on hand is insufficient for allocation {group.Key}.");
            }

            stocks.Add(stock.Id, stock);
        }

        return (allocations, stocks);
    }

    private static int GetAllocationQuantity(
        IEnumerable<OrderItemStockAllocation> allocations)
    {
        var total = allocations.Sum(
            allocation => (long)allocation.Quantity);

        if (total > int.MaxValue)
        {
            throw new InvalidOperationException(
                "The stock allocation quantity exceeds the supported range.");
        }

        return (int)total;
    }

    private static void EnsureUtc(
        DateTimeOffset value,
        string parameterName)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timestamp must use UTC.",
                parameterName);
        }
    }

    private sealed record PlannedAllocation(
        OrderItem OrderItem,
        WarehouseStockReservationCandidate Candidate,
        int Quantity);
}