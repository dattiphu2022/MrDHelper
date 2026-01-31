using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MrDHelper.Models;
using MrDHelper.MudBlazor.Search;
using System.Data.Common;

namespace MrDHelper.AppDomain.EfSqliteFts5
{
    public sealed class FtsSearchService
    {
        private readonly DbContext _db;

        public FtsSearchService(DbContext db) => _db = db;

        public Task<PagedResult<TEntity>> SearchAsync<TEntity>(
        SearchQuery query,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? defaultOrder,
        CancellationToken ct = default)
        where TEntity : class, IHasGuidId, IFtsIndexed
        {
            var opts = defaultOrder is null
                ? null
                : new FtsSearchOptions<TEntity> { DefaultOrder = defaultOrder };

            return SearchAsync<TEntity>(query,
                options: opts,
                ct: ct);
        }

        public async Task<PagedResult<TEntity>> SearchAsync<TEntity>(
            SearchQuery query,
            FtsSearchOptions<TEntity>? options = null, CancellationToken ct = default)
            where TEntity : class, IHasGuidId, IFtsIndexed
        {
            if (!FtsRegistry.TryGet<TEntity>(out var spec))
                throw new InvalidOperationException($"Entity {typeof(TEntity).Name} chưa đăng ký FTS trong FtsRegistry.");

            if (query.Page < 0) query.Page = 0;
            if (query.PageSize <= 0) query.PageSize = 20;


            if (string.IsNullOrWhiteSpace(query.Search))
            {
                var baseQ = _db.Set<TEntity>().AsNoTracking();

                if (options?.Include is not null)
                {
                    baseQ = options.Include(baseQ);
                }

                var total0 = await baseQ.CountAsync(ct);

                IOrderedQueryable<TEntity> ordered =
                options?.DefaultOrder is not null
                    ? options.DefaultOrder(baseQ)
                    : baseQ.OrderBy(x => x.Id); // paging ổn định

                var items0 = await ordered
                    .Skip(query.Page * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(ct);

                return new PagedResult<TEntity>(items0, total0, query.Page, query.PageSize);
            }



            var match = VietFts.BuildMatchQuery(query.Search ?? string.Empty, prefix: true);
            if (string.IsNullOrWhiteSpace(match))
                return new PagedResult<TEntity>(new List<TEntity>(), 0, query.Page, query.PageSize);

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
                ("Take", query.PageSize),
                ("Skip", query.Page * query.PageSize));

            if (ids.Count == 0)
                return new PagedResult<TEntity>(new List<TEntity>(), 0, query.Page, query.PageSize);

            // Join về entity thật
            IQueryable<TEntity> entityQ = _db.Set<TEntity>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id));

            if (options?.Include is not null)
                entityQ = options.Include(entityQ);

            var items = await entityQ.ToListAsync(ct);

            // giữ thứ tự rank của FTS
            var order = ids.Select((id, idx) => new { id, idx }).ToDictionary(x => x.id, x => x.idx);
            items = items.OrderBy(x => order[x.Id]).ToList();

            return new PagedResult<TEntity>(items, total, query.Page, query.PageSize);
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
