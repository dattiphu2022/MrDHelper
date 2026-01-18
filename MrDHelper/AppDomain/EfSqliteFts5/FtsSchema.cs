using Microsoft.EntityFrameworkCore;

namespace MrDHelper.AppDomain.EfSqliteFts5;

public static class FtsSchema
{
    public static async Task EnsureFtsTablesAsync(DbContext db, CancellationToken ct = default)
    {
        foreach (var spec in FtsRegistry.All)
        {
            // Identifier đã whitelist trong registry => có thể ghép string an toàn
            var sql = $@"
CREATE VIRTUAL TABLE IF NOT EXISTS {spec.FtsTable}
USING fts5(
    EntityId UNINDEXED,
    AllText,
    AllTextNd,
    tokenize='unicode61 remove_diacritics 2',
    prefix='2 3 4'
);";

            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }
}
