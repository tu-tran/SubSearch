@call Params.bat

srm install "%~dp0\SubSearch.dll" -codebase
"%NET_FRAMEWORK%\regasm.exe" "%~dp0\SubSearch.dll" /register /codebase