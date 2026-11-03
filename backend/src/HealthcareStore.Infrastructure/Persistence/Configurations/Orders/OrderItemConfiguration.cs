using HealthcareStore.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items", table =>
        {
            table.HasCheckConstraint(
                "CK_order_items_ValidPackageQuantity",
                "\"PackageQuantity\" > 0");

            table.HasCheckConstraint(
                "CK_order_items_ValidQuantity",
                "\"Quantity\" > 0");

            table.HasCheckConstraint(
                "CK_order_items_ValidUnitPrices",
                """
                "UnitMrp" >= 0
                AND "UnitSellingPrice" >= 0
                AND "UnitSellingPrice" <= "UnitMrp"
                """);

            table.HasCheckConstraint(
                "CK_order_items_NonNegativeTax",
                "\"LineTaxAmount\" >= 0");

            table.HasCheckConstraint(
                "CK_order_items_ValidDiscount",
                """
                "LineDiscountAmount" >= 0
                AND "LineDiscountAmount" <=
                    ("UnitSellingPrice" * "Quantity")
                """);
        });

        builder.HasKey(item => item.Id)
            .HasName("PK_order_items");

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.HasIndex(item => item.OrderId)
            .HasDatabaseName("IX_order_items_OrderId");

        builder.HasOne<Order>()
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_order_items_orders_OrderId");

        builder.Property(item => item.ProductVariantId)
            .IsRequired();

        builder.Property(item => item.ProductName)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(item => item.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(item => item.PackageQuantity)
            .HasPrecision(12, 3);

        builder.Property(item => item.PackageUnit)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.UnitMrp)
            .HasPrecision(14, 2);

        builder.Property(item => item.UnitSellingPrice)
            .HasPrecision(14, 2);

        builder.Property(item => item.LineTaxAmount)
            .HasPrecision(14, 2);

        builder.Property(item => item.LineDiscountAmount)
            .HasPrecision(14, 2);

        builder.Property(item => item.RequiresPrescription)
            .IsRequired();

        builder.Ignore(item => item.LineSubtotal);
        builder.Ignore(item => item.LineTotal);
    }
}
