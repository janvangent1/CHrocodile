@echo off
REM Build script for creating CHRocodile GUI executable
REM Run this script to create a single-file .exe

echo ========================================
echo CHRocodile GUI - Build Executable
echo ========================================
echo.

REM Check if virtual environment is activated
python -c "import sys; sys.exit(0 if hasattr(sys, 'real_prefix') or (hasattr(sys, 'base_prefix') and sys.base_prefix != sys.prefix) else 1)" 2>nul
if errorlevel 1 (
    echo Activating virtual environment...
    call venv\Scripts\activate.bat
)

REM Check if PyInstaller is installed
python -c "import PyInstaller" 2>nul
if errorlevel 1 (
    echo Installing PyInstaller...
    pip install pyinstaller
)

echo.
echo Building executable...
echo.

REM Build using spec file
python -m PyInstaller CHRocodileGUI.spec --clean

if errorlevel 1 (
    echo.
    echo Build failed! Trying alternative method...
    python build_exe.py
)

echo.
echo ========================================
echo Build complete!
echo ========================================
echo.
echo Executable location: dist\CHRocodileGUI.exe
echo.
pause
