using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace MrDHelper.AppDomain.EfSqliteFts5
{
    public sealed class FtsSearchService
    {
        private readonly DbContext _db;

        public FtsSearchService(DbContext db) => _db = db;

        public async Task<(int Total, List<TEntity> Items)> SearchAsync<TEntity>(
            string search, int page, int pageSize, CancellationToken ct = default)
            where TEntity : class, IHasGuidId, IFtsIndexed
        {
            if (!FtsRegistry.TryGet<TEntity>(out var spec))
                throw new InvalidOperationException($"Entity {typeof(TEntity).Name} chưa đăng ký FTS trong FtsRegistry.");

            if (page < 0) page = 0;
            if (pageSize <= 0) pageSize = 20;

            var match = VietFts.BuildMatchQuery(search, prefix: true);
            if (string.IsNullOrWhiteSpace(match))
                return (0, new List<TEntity>());

            await using var con = _db.Database.GetDbConnection();
            if (con.State != System.Data.ConnectionState.Open)
                await con.OpenAsync(ct);

            // Total
            var total = await ScalarIntAsync(
                con,
                _db.Database.CurrentTransaction?.GetDbTransaction(),
                sql: $@"SELECT count(*) FROM {spec.FtsTable} WHERE AllTextNd MATCH @Match;",
                ct,
                ("Match", match));

            // ids theo rank
            var ids = await QueryGuidListAsync(
                con,
                _db.Database.CurrentTransaction?.GetDbTransaction(),
                sql: $@"
SELECT EntityId
FROM {spec.FtsTable}
WHERE AllTextNd MATCH @Match
ORDER BY bm25({spec.FtsTable})
LIMIT @Take OFFSET @Skip;",
                ct,
                ("Match", match),
                ("Take", pageSize),
                ("Skip", page * pageSize));

            if (ids.Count == 0)
                return (total, new List<TEntity>());

            // Join về entity thật
            var items = await _db.Set<TEntity>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(ct);

            // giữ thứ tự rank của FTS
            var order = ids.Select((id, idx) => new { id, idx }).ToDictionary(x => x.id, x => x.idx);
            items = items.OrderBy(x => order[x.Id]).ToList();

            return (total, items);
        }

        private static async Task<int> ScalarIntAsync(
            DbConnection con, DbTransaction? tx, string sql, CancellationToken ct,
            params (string Name, object? Value)[] parameters)
        {
            await using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = tx;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@" + name;
                p.Value = value ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            var obj = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(obj);
        }

        private static async Task<List<Guid>> QueryGuidListAsync(
            DbConnection con, DbTransaction? tx, string sql, CancellationToken ct,
            params (string Name, object? Value)[] parameters)
        {
            var list = new List<Guid>();

            await using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = tx;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@" + name;
                p.Value = value ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                list.Add(Guid.Parse(reader.GetString(0)));

            return list;
        }
    }
}
