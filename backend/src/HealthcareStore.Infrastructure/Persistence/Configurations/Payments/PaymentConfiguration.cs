using HealthcareStore.Domain.Orders;
using HealthcareStore.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", table =>
        {
            table.HasCheckConstraint(
                "CK_payments_ValidAmount",
                "\"Amount\" > 0");

            table.HasCheckConstraint(
                "CK_payments_ValidStatus",
                "\"Status\" BETWEEN 1 AND 4");

            table.HasCheckConstraint(
                "CK_payments_ValidProvider",
                "\"Provider\" = 1");

            table.HasCheckConstraint(
                "CK_payments_ValidExpiry",
                "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");

            table.HasCheckConstraint(
                "CK_payments_CaptureTimestamp",
                """
                ("Status" = 2 AND "CapturedAtUtc" IS NOT NULL)
                OR ("Status" <> 2 AND "CapturedAtUtc" IS NULL)
                """);

            table.HasCheckConstraint(
                "CK_payments_CurrencyLength",
                "length(\"Currency\") = 3");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProviderOrderId)
            .HasMaxLength(128);

        builder.Property(x => x.Amount)
            .HasPrecision(14, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique()
            .HasDatabaseName("UX_payments_OrderId");

        builder.HasIndex(x => x.ProviderOrderId)
            .IsUnique()
            .HasFilter("\"ProviderOrderId\" IS NOT NULL")
            .HasDatabaseName(
                "UX_payments_ProviderOrderId");

        builder.HasIndex(x => new
        {
            x.Status,
            x.ExpiresAtUtc
        })
        .HasDatabaseName("IX_payments_Status_ExpiresAtUtc");

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}