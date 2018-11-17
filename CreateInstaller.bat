@CD /D "%~dp0"

@call Params.bat
@echo Creating installers...
@CALL "%CD%\packages\Tools.InnoSetup.5.5.9\tools\ISCC.exe" "%CD%\Installer\Script\SubSearchInstallerScript.iss" > "Publish.log" 2>&1
@IF %ERRORLEVEL% EQU 0 GOTO  End

:Error
@echo An error occured. Process halted!
@pause

:End