using HealthcareStore.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.ParentCategoryId);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductType,
            x.Slug
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.ProductType,
            x.ParentCategoryId
        });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.ProductType,
                x.ParentCategoryId
            })
            .HasPrincipalKey(x => new
            {
                x.ProductType,
                x.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}