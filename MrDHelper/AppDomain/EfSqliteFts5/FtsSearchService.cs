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

            return SearchAsync<TEntity>(query, options: opts, ct: ct);
        }

        public async Task<PagedResult<TEntity>> SearchAsync<TEntity>(
            SearchQuery query,
            FtsSearchOptions<TEntity>? options = null,
            CancellationToken ct = default)
            where TEntity : class, IHasGuidId, IFtsIndexed
        {
            if (!FtsRegistry.TryGet<TEntity>(out var spec))
                throw new InvalidOperationException($"Entity {typeof(TEntity).Name} has not been registered for FTS in FtsRegistry.");

            if (query.Page < 0) query.Page = 0;
            if (query.PageSize <= 0) query.PageSize = 20;

            // ===== Base query (apply Filter/Include) =====
            IQueryable<TEntity> baseQ = _db.Set<TEntity>().AsNoTracking();

            if (options?.Filter is not null)
                baseQ = options.Filter(baseQ);

            // Apply Include only when entity loading is required.
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = options?.Include;

            // ===== CASE 1: no keyword => normal EF paging with filter =====
            if (string.IsNullOrWhiteSpace(query.Search))
            {
                var total0 = await baseQ.CountAsync(ct);

                IQueryable<TEntity> q = baseQ;

                if (include is not null)
                    q = include(q);

                IOrderedQueryable<TEntity> ordered =
                    options?.DefaultOrder is not null
                        ? options.DefaultOrder(q)
                        : q.OrderBy(x => x.Id);


                var items0 = await ordered
                    .Skip(query.Page * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(ct);

                return new PagedResult<TEntity>(items0, total0, query.Page, query.PageSize);
            }

            // ===== CASE 2: keyword => FTS + bm25 rank =====
            var match = VietFts.BuildMatchQuery(query.Search ?? string.Empty, prefix: true);
            if (string.IsNullOrWhiteSpace(match))
                return new PagedResult<TEntity>(new List<TEntity>(), 0, query.Page, query.PageSize);

            await using var con = _db.Database.GetDbConnection();
            if (con.State != System.Data.ConnectionState.Open)
                await con.OpenAsync(ct);

            // With a filter, we cannot fetch LIMIT/OFFSET directly and filter afterward because the page may become empty.
            // Scan in chunks and filter through baseQ before determining the requested page.

            var pageSize = query.PageSize;
            var needStart = query.Page * pageSize;
            var needEnd = needStart + pageSize;

            var scanMultiplier = Math.Max(2, options?.ScanMultiplier ?? 10);
            var chunkSize = pageSize * scanMultiplier;

            var maxScanPages = Math.Max(10, options?.MaxScanPages ?? 200);
            var maxScan = pageSize * maxScanPages;

            var acceptedIds = new List<Guid>(capacity: needEnd + pageSize); // Filtered ids in rank order.
            var acceptedSet = new HashSet<Guid>(); // Prevent duplicates.

            var totalAccepted = 0; // Total matching the filter.
            var offset = 0;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // Read one chunk of ranked ids from FTS.
                var chunkIds = await QueryGuidListAsync(
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
                    ("Take", chunkSize),
                    ("Skip", offset));

                if (chunkIds.Count == 0)
                    break;

                offset += chunkIds.Count;

                // Filter chunkIds through baseQ so the result respects all upstream filters.
                // This query runs in the database as WHERE Id IN (...).
                var validInDb = await baseQ
                    .Where(x => chunkIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(ct);

                if (validInDb.Count > 0)
                {
                    var validSet = validInDb.ToHashSet();

                    // Preserve the rank order from chunkIds.
                    foreach (var id in chunkIds)
                    {
                        if (!validSet.Contains(id)) continue;
                        if (acceptedSet.Add(id))
                            acceptedIds.Add(id);
                    }

                    totalAccepted += validInDb.Count;
                }

                // Stop once we have enough data for the requested page.
                if (acceptedIds.Count >= needEnd)
                {
                    // We still need an accurate total count, so continue scanning unless maxScan is reached.
                    if (offset >= maxScan)
                        break;

                    // If an approximate TotalCount is acceptable, you could break here instead.
                    // This implementation keeps scanning to preserve accurate counts.
                }

                // Prevent excessively heavy scanning.
                if (offset >= maxScan)
                    break;
            }

            // Total after filtering.
            // totalAccepted counts valid ids per chunk, but duplicates across chunks are possible, although rare.
            // For simplicity and correctness, use acceptedSet.Count as the distinct total.
            // If scanning stops because of maxScan, the total may be lower than the true absolute total.
            var total = acceptedSet.Count;

            if (acceptedIds.Count == 0)
                return new PagedResult<TEntity>(new List<TEntity>(), 0, query.Page, query.PageSize);

            // Extract the ids for the current page.
            var pageIds = acceptedIds
                .Skip(needStart)
                .Take(pageSize)
                .ToList();

            if (pageIds.Count == 0)
                return new PagedResult<TEntity>(new List<TEntity>(), total, query.Page, query.PageSize);

            // Load entities for the current page ids.
            IQueryable<TEntity> entityQ = _db.Set<TEntity>().AsNoTracking()
                .Where(x => pageIds.Contains(x.Id));

            // Apply include when needed.
            if (include is not null)
                entityQ = include(entityQ);

            var items = await entityQ.ToListAsync(ct);

            // Preserve rank order.
            var order = pageIds.Select((id, idx) => new { id, idx }).ToDictionary(x => x.id, x => x.idx);
            items = items.OrderBy(x => order[x.Id]).ToList();

            return new PagedResult<TEntity>(items, total, query.Page, query.PageSize);
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
