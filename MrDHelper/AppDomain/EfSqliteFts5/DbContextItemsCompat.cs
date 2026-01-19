using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace MrDHelper.AppDomain.EfSqliteFts5;

public static class DbContextItemsCompat
{
    private static readonly ConditionalWeakTable<DbContext, Dictionary<string, object?>> _itemsTable = new();

    public static Dictionary<string, object?> Items(this DbContext context)
    {
        return _itemsTable.GetOrCreateValue(context);
    }
}
