@CD /D "%~dp0"
@SET OUTPUT_DIR=%CD%\Bin\Release
@SET DEPLOY_DIR=%LOCALAPPDATA%\SubSearch
@SET NET_FRAMEWORK="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
@SET SOLUTION_FILE="%CD%\SubSearch.sln"
@set LOG_FILE=Release.log
