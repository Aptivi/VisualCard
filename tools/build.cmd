@echo off

REM This script builds and packs the artifacts. Use when you have VS installed.
set releaseconfig=%1
if "%releaseconfig%" == "" set releaseconfig=Release

set buildoptions=%*
call set buildoptions=%%buildoptions:*%1=%%
if "%buildoptions%" == "*=" set buildoptions=

REM Turn off telemetry and logo
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_NOLOGO=1

:download
echo Downloading packages...
"%ProgramFiles%\dotnet\dotnet.exe" restore "..\VisualCard.sln" -p:Configuration=%releaseconfig% %buildoptions%
if %errorlevel% == 0 goto :build
echo There was an error trying to download packages (%errorlevel%).
goto :finished

:build
echo Building VisualCard...
"%ProgramFiles%\dotnet\dotnet.exe" build "..\VisualCard.sln" -p:Configuration=%releaseconfig% %buildoptions%
if %errorlevel% == 0 goto :success
echo There was an error trying to build (%errorlevel%).
goto :finished

:success
echo Build successful.
:finished
