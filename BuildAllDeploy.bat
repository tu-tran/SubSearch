@CD /D "%~dp0"
@CALL "%~dp0\BuildAll.bat"
@SET DEPLOY_DIR=%LOCALAPPDATA%\SubSearch
@echo Build completed with code %ERRORLEVEL%
@IF %ERRORLEVEL% NEQ 0 GOTO  End

:Deploy
@echo DEPLOYING TO LOCAL INSTALLATION
@echo ==========================================
@echo Stopping Shell...
@call taskkill /F /IM explorer.exe
@echo Deploy "%OUTPUT_DIR%" - "%LOCALAPPDATA%"
@xcopy "%OUTPUT_DIR%\*" "%DEPLOY_DIR%" /E /C /Y
@start explorer.exe
@echo Registering Shell...
@cd /D %DEPLOY_DIR%
@CALL "%DEPLOY_DIR%\install.bat"

@IF %ERRORLEVEL% NEQ 0 GOTO  Error
@GOTO    End

:Error
@echo An error occured. Process halted!
@pause

:End
