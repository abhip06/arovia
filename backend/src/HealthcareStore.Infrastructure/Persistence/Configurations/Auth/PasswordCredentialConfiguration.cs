using HealthcareStore.Domain.Auth;
using HealthcareStore.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Auth;

public sealed class PasswordCredentialConfiguration : IEntityTypeConfiguration<PasswordCredential>
{
    public void Configure(EntityTypeBuilder<PasswordCredential> builder)
    {
        builder.ToTable("password_credentials");

        builder.HasKey(credential => credential.Id);

        builder.Property(credential => credential.UserId)
            .IsRequired();

        builder.HasIndex(credential => credential.UserId)
            .IsUnique();

        builder.Property(credential => credential.HashedPassword)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(credential => credential.CreatedAt)
            .IsRequired();

        builder.Property(credential => credential.UpdatedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<PasswordCredential>(
                credential => credential.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}