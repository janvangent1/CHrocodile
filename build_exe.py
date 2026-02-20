#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Build script for creating a single-file executable using PyInstaller.
"""

import os
import sys
import PyInstaller.__main__

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
        args.extend([
            '--hidden-import', 'pyads',
        ])
    except:
        pass  # pyads not available, that's OK
    
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
        PyInstaller.__main__.run([spec_file, '--clean'])
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
