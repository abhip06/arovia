using HealthcareStore.Domain.Orders;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", table =>
        {
            table.HasCheckConstraint(
                "CK_orders_NonNegativeTotals",
                """
                "Subtotal" >= 0
                AND "DiscountTotal" >= 0
                AND "DiscountTotal" <= "Subtotal"
                AND "TaxTotal" >= 0
                AND "ShippingFee" >= 0
                AND "GrandTotal" >= 0
                """);

            table.HasCheckConstraint(
                "CK_orders_GrandTotalMatchesBreakdown",
                """
                "GrandTotal" =
                    "Subtotal" - "DiscountTotal" + "TaxTotal" + "ShippingFee"
                """);

            table.HasCheckConstraint(
                "CK_orders_ValidStatus",
                "\"Status\" BETWEEN 1 AND 7");

            table.HasCheckConstraint(
                "CK_orders_CurrencyLength",
                "length(\"Currency\") = 3");
        });

        builder.HasKey(order => order.Id)
            .HasName("PK_orders");

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.OrderNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique()
            .HasDatabaseName("UX_orders_OrderNumber");

        builder.Property(order => order.UserId)
            .IsRequired();

        builder.HasIndex(order => new
        {
            order.UserId,
            order.CreatedAtUtc
        })
        .HasDatabaseName("IX_orders_UserId_CreatedAtUtc");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_orders_users_UserId");

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(order => order.Subtotal)
            .HasPrecision(14, 2);

        builder.Property(order => order.DiscountTotal)
            .HasPrecision(14, 2);

        builder.Property(order => order.TaxTotal)
            .HasPrecision(14, 2);

        builder.Property(order => order.ShippingFee)
            .HasPrecision(14, 2);

        builder.Property(order => order.GrandTotal)
            .HasPrecision(14, 2);

        builder.Property(order => order.CreatedAtUtc)
            .IsRequired();

        builder.Property(order => order.UpdatedAtUtc)
            .IsRequired();

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ComplexProperty(order => order.ShippingAddress, address =>
        {
            address.Property(value => value.RecipientName)
                .HasMaxLength(120)
                .HasColumnName("ShippingAddress_RecipientName")
                .IsRequired();

            address.Property(value => value.RecipientPhone)
                .HasMaxLength(32)
                .HasColumnName("ShippingAddress_RecipientPhone")
                .IsRequired();

            address.Property(value => value.AddressLine1)
                .HasMaxLength(250)
                .HasColumnName("ShippingAddress_AddressLine1")
                .IsRequired();

            address.Property(value => value.AddressLine2)
                .HasMaxLength(250)
                .HasColumnName("ShippingAddress_AddressLine2");

            address.Property(value => value.City)
                .HasMaxLength(100)
                .HasColumnName("ShippingAddress_City")
                .IsRequired();

            address.Property(value => value.State)
                .HasMaxLength(100)
                .HasColumnName("ShippingAddress_State")
                .IsRequired();

            address.Property(value => value.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("ShippingAddress_PostalCode")
                .IsRequired();

            address.Property(value => value.Country)
                .HasMaxLength(100)
                .HasColumnName("ShippingAddress_Country")
                .IsRequired();
        });
    }
}