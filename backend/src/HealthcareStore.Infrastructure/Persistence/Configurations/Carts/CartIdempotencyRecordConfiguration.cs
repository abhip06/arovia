using HealthcareStore.Domain.Users;
using HealthcareStore.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Carts;

public sealed class CartIdempotencyRecordConfiguration
    : IEntityTypeConfiguration<CartIdempotencyRecord>
{
    public void Configure(
        EntityTypeBuilder<CartIdempotencyRecord> builder)
    {
        builder.ToTable("cart_idempotency_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RequestHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IdempotencyKey
        })
        .IsUnique();

        builder.HasIndex(x => x.ExpiresAt);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}