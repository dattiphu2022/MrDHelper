namespace MrDHelper.AppDomain.EfSqliteFts5;

public sealed record FtsSpec(
string EntityName,
string MainTable,
string FtsTable,
string IdColumn
);
