@echo off
rem ...
set errorlevel=
taskkill /F /T /IM vstest.executionengine.exe 2>nul
taskkill /F /T /IM vstest.executionengine.x86.exe 2>nul
exit /b 0