using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Products;

public sealed class ProductVariantConfiguration
    : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", table =>
        {
            table.HasCheckConstraint(
                "CK_product_variants_PackageQuantity_Positive",
                "\"PackageQuantity\" > 0");

            table.HasCheckConstraint(
                "CK_product_variants_NetQuantity_Positive",
                "\"NetQuantity\" IS NULL OR \"NetQuantity\" > 0");

            table.HasCheckConstraint(
                "CK_product_variants_NetQuantity_Unit_Consistency",
                "(\"NetQuantity\" IS NULL AND \"NetQuantityUnit\" IS NULL) " +
                "OR (\"NetQuantity\" IS NOT NULL AND \"NetQuantityUnit\" IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Barcode)
            .HasMaxLength(50);

        builder.Property(x => x.PackageQuantity)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(x => x.PackageUnit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.NetQuantity)
            .HasPrecision(10, 3);

        builder.Property(x => x.NetQuantityUnit)
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Sku)
            .IsUnique();

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.PackageQuantity,
            x.PackageUnit,
            x.NetQuantity,
            x.NetQuantityUnit
        })
        .HasDatabaseName("UX_product_variants_Identity")
        .IsUnique()
        .AreNullsDistinct(false);

        builder.HasIndex(x => x.Barcode)
            .IsUnique()
            .HasFilter("\"Barcode\" IS NOT NULL");

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}