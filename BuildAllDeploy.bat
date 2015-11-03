@CD /D "%~dp0"
@call Params.bat

@CALL "%~dp0\BuildAll.bat"
@IF %ERRORLEVEL% NEQ 0 GOTO  End

@CALL "%~dp0\Deploy.bat"
