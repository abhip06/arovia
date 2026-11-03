using HealthcareStore.Domain.Orders;
using HealthcareStore.Domain.Users;
using HealthcareStore.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<OrderIdempotencyRecord>
{
    public void Configure(
        EntityTypeBuilder<OrderIdempotencyRecord> builder)
    {
        builder.ToTable("order_idempotency_records", table =>
        {
            table.HasCheckConstraint(
                "CK_order_idempotency_records_ValidExpiry",
                "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");

            table.HasCheckConstraint(
                "CK_order_idempotency_records_ValidRequestHash",
                "\"RequestHash\" ~ '^[0-9a-f]{64}$'");
        });

        builder.HasKey(record => new
        {
            record.UserId,
            record.IdempotencyKey
        })
            .HasName("PK_order_idempotency_records");

        builder.Property(record => record.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.RequestHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.UserId)
            .IsRequired();

        builder.Property(record => record.OrderId)
            .IsRequired();

        builder.Property(record => record.CreatedAtUtc)
            .IsRequired();

        builder.Property(record => record.ExpiresAtUtc)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName(
                "FK_order_idempotency_records_users_UserId");

        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<OrderIdempotencyRecord>(record => record.OrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(
                "FK_order_idempotency_records_orders_OrderId");

        builder.HasIndex(record => record.OrderId)
            .IsUnique()
            .HasDatabaseName(
                "UX_order_idempotency_records_OrderId");

        builder.HasIndex(record => record.ExpiresAtUtc)
            .HasDatabaseName(
                "IX_order_idempotency_records_ExpiresAtUtc");
    }
}