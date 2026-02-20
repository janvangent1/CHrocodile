@echo off
REM Setup script for CHRocodile GUI Application Virtual Environment
REM This script creates a virtual environment and installs dependencies

echo Creating virtual environment...
python -m venv venv

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to create virtual environment
    echo Make sure Python 3.7+ is installed and accessible
    pause
    exit /b 1
)

echo.
echo Activating virtual environment...
call venv\Scripts\activate.bat

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to activate virtual environment
    pause
    exit /b 1
)

echo.
echo Upgrading pip...
python -m pip install --upgrade pip

echo.
echo Installing requirements...
pip install -r requirements.txt

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to install requirements
    pause
    exit /b 1
)

echo.
echo ========================================
echo Setup complete!
echo ========================================
echo.
echo To activate the virtual environment in the future, run:
echo   venv\Scripts\activate.bat
echo.
echo To run the application:
echo   python chrocodile_gui.py
echo.
pause


