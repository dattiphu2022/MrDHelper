<div id="top"></div>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![Nuget version][nugetversion-shield]][nugetversion-url]
[![Nuget downloads][nugetdownload-shield]][nugetdownload-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/dattiphu2022/MrDHelper">
    <img src="Images/logo.jpg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">Mrd common use helper</h3>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>      
    </li>    
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

This project is providing "extension methods" that are usually used in short way.


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
<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- [x] Add Common functions.
- [ ] Add more functions.


See the [open issues](https://github.com/dattiphu2022/MrDHelper/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Nguyễn Quốc Đạt - [@NguyenQuocĐat1989](https://www.facebook.com/NguyenQuocDat1989)

Project Link: [https://github.com/dattiphu2022/MrDHelper](https://github.com/dattiphu2022/MrDHelper)

<p align="right">(<a href="#top">back to top</a>)</p>



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

<p align="right">(<a href="#top">back to top</a>)</p>



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
