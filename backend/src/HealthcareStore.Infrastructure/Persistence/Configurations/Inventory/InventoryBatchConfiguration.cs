using HealthcareStore.Domain.Inventory;
using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Inventory;

public sealed class InventoryBatchConfiguration
    : IEntityTypeConfiguration<InventoryBatch>
{
    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
    {
        builder.ToTable("inventory_batches", table =>
        {
            table.HasCheckConstraint(
                "CK_inventory_batches_Mrp_NonNegative",
                "\"Mrp\" >= 0");

            table.HasCheckConstraint(
                "CK_inventory_batches_SellableUntil_NotAfterExpiry",
                "\"SellableUntil\" IS NULL " +
                "OR \"ExpiryDate\" IS NULL " +
                "OR \"SellableUntil\" <= \"ExpiryDate\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductVariantId)
            .IsRequired();

        builder.Property(x => x.BatchNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ExpiryDate);

        builder.Property(x => x.SellableUntil);

        builder.Property(x => x.Mrp)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductVariantId,
            x.BatchNumber
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.ProductVariantId,
            x.SellableUntil
        });

        builder.HasOne<ProductVariant>()
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}