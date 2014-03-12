@echo off
rem ...
set errorlevel=
taskkill /F /IM vstest.executionengine.exe
exit /b 0