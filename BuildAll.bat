@CD /D "%~dp0"

@call Params.bat
@del /F /S /Q *.log > nul 2>&1

@echo Restoring NuGet packages...
@"%CD%\.nuget\NuGet.exe" restore %SOLUTION_FILE%
@IF %ERRORLEVEL% NEQ 0 GOTO  Error
@echo.

:StartBuild
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
@CALL "%CD%\packages\Tools.InnoSetup.5.5.9\tools\ISCC.exe" "%CD%\Installer\Script\SubSearchInstallerScript.iss" > "Publish.log" 2>&1
@IF %ERRORLEVEL% EQU 0 GOTO  End

:Error
@echo An error occured. Process halted!
@pause

:End