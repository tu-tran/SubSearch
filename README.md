![SubSearch - Lightweight instant subtitle downloader](https://lh3.googleusercontent.com/-m24ec1Bwbs4/Vaep-wtRcoI/AAAAAAAAD-g/NeF2n4f1ivw/s64-Ic42/SubSearchBig.png)
![Windows Explorer Shell Extension](https://lh3.googleusercontent.com/-eUG4WTu6yd0/VaiJF-geueI/AAAAAAAAD_A/NRGaxfqHvC8/s394-Ic42/Slogan.png)

#SubSearch - Lightweight instant subtitle downloader
SubSearch is a subtitle downloader implemented using .NET (C#)
**The application requires .NET Framework 4.5** to be installed first. It can be downloaded and installed from [https://www.microsoft.com/en-sg/download/details.aspx?id=30653](https://www.microsoft.com/en-sg/download/details.aspx?id=30653)

[![Donation](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=hostmax%40gmail%2ecom&lc=FI&item_name=SubSearch&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

The code is licensed under the MIT license. See opensource.org for more details.

##Features
* Lightweight subtitle downloader through Windows Explorer shell context menu
* Support both single movie file and all movie files inside a folder

##Download
All releases are published on [GitHub Release page](https://github.com/tu-tran/SubSearch/releases).

##Getting started
###For end-users
1. Go to  [Release page](https://github.com/tu-tran/SubSearch/releases) to download the binaries (in ZIP or installation package for x86 and x64 system)
2. If using the installer: Just proceed to the end!
3. If using the zip package: Unzip the files to a folder, then execute "install.bat" to install the application
4. Right click on any movie file/folder in Windows Explorer, click on the new item named "Download subtitle"
5. If there are multiple matches, a dialog will appear asking users to select the correct movie title (by double clicking on the title)
6. If there are multiple subtitles for the movie title, a dialog will appear asking users to select the desired subtitle (by double clicking on the subtitle)

###For developers
The application is a smallish one which utilizes the [SharpShell library](https://github.com/dwmkerr/sharpshell) for Windows shell registration. It also provides a comprehensive toolkit for debugging and installing the shell extension. R
The entry point is the solution file SubSearch.sln. After compiling the code, there will a file named "install.bat" in the output folder which can be used to install the assembly to the Windows shell context menu.

##How to download the subtitle
1. Open the movie file location (e.g. using Windows Explorer)
![Right click on movie file and select "Download subtitle"](https://lh3.googleusercontent.com/--hpRAs_EyZ4/VaiMcGGSFEI/AAAAAAAAD_k/SmEym47tPWc/s394-Ic42/RightClickAndSelect.png)
2. Right click on the move file/folder, select "Download subtitle". SubSearch will try to figure out the actual movie title based on the file name (or all the files inside a folder if you right-clicked on a folder)
3. A list of matching movie titles will be shown. Select the expected movie title by double-clicking on the title name in the list (if an exact title match was found, this step will be skipped by SubSearch)
4. Double-click on the subtitle you want to download
![Download subtitle](https://lh3.googleusercontent.com/-uPWDYxbKvg0/VaiMJJ5PwgI/AAAAAAAAD_U/L-4i1u7V5N8/s531-Ic42/SelectSubtitle.png)
5. Open the movie and enjoy!

##Questions
Use the [GitHub Issues page](https://github.com/tu-tran/SubSearch/issues) to create new inquiries/questions/bug reports/feature requests...

##History
The project was implemented for personal use: no FREE software was available for fast subtitle downloading --> I make one during a day off from work.

##Contribute
This project is continuously evolving and any kind of help is greatly appreciated. You are more than welcome to get involved in the development of the project.
