using HealthcareStore.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentEventConfiguration
    : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("payment_events", table =>
        {
            table.HasCheckConstraint(
                "CK_payment_events_ValidEventType",
                "\"EventType\" BETWEEN 1 AND 14");

            table.HasCheckConstraint(
                "CK_payment_events_ValidSource",
                "\"Source\" BETWEEN 1 AND 5");

            table.HasCheckConstraint(
                "CK_payment_events_NonNegativeAmount",
                "\"Amount\" IS NULL OR \"Amount\" >= 0");

            table.HasCheckConstraint(
                "CK_payment_events_AmountCurrencyPair",
                """
                ("Amount" IS NULL AND "Currency" IS NULL)
                OR ("Amount" IS NOT NULL AND "Currency" IS NOT NULL)
                """);

            table.HasCheckConstraint(
                "CK_payment_events_CurrencyLength",
                "\"Currency\" IS NULL OR length(\"Currency\") = 3");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.Property(x => x.PaymentAttemptId);

        builder.Property(x => x.EventType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProviderEventId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderOrderId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(128);

        builder.Property(x => x.Amount)
            .HasPrecision(14, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.RecordedAtUtc)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.ProviderEventId)
            .IsUnique()
            .HasFilter("\"ProviderEventId\" IS NOT NULL")
            .HasDatabaseName(
                "UX_payment_events_ProviderEventId");

        builder.HasIndex(x => new
        {
            x.PaymentId,
            x.RecordedAtUtc
        })
        .HasDatabaseName(
            "IX_payment_events_PaymentId_RecordedAtUtc");

        builder.HasIndex(x => new
        {
            x.PaymentAttemptId,
            x.RecordedAtUtc
        })
        .HasDatabaseName(
            "IX_payment_events_PaymentAttemptId_RecordedAtUtc");

        builder.HasIndex(x => x.ProviderPaymentId)
            .HasDatabaseName(
                "IX_payment_events_ProviderPaymentId");

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PaymentAttempt>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.PaymentId,
                x.PaymentAttemptId
            })
            .HasPrincipalKey(x => new
            {
                x.PaymentId,
                x.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
            }
}