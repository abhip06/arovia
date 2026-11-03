using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Products;

public sealed class ProductPriceConfiguration
    : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("product_prices", table =>
        {
            table.HasCheckConstraint(
                "CK_product_prices_SellingPrice_NonNegative",
                "\"SellingPrice\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductVariantId)
            .IsRequired();

        builder.Property(x => x.SellingPrice)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductVariantId,
            x.EffectiveFrom
        })
        .IsUnique();

        builder.HasOne<ProductVariant>()
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}