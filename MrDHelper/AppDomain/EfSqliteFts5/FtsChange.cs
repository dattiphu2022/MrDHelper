namespace MrDHelper.AppDomain.EfSqliteFts5;
public enum FtsChangeKind { Upsert, Delete }

public sealed class FtsChange
{
    public required FtsSpec Spec { get; init; }
    public required FtsChangeKind Kind { get; init; }
    public required Guid EntityId { get; init; }

    public string? AllText { get; init; }
    public string? AllTextNd { get; init; }

    public static FtsChange Upsert(FtsSpec spec, Guid id, string all, string nd)
        => new() { Spec = spec, Kind = FtsChangeKind.Upsert, EntityId = id, AllText = all, AllTextNd = nd };

    public static FtsChange Delete(FtsSpec spec, Guid id)
        => new() { Spec = spec, Kind = FtsChangeKind.Delete, EntityId = id };
}
