# -*- mode: python ; coding: utf-8 -*-
"""
PyInstaller spec file for CHRocodile GUI application.
This file provides more control over the build process.
"""

import os
import sys
from PyInstaller.utils.hooks import collect_data_files, collect_submodules

block_cipher = None

# Collect all data files
datas = []

# Add chrocodilelib directory (entire directory structure)
chrocodilelib_path = os.path.join('chrocodilelib')
if os.path.exists(chrocodilelib_path):
    # Include the entire chrocodilelib directory
    datas.append((chrocodilelib_path, 'chrocodilelib'))
    
    # Also try to find DLL files in libcore/lib
    lib_dll_path = os.path.join(chrocodilelib_path, 'libcore', 'lib')
    if os.path.exists(lib_dll_path):
        # Collect DLL files
        for root, dirs, files in os.walk(lib_dll_path):
            for file in files:
                if file.endswith(('.dll', '.so', '.cfg')):
                    src = os.path.join(root, file)
                    # Preserve directory structure
                    rel_path = os.path.relpath(src, chrocodilelib_path)
                    datas.append((src, os.path.join('chrocodilelib', os.path.dirname(rel_path))))

# Collect matplotlib data files
try:
    matplotlib_datas = collect_data_files('matplotlib')
    datas.extend(matplotlib_datas)
except:
    pass

# Hidden imports
hiddenimports = [
    'tkinter',
    'tkinter.ttk',
    'tkinter.filedialog',
    'tkinter.messagebox',
    'numpy',
    'matplotlib',
    'matplotlib.backends.backend_tkagg',
    'matplotlib.figure',
    'queue',
    'threading',
    'chrpy',
    'chrpy.chr_connection',
    'chrpy.chr_cmd_id',
    'chrpy.chr_utils',
    'chrpy.chr_def',
    'chrpy.chr_dll',
    'chrpy.chr_plugins',
    'settings_manager',
]

# Try to add pyads if available
try:
    import pyads
    hiddenimports.append('pyads')
except:
    pass

# Find CHRocodile DLL files
# DLLs are included via the datas section (entire chrocodilelib directory)
# PyInstaller will extract them to the temp directory and they'll be found via path resolution
binaries = []
# Note: DLLs are handled via datas section above, but we can also add them as binaries
# if needed for explicit loading. For now, datas section should be sufficient.
chrocodilelib_lib_path = os.path.join('chrocodilelib', 'libcore', 'lib')
if os.path.exists(chrocodilelib_lib_path):
    # Add DLL files as binaries (ensures they're available for ctypes loading)
    for root, dirs, files in os.walk(chrocodilelib_lib_path):
        for file in files:
            if file.endswith(('.dll', '.so')):
                src = os.path.join(root, file)
                # Preserve directory structure - DLLs go to same location relative to executable
                # In onefile mode, they'll be in _MEIPASS/chrocodilelib/libcore/lib/...
                rel_path = os.path.relpath(src, chrocodilelib_lib_path)
                binaries.append((src, os.path.join('chrocodilelib', 'libcore', 'lib', os.path.dirname(rel_path))))

a = Analysis(
    ['chrocodile_gui.py'],
    pathex=[],
    binaries=binaries,
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=['pytest', 'test', 'tests'],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='CHRocodileGUI',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,  # No console window for GUI app
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon=None,  # Add icon path here if you have one
)
