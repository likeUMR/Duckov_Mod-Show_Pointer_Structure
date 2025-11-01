@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo === Mod Build and Deploy Script ===
echo.

REM 1. Read name from info.ini
set "infoIniPath=builded\info.ini"
if not exist "%infoIniPath%" (
    echo Error: Cannot find info.ini file: %infoIniPath%
    exit /b 1
)

echo Reading info.ini...
set "modName="
for /f "tokens=2 delims==" %%a in ('findstr /i /c:"name" "%infoIniPath%"') do (
    set "modName=%%a"
    set "modName=!modName: =!"
    goto :found
)

:found
if "!modName!"=="" (
    echo Error: Cannot find 'name' field in info.ini
    exit /b 1
)

echo Mod Name: !modName!
echo.

REM 2. Execute dotnet build
echo Building project...
dotnet build
if errorlevel 1 (
    echo Error: Build failed!
    exit /b 1
)

echo Build successful!
echo.

REM 3. Determine source DLL path
set "sourceDllPath=bin\Debug\netstandard2.1\!modName!.dll"
if not exist "!sourceDllPath!" (
    echo Error: Cannot find compiled DLL: !sourceDllPath!
    exit /b 1
)

echo Source DLL: !sourceDllPath!

REM 4. Determine target path
set "gamePath=G:\Steam\steamapps\common\Escape from Duckov"
set "targetDir=!gamePath!\Duckov_Data\Mods\!modName!"
set "targetDllPath=!targetDir!\!modName!.dll"

echo Target Path: !targetDllPath!
echo.

REM 5. Create target directory if it doesn't exist
if not exist "!targetDir!" (
    echo Creating target directory: !targetDir!
    mkdir "!targetDir!"
)

REM 6. Copy DLL file
echo Copying DLL file...
if exist "!targetDllPath!" (
    echo Target file exists, will be replaced
)

copy /Y "!sourceDllPath!" "!targetDllPath!" >nul
if errorlevel 1 (
    echo Error: File copy failed
    exit /b 1
)

if exist "!targetDllPath!" (
    echo.
    echo Success! DLL deployed to: !targetDllPath!
) else (
    echo Error: File copy failed
    exit /b 1
)

echo.
echo === Done ===
pause

