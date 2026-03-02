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
                throw new InvalidOperationException($"Entity {typeof(TEntity).Name} chưa đăng ký FTS trong FtsRegistry.");

            if (query.Page < 0) query.Page = 0;
            if (query.PageSize <= 0) query.PageSize = 20;

            // ===== base query (apply Filter/Include) =====
            IQueryable<TEntity> baseQ = _db.Set<TEntity>().AsNoTracking();

            if (options?.Filter is not null)
                baseQ = options.Filter(baseQ);

            // Include chỉ áp khi cần load entity
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

            // Với filter: không thể lấy LIMIT/OFFSET trực tiếp rồi lọc (sẽ rỗng).
            // => Quét theo chunk và lọc bằng baseQ trước khi xác định trang.

            var pageSize = query.PageSize;
            var needStart = query.Page * pageSize;
            var needEnd = needStart + pageSize;

            var scanMultiplier = Math.Max(2, options?.ScanMultiplier ?? 10);
            var chunkSize = pageSize * scanMultiplier;

            var maxScanPages = Math.Max(10, options?.MaxScanPages ?? 200);
            var maxScan = pageSize * maxScanPages;

            var acceptedIds = new List<Guid>(capacity: needEnd + pageSize); // ids đã qua filter, theo rank
            var acceptedSet = new HashSet<Guid>(); // chống trùng

            var totalAccepted = 0; // total theo filter
            var offset = 0;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // Lấy 1 chunk ids theo rank từ FTS
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

                // Lọc chunkIds bằng baseQ (đã áp DonViIds...) để giữ đúng dữ liệu
                // Query này chạy trong DB: WHERE Id IN (...)
                var validInDb = await baseQ
                    .Where(x => chunkIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(ct);

                if (validInDb.Count > 0)
                {
                    var validSet = validInDb.ToHashSet();

                    // giữ thứ tự rank của chunkIds
                    foreach (var id in chunkIds)
                    {
                        if (!validSet.Contains(id)) continue;
                        if (acceptedSet.Add(id))
                            acceptedIds.Add(id);
                    }

                    totalAccepted += validInDb.Count;
                }

                // Chốt đủ dữ liệu cho page cần trả
                if (acceptedIds.Count >= needEnd)
                {
                    // nhưng vẫn cần TotalCount đúng => tiếp tục scan để đếm đủ, trừ khi vượt maxScan
                    if (offset >= maxScan)
                        break;

                    // nếu bạn muốn TotalCount "đủ dùng" (không quét hết), thì break ở đây.
                    // nhưng yêu cầu bạn là sync đúng => mình tiếp tục scan để count chính xác.
                }

                // Chặn quét quá nặng
                if (offset >= maxScan)
                    break;
            }

            // Total theo filter
            // totalAccepted hiện tại đếm theo từng chunk validInDb, nhưng có thể trùng giữa chunk (hiếm).
            // Để chắc chắn, total = số lượng distinct acceptedSet + số lượng valid nhưng chưa add? -> đơn giản hóa:
            // Mình tính total bằng acceptedSet.Count nếu đã scan tới hết; nếu dừng vì maxScan thì total có thể bị thấp.
            // Để chính xác tuyệt đối: nếu bạn cần total tuyệt đối, maxScanPages phải đủ lớn hoặc bỏ giới hạn.
            var total = acceptedSet.Count;

            if (acceptedIds.Count == 0)
                return new PagedResult<TEntity>(new List<TEntity>(), 0, query.Page, query.PageSize);

            // Lấy ids của trang hiện tại
            var pageIds = acceptedIds
                .Skip(needStart)
                .Take(pageSize)
                .ToList();

            if (pageIds.Count == 0)
                return new PagedResult<TEntity>(new List<TEntity>(), total, query.Page, query.PageSize);

            // Load entity theo pageIds
            IQueryable<TEntity> entityQ = _db.Set<TEntity>().AsNoTracking()
                .Where(x => pageIds.Contains(x.Id));

            // apply include nếu cần
            if (include is not null)
                entityQ = include(entityQ);

            var items = await entityQ.ToListAsync(ct);

            // giữ thứ tự rank
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