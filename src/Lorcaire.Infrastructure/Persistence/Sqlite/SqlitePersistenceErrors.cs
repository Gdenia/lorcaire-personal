using Lorcaire.Application.Errors;
using Microsoft.Data.Sqlite;

namespace Lorcaire.Infrastructure.Persistence.Sqlite;

internal static class SqlitePersistenceErrors
{
    public static ConflictException SaveConflict(
        string entityName,
        SqliteException exception) =>
        new(
            $"The {entityName} could not be saved because it conflicts " +
            "with existing data.",
            exception);

    public static ConflictException DeleteConflict(
        string entityName,
        SqliteException exception) =>
        new(
            $"The {entityName} cannot be deleted because other information " +
            "depends on it.",
            exception);

    public static ConflictException MissingDuringUpdate(
        string entityName) =>
        new(
            $"The {entityName} could not be updated because it no longer " +
            "exists.");
}
