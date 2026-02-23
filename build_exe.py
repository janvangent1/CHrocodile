#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Build script for creating a single-file executable using PyInstaller.
"""

import os
import sys
import PyInstaller.__main__

# Check if pyads is available before building
print("=" * 60)
print("Checking for required packages...")
try:
    import pyads
    print(f"[OK] pyads found at: {pyads.__file__}")
except ImportError:
    print("[ERROR] pyads NOT FOUND!")
    print("  Install with: pip install pyads")
    print("  Without pyads, ADS features will be disabled in the EXE")
    response = input("\nContinue building anyway? (y/n): ")
    if response.lower() != 'y':
        print("Build cancelled.")
        sys.exit(1)
print("=" * 60)
print()

def build_exe():
    """Build single-file executable."""
    
    # Get the script directory
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Main script
    main_script = os.path.join(script_dir, 'chrocodile_gui.py')
    
    # PyInstaller arguments
    args = [
        main_script,
        '--name=CHRocodileGUI',
        '--onefile',  # Single executable file
        '--windowed',  # No console window (GUI app)
        '--clean',  # Clean cache before building
        
        # Include data files
        '--add-data', f'chrocodilelib{os.pathsep}chrocodilelib',  # Include chrocodilelib directory
        
        # Hidden imports (modules that PyInstaller might miss)
        '--hidden-import', 'tkinter',
        '--hidden-import', 'tkinter.ttk',
        '--hidden-import', 'tkinter.filedialog',
        '--hidden-import', 'tkinter.messagebox',
        '--hidden-import', 'numpy',
        '--hidden-import', 'matplotlib',
        '--hidden-import', 'matplotlib.backends.backend_tkagg',
        '--hidden-import', 'matplotlib.figure',
        '--hidden-import', 'queue',
        '--hidden-import', 'threading',
        '--hidden-import', 'chrpy',
        '--hidden-import', 'chrpy.chr_connection',
        '--hidden-import', 'chrpy.chr_cmd_id',
        '--hidden-import', 'chrpy.chr_utils',
        '--hidden-import', 'chrpy.chr_def',
        '--hidden-import', 'chrpy.chr_dll',
        '--hidden-import', 'chrpy.chr_plugins',
        
        # Exclude unnecessary modules to reduce size
        '--exclude-module', 'pytest',
        '--exclude-module', 'unittest',
        '--exclude-module', 'test',
        '--exclude-module', 'tests',
        
        # Icon (optional - create one if you have it)
        # '--icon=icon.ico',
        
        # Version info (optional)
        # '--version-file=version_info.txt',
    ]
    
    # Add pyads if available (but don't fail if not)
    try:
        import pyads
        from PyInstaller.utils.hooks import collect_submodules
        args.extend([
            '--hidden-import', 'pyads',
        ])
        # Collect all pyads submodules
        try:
            pyads_submodules = collect_submodules('pyads')
            for submod in pyads_submodules:
                args.extend(['--hidden-import', submod])
            print(f"Including {len(pyads_submodules)} pyads submodules")
        except:
            pass
    except ImportError:
        print("Warning: pyads not available at build time - ADS features will be disabled in EXE")
    except Exception as e:
        print(f"Warning: Error checking for pyads: {e}")
    
    print("Building executable with PyInstaller...")
    print(f"Script: {main_script}")
    print(f"Output will be in: dist/CHRocodileGUI.exe")
    print()
    print("Note: Using spec file for better control over DLL inclusion.")
    print("If you want to use command-line args, modify build_exe.py")
    print()
    
    # Use spec file instead (better for DLL handling)
    spec_file = os.path.join(script_dir, 'CHRocodileGUI.spec')
    if os.path.exists(spec_file):
        print(f"Using spec file: {spec_file}")
        # Try with --clean first, but if it fails due to permissions, try without
        try:
            PyInstaller.__main__.run([spec_file, '--clean'])
        except PermissionError:
            print("Warning: Could not clean build directory (may be locked), building without clean...")
            PyInstaller.__main__.run([spec_file])
    else:
        # Fallback to command-line args
        print("Spec file not found, using command-line arguments...")
        PyInstaller.__main__.run(args)
    
    print()
    print("=" * 60)
    print("Build complete!")
    print("=" * 60)
    print(f"Executable location: {os.path.join(script_dir, 'dist', 'CHRocodileGUI.exe')}")
    print()
    print("Note:")
    print("- The executable includes all Python dependencies")
    print("- CHRocodile library files are bundled")
    print("- TwinCAT DLL (TcAdsDll.dll) must be available on target PC for ADS")
    print("- Test the executable before deployment")

if __name__ == '__main__':
    build_exe()
