[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![Nuget version][nugetversion-shield]][nugetversion-url]
[![Nuget downloads][nugetdownload-shield]][nugetdownload-url]


<!-- ABOUT THE PROJECT -->
## About The Project

This project is providing "extension methods" that are usually used in short way.
All methods have its own tests, check it on source code and help me to improve it if you can. Thank you!

<!-- USAGE EXAMPLES -->
## Usage

1. Reference to MrdHelper
2. ```using MrdHelper```
3. Use the extension methods that you want.
```c#
IEnumerable<T>?.ForEach<T>(Action<T>);
IEnumerable<T>?.ForEachAsync<T>(Fun<T,Task>) //awaitable, eg: await IEnumerable<T>?.ForEachAsync(async (t)=> { await Task.Delay(10); });

IList<T>.AddDummyItemsToMaximumCountOf<T>(int collectionFinalCount, T fillValue);

<T>.IsNull(); <T>.NotNull();

bool?.IsFalse(); bool?.IsTrue();
bool?.NotFalse(); bool?.NotTrue();

"string".GetMd5();
"string".IsNullOrEmpty(); "string".IsNullOrWhiteSpace();
"string".NotNullOrEmpty(); "string".NotNullOrWhiteSpace();

TaskHelper.RunSync<TResult>(Func<Task<TResult>, TResult>); // eg: var result = TaskHelper.RunSync<TResult>(()=>GetResultAsync());
```

New in 1.0.8+1.0.9
```c#
var someClass = new SomeClass();
var cell = SomeClass.ConvertToCell(someClass);
cell[nameof(SomeClass.Property1)] = newValue;
var otherSomeClass = cell.ConvertTo<SomeClass>();

-----------
var sqlConnectionString = new SqlServerConnectionStringBuilder{
	Server = server,
    Database = database,
    UserId = userId,
    Password = passWord,
    Trusted_Connection = trusted,
    ConnectViaIP = viaIp
}
bool validateResult = sqlConnectionString.ValidateConnectionString();
var finalString = sqlConnectionString.FinalConnectionString;
```


<!-- ROADMAP -->
## Roadmap

- [x] Add Common functions.
- [ ] Add more functions.


## Contact

Nguyễn Quốc Đạt - [@NguyenQuocĐat1989](https://www.facebook.com/NguyenQuocDat1989)

Project Link: [https://github.com/dattiphu2022/MrDHelper](https://github.com/dattiphu2022/MrDHelper)




<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

Use this space to list resources you find helpful and would like to give credit to. I've included a few of my favorites to kick things off!

* [Choose an Open Source License](https://choosealicense.com)
* [GitHub Emoji Cheat Sheet](https://www.webpagefx.com/tools/emoji-cheat-sheet)
* [Malven's Flexbox Cheatsheet](https://flexbox.malven.co/)
* [Malven's Grid Cheatsheet](https://grid.malven.co/)
* [Img Shields](https://shields.io)
* [GitHub Pages](https://pages.github.com)
* [Font Awesome](https://fontawesome.com)
* [React Icons](https://react-icons.github.io/react-icons/search)



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
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
