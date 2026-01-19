namespace MrDHelper.AppDomain.EfSqliteFts5;

public interface IHasGuidId
{
    Guid Id { get; }
}

public interface IFtsIndexed
{
    string BuildFtsAllText();
}
