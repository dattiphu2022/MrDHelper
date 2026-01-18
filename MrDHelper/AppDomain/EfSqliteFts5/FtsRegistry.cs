namespace MrDHelper.AppDomain.EfSqliteFts5;


public static class FtsRegistry
{
    // đăng ký 1 lần cho toàn app
    private static readonly Dictionary<Type, FtsSpec> _specs = new();
    private static readonly object _gate = new();

    public static void Register<TEntity>(string mainTable, string ftsTable, string idColumn = "Id")
        where TEntity : class, IHasGuidId, IFtsIndexed
    {
        mainTable = RequireSafeIdentifier(mainTable);
        ftsTable = RequireSafeIdentifier(ftsTable);
        idColumn = RequireSafeIdentifier(idColumn);

        var spec = new FtsSpec(typeof(TEntity).Name, mainTable, ftsTable, idColumn);

        lock (_gate)
        {
            _specs[typeof(TEntity)] = spec;
        }
    }

    public static bool TryGet<TEntity>(out FtsSpec spec) where TEntity : class
    {
        lock (_gate)
            return _specs.TryGetValue(typeof(TEntity), out spec!);
    }

    public static bool TryGet(Type entityType, out FtsSpec spec)
    {
        lock (_gate)
            return _specs.TryGetValue(entityType, out spec!);
    }

    public static IReadOnlyCollection<FtsSpec> All
    {
        get
        {
            lock (_gate)
                return _specs.Values.ToArray();
        }
    }

    private static string RequireSafeIdentifier(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Identifier rỗng.", nameof(s));

        foreach (var ch in s)
        {
            if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                throw new ArgumentException($"Identifier không an toàn: {s}");
        }
        return s;
    }
}
