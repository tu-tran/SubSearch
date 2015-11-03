@CD /D "%~dp0"
@call Params.bat

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
