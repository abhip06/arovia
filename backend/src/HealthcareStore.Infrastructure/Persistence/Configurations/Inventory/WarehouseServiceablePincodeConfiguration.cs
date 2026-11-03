using HealthcareStore.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareStore.Infrastructure.Persistence.Configurations.Inventory;

public sealed class WarehouseServiceablePincodeConfiguration
    : IEntityTypeConfiguration<WarehouseServiceablePincode>
{
    public void Configure(
        EntityTypeBuilder<WarehouseServiceablePincode> builder)
    {
        builder.ToTable(
            "warehouse_serviceable_pincodes");

        builder.HasKey(
            x => new
            {
                x.WarehouseId,
                x.Pincode
            });

        builder.Property(
                x => x.Pincode)
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(
                x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(
                x => x.WarehouseId)
            .OnDelete(
                DeleteBehavior.Restrict);

        builder.HasIndex(
                x => x.Pincode)
            .HasDatabaseName(
                "IX_warehouse_serviceable_pincodes_Pincode");
    }
}