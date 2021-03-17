@echo off
@GOTO %PROCESSOR_ARCHITECTURE%

:AMD64
@SET PROGRAM_FILES=%ProgramFiles(x86)%
@GOTO SET_MSBUILD

:X86
@SET PROGRAM_FILES=%ProgramFiles%
@GOTO SET_MSBUILD

:SET_MSBUILD
for %%x in (
	"%PROGRAM_FILES%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2019\Enterprise\RegAsm\Current\Bin"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\Enterprise\RegAsm\15.0\Bin"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\Professional\RegAsm\15.0\Bin"
	"%PROGRAM_FILES%\Microsoft Visual Studio\2017\BuildTools\RegAsm\15.0\Bin"
	"%WINDIR%\Microsoft.NET\Framework\v4.0.30319"
	"%PROGRAM_FILES%\RegAsm\14.0\Bin"
) do (	
	@IF EXIST "%%x\RegAsm.exe" (
		@SET NET_FRAMEWORK=%%x
		@ECHO Use RegAsm from %%x
		@GOTO END
	)
)

@ECHO Could not locate RegAsm. Process Halted!
@EXIT /B -1

:END
@ECHO Detected RegAsm at [%NET_FRAMEWORK%]