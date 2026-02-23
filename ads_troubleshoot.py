# -*- coding: utf-8 -*-
"""
Minimal ADS / TwinCAT router troubleshooting script.
Run as script or as frozen .exe on Beckhoff PC to test pyads + DLL loading.
"""

import os
import sys
import subprocess

def get_service_status(service_name):
    """Return status string for a Windows service, or 'not found'."""
    try:
        r = subprocess.run(
            ["sc", "query", service_name],
            capture_output=True,
            text=True,
            timeout=5,
        )
        if r.returncode != 0:
            return "not installed"
        if "RUNNING" in r.stdout:
            return "RUNNING"
        if "STOPPED" in r.stdout:
            return "STOPPED"
        return "unknown"
    except Exception:
        return "error checking"

print("=" * 60)
print("ADS Router / pyads Troubleshooter")
print("=" * 60)
print(f"Python: {sys.version}")
print(f"Frozen (exe): {getattr(sys, 'frozen', False)}")
print()

# Check TwinCAT-related services before loading pyads
print("Windows services (TwinCAT):")
for name, label in [
    ("TcSysSrv", "TwinCAT System Service"),
    ("TcAdsRouter", "TwinCAT ADS Router"),
    ("TcAdsDll", "TwinCAT ADS DLL Service"),
]:
    status = get_service_status(name)
    print(f"  {label} ({name}): {status}")
print("Note: On full XAE, TcAdsRouter/TcAdsDll as separate services are often NOT present.")
print("      The ADS router runs inside TcSysSrv (and TcSysUI). So TcSysSrv RUNNING is what matters.")
print()

# Fix TwinCAT DLL loading for frozen executable
if getattr(sys, 'frozen', False):
    tc_path = r"C:\Program Files (x86)\Beckhoff\TwinCAT\Common64"
    print(f"Adding DLL directory: {tc_path}")
    if os.path.exists(tc_path):
        os.add_dll_directory(tc_path)
        print("  -> Directory exists, added to search path.")
    else:
        print("  -> WARNING: Directory does not exist!")
    dll_file = os.path.join(tc_path, "TcAdsDll.dll")
    print(f"  TcAdsDll.dll exists: {os.path.exists(dll_file)}")
    print()

print("Importing pyads...")
try:
    import pyads
    print("  -> pyads imported OK.")
except Exception as e:
    print(f"  -> FAILED: {e}")
    input("Press Enter to exit...")
    sys.exit(1)

print()

# On Windows: set_local_address() is NOT supported for Windows clients.
print("Note: On Windows, set_local_address() is not used (router assigns address automatically).")
print()

# Test 1: Low-level open_port()
print("Test 1: pyads.open_port() ...")
err1 = None
try:
    port = pyads.open_port()
    print(f"  -> Port opened: {port}")
    pyads.close_port()
    print("  -> Port closed. SUCCESS.")
    test1_ok = True
except Exception as e:
    print(f"  -> FAILED: {e}")
    err1 = e
    test1_ok = False
    # Show ADS error code if present
    if hasattr(e, "ads_err_code"):
        print(f"  -> ADS error code: {e.ads_err_code}")
print()

# Test 2: High-level Connection (what the main app uses)
print("Test 2: pyads.Connection('127.0.0.1.1.1', 851).open() ...")
conn = None
err2 = None
try:
    conn = pyads.Connection("127.0.0.1.1.1", 851)
    conn.open()
    print("  -> Connection opened. SUCCESS.")
    if conn.is_open:
        conn.close()
        print("  -> Connection closed.")
    test2_ok = True
except Exception as e:
    print(f"  -> FAILED: {e}")
    err2 = e
    test2_ok = False
    if hasattr(e, "ads_err_code"):
        print(f"  -> ADS error code: {e.ads_err_code}")
    if conn is not None:
        try:
            conn.close()
        except Exception:
            pass
print()

# Summary
if test1_ok and test2_ok:
    print("All tests passed. ADS router is OK.")
elif test2_ok and not test1_ok:
    print("Connection() works but open_port() fails.")
    print("-> Main app uses Connection() so it may still work.")
elif test1_ok and not test2_ok:
    print("open_port() works but Connection() fails.")
    print("-> Check PLC is in RUN and port 851 is correct.")
else:
    print("Both tests failed. Next steps:")
    print()
    print("  If TcSysSrv is RUNNING (and TcAdsRouter not installed - normal for XAE):")
    print("  - Run this exe from a LOCAL folder (e.g. C:\\Temp\\ADS_Troubleshoot.exe).")
    print("    Do NOT run from a network drive or OneDrive-synced folder.")
    print("  - Add TwinCAT to PATH for this session, then run the exe again:")
    print('    set PATH=%%PATH%%;C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\Common64')
    print("    (Open cmd, run the set command, then run the exe from the same window.)")
    print("  - Start TcSysUI.exe if not running; wait 10 s; run this tool again.")
    print("  - Restart the PC (another process may hold the only ADS client port).")
    print()
    print("  To see if the problem is the frozen exe:")
    print("  - Install Python on this PC, copy ads_troubleshoot.py, run: python ads_troubleshoot.py")
    print("  - If the script works but the exe fails, the issue is specific to the frozen executable.")
print()
input("Press Enter to exit...")
