using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HealthcareStore.Infrastructure.Persistence.Helpers;

public static class PostgresExceptionHelper
{
    public static bool IsUniqueViolation(
        DbUpdateException exception,
        string databaseObjectName)
    {
        return HasPostgresViolation(
            exception,
            PostgresErrorCodes.UniqueViolation,
            databaseObjectName);
    }

    public static bool IsForeignKeyViolation(
        DbUpdateException exception,
        string? databaseObjectName = null)
    {
        return HasPostgresViolation(
            exception,
            PostgresErrorCodes.ForeignKeyViolation,
            databaseObjectName);
    }

    private static bool HasPostgresViolation(
        DbUpdateException exception,
        string sqlState,
        string? databaseObjectName)
    {
        for (
            Exception? current = exception;
            current is not null;
            current = current.InnerException)
        {
            if (current is not PostgresException postgresException)
                continue;

            var isMatchingViolation =
                postgresException.SqlState == sqlState ||
                (sqlState == PostgresErrorCodes.ForeignKeyViolation &&
                 postgresException.SqlState == PostgresErrorCodes.RestrictViolation);

            if (!isMatchingViolation)
                continue;

            if (databaseObjectName is null ||
                postgresException.ConstraintName == databaseObjectName)
            {
                return true;
            }
        }

        return false;
    }
}
