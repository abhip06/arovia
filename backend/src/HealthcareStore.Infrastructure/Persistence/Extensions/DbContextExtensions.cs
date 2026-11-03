using Microsoft.EntityFrameworkCore;

namespace HealthcareStore.Infrastructure.Persistence.Extensions;

public static class DbContextExtensions
{
    public static void Detach<TEntity>(
        this DbContext dbContext,
        TEntity entity)
        where TEntity : class
    {
        dbContext.Entry(entity).State = EntityState.Detached;
    }
}