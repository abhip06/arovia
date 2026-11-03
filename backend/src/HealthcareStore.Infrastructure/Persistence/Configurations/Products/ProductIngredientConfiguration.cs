using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Products;

public sealed class ProductIngredientConfiguration
    : IEntityTypeConfiguration<ProductIngredient>
{
    public void Configure(EntityTypeBuilder<ProductIngredient> builder)
    {
        builder.ToTable("product_ingredients", table =>
        {
            table.HasCheckConstraint(
                "CK_product_ingredients_Strength_Positive",
                "\"StrengthValue\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ActiveIngredientId)
            .IsRequired();

        builder.Property(x => x.StrengthValue)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(x => x.StrengthUnit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.ActiveIngredientId,
            x.Role
        })
        .IsUnique();

        builder.HasIndex(x => x.ActiveIngredientId);

        builder.HasOne<ActiveIngredient>()
            .WithMany()
            .HasForeignKey(x => x.ActiveIngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}