<div id="top"></div>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![Nuget version][nugetversion-shield]][nugetversion-url]
[![Nuget downloads][nugetdownload-shield]][nugetdownload-url]

<br />
<div align="center">
  <a href="https://github.com/dattiphu2022/MrDHelper">
    <img src="Images/logo.jpg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">MrD Common Helper</h3>
</div>

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

## About The Project

MrDHelper provides commonly used extension methods and helper utilities for .NET applications.

## Usage

New in 2.0.5 + 2.0.6 + 2.0.7

```csharp
Add EfSqliteFts5
```

1. Inherit from the base types.

```csharp
using VietFtsSearch;

public sealed class DonVi : AuditableBase, IFtsIndexed, IHasGuidId
{
    public string PhienHieu { get; set; } = string.Empty;
    public string TenDayDu { get; set; } = string.Empty;
    public string? TenVietTat { get; set; }

    public string BuildFtsAllText()
        => string.Join(" | ", new[] { PhienHieu, TenDayDu, TenVietTat }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
}
```

2. Register the spec once at application startup.
Example in `Program.cs` or your DI setup:

```csharp
using VietFtsSearch;

FtsRegistry.Register<DonVi>(mainTable: "DonVis", ftsTable: "DonVi_fts", idColumn: "Id");
// Add another Register line for each additional entity.
```

3. Add the interceptor to `DbContextOptions`.

```csharp
using VietFtsSearch;
using Microsoft.EntityFrameworkCore;

services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite("Data Source=./applicationdatabase.db;");
    opt.AddInterceptors(new SqliteFtsSaveChangesInterceptor());
});
```

4. Ensure the FTS schema after migrations.

```csharp
using VietFtsSearch;

await db.Database.MigrateAsync();
await FtsSchema.EnsureFtsTablesAsync(db);
```

5. Search.

```csharp
using VietFtsSearch;

var search = new FtsSearchService(db);
var pagedResult = await search.SearchAsync<DonVi>(SearchQuery, FtsSearchOption, CancelationToken);
```

## Roadmap

- [x] Add common functions.
- [ ] Add more functions.

See the [open issues](https://github.com/dattiphu2022/MrDHelper/issues) for the full list of proposed features and known issues.

<p align="right">(<a href="#top">back to top</a>)</p>

## Contributing

Contributions are what make the open source community such a great place to learn, inspire, and create. Any contributions you make are greatly appreciated.

If you have a suggestion that would improve the project, fork the repository and create a pull request. You can also open an issue with the `enhancement` tag.

1. Fork the project.
2. Create your feature branch: `git checkout -b feature/AmazingFeature`
3. Commit your changes: `git commit -m 'Add some AmazingFeature'`
4. Push to the branch: `git push origin feature/AmazingFeature`
5. Open a pull request.

<p align="right">(<a href="#top">back to top</a>)</p>

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>

## Contact

Nguyen Quoc Dat - [@NguyenQuocDat1989](https://www.facebook.com/NguyenQuocDat1989)

Project Link: [https://github.com/dattiphu2022/MrDHelper](https://github.com/dattiphu2022/MrDHelper)

<p align="right">(<a href="#top">back to top</a>)</p>

## Acknowledgments

Helpful resources used by this project:

* [Choose an Open Source License](https://choosealicense.com)
* [GitHub Emoji Cheat Sheet](https://www.webpagefx.com/tools/emoji-cheat-sheet)
* [Malven's Flexbox Cheatsheet](https://flexbox.malven.co/)
* [Malven's Grid Cheatsheet](https://grid.malven.co/)
* [Img Shields](https://shields.io)
* [GitHub Pages](https://pages.github.com)
* [Font Awesome](https://fontawesome.com)
* [React Icons](https://react-icons.github.io/react-icons/search)

<p align="right">(<a href="#top">back to top</a>)</p>

[contributors-shield]: https://img.shields.io/github/contributors/dattiphu2022/MrDHelper?style=for-the-badge
[contributors-url]: https://github.com/dattiphu2022/MrDHelper/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/dattiphu2022/MrDHelper?style=for-the-badge
[forks-url]: https://github.com/dattiphu2022/MrDHelper/network/members
[stars-shield]: https://img.shields.io/github/stars/dattiphu2022/MrDHelper?style=for-the-badge
[stars-url]: https://github.com/dattiphu2022/MrDHelper/stargazers
[issues-shield]: https://img.shields.io/github/issues/dattiphu2022/MrDHelper?style=for-the-badge
[issues-url]: https://github.com/dattiphu2022/MrDHelper/issues
[license-shield]: https://img.shields.io/github/license/dattiphu2022/MrDHelper?style=for-the-badge
[license-url]: https://github.com/dattiphu2022/MrDHelper/blob/master/LICENSE.txt
[nugetdownload-shield]: https://img.shields.io/nuget/dt/MrDHelper?style=for-the-badge
[nugetdownload-url]: https://www.nuget.org/packages/MrDHelper
[nugetversion-shield]: https://img.shields.io/nuget/v/MrDHelper?style=for-the-badge
[nugetversion-url]: https://www.nuget.org/packages/MrDHelper#versions-body-tab
