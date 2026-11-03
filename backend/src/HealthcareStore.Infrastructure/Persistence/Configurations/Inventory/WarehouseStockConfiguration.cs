using HealthcareStore.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Inventory;

public sealed class WarehouseStockConfiguration
    : IEntityTypeConfiguration<WarehouseStock>
{
    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.ToTable("warehouse_stocks", table =>
        {
            table.HasCheckConstraint(
                "CK_warehouse_stocks_CostPrice_NonNegative",
                "\"CostPrice\" >= 0");

            table.HasCheckConstraint(
                "CK_warehouse_stocks_QuantityOnHand_NonNegative",
                "\"QuantityOnHand\" >= 0");

            table.HasCheckConstraint(
                "CK_warehouse_stocks_QuantityReserved_NonNegative",
                "\"QuantityReserved\" >= 0");

            table.HasCheckConstraint(
                "CK_warehouse_stocks_QuantityReserved_NotExceedingOnHand",
                "\"QuantityReserved\" <= \"QuantityOnHand\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InventoryBatchId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.CostPrice)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.QuantityOnHand)
            .IsRequired();

        builder.Property(x => x.QuantityReserved)
            .IsRequired();

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.InventoryBatchId,
            x.WarehouseId
        })
        .IsUnique();

        builder.HasIndex(x => x.WarehouseId);

        builder.HasOne<InventoryBatch>()
            .WithMany()
            .HasForeignKey(x => x.InventoryBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}