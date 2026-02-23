# -*- mode: python ; coding: utf-8 -*-
"""
Minimal spec for ADS troubleshoot exe - small, console app for debugging.
"""

a = Analysis(
    ['ads_troubleshoot.py'],
    pathex=[],
    binaries=[],
    datas=[],
    hiddenimports=['pyads'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='ADS_Troubleshoot',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=True,   # Show console output
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
