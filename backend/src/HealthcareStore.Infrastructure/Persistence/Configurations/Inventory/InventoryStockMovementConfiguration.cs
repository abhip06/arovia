using HealthcareStore.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Inventory;

public sealed class InventoryStockMovementConfiguration
    : IEntityTypeConfiguration<InventoryStockMovement>
{
    public void Configure(EntityTypeBuilder<InventoryStockMovement> builder)
    {
        builder.ToTable("inventory_stock_movements", table =>
        {
            table.HasCheckConstraint(
                "CK_inventory_stock_movements_QuantityDelta_NonZero",
                "\"QuantityDelta\" <> 0");

            table.HasCheckConstraint(
                "CK_inventory_stock_movements_Reference_Consistency",
                "(\"ReferenceType\" IS NULL AND \"ReferenceId\" IS NULL) " +
                "OR (\"ReferenceType\" IS NOT NULL AND \"ReferenceId\" IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WarehouseStockId)
            .IsRequired();

        builder.Property(x => x.MovementType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.QuantityDelta)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(50);

        builder.Property(x => x.ReferenceId);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.WarehouseStockId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.ReferenceType,
            x.ReferenceId
        });

        builder.HasIndex(x => x.CreatedByUserId);

        builder.HasOne<WarehouseStock>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseStockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HealthcareStore.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}