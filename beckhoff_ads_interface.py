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

def _candidate_twincat_dll_dirs():
    """Return likely TwinCAT Common64 locations, in preferred order."""
    candidates = []

    # Runtime/XAE installs often set TWINCAT3DIR; prefer that when present.
    tc3dir = os.environ.get("TWINCAT3DIR")
    if tc3dir:
        candidates.append(os.path.normpath(os.path.join(tc3dir, "..", "Common64")))
        candidates.append(os.path.normpath(os.path.join(tc3dir, "Common64")))

    # Common installation locations (runtime and XAE variants).
    candidates.extend([
        r"C:\TwinCAT\Common64",
        r"C:\TwinCAT\3.1\Common64",
        r"C:\Program Files (x86)\Beckhoff\TwinCAT\Common64",
    ])

    # Keep order but remove duplicates.
    unique = []
    seen = set()
    for path in candidates:
        key = path.lower()
        if key not in seen:
            unique.append(path)
            seen.add(key)
    return unique


# Try to import pyads; only add DLL directories as fallback if import fails.
PYADS_AVAILABLE = False
pyads = None
_import_error = None

try:
    import pyads as _pyads_mod
    _ = _pyads_mod.PORT_TC3PLC1
    pyads = _pyads_mod
    PYADS_AVAILABLE = True
except (ImportError, OSError, FileNotFoundError) as e:
    _import_error = e

if not PYADS_AVAILABLE and getattr(sys, "frozen", False):
    for _tc_path in _candidate_twincat_dll_dirs():
        try:
            if os.path.exists(_tc_path):
                os.add_dll_directory(_tc_path)
                import pyads as _pyads_mod
                _ = _pyads_mod.PORT_TC3PLC1
                pyads = _pyads_mod
                PYADS_AVAILABLE = True
                break
        except (ImportError, OSError, FileNotFoundError):
            continue

if not PYADS_AVAILABLE:
    print(f"Warning: pyads not available: {_import_error}")
    print("Note: TwinCAT Runtime or XAE must be installed.")


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
        write_timeout: float = 0.5,
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
        self.port = port if port is not None else pyads.PORT_TC3PLC1
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

        # Polling stats (for monitor display)
        self._poll_interval = 0.0
        self._poll_count = 0
        self._last_poll_time: Optional[float] = None

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

        # Clean up any previous connection
        if self.plc is not None:
            try:
                self.plc.close()
            except Exception:
                pass
            self.plc = None

        def _try_open(target_ams_netid: str, target_port: int):
            plc = pyads.Connection(target_ams_netid, target_port)
            try:
                plc.open()
                if not plc.is_open:
                    raise RuntimeError("ADS connection did not open")
                return plc, None
            except Exception as e:
                try:
                    plc.close()
                except Exception:
                    pass
                return None, e

        # Try configured endpoint first; then optional localhost fallback.
        attempts = [(self.ams_netid, self.port, "configured")]
        fallback = ("127.0.0.1.1.1", pyads.PORT_TC3PLC1, "localhost fallback")
        if (str(self.ams_netid).strip(), int(self.port)) != (fallback[0], int(fallback[1])):
            attempts.append(fallback)

        errors = []
        for ams_netid, port, label in attempts:
            plc, err = _try_open(ams_netid, port)
            if plc is not None:
                self.plc = plc
                self._last_error = None
                self._log("connection", f"Connected to PLC at {ams_netid}:{port} ({label})")
                return True
            errors.append(f"{label}: {err if err else 'Unknown error'}")

        self._last_error = " | ".join(errors)
        self._log("error", f"Connection failed: {self._last_error}")
        return False

    def disconnect(self):
        """Disconnect from PLC."""
        self.running = False

        # Only join the polling thread if called from a different thread
        if self.thread and self.thread is not threading.current_thread():
            self.thread.join(timeout=2.0)
            self.thread = None

        if self.plc:
            try:
                self.plc.close()
                log_cb = self.log_callback
                self.log_callback = None
                if log_cb:
                    try:
                        log_cb("connection", "Disconnected from PLC", {})
                    except Exception:
                        pass
            except Exception:
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
        self._poll_interval = poll_interval
        self._poll_count = 0
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

    def get_polling_stats(self) -> Dict[str, Any]:
        """Return polling stats for monitor display."""
        return {
            "running": self.running,
            "interval": self._poll_interval,
            "poll_count": self._poll_count,
            "last_poll_time": self._last_poll_time,
        }

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

                # Update polling stats
                self._poll_count += 1
                self._last_poll_time = time.time()

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
        """Attempt to reconnect to PLC (called from polling thread)."""
        # Close only the ADS connection, do NOT call disconnect() which would
        # set self.running=False and kill the polling thread we're running in.
        if self.plc:
            try:
                self.plc.close()
            except Exception:
                pass
            self.plc = None

        time.sleep(0.3)

        try:
            if self.connect():
                self._log("connection", "Reconnected successfully")
            else:
                time.sleep(1.0)
        except Exception as e:
            self._log("error", f"Reconnect failed: {e}")
            time.sleep(1.0)
