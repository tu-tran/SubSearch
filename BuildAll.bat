@set NET_FRAMEWORK="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
@set SOLUTION_FILE="SubSearch.sln"

@del /F /S /Q *.log > nul 2>&1

@echo Restoring NuGet packages...
@%CD%\.nuget\NuGet.exe restore %SOLUTION_FILE%
@IF %ERRORLEVEL% NEQ 0 GOTO  Error
@echo.

:StartBuild
@set OUTPUT_DIR=%CD%\Bin\Release
@set LOG_FILE=Release.log
@echo =========================================================
@echo COMPILATION
@echo =========================================================
@echo Cleaning up Release...
@del /F /S /Q "%OUTPUT_DIR%\*" > nul 2>&1

@echo Building Release...
@%NET_FRAMEWORK% %SOLUTION_FILE% /t:Rebuild /p:Configuration=Release > "%LOG_FILE%" 2>&1
@echo Exit code %ERRORLEVEL% >> "%LOG_FILE%"
@IF %ERRORLEVEL% NEQ 0 GOTO  Error

@del /F /S /Q "%OUTPUT_DIR%\*.pdb" > nul 2>&1
@del /F /S /Q "%OUTPUT_DIR%\*.xml" > nul 2>&1

@echo.
@echo Creating installers...
@"%CD%\packages\Tools.InnoSetup.5.5.5\tools\ISCC.exe" "%CD%\SubSearchInstallerScript.iss" > "Publish.log" 2>&1
@IF %ERRORLEVEL% EQU 0 GOTO  End

:Error
@echo An error occured. Process halted!
@pause

:End