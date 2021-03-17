@call Params.bat

srm uninstall "%~dp0\SubSearch.dll"
"%NET_FRAMEWORK%\regasm.exe" "%~dp0\SubSearch.dll" /unregister /codebase