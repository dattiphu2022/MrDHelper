using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MrDHelper.AppDomain.AuditableModelBase;
using MrDHelper.AppDomain.AuditHistoryModel;
using System.Security.Claims;

namespace MrDHelper.AppData.Base
{
    // Fix: Add a constraint to ensure TRole is derived from IdentityRole<Guid>  
    public abstract class AuditDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, Guid>
        where TUser : IdentityUser<Guid>
        where TRole : IdentityRole<Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected AuditDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public abstract DbSet<AuditHistory> AuditHistories { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var auditEntries = OnBeforeSaveChanges();
            if (!auditEntries.Any()) return await base.SaveChangesAsync(cancellationToken);

            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChangesAsync(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                if (entry.Entity == null) continue;

                var now = DateTimeOffset.UtcNow;
                var userName = GetCurrentUser();

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedBy = userName;
                        break;
                    case EntityState.Modified:
                        entry.Entity.EditedDate = now;
                        entry.Entity.EditedBy = userName;
                        break;
                    case EntityState.Deleted:
                        entry.Entity.IsDeleted = true;
                        entry.State = EntityState.Modified;
                        break;
                }

                foreach (var property in entry.Properties)
                {
                    if (!property.IsModified || property.Metadata.Name == "UpdatedDate" || property.Metadata.Name == "UpdatedBy")
                        continue;

                    var entityId = entry.Properties.Any(p => p.Metadata.Name == "Id")
                        ? entry.Property("Id")?.CurrentValue?.ToString()
                        : null;

                    if (string.IsNullOrEmpty(entityId)) continue;

                    auditEntries.Add(new AuditEntry
                    {
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entityId,
                        PropertyName = property.Metadata.Name,
                        OldValue = property.OriginalValue?.ToString(),
                        NewValue = property.CurrentValue?.ToString(),
                        ChangedAt = now,
                        ChangedBy = userName
                    });
                }
            }

            return auditEntries;
        }

        private async Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries)
        {
            if (!auditEntries.Any()) return;

            await Set<AuditHistory>().AddRangeAsync(auditEntries.Select(auditEntry => new AuditHistory
            {
                EntityName = auditEntry.EntityName,
                EntityId = auditEntry.EntityId,
                PropertyName = auditEntry.PropertyName,
                OldValue = auditEntry.OldValue,
                NewValue = auditEntry.NewValue,
                ChangedAt = auditEntry.ChangedAt,
                ChangedBy = auditEntry.ChangedBy
            }));

            await base.SaveChangesAsync();
        }

        protected virtual string GetCurrentUser()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        }
    }

    internal class AuditEntry
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
    }
}
