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

# ---- Test 1: Connection (same pattern as working colleague script) ----
print("Test 1: pyads.Connection('127.0.0.1.1.1', pyads.PORT_TC3PLC1) ...")
plc = None
test1_ok = False
try:
    plc = pyads.Connection("127.0.0.1.1.1", pyads.PORT_TC3PLC1)
    plc.open()
    print("  -> Connection opened. SUCCESS.")
    test1_ok = True
except Exception as e:
    print(f"  -> FAILED: {e}")
    if hasattr(e, "ads_err_code"):
        print(f"  -> ADS error code: {e.ads_err_code}")
finally:
    if plc is not None:
        try:
            if plc.is_open:
                plc.close()
            print("  -> Connection closed.")
        except Exception:
            pass
print()

# ---- Test 2: Connection + read_state (PLC reachability) ----
print("Test 2: Connection + plc.read_state() ...")
plc = None
test2_ok = False
try:
    plc = pyads.Connection("127.0.0.1.1.1", pyads.PORT_TC3PLC1)
    plc.open()
    state = plc.read_state()
    print(f"  -> read_state() OK: ads_state={state.ads_state}, device_state={state.device_state}")
    test2_ok = True
except Exception as e:
    print(f"  -> FAILED: {e}")
    if hasattr(e, "ads_err_code"):
        print(f"  -> ADS error code: {e.ads_err_code}")
finally:
    if plc is not None:
        try:
            if plc.is_open:
                plc.close()
            print("  -> Connection closed.")
        except Exception:
            pass
print()

# ---- Test 3: Read a known test variable (like colleague script) ----
print("Test 3: Read GVL_Test.counter (DINT) ...")
plc = None
test3_ok = False
try:
    plc = pyads.Connection("127.0.0.1.1.1", pyads.PORT_TC3PLC1)
    plc.open()
    counter = plc.read_by_name("GVL_Test.counter", pyads.PLCTYPE_DINT)
    print(f"  -> GVL_Test.counter = {counter}. SUCCESS.")
    test3_ok = True
except pyads.ADSError as e:
    print(f"  -> FAILED: {e}")
    if hasattr(e, "err_code"):
        print(f"  -> ADS error code: {e.err_code}")
    if "not found" in str(e).lower() or e.err_code == 1808:
        print("  -> Variable 'GVL_Test.counter' does not exist in PLC program.")
        print("     This is OK if your PLC uses a different GVL name.")
except Exception as e:
    print(f"  -> FAILED: {e}")
finally:
    if plc is not None:
        try:
            if plc.is_open:
                plc.close()
        except Exception:
            pass
print()

# ---- Test 4: Read GVL_CHRocodile variables (what the main app uses) ----
print("Test 4: Read GVL_CHRocodile.bTriggerMeasurement (BOOL) ...")
plc = None
test4_ok = False
try:
    plc = pyads.Connection("127.0.0.1.1.1", pyads.PORT_TC3PLC1)
    plc.open()
    val = plc.read_by_name("GVL_CHRocodile.bTriggerMeasurement", pyads.PLCTYPE_BOOL)
    print(f"  -> GVL_CHRocodile.bTriggerMeasurement = {val}. SUCCESS.")
    test4_ok = True
except pyads.ADSError as e:
    print(f"  -> FAILED: {e}")
    if hasattr(e, "err_code"):
        print(f"  -> ADS error code: {e.err_code}")
    if "not found" in str(e).lower() or (hasattr(e, "err_code") and e.err_code == 1808):
        print("  -> Variable 'GVL_CHRocodile.bTriggerMeasurement' does NOT exist on this PLC!")
        print("     *** THIS is why the main app fails locally but works remotely. ***")
        print("     The GVL_CHRocodile variables only exist on the remote PLC.")
except Exception as e:
    print(f"  -> FAILED: {e}")
finally:
    if plc is not None:
        try:
            if plc.is_open:
                plc.close()
        except Exception:
            pass
print()

# ---- Summary ----
print("=" * 60)
print("SUMMARY")
print("=" * 60)

if test1_ok and test2_ok:
    print("Connection to local PLC: OK")
else:
    print("Connection to local PLC: FAILED")
    print("  -> Check TwinCAT System Service (TcSysSrv) is RUNNING")
    print("  -> Try running as Administrator")
    print()

if test3_ok:
    print("GVL_Test.counter: EXISTS (colleague's test variable)")
elif test1_ok:
    print("GVL_Test.counter: NOT FOUND (not critical)")

if test4_ok:
    print("GVL_CHRocodile variables: EXISTS (main app will work)")
elif test1_ok:
    print("GVL_CHRocodile variables: NOT FOUND")
    print()
    print("  *** ROOT CAUSE: The main app fails locally because the PLC program")
    print("      on this PC does not have GVL_CHRocodile variables. ***")
    print()
    print("  Your colleague's script works because it reads GVL_Test.counter,")
    print("  which DOES exist on this PLC.")
    print()
    print("  To fix this, either:")
    print("  1. Add GVL_CHRocodile to the local PLC program, OR")
    print("  2. Change the symbol_prefix in chrocodile_settings.json to match")
    print("     the local PLC's GVL name (e.g., 'GVL_Test.')")

if not test1_ok:
    print()
    print("Additional troubleshooting:")
    print("  - Run from a LOCAL folder (not OneDrive/network drive)")
    print("  - Ensure TcSysUI.exe is running in the system tray")
    print("  - Restart the PC if needed")
print()
input("Press Enter to exit...")
