![SubSearch - Lightweight instant subtitle downloader](https://lh3.googleusercontent.com/-m24ec1Bwbs4/Vaep-wtRcoI/AAAAAAAAD-g/NeF2n4f1ivw/s64-Ic42/SubSearchBig.png)
![Windows Explorer Shell Extension](https://lh3.googleusercontent.com/-eUG4WTu6yd0/VaiJF-geueI/AAAAAAAAD_A/NRGaxfqHvC8/s394-Ic42/Slogan.png)

#SubSearch - Lightweight instant subtitle downloader
SubSearch is a subtitle downloader implemented using .NET (C#)

The code is licensed under the MIT license. See opensource.org and tl;dr for more details.

##Features
* Lightweight subtitle downloader through Windows Explorer shell context menu

##Download
All releases are published on [GitHub Release page](https://github.com/tu-tran/SubSearch/releases).

##Getting started
###For end-users
1. Go to  [Release page](https://github.com/tu-tran/SubSearch/releases) to download the binaries (in ZIP or installation package for x86 and x64 system)
2. If using the installer: Just proceed to the end!
3. If using the zip package: Unzip the files to a folder, then execute "install.bat" to install the application
4. Right click on any movie files in Windows Explorer, click on the new item named "Download subtitle"
5. If there are multiple matches, a dialog will appear asking users to select the correct movie title (by double clicking on the title)
6. If there are multiple subtitles for the movie title, a dialog will appear asking users to select the desired subtitle (by double clicking on the subtitle)

###For developers
The application is a smallish one which utilizes the [SharpShell library](https://github.com/dwmkerr/sharpshell) for Windows shell registration. It also provides a comprehensive toolkit for debugging and installing the shell extension. R
The entry point is the solution file SubSearch.sln. After compiling the code, there will a file named "install.bat" in the output folder which can be used to install the assembly to the Windows shell context menu.

##Questions
Use the [GitHub Issues page](https://github.com/tu-tran/SubSearch/issues) to create new inquiries/questions/bug reports/feature requests...

##History
The project was implemented for personal use: no FREE software was available for fast subtitle downloading --> I make one during a day off from work.

##Contribute
This project is continuously evolving and any kind of help is greatly appreciated. You are more than welcome to get involved in the development of the project.
