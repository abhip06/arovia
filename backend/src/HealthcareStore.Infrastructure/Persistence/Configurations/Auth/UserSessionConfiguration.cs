using HealthcareStore.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Auth;

public sealed class UserSessionConfiguration
    : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RefreshTokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.TokenFamilyId)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.LastUsedAt);

        builder.Property(x => x.RevokedAt);

        builder.Property(x => x.ReplacedBySessionId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.RefreshTokenHash)
            .IsUnique();

        builder.HasIndex(x => x.TokenFamilyId);

        builder.HasIndex(x => x.ExpiresAt);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.TokenFamilyId
        });

        builder.HasOne<HealthcareStore.Domain.Users.User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserSession>()
            .WithMany()
            .HasForeignKey(x => x.ReplacedBySessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}