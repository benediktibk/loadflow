@echo off
rem ...
set errorlevel=
taskkill /F /T /IM vstest.executionengine.exe
taskkill /F /T /IM vstest.executionengine.x86.exe
exit /b 0