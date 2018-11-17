@echo off

@CD /D "%~dp0"
@SET OUTPUT_DIR=%CD%\Bin\Release
@SET DEPLOY_DIR=%LOCALAPPDATA%\SubSearch
@SET SOLUTION_FILE="%CD%\SubSearch.sln"
@set LOG_FILE=Release.log

@SET NET_FRAMEWORK="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

@reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" /v MSBuildToolsPath > nul 2>&1
if ERRORLEVEL 1 goto MissingMSBuildToolsFromRegistry

for /f "skip=2 tokens=2,*" %%A in ('reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" /v MSBuildToolsPath') do SET "NET_FRAMEWORK=%%B"

IF NOT EXIST "%NET_FRAMEWORK%" goto MissingMSBuildToolsFromRegistry
IF NOT EXIST "%NET_FRAMEWORK%msbuild.exe" goto MissingMSBuildToolsFromRegistry

goto:eof
:MissingMSBuildToolsFromRegistry
@setlocal
@if "%PROCESSOR_ARCHITECTURE%"=="x86" set PROGRAMS=%ProgramFiles%
@if defined ProgramFiles(x86) set PROGRAMS=%ProgramFiles(x86)%
for %%e in (Community Professional Enterprise) do (
    if exist "%PROGRAMS%\Microsoft Visual Studio\2017\%%e\MSBuild\15.0\Bin\MSBuild.exe" (
        set "NET_FRAMEWORK=%PROGRAMS%\Microsoft Visual Studio\2017\%%e\MSBuild\15.0\Bin\MSBuild.exe"
    )
)

@echo Using MSBuild from %NET_FRAMEWORK%