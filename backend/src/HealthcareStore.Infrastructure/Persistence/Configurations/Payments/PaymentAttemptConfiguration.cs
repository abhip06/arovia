using HealthcareStore.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Payments;

public sealed class PaymentAttemptConfiguration
    : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts", table =>
        {
            table.HasCheckConstraint(
                "CK_payment_attempts_ValidStatus",
                "\"Status\" BETWEEN 1 AND 7");

            table.HasCheckConstraint(
                "CK_payment_attempts_ValidMethod",
                "\"Method\" IS NULL OR \"Method\" BETWEEN 1 AND 3");

            table.HasCheckConstraint(
                "CK_payment_attempts_RequestHash",
                "\"RequestHash\" ~ '^[0-9a-f]{64}$'");

            table.HasCheckConstraint(
                "CK_payment_attempts_CaptureTimestamp",
                """
                ("Status" = 5 AND "CapturedAtUtc" IS NOT NULL)
                OR ("Status" <> 5 AND "CapturedAtUtc" IS NULL)
                """);
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(128);

        builder.Property(x => x.Method)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FailureCode)
            .HasMaxLength(100);

        builder.Property(x => x.FailureDescription)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.PaymentId,
            x.IdempotencyKey
        })
        .IsUnique()
        .HasDatabaseName(
            "UX_payment_attempts_PaymentId_IdempotencyKey");

        builder.HasIndex(x => x.ProviderPaymentId)
            .IsUnique()
            .HasFilter("\"ProviderPaymentId\" IS NOT NULL")
            .HasDatabaseName(
                "UX_payment_attempts_ProviderPaymentId");

        builder.HasIndex(x => new
        {
            x.PaymentId,
            x.Status
        })
        .HasDatabaseName(
            "IX_payment_attempts_PaymentId_Status");

        builder.HasAlternateKey(x => new
        {
            x.PaymentId,
            x.Id
        })
        .HasName("AK_payment_attempts_PaymentId_Id");

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}