@echo off
rem ...
set errorlevel=
taskkill /F /T /IM vstest.executionengine.exe
exit /b 0