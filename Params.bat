@echo off
@CD /D "%~dp0"
@SET OUTPUT_DIR=%CD%\Bin\Release
@SET DEPLOY_DIR=%LOCALAPPDATA%\SubSearch
@SET SOLUTION_FILE="%CD%\SubSearch.sln"
@set LOG_FILE=Release.log

@GOTO %PROCESSOR_ARCHITECTURE%

:AMD64
@SET PROGRAM_FILES=%ProgramFiles(x86)%
@GOTO SET_MSBUILD

:X86
@SET PROGRAM_FILES=%ProgramFiles%
@GOTO SET_MSBUILD

:SET_MSBUILD
for %%x in (
	"%PROGRAM_FILES%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
	"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
	"%PROGRAM_FILES%\MSBuild\14.0\Bin\MSBuild.exe"
) do (	
	@IF EXIST %%x (
		@SET NET_FRAMEWORK=%%x
		@ECHO Use MSBuild from %%x
		@GOTO END
	)
)

@ECHO Could not locate MSBuild. Process Halted!
@EXIT /B -1

:END
@ECHO Detected MSBuild at [%NET_FRAMEWORK%]