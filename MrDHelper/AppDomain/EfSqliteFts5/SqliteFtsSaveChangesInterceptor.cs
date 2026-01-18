using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace MrDHelper.AppDomain.EfSqliteFts5
{
    public sealed class SqliteFtsSaveChangesInterceptor : SaveChangesInterceptor
    {
        private const string PendingKey = "__VietFtsSearch_PendingChanges";

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            var db = eventData.Context;
            if (db is null) return result;

            var pending = CollectFtsChanges(db);
            db.Items()[PendingKey] = pending;

            return result;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var db = eventData.Context;
            if (db is null) return result;

            var pending = CollectFtsChanges(db);
            db.Items()[PendingKey] = pending;

            return result;
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            var db = eventData.Context;
            if (db is null) return result;

            ApplyIfAnyAsync(db, CancellationToken.None).GetAwaiter().GetResult();
            return result;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var db = eventData.Context;
            if (db is null) return result;

            await ApplyIfAnyAsync(db, cancellationToken);
            return result;
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            // fail => bỏ pending
            var db = eventData.Context;
            if (db is null) return;
            db.Items().Remove(PendingKey);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            var db = eventData.Context;
            if (db is null) return Task.CompletedTask;
            db.Items().Remove(PendingKey);
            return Task.CompletedTask;
        }

        private static List<FtsChange> CollectFtsChanges(DbContext db)
        {
            db.ChangeTracker.DetectChanges();

            var list = new List<FtsChange>();

            foreach (var entry in db.ChangeTracker.Entries())
            {
                if (entry.Entity is not IHasGuidId entity) continue;
                if (entry.Entity is not IFtsIndexed ftsDoc) continue;

                if (!FtsRegistry.TryGet(entry.Entity.GetType(), out var spec)) continue;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var all = ftsDoc.BuildFtsAllText();
                    var nd = VietFts.Normalize(all);
                    list.Add(FtsChange.Upsert(spec, entity.Id, all, nd));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    list.Add(FtsChange.Delete(spec, entity.Id));
                }
            }

            return list;
        }

        private static async Task ApplyIfAnyAsync(DbContext db, CancellationToken ct)
        {
            if (!db.Items().TryGetValue(PendingKey, out var obj) || obj is not List<FtsChange> pending || pending.Count == 0)
                return;

            db.Items().Remove(PendingKey);

            await ApplyFtsChangesAsync(db, pending, ct);
        }

        private static async Task ApplyFtsChangesAsync(DbContext db, List<FtsChange> changes, CancellationToken ct)
        {
            await using var con = db.Database.GetDbConnection();
            if (con.State != System.Data.ConnectionState.Open)
                await con.OpenAsync(ct);

            // Bám transaction nếu có
            var tx = db.Database.CurrentTransaction?.GetDbTransaction();

            foreach (var c in changes)
            {
                if (c.Kind == FtsChangeKind.Delete)
                {
                    await ExecAsync(con, tx,
                        $"DELETE FROM {c.Spec.FtsTable} WHERE EntityId = @Id;",
                        ct,
                        ("Id", c.EntityId.ToString()));
                }
                else
                {
                    await ExecAsync(con, tx,
                        $@"
DELETE FROM {c.Spec.FtsTable} WHERE EntityId = @Id;
INSERT INTO {c.Spec.FtsTable} (EntityId, AllText, AllTextNd)
VALUES (@Id, @AllText, @AllTextNd);",
                        ct,
                        ("Id", c.EntityId.ToString()),
                        ("AllText", c.AllText ?? ""),
                        ("AllTextNd", c.AllTextNd ?? ""));
                }
            }
        }

        private static async Task ExecAsync(
            DbConnection con,
            DbTransaction? tx,
            string sql,
            CancellationToken ct,
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

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

}
