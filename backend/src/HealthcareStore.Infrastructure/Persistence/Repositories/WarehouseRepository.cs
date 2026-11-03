using HealthcareStore.Application.Abstractions.Persistence;
using HealthcareStore.Application.Helpers;
using HealthcareStore.Domain.Inventory;
using HealthcareStore.Infrastructure.Persistence.Extensions;
using HealthcareStore.Infrastructure.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository(
    ApplicationDbContext dbContext) : IWarehouseRepository
{
    private const string CodeIndexName = "IX_warehouses_Code";

    public async Task<WarehouseInsertResult> TryCreateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken)
    {
        dbContext.Warehouses.Add(warehouse);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return WarehouseInsertResult.Created;
        }
        catch (DbUpdateException ex)
            when (PostgresExceptionHelper.IsUniqueViolation(ex, CodeIndexName))
        {
            dbContext.Detach(warehouse);

            return WarehouseInsertResult.DuplicateCode;
        }
    }

    public Task<Warehouse?> GetForUpdateAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return dbContext.Warehouses
            .FirstOrDefaultAsync(
                x => x.Id == warehouseId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<WarehouseStatus?> GetStatusAsync(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return dbContext.Warehouses
            .AsNoTracking()
            .Where(x => x.Id == warehouseId)
            .Select(x => (WarehouseStatus?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<WarehouseReadModel>> GetPageForReadAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Warehouses.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items =
            await query
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WarehouseReadModel(
                x.Code,
                x.Name,
                x.AddressLine1,
                x.AddressLine2,
                x.City,
                x.State,
                x.PostalCode,
                x.Country,
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseReadModel>(
            items,
            page,
            pageSize,
            totalCount);
    }
}