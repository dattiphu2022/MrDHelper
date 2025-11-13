using Microsoft.EntityFrameworkCore;
using MrDHelper.AppDomain.AuditHistoryModel;
using NKE.BlazorUI.AppData.Base;

namespace NKE.BlazorUI.AppData.Repositories
{
    public sealed class AuditHistory_Repository(DbContext context)
                        : RepositoryBase<AuditHistory>(context)
    {
        public override async Task<IEnumerable<AuditHistory>> GetAllWithFullyLoadedProperties(CancellationToken cancellationToken)
        {
            var allRecords = await _context.Set<AuditHistory>()
                .ToListAsync(cancellationToken);

            return allRecords;
        }
    }
}