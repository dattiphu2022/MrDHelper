# MrDHelper

[![NuGet version](https://img.shields.io/nuget/v/MrDHelper?style=for-the-badge)](https://www.nuget.org/packages/MrDHelper)
[![NuGet downloads](https://img.shields.io/nuget/dt/MrDHelper?style=for-the-badge)](https://www.nuget.org/packages/MrDHelper)
[![License](https://img.shields.io/github/license/dattiphu2022/MrDHelper?style=for-the-badge)](LICENSE.txt)

Common helpers and extension methods for .NET projects, with support for:

- collection helpers
- string, bool, date, and geo utilities
- `Result` / `Result<T>` models
- `DisplayAttribute` reflection helpers
- EF Core paging and search extensions
- SQLite FTS5 integration
- MudBlazor search state helpers

## Target Framework

- `.NET 9`

## Installation

```bash
dotnet add package MrDHelper
```

## Quick Start

```csharp
using MrDHelper;
using MrDHelper.CollectionHelpers.IEnummerableHelper;
using MrDHelper.CollectionHelpers.IListHelper;
using MrDHelper.Models;

var values = new[] { 1, 2, 3 };
var total = 0;

values.ForEach(x => total += x);

var md5 = "hello".GetMd5();
var normalized = StringHelper.Normalize("  Ha Noi  ");

var result = Result<int>.Ok(total, "done");
```

## Main Features

### Collection Helpers

```csharp
using MrDHelper.CollectionHelpers.IEnummerableHelper;
using MrDHelper.CollectionHelpers.IListHelper;

IEnumerable<int> numbers = new[] { 1, 2, 3 };
await numbers.ForEachAsync(async x => await Task.Delay(x));

IList<string> items = new List<string> { "A" };
items.AddDummyItemsToMaximumCountOf(3, "-");
// Result: [ "A", "-", "-" ]
```

### Primitive and Value Helpers

```csharp
using MrDHelper;
using MrDHelper.ValueTypeHelpers.BoolHelper;
using MrDHelper.ValueTypeHelpers.DateTimeHelper;
using MrDHelper.ValueTypeHelpers.GeoPointHelper;

var isEmpty = "".IsNullOrEmpty();
var isTrue = ((bool?)true).IsTrue();
var when = DateTimeOffset.UtcNow.ToVietnamString();
var compact = 1540d.ToCompactDistance(); // "1.5km"
```

### Result Models

```csharp
using MrDHelper.Models;

var success = Result<string>.Ok("payload", "completed");
var failure = Result<string>.Failure("VALIDATION", "Invalid input");

var message = success.Match(
    onSuccess: value => $"OK: {value}",
    onFailure: error => $"ERR: {error.Description}");
```

### DisplayAttribute Helpers

```csharp
using MrDHelper.GenericHelper;

var displayName = myEnumValue.GetDisplayName();
var orderedProps = DisplayExtensions.GetDisplayOrderedProperties(typeof(MyViewModel));
var enumItems = DisplayExtensions.GetValueLabelListForEnum<MyEnum>();
```

### Cell Conversion Helpers

```csharp
using MrDHelper.CellHelpers;

var cell = model.ConvertToCell();
cell["Name"] = "Updated";

var restored = cell.ConvertTo<MyModel>();
```

### EF Core Paging and Search

```csharp
using MrDHelper.AppData.Extensions;

var page = await db.Reports
    .AsNoTracking()
    .ApplySearchClientSide("ha noi", x => x.Title, x => x.Summary)
    .OrderBy(x => x.Id)
    .ToPagedAsync(page: 0, pageSize: 20);
```

Available extensions include:

- `ToPagedAsync`
- `ToPagedSliceAsync`
- `ApplySearchClientSide`
- `ApplySearchAnyClientSide`
- `ApplySearchAnyServerSide`

### SQLite FTS5

`MrDHelper` includes a lightweight SQLite FTS5 integration layer for EF Core.

1. Implement `IHasGuidId` and `IFtsIndexed`.

```csharp
using MrDHelper.AppDomain.EfSqliteFts5;

public sealed class DonVi : IHasGuidId, IFtsIndexed
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string BuildFtsAllText()
        => string.Join(" | ", new[] { Code, Name });
}
```

2. Register the FTS mapping once at startup.

```csharp
using MrDHelper.AppDomain.EfSqliteFts5;

FtsRegistry.Register<DonVi>(
    mainTable: "DonVis",
    ftsTable: "DonVi_fts",
    idColumn: "Id");
```

3. Add the save interceptor and ensure the FTS schema.

```csharp
using Microsoft.EntityFrameworkCore;
using MrDHelper.AppDomain.EfSqliteFts5;

services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
    options.AddInterceptors(new SqliteFtsSaveChangesInterceptor());
});

await db.Database.MigrateAsync();
await FtsSchema.EnsureFtsTablesAsync(db);
```

4. Search with `FtsSearchService`.

```csharp
using MrDHelper.AppDomain.EfSqliteFts5;
using MrDHelper.MudBlazor.Search;

var service = new FtsSearchService(db);

var result = await service.SearchAsync<DonVi>(
    new SearchQuery { Search = "ha noi", Page = 0, PageSize = 20 },
    q => q.OrderBy(x => x.Name),
    cancellationToken);
```

### MudBlazor Search State

```csharp
using MrDHelper.MudBlazor.Search;

var store = new SearchQueryStore();
store.SetActiveKey("users");

store.UpdateActive(query =>
{
    query.Search = "admin";
    query.Page = 0;
}, SearchQueryChangeSource.Ui);
```

## Test

```bash
dotnet test MrDHelper.sln
```

## Repository Structure

- `MrDHelper/`: library source
- `MrDHelper.Tests/`: NUnit test project
- `Images/`: package and repository assets

## Contributing

Issues and pull requests are welcome.

If you change public helpers or extension methods, please update tests together with the code.

## License

Distributed under the MIT License. See [LICENSE.txt](LICENSE.txt).

## Links

- NuGet: [MrDHelper](https://www.nuget.org/packages/MrDHelper)
- Repository: [dattiphu2022/MrDHelper](https://github.com/dattiphu2022/MrDHelper)
