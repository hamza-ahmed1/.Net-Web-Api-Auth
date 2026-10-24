
@echo off
title Auth - .NET Backend Server

REM ---- CONFIG ----
set "PROJECT_DIR=%~dp0"
set "PROJECT_NAME=Auth"
set "PORT=5000"
REM ----------------

echo ==========================================
echo       %PROJECT_NAME% Backend Launcher
echo ==========================================
echo.

cd /d "%PROJECT_DIR%" || (
    echo ERROR: Project folder not found.
    pause
    exit /b 1
)

REM Stop processes listening on the configured port
echo Checking port %PORT%...

for /f "tokens=5" %%a in ('netstat -ano ^| findstr /R /C:":%PORT% .*LISTENING"') do (
    echo Stopping process %%a...
    taskkill /PID %%a /F >nul 2>&1
)

echo.
echo Starting %PROJECT_NAME%...

dotnet run

echo.
echo Backend stopped or exited with an error.
pause
