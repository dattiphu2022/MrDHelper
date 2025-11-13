using Microsoft.EntityFrameworkCore;
using MrDHelper.AppData.Base;
using MrDHelper.AppDomain.AuditHistoryModel;

namespace MrDHelper.AppData.Repositories;

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