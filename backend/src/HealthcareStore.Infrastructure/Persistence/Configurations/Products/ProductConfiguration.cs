using HealthcareStore.Domain.Catalog;
using HealthcareStore.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Products;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", table =>
        {
            table.HasCheckConstraint(
                "CK_products_TaxRate_Range",
                "\"TaxRate\" >= 0 AND \"TaxRate\" <= 100");

            table.HasCheckConstraint(
                "CK_products_ReturnWindowDays_NonNegative",
                "\"ReturnWindowDays\" >= 0");

            table.HasCheckConstraint(
                "CK_products_ReturnPolicy",
                "\"Returnable\" OR \"ReturnWindowDays\" = 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(x => x.ProductType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ManufacturerId)
            .IsRequired();

        builder.Property(x => x.BrandId);

        builder.Property(x => x.RequiresPrescription)
            .IsRequired();

        builder.Property(x => x.HsnCode)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(x => x.TaxRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Returnable)
            .IsRequired();

        builder.Property(x => x.ReturnWindowDays)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.ManufacturerId);

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new
        {
            x.ProductType,
            x.ManufacturerId,
            x.BrandId,
            x.NormalizedName
        })
        .HasDatabaseName("UX_products_CatalogIdentity")
        .IsUnique()
        .AreNullsDistinct(false);

        builder.HasMany<ProductIngredient>(x => x.Ingredients)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Brand>()
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}