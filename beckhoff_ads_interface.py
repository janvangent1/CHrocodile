# -*- coding: utf-8 -*-
"""
Clean Beckhoff ADS Interface using pyads
Stable, minimal, production-ready implementation
"""

import os
import sys
import time
import threading
from typing import Callable, Optional, Dict, Any

# Fix TwinCAT DLL loading for frozen executable (PyInstaller, etc.)
# Must be BEFORE importing pyads. Frozen .exe does not inherit DLL search paths.
if getattr(sys, 'frozen', False):
    _tc_path = r"C:\Program Files (x86)\Beckhoff\TwinCAT\Common64"
    if os.path.exists(_tc_path):
        os.add_dll_directory(_tc_path)

# Try to import pyads - handle missing DLL gracefully
PYADS_AVAILABLE = False
try:
    import pyads
    _ = pyads.PORT_TC3PLC1  # Test if DLL is available
    PYADS_AVAILABLE = True
except (ImportError, OSError, FileNotFoundError) as e:
    PYADS_AVAILABLE = False
    pyads = None
    print(f"Warning: pyads not available: {e}")
    print("Note: TwinCAT Runtime or XAE must be installed.")
    print("DLL location: C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\Common64")


class BeckhoffADSInterface:
    """
    Clean ADS interface for TwinCAT 3 PLC communication.
    """

    def __init__(
        self,
        ams_netid: str = '127.0.0.1.1.1',
        port: int = None,
        callback: Optional[Callable] = None,
        symbol_prefix: str = 'GVL_CHRocodile.',
        log_callback: Optional[Callable] = None,
        write_timeout: float = 1.0,
    ):
        """
        Initialize ADS interface.
        
        Args:
            ams_netid: PLC AMS NetID (e.g. "127.0.0.1.1.1")
            port: ADS port (default: 851 = PORT_TC3PLC1)
            callback: Callback function for PLC commands (command: str) -> dict
            symbol_prefix: Prefix of PLC variables
            log_callback: Optional logging function (event_type: str, message: str, data: dict = None)
            write_timeout: Timeout for write operations (seconds)
        """
        if not PYADS_AVAILABLE:
            raise ImportError("pyads library is required. Install with: pip install pyads")

        self.ams_netid = ams_netid
        self.port = port if port is not None else (pyads.PORT_TC3PLC1 if pyads else 851)
        self.callback = callback
        self.symbol_prefix = symbol_prefix
        self.log_callback = log_callback
        self.write_timeout = write_timeout

        self.plc: Optional[pyads.Connection] = None
        self.running = False
        self.thread = None
        self._last_error = None

        # PLC variable names (can be customized)
        self.var_trigger = f'{symbol_prefix}bTriggerMeasurement'
        self.var_start_cont = f'{symbol_prefix}bStartContinuous'
        self.var_stop_cont = f'{symbol_prefix}bStopContinuous'
        self.var_interval = f'{symbol_prefix}nIntervalMs'
        self.var_busy = f'{symbol_prefix}bMeasurementBusy'
        self.var_ready = f'{symbol_prefix}bMeasurementReady'
        self.var_ack = f'{symbol_prefix}bMeasurementAck'
        self.var_thickness = f'{symbol_prefix}rThickness'
        self.var_peak1 = f'{symbol_prefix}rPeak1'
        self.var_peak2 = f'{symbol_prefix}rPeak2'
        self.var_count = f'{symbol_prefix}nMeasurementCount'
        self.var_error = f'{symbol_prefix}sError'

        # State tracking
        self._last_trigger_state = False
        self._last_start_cont_state = False
        self._last_stop_cont_state = False
        self._measurement_in_progress = False

    # -----------------------------------------------------
    # Logging helper
    # -----------------------------------------------------

    def _log(self, event_type: str, message: str, data: Dict[str, Any] = None):
        """Log message via callback or print."""
        if self.log_callback:
            try:
                self.log_callback(event_type, message, data or {})
            except Exception:
                # Log callback might be invalid (window destroyed, etc.)
                # Clear it to prevent future errors
                self.log_callback = None
                print(f"[ADS {event_type.upper()}] {message}")
        else:
            print(f"[ADS {event_type.upper()}] {message}")

    # -----------------------------------------------------
    # Connection
    # -----------------------------------------------------

    def connect(self) -> bool:
        """Connect to PLC."""
        if not PYADS_AVAILABLE:
            self._last_error = "pyads library not available"
            return False

        try:
            if self.plc is not None:
                try:
                    if self.plc.is_open:
                        self.plc.close()
                except:
                    pass
                self.plc = None
                time.sleep(0.1)

            # On Windows: do NOT call open_port() or set_local_address().
            # "SetLocalAddress is not supported for Windows clients" - router assigns address.
            # open_port() can fail on Windows with "Failed to open port on AMS router" even when router is OK.
            # Connection().open() uses a different path and may work when open_port() fails.
            is_localhost = (self.ams_netid.startswith('127.0.0.1') or 
                           self.ams_netid == 'localhost' or
                           self.ams_netid == '127.0.0.1.1.1')

            self.plc = pyads.Connection(self.ams_netid, self.port)
            self.plc.open()
            
            if not self.plc.is_open:
                raise Exception("Connection opened but is_open() returned False")
            
            self._log("connection", f"Connected to PLC at {self.ams_netid}:{self.port}")
            self._last_error = None
            return True
            
        except Exception as e:
            error_msg = str(e)
            self._last_error = error_msg
            
            # Enhanced error message for localhost
            if is_localhost:
                error_msg += (
                    "\n\nTroubleshooting for localhost connection:\n"
                    "- Ensure TwinCAT XAE is installed and System UI (TcSysUI.exe) is running\n"
                    "- ADS Router is loaded by default with XAE\n"
                    "- Try running as Administrator\n"
                    "- Check if another application is using the ADS port\n"
                    "- Verify TwinCAT is properly installed\n"
                    "- DLL location: C:\\Program Files (x86)\\Beckhoff\\TwinCAT\\Common64"
                )
            
            self._log("error", f"Connection failed: {error_msg}")
            self.plc = None
            return False

    def disconnect(self):
        """Disconnect from PLC."""
        # Stop polling thread if running (no separate stop() method)
        self.running = False
        if self.thread:
            self.thread.join(timeout=2.0)
            self.thread = None
        if self.plc:
            try:
                if self.plc.is_open:
                    self.plc.close()
                # Clear log callback before logging to avoid issues during shutdown
                log_cb = self.log_callback
                self.log_callback = None
                if log_cb:
                    try:
                        log_cb("connection", "Disconnected from PLC", {})
                    except:
                        pass  # Window might be destroyed
            except Exception as e:
                # Don't log errors during disconnect - might cause issues
                pass
            finally:
                self.plc = None

    def is_connected(self) -> bool:
        """Check if connected to PLC."""
        return self.plc is not None and self.plc.is_open

    # -----------------------------------------------------
    # Safe read/write
    # -----------------------------------------------------

    def _read(self, name: str, plc_type):
        """Read PLC variable."""
        if not self.plc or not self.plc.is_open:
            raise RuntimeError("PLC not connected")
        return self.plc.read_by_name(name, plc_type)

    def _write(self, name: str, value, plc_type) -> bool:
        """Write PLC variable with timeout."""
        if not self.plc or not self.plc.is_open:
            return False

        def write_op():
            self.plc.write_by_name(name, value, plc_type)

        result = [None]
        exception = [None]

        def write_thread():
            try:
                write_op()
                result[0] = True
            except Exception as e:
                exception[0] = e

        thr = threading.Thread(target=write_thread, daemon=True)
        thr.start()
        thr.join(timeout=self.write_timeout)

        if thr.is_alive():
            self._log("error", f"Write timeout: {name}")
            return False

        if exception[0]:
            self._log("error", f"Write error {name}: {exception[0]}")
            return False

        return True

    # -----------------------------------------------------
    # Polling
    # -----------------------------------------------------

    def start_polling(self, poll_interval: float = 0.1) -> bool:
        """Start polling PLC variables."""
        if self.running:
            return False

        if not self.plc or not self.plc.is_open:
            if not self.connect():
                return False

        # Read initial trigger state to avoid false trigger
        try:
            self._last_trigger_state = self._read(self.var_trigger, pyads.PLCTYPE_BOOL)
        except Exception:
            self._last_trigger_state = False

        self.running = True
        self.thread = threading.Thread(target=self._loop, args=(poll_interval,), daemon=True)
        self.thread.start()
        self._log("connection", f"Polling started (interval: {poll_interval}s)")
        return True

    def stop_polling(self):
        """Stop polling."""
        self.running = False
        if self.thread:
            self.thread.join(timeout=2.0)
            self.thread = None
        # Log before disconnecting (disconnect clears log_callback)
        try:
            self._log("connection", "Polling stopped")
        except:
            pass  # Log callback might be invalid
        self.disconnect()

    def is_running(self) -> bool:
        """Check if polling is running."""
        return self.running

    def get_last_error(self) -> Optional[str]:
        """Get last error message."""
        return self._last_error

    def get_disabled_variables(self):
        """Get disabled variables (for compatibility with monitor)."""
        return set()  # Simplified version doesn't disable variables

    def reset_disabled_variables(self):
        """Reset disabled variables (for compatibility with monitor)."""
        pass  # Simplified version doesn't disable variables

    # -----------------------------------------------------
    # Polling Loop
    # -----------------------------------------------------

    def _loop(self, interval: float):
        """Main polling loop."""
        while self.running:
            try:
                if not self.running:  # Check again after potential blocking operations
                    break
                    
                if not self.plc or not self.plc.is_open:
                    time.sleep(interval)
                    continue

                # Read trigger
                trigger = self._read(self.var_trigger, pyads.PLCTYPE_BOOL)

                # Rising edge detection
                if trigger and not self._last_trigger_state and not self._measurement_in_progress:
                    self._handle_trigger()

                # Continuous measurement controls
                try:
                    start_cont = self._read(self.var_start_cont, pyads.PLCTYPE_BOOL)
                    stop_cont = self._read(self.var_stop_cont, pyads.PLCTYPE_BOOL)

                    if start_cont and not self._last_start_cont_state:
                        try:
                            interval_ms = self._read(self.var_interval, pyads.PLCTYPE_UDINT)
                        except:
                            interval_ms = 100
                        if self.callback:
                            self.callback("start_continuous", interval_ms=interval_ms)
                        self._log("trigger", f"Start continuous (interval: {interval_ms}ms)")

                    if stop_cont and not self._last_stop_cont_state:
                        if self.callback:
                            self.callback("stop_continuous")
                        self._log("trigger", "Stop continuous")

                    self._last_start_cont_state = start_cont
                    self._last_stop_cont_state = stop_cont
                except:
                    pass  # Continuous variables might not exist

                # Update trigger state
                if not self._measurement_in_progress:
                    self._last_trigger_state = trigger

            except pyads.ADSError as e:
                if self.running:  # Only log if still running
                    self._log("error", f"ADS error: {e}")
                if not self.is_connected() and self.running:
                    self._attempt_reconnect()
            except Exception as e:
                if self.running:  # Only log if still running
                    self._log("error", f"Polling error: {e}")

            if self.running:  # Only sleep if still running
                time.sleep(interval)

    # -----------------------------------------------------
    # Measurement Handling
    # -----------------------------------------------------

    def _handle_trigger(self):
        """Handle measurement trigger."""
        self._measurement_in_progress = True
        self._log("trigger", "Measurement triggered")

        try:
            self._write(self.var_busy, True, pyads.PLCTYPE_BOOL)
            self._write(self.var_ready, False, pyads.PLCTYPE_BOOL)

            if self.callback:
                self.callback("trigger_measurement")

        except Exception as e:
            self._log("error", f"Trigger error: {e}")
            self._write(self.var_error, str(e)[:255], pyads.PLCTYPE_STRING)
            self._write(self.var_busy, False, pyads.PLCTYPE_BOOL)
            self._measurement_in_progress = False

    def write_measurement_result(self, measurement_data: Dict[str, Any]):
        """Write measurement results to PLC."""
        if not self.plc or not self.plc.is_open:
            self._measurement_in_progress = False
            return

        try:
            # Sync trigger state before writing handshake
            try:
                current_trigger = self._read(self.var_trigger, pyads.PLCTYPE_BOOL)
                self._last_trigger_state = current_trigger
            except:
                pass

            # Write results
            thickness = measurement_data.get('thickness')
            if thickness is not None:
                self._write(self.var_thickness, float(thickness), pyads.PLCTYPE_REAL)

            peak1 = measurement_data.get('peak1')
            if peak1 is not None:
                self._write(self.var_peak1, float(peak1), pyads.PLCTYPE_REAL)

            peak2 = measurement_data.get('peak2')
            if peak2 is not None:
                self._write(self.var_peak2, float(peak2), pyads.PLCTYPE_REAL)

            count = measurement_data.get('measurement_count', 0)
            self._write(self.var_count, int(count), pyads.PLCTYPE_UDINT)

            # Handshake
            self._write(self.var_busy, False, pyads.PLCTYPE_BOOL)
            self._write(self.var_ready, True, pyads.PLCTYPE_BOOL)
            self._write(self.var_error, '', pyads.PLCTYPE_STRING)

            self._measurement_in_progress = False
            self._log("handshake", "Measurement complete")

        except Exception as e:
            self._log("error", f"Write result error: {e}")
            self._write(self.var_busy, False, pyads.PLCTYPE_BOOL)
            self._measurement_in_progress = False

    # -----------------------------------------------------
    # Reconnect Logic
    # -----------------------------------------------------

    def _attempt_reconnect(self):
        """Attempt to reconnect to PLC."""
        try:
            self.disconnect()
            time.sleep(1)
            if self.connect():
                self._log("connection", "Reconnected successfully")
        except Exception as e:
            self._log("error", f"Reconnect failed: {e}")
            time.sleep(2)
