using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class WarehouseServiceabilityRepository(
    ApplicationDbContext dbContext)
    : IWarehouseServiceabilityRepository
{
    public async Task<IReadOnlyList<Guid>>
        GetWarehouseIdsByPincodeAsync(
            string pincode,
            CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            pincode);

        var normalizedPincode =
            pincode.Trim();

        return await dbContext
            .WarehouseServiceablePincodes
            .AsNoTracking()
            .Where(
                x =>
                    x.Pincode == normalizedPincode &&
                    dbContext.Warehouses.Any(
                        warehouse =>
                            warehouse.Id == x.WarehouseId &&
                            warehouse.Status == WarehouseStatus.Active))
            .Select(
                x => x.WarehouseId)
            .Distinct()
            .ToListAsync(
                cancellationToken);
    }

    public Task<bool> WarehouseExistsAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        return dbContext.Warehouses
            .AsNoTracking()
            .AnyAsync(
                warehouse => warehouse.Id == warehouseId,
                cancellationToken);
    }

    public async Task<int> AddMappingsAsync(
        IReadOnlyCollection<WarehouseServiceablePincode> mappings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        if (mappings.Count == 0)
        {
            return 0;
        }

        var firstWarehouseId =
            mappings.First().WarehouseId;

        if (mappings.Any(
                mapping =>
                    mapping.WarehouseId != firstWarehouseId))
        {
            throw new ArgumentException(
                "All serviceable pincode mappings must belong to the same warehouse.",
                nameof(mappings));
        }

        var values = new string[mappings.Count];
        var parameters = new List<object>(
            mappings.Count * 3);

        var index = 0;

        foreach (var mapping in mappings)
        {
            var warehouseIdParameter =
                new NpgsqlParameter(
                    $"p_warehouse_id_{index}",
                    NpgsqlDbType.Uuid)
                {
                    Value = mapping.WarehouseId
                };

            var pincodeParameter =
                new NpgsqlParameter(
                    $"p_pincode_{index}",
                    NpgsqlDbType.Varchar)
                {
                    Value = mapping.Pincode
                };

            var createdAtParameter =
                new NpgsqlParameter(
                    $"p_created_at_{index}",
                    NpgsqlDbType.TimestampTz)
                {
                    Value = mapping.CreatedAt
                };

            values[index] =
                $"(@p_warehouse_id_{index}, " +
                $"@p_pincode_{index}, " +
                $"@p_created_at_{index})";

            parameters.Add(warehouseIdParameter);
            parameters.Add(pincodeParameter);
            parameters.Add(createdAtParameter);

            index++;
        }

        var sql =
            $"""
            INSERT INTO warehouse_serviceable_pincodes
                ("WarehouseId", "Pincode", "CreatedAt")
            VALUES
                {string.Join(",\n", values)}
            ON CONFLICT ("WarehouseId", "Pincode")
            DO NOTHING;
            """;

        return await dbContext.Database.ExecuteSqlRawAsync(
            sql,
            parameters,
            cancellationToken);
    }

    public async Task<int> RemoveMappingsAsync(
        Guid warehouseId,
        IReadOnlyCollection<string> pincodes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pincodes);

        if (warehouseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Warehouse ID cannot be empty.",
                nameof(warehouseId));
        }

        if (pincodes.Count == 0)
        {
            return 0;
        }

        return await dbContext
            .WarehouseServiceablePincodes
            .Where(
                mapping =>
                    mapping.WarehouseId == warehouseId &&
                    pincodes.Contains(mapping.Pincode))
            .ExecuteDeleteAsync(
                cancellationToken);
    }
}