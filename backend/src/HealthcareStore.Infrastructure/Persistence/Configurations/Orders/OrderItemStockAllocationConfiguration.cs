using HealthcareStore.Domain.Inventory;
using HealthcareStore.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderItemStockAllocationConfiguration
    : IEntityTypeConfiguration<OrderItemStockAllocation>
{
    public void Configure(
        EntityTypeBuilder<OrderItemStockAllocation> builder)
    {
        builder.ToTable("order_item_stock_allocations", table =>
        {
            table.HasCheckConstraint(
                "CK_order_item_stock_allocations_ValidQuantity",
                "\"Quantity\" > 0");

            table.HasCheckConstraint(
                "CK_order_item_stock_allocations_ValidStatus",
                "\"Status\" BETWEEN 1 AND 3");
        });

        builder.HasKey(allocation => allocation.Id)
            .HasName("PK_order_item_stock_allocations");

        builder.Property(allocation => allocation.Id)
            .ValueGeneratedNever();

        builder.Property(allocation => allocation.OrderItemId)
            .IsRequired();

        builder.Property(allocation => allocation.WarehouseStockId)
            .IsRequired();

        builder.Property(allocation => allocation.Quantity)
            .IsRequired();

        builder.Property(allocation => allocation.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(allocation => allocation.CreatedAtUtc)
            .IsRequired();

        builder.Property(allocation => allocation.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(allocation => new
        {
            allocation.OrderItemId,
            allocation.WarehouseStockId
        })
            .IsUnique()
            .HasDatabaseName(
                "UX_order_item_stock_allocations_OrderItemId_WarehouseStockId");

        builder.HasIndex(allocation => new
        {
            allocation.WarehouseStockId,
            allocation.Status
        })
            .HasDatabaseName(
                "IX_order_item_stock_allocations_WarehouseStockId_Status");

        builder.HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(allocation => allocation.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(
                "FK_order_item_stock_allocations_order_items_OrderItemId");

        builder.HasOne<WarehouseStock>()
            .WithMany()
            .HasForeignKey(allocation => allocation.WarehouseStockId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(
                "FK_order_item_stock_allocations_warehouse_stocks_WarehouseStockId");
    }
}