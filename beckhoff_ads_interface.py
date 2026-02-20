# -*- coding: utf-8 -*-
"""
Beckhoff ADS Interface Module using pyads library
Provides ADS-based communication for triggering measurements from TwinCAT PLC.
This uses Beckhoff's native ADS protocol via the pyads library.
"""

import threading
import time
import os
import sys
from typing import Optional, Callable
import queue

# Try to help pyads find TwinCAT DLL by adding common locations to DLL search path
# TwinCAT 3.1.4024 and earlier: C:\TwinCAT\AdsApi\TcAdsDll\x64\
# TwinCAT 3.1.4026 and later: C:\Program Files (x86)\Beckhoff\TwinCAT\Common64\
# We check BOTH locations to support all TwinCAT versions
_twincat_dll_paths = [
    # Old location (TwinCAT 3.1.4024 and earlier)
    r"C:\TwinCAT\AdsApi\TcAdsDll\x64",      # 64-bit
    r"C:\TwinCAT\AdsApi\TcAdsDll\Win32",    # 32-bit
    # New location (TwinCAT 3.1.4026 and later)
    r"C:\Program Files (x86)\Beckhoff\TwinCAT\Common64",  # 64-bit (x86 folder but 64-bit DLL)
    r"C:\Program Files\Beckhoff\TwinCAT\Common64",       # Alternative 64-bit location
]

# Add ALL existing DLL directories to search path (checks both old and new locations)
_found_dll_paths = []
for dll_path in _twincat_dll_paths:
    if os.path.exists(dll_path):
        # Check if DLL actually exists in this directory
        dll_file = os.path.join(dll_path, "TcAdsDll.dll")
        if os.path.exists(dll_file):
            _found_dll_paths.append(dll_path)
            try:
                # Python 3.8+ - add DLL directory to search path
                if hasattr(os, 'add_dll_directory'):
                    os.add_dll_directory(dll_path)
                # Also add to PATH for older Python versions or ctypes
                if dll_path not in os.environ.get('PATH', ''):
                    os.environ['PATH'] = dll_path + os.pathsep + os.environ.get('PATH', '')
            except Exception:
                pass

# Try to import pyads, make it optional
# Note: pyads tries to load TcAdsDll.dll at import time, so we catch all exceptions
PYADS_AVAILABLE = False
try:
    import pyads
    # Test if DLL is actually available by trying to access a constant
    _ = pyads.PORT_TC3PLC1
    PYADS_AVAILABLE = True
except ImportError:
    print("Warning: pyads library not available. Install with: pip install pyads")
except (OSError, FileNotFoundError, AttributeError) as e:
    # pyads is installed but TwinCAT DLL is missing or not accessible
    PYADS_AVAILABLE = False
    print(f"Warning: pyads requires TwinCAT ADS DLL (TcAdsDll.dll).")
    print(f"Error: {type(e).__name__}: {e}")
    print("Note: TwinCAT Runtime must be installed on this PC for ADS communication.")
    print("Searched in (both old and new locations):")
    for path in _twincat_dll_paths:
        dll_file = os.path.join(path, "TcAdsDll.dll")
        if os.path.exists(dll_file):
            print(f"  ✓ {path} (DLL found)")
        elif os.path.exists(path):
            print(f"  ⚠ {path} (directory exists, but DLL not found)")
        else:
            print(f"  ✗ {path} (not found)")
    print("The application will continue but Beckhoff ADS interface will be disabled.")
except Exception as e:
    # Catch any other import errors
    PYADS_AVAILABLE = False
    print(f"Warning: pyads import failed: {type(e).__name__}: {e}")
    print("The application will continue but Beckhoff ADS interface will be disabled.")


class BeckhoffADSInterface:
    """
    ADS-based interface for Beckhoff PLC communication using pyads.
    Reads PLC variables to detect measurement triggers and writes results back.
    
    This interface works by:
    1. Reading PLC variables (e.g., bTriggerMeasurement) periodically
    2. Writing measurement results back to PLC variables (e.g., rThickness)
    3. Using ADS notifications for efficient real-time updates
    """
    
    def __init__(self, ams_netid: str = '127.0.0.1.1.1', 
                 port: int = None,
                 callback: Optional[Callable] = None,
                 symbol_prefix: str = 'GVL_CHRocodile.',
                 log_callback: Optional[Callable] = None,
                 write_timeout: float = 1.0):
        """
        Initialize the ADS interface.
        
        Args:
            ams_netid: AMS NetID of the PLC (default: localhost)
            port: ADS port (default: PORT_TC3PLC1 = 851)
            callback: Callback function to trigger measurements
                      Signature: callback(command: str) -> dict
            symbol_prefix: Prefix for PLC variable names (default: 'GVL_CHRocodile.')
            log_callback: Optional callback for logging communication events
                         Signature: log_callback(event_type: str, message: str, data: dict = None)
        """
        if not PYADS_AVAILABLE:
            raise ImportError("pyads library is required. Install with: pip install pyads")
        
        self.ams_netid = ams_netid
        self.port = port
        self.callback = callback
        self.log_callback = log_callback
        self.symbol_prefix = symbol_prefix
        self.write_timeout = write_timeout  # Default timeout for write operations
        
        self.plc = None
        self.running = False
        self.poll_thread = None
        
        # PLC variable names (can be customized)
        # Main trigger variable - cyclically checked, triggers measurement when high
        self.var_trigger = f'{symbol_prefix}bTriggerMeasurement'
        
        # Optional: continuous measurement controls
        self.var_start_cont = f'{symbol_prefix}bStartContinuous'
        self.var_stop_cont = f'{symbol_prefix}bStopContinuous'
        self.var_interval = f'{symbol_prefix}nIntervalMs'
        
        # Handshake variables (written by Python)
        self.var_busy = f'{symbol_prefix}bMeasurementBusy'  # TRUE while measurement in progress
        self.var_ready = f'{symbol_prefix}bMeasurementReady'  # TRUE when measurement complete
        self.var_ack = f'{symbol_prefix}bMeasurementAck'  # PLC acknowledges receipt (read by Python)
        
        # Result variables (written by Python)
        self.var_thickness = f'{symbol_prefix}rThickness'
        self.var_peak1 = f'{symbol_prefix}rPeak1'
        self.var_peak2 = f'{symbol_prefix}rPeak2'
        self.var_count = f'{symbol_prefix}nMeasurementCount'
        self.var_error = f'{symbol_prefix}sError'
        
        # State tracking for edge detection
        self.last_trigger_state = False
        self.last_start_cont_state = False
        self.last_stop_cont_state = False
        self.measurement_in_progress = False
        
        # Smart timeout handling: track consecutive timeouts per variable
        # If a variable times out 3 times in a row, disable it to avoid unnecessary writes
        self._write_timeout_count = {}  # Track consecutive timeouts per variable
        self._disabled_variables = set()  # Variables disabled due to repeated timeouts
        self._max_consecutive_timeouts = 3  # Disable after 3 consecutive timeouts
        
        # Store last connection error for detailed reporting
        self._last_connection_error = None
        self._last_polling_error = None
        
    def _safe_write(self, variable_name: str, value, plc_type, timeout: float = None):
        """
        Safely write to PLC variable with timeout to prevent blocking on non-existent variables.
        Implements smart timeout handling: if a variable times out 3 times consecutively,
        it will be disabled to avoid unnecessary writes.
        
        Args:
            variable_name: Name of PLC variable
            value: Value to write
            plc_type: pyads PLCTYPE constant
            timeout: Timeout in seconds (default: uses self.write_timeout)
        
        Returns:
            True if write succeeded, False otherwise
        """
        if timeout is None:
            timeout = self.write_timeout
        
        if not self.plc or not self.plc.is_open:
            return False
        
        # Check if variable is disabled due to repeated timeouts
        if variable_name in self._disabled_variables:
            # Variable is disabled - don't attempt write (silently skip)
            return False
        
        def write_operation():
            try:
                self.plc.write_by_name(variable_name, value, plc_type)
                return True
            except Exception as e:
                raise e
        
        # Use threading to implement timeout
        result = [None]
        exception = [None]
        
        def write_thread():
            try:
                result[0] = write_operation()
            except Exception as e:
                exception[0] = e
        
        write_thr = threading.Thread(target=write_thread, daemon=True)
        write_thr.start()
        write_thr.join(timeout=timeout)
        
        if write_thr.is_alive():
            # Write timed out - variable probably doesn't exist
            # Increment timeout counter
            if variable_name not in self._write_timeout_count:
                self._write_timeout_count[variable_name] = 0
            self._write_timeout_count[variable_name] += 1
            
            timeout_count = self._write_timeout_count[variable_name]
            
            if timeout_count >= self._max_consecutive_timeouts:
                # Disable this variable - it doesn't exist or is not accessible
                self._disabled_variables.add(variable_name)
                if self.log_callback:
                    self.log_callback("error", 
                                    f"Variable disabled after {timeout_count} consecutive timeouts: {variable_name}", 
                                    {"variable": variable_name, "timeout_count": timeout_count, 
                                     "type": "variable_disabled"})
            else:
                if self.log_callback:
                    self.log_callback("error", 
                                    f"Write timeout ({timeout_count}/{self._max_consecutive_timeouts}): {variable_name}", 
                                    {"variable": variable_name, "timeout": timeout, 
                                     "timeout_count": timeout_count, "type": "write_timeout"})
            return False
        
        if exception[0]:
            # Write failed with exception
            # Reset timeout counter on exception (might be temporary issue)
            if variable_name in self._write_timeout_count:
                self._write_timeout_count[variable_name] = 0
            
            if self.log_callback:
                self.log_callback("error", f"Write failed: {variable_name} - {exception[0]}", 
                                {"variable": variable_name, "error": str(exception[0]), "type": "write_error"})
            return False
        
        # Write succeeded - reset timeout counter
        if variable_name in self._write_timeout_count:
            self._write_timeout_count[variable_name] = 0
        
        # If variable was disabled but write now succeeds, re-enable it
        if variable_name in self._disabled_variables:
            self._disabled_variables.remove(variable_name)
            if self.log_callback:
                self.log_callback("connection", f"Variable re-enabled (write succeeded): {variable_name}", 
                                {"variable": variable_name})
        
        return result[0] is True
    
    def reset_disabled_variables(self):
        """
        Reset disabled variables list to allow retrying writes.
        Useful if PLC variables are added after application start.
        """
        disabled_count = len(self._disabled_variables)
        self._disabled_variables.clear()
        self._write_timeout_count.clear()
        if self.log_callback:
            self.log_callback("connection", f"Reset {disabled_count} disabled variables - all variables re-enabled", 
                            {"disabled_count": disabled_count})
    
    def get_disabled_variables(self):
        """
        Get list of currently disabled variables.
        
        Returns:
            Set of disabled variable names
        """
        return self._disabled_variables.copy()
    
    def get_last_error(self):
        """
        Get the last connection or polling error message.
        
        Returns:
            Error message string or None if no error occurred
        """
        return self._last_connection_error or self._last_polling_error
    
    def connect(self) -> bool:
        """
        Connect to the PLC via ADS.
        If a connection already exists, it will be closed first.
        
        Returns:
            True if connected successfully, False otherwise
        """
        # Close existing connection if any (to handle "port already open" errors)
        # This is important when switching between remote and localhost connections
        if self.plc is not None:
            try:
                if hasattr(self.plc, 'is_open') and self.plc.is_open:
                    if self.log_callback:
                        self.log_callback("connection", "Closing existing connection before reconnecting...", {})
                    self.plc.close()
                    # Wait a bit to ensure port is released
                    time.sleep(0.2)
            except Exception as e:
                # Ignore errors when closing existing connection, but log them
                if self.log_callback:
                    self.log_callback("connection", f"Note: Error closing existing connection: {e}", {})
            finally:
                # Always set to None to ensure cleanup
                self.plc = None
                # Additional small delay to ensure OS releases the port
                time.sleep(0.1)
        
        try:
            # Determine port (use default if None)
            port = self.port if self.port is not None else pyads.PORT_TC3PLC1
            port_display = port if self.port is not None else f"{port} (default)"
            
            if self.log_callback:
                self.log_callback("connection", f"Attempting to connect to PLC at {self.ams_netid}:{port_display}", 
                                {"ams_netid": self.ams_netid, "port": port})
            
            self.plc = pyads.Connection(self.ams_netid, port)
            self.plc.open()
            
            # Verify connection is actually working by checking if it's open
            if not self.plc.is_open:
                raise Exception("Connection opened but is_open() returned False")
            
            msg = f"Connected to PLC at {self.ams_netid}:{port_display}"
            print(f"[ADS Interface] {msg}")
            if self.log_callback:
                self.log_callback("connection", msg, {"ams_netid": self.ams_netid, "port": port})
            return True
            
        except pyads.ADSError as e:
            # ADS-specific errors - pass through original error message
            error_code = getattr(e, 'ads_err_code', None)
            error_msg = str(e)
            
            # Handle port already connected (code 19) - try to reconnect
            if error_code == 19:
                try:
                    if self.plc:
                        self.plc.close()
                    self.plc = None
                    time.sleep(0.1)
                    self.plc = pyads.Connection(self.ams_netid, port)
                    self.plc.open()
                    msg = f"Reconnected to PLC at {self.ams_netid}:{port_display} after closing existing connection"
                    print(f"[ADS Interface] {msg}")
                    if self.log_callback:
                        self.log_callback("connection", msg, {"ams_netid": self.ams_netid, "port": port})
                    return True
                except Exception as retry_e:
                    error_msg = f"{error_msg} (Retry failed: {retry_e})"
            
            print(f"[ADS Interface] Connection failed: {error_msg}")
            if self.log_callback:
                self.log_callback("error", error_msg, {
                    "error": error_msg,
                    "error_code": error_code,
                    "ams_netid": self.ams_netid,
                    "port": port,
                    "type": "ads_connection_error"
                })
            self.plc = None
            self._last_connection_error = error_msg
            return False
            
        except Exception as e:
            error_type = type(e).__name__
            error_msg = str(e)
            
            # Handle port locked errors - try to reconnect
            if "failed to open port" in error_msg.lower() or "open port" in error_msg.lower() or "port.*lock" in error_msg.lower() or "locked" in error_msg.lower():
                try:
                    if self.plc:
                        try:
                            if hasattr(self.plc, 'is_open') and self.plc.is_open:
                                self.plc.close()
                        except:
                            pass
                    self.plc = None
                    time.sleep(0.5)
                    port = self.port if self.port is not None else pyads.PORT_TC3PLC1
                    port_display = port if self.port is not None else f"{port} (default)"
                    self.plc = pyads.Connection(self.ams_netid, port)
                    self.plc.open()
                    msg = f"Reconnected to PLC at {self.ams_netid}:{port_display} after resolving port conflict"
                    print(f"[ADS Interface] {msg}")
                    if self.log_callback:
                        self.log_callback("connection", msg, {"ams_netid": self.ams_netid, "port": port})
                    return True
                except Exception as retry_e:
                    error_msg = f"{error_msg} (Retry failed: {retry_e})"
            
            print(f"[ADS Interface] Connection failed: {error_type}: {error_msg}")
            if self.log_callback:
                self.log_callback("error", f"{error_type}: {error_msg}", {
                    "error": error_msg,
                    "error_type": error_type,
                    "ams_netid": self.ams_netid,
                    "port": self.port,
                    "type": "connection_error"
                })
            self.plc = None
            self._last_connection_error = f"{error_type}: {error_msg}"
            return False
    
    def disconnect(self):
        """Disconnect from the PLC and ensure port is released."""
        if self.plc:
            try:
                # Check if connection is open before closing
                if self.plc.is_open:
                    self.plc.close()
                    # Small delay to ensure port is released by OS
                    time.sleep(0.1)
                msg = "Disconnected from PLC"
                print(f"[ADS Interface] {msg}")
                if self.log_callback:
                    self.log_callback("connection", msg)
            except Exception as e:
                msg = f"Error during disconnect: {e}"
                print(f"[ADS Interface] {msg}")
                if self.log_callback:
                    self.log_callback("error", msg, {"error": str(e), "type": "disconnect_error"})
            finally:
                # Always set to None, even if close() failed
                self.plc = None
    
    def start_polling(self, poll_interval: float = 0.1):
        """
        Start cyclically polling PLC variables for triggers.
        
        The polling loop continuously checks the trigger variable.
        When it detects a rising edge (FALSE -> TRUE), it triggers a measurement.
        
        Args:
            poll_interval: Polling interval in seconds (default: 0.1 = 100ms)
                          Lower values = more responsive but higher CPU usage
        """
        if self.running:
            return False
        
        if not self.plc or not self.plc.is_open:
            if not self.connect():
                error_msg = "Failed to connect to PLC - polling not started"
                if self._last_connection_error:
                    error_msg += f"\n{self._last_connection_error}"
                if self.log_callback:
                    self.log_callback("error", error_msg, {
                        "type": "connection_failed",
                        "detailed_error": self._last_connection_error
                    })
                self._last_polling_error = error_msg
                return False
        
        # Read current trigger state before starting polling to avoid false rising edge detection
        # This prevents triggering a measurement immediately after restart if trigger is already HIGH
        try:
            current_trigger = self.plc.read_by_name(self.var_trigger, pyads.PLCTYPE_BOOL)
            self.last_trigger_state = current_trigger
            if self.log_callback:
                self.log_callback("connection", f"Initial trigger state read: {self.var_trigger} = {current_trigger}", 
                                {"variable": self.var_trigger, "value": current_trigger})
        except pyads.ADSError as e:
            # ADS-specific error reading trigger - pass through original error
            error_code = getattr(e, 'ads_err_code', None)
            error_msg = str(e)
            
            self.last_trigger_state = False
            if self.log_callback:
                self.log_callback("error", f"Could not read initial trigger state: {error_msg}", {
                    "error": error_msg,
                    "error_code": error_code,
                    "variable": self.var_trigger,
                    "type": "read_error"
                })
            self._last_polling_error = f"Could not read initial trigger state: {error_msg}"
            
        except Exception as e:
            # Other errors reading trigger state - pass through original error
            error_type = type(e).__name__
            error_msg = str(e)
            
            self.last_trigger_state = False
            if self.log_callback:
                self.log_callback("error", f"Could not read initial trigger state: {error_type}: {error_msg}", {
                    "error": error_msg,
                    "error_type": error_type,
                    "variable": self.var_trigger,
                    "type": "read_error"
                })
            self._last_polling_error = f"Could not read initial trigger state: {error_type}: {error_msg}"
        
        self.running = True
        self.poll_thread = threading.Thread(
            target=self._poll_loop,
            args=(poll_interval,),
            daemon=True
        )
        self.poll_thread.start()
        msg = f"Started polling PLC variables (interval: {poll_interval}s)"
        print(f"[ADS Interface] {msg}")
        if self.log_callback:
            self.log_callback("connection", msg, {"poll_interval": poll_interval})
        return True
    
    def stop_polling(self):
        """Stop polling PLC variables."""
        self.running = False
        if self.poll_thread:
            self.poll_thread.join(timeout=2.0)
        msg = "Stopped polling PLC variables"
        print(f"[ADS Interface] {msg}")
        if self.log_callback:
            self.log_callback("connection", msg)
        # Don't reset trigger state here - preserve it for when polling restarts
        # This prevents false triggers when restarting polling
        self.disconnect()
    
    def _poll_loop(self, interval: float):
        """
        Main polling loop running in separate thread.
        Cyclically checks PLC variables and triggers measurements when trigger variable goes high.
        """
        while self.running:
            try:
                if not self.plc or not self.plc.is_open:
                    time.sleep(interval)
                    continue
                
                # Read trigger variables
                try:
                    # Main trigger variable - cyclically checked
                    trigger = self.plc.read_by_name(self.var_trigger, pyads.PLCTYPE_BOOL)
                    if self.log_callback:
                        self.log_callback("read", f"Read {self.var_trigger} = {trigger}", 
                                        {"variable": self.var_trigger, "value": trigger})
                    
                    # Detect rising edge (FALSE -> TRUE): trigger measurement when variable goes high
                    if trigger and not self.last_trigger_state and not self.measurement_in_progress:
                        # Trigger measurement on rising edge
                        if self.log_callback:
                            self.log_callback("trigger", f"Measurement trigger detected (rising edge)", 
                                            {"variable": self.var_trigger})
                        if self.callback:
                            try:
                                # Set busy flag to indicate measurement started
                                self.measurement_in_progress = True
                                # Use safe_write with timeout to prevent blocking on non-existent variables
                                self._safe_write(self.var_busy, True, pyads.PLCTYPE_BOOL)
                                self._safe_write(self.var_ready, False, pyads.PLCTYPE_BOOL)
                                if self.log_callback:
                                    self.log_callback("write", f"Set {self.var_busy} = TRUE, {self.var_ready} = FALSE", 
                                                    {"variable": self.var_busy, "value": True})
                                
                                # Trigger measurement (async - will complete later)
                                result = self.callback("trigger_measurement")
                                # Measurement will be triggered, result written via write_measurement_result()
                            except Exception as e:
                                msg = f"Error triggering measurement: {e}"
                                print(f"[ADS Interface] {msg}")
                                if self.log_callback:
                                    self.log_callback("error", msg, {"error": str(e), "type": "trigger_error"})
                                self.measurement_in_progress = False
                                # Write error to PLC if variable exists (with timeout)
                                self._safe_write(self.var_error, str(e)[:255], pyads.PLCTYPE_STRING)
                                self._safe_write(self.var_busy, False, pyads.PLCTYPE_BOOL)
                    
                    # Check for acknowledgment from PLC (optional - PLC resets ready flag)
                    try:
                        ack = self.plc.read_by_name(self.var_ack, pyads.PLCTYPE_BOOL)
                        if ack and self.measurement_in_progress:
                            # PLC acknowledged, can reset if needed
                            # Note: We don't reset busy here, measurement might still be in progress
                            if self.log_callback:
                                self.log_callback("handshake", f"PLC acknowledgment received", 
                                                 {"variable": self.var_ack, "value": ack})
                    except Exception as e:
                        # Ack variable might not exist - this is OK, don't log as error
                        pass
                    
                    # Optional: continuous measurement controls
                    try:
                        start_cont = self.plc.read_by_name(self.var_start_cont, pyads.PLCTYPE_BOOL)
                        stop_cont = self.plc.read_by_name(self.var_stop_cont, pyads.PLCTYPE_BOOL)
                        
                        if start_cont and not self.last_start_cont_state:
                            # Start continuous measurement
                            try:
                                interval_ms = self.plc.read_by_name(self.var_interval, pyads.PLCTYPE_UDINT)
                            except Exception as e:
                                interval_ms = 100
                                if self.log_callback:
                                    self.log_callback("error", f"Failed to read interval variable: {e}", 
                                                     {"error": str(e), "variable": self.var_interval, "type": "read_error"})
                            
                            if self.callback:
                                if self.log_callback:
                                    self.log_callback("trigger", f"Start continuous measurement (interval: {interval_ms}ms)", 
                                                     {"interval_ms": interval_ms})
                                self.callback("start_continuous", interval_ms=interval_ms)
                        
                        if stop_cont and not self.last_stop_cont_state:
                            # Stop continuous measurement
                            if self.callback:
                                if self.log_callback:
                                    self.log_callback("trigger", "Stop continuous measurement")
                                self.callback("stop_continuous")
                        
                        # Update state
                        self.last_start_cont_state = start_cont
                        self.last_stop_cont_state = stop_cont
                    except Exception as e:
                        # Continuous measurement variables might not exist - log as warning, not error
                        if self.log_callback and "Symbol" not in str(e):  # Don't log "symbol not found" as it's expected
                            self.log_callback("error", f"Error reading continuous measurement variables: {e}", 
                                             {"error": str(e), "type": "read_error"})
                        pass
                    
                    # Update trigger state (for edge detection)
                    # Only update last_trigger_state when NOT in a measurement
                    # This ensures we can detect a new rising edge after measurement completes
                    if not self.measurement_in_progress:
                        self.last_trigger_state = trigger
                    # If measurement is in progress, don't update last_trigger_state
                    # This preserves the state until measurement completes, allowing proper edge detection
                    
                except pyads.ADSError as e:
                    # ADS-specific error - pass through original error
                    error_code = getattr(e, 'ads_err_code', None)
                    error_msg = str(e)
                    
                    # Check if connection is still open
                    connection_status = "open" if (self.plc and self.plc.is_open) else "closed"
                    
                    print(f"[ADS Interface] ADS error reading variables: {error_msg}")
                    if self.log_callback:
                        self.log_callback("error", f"ADS error reading variables: {error_msg}", {
                            "error": error_msg,
                            "ads_err_code": error_code,
                            "connection_status": connection_status,
                            "type": "ads_error"
                        })
                    
                    # If connection is closed, try to reconnect
                    if connection_status == "closed":
                        if self.log_callback:
                            self.log_callback("connection", "Connection lost, attempting to reconnect...", {})
                        try:
                            if self.connect():
                                if self.log_callback:
                                    self.log_callback("connection", "Reconnected successfully", {})
                            else:
                                if self.log_callback:
                                    self.log_callback("error", "Reconnection failed", {})
                        except Exception as reconnect_e:
                            if self.log_callback:
                                self.log_callback("error", f"Reconnection error: {reconnect_e}", {})
                    
                    time.sleep(interval)
                    continue
                except Exception as e:
                    # Other errors - pass through original error
                    error_type = type(e).__name__
                    error_msg = str(e)
                    connection_status = "open" if (self.plc and self.plc.is_open) else "closed"
                    
                    print(f"[ADS Interface] Error reading PLC variables: {error_type}: {error_msg}")
                    if self.log_callback:
                        self.log_callback("error", f"Error reading PLC variables: {error_type}: {error_msg}", {
                            "error": error_msg,
                            "error_type": error_type,
                            "connection_status": connection_status,
                            "type": "read_error"
                        })
                    
                    # If connection is closed, try to reconnect
                    if connection_status == "closed":
                        if self.log_callback:
                            self.log_callback("connection", "Connection lost, attempting to reconnect...", {})
                        try:
                            if self.connect():
                                if self.log_callback:
                                    self.log_callback("connection", "Reconnected successfully", {})
                            else:
                                if self.log_callback:
                                    self.log_callback("error", "Reconnection failed", {})
                        except Exception as reconnect_e:
                            if self.log_callback:
                                self.log_callback("error", f"Reconnection error: {reconnect_e}", {})
                    
                    time.sleep(interval)
                    continue
                
                time.sleep(interval)
                
            except Exception as e:
                msg = f"Polling error: {e}"
                print(f"[ADS Interface] {msg}")
                if self.log_callback:
                    self.log_callback("error", msg, {"error": str(e), "type": "polling_error"})
                time.sleep(interval)
    
    def write_measurement_result(self, measurement_data: dict):
        """
        Write measurement result to PLC variables.
        Called automatically when a measurement completes.
        Implements handshake: sets busy=False, ready=True to signal completion.
        
        Args:
            measurement_data: Dictionary containing measurement data
        """
        if not self.plc or not self.plc.is_open:
            self.measurement_in_progress = False
            return
        
        try:
            # IMPORTANT: Read current trigger state BEFORE writing handshake signals
            # This captures the trigger state while measurement is still "in progress" from PLC's perspective
            # This prevents race condition where PLC resets trigger between our writes and our read
            try:
                current_trigger = self.plc.read_by_name(self.var_trigger, pyads.PLCTYPE_BOOL)
                # Sync trigger state immediately - this prevents false trigger detection
                self.last_trigger_state = current_trigger
                if self.log_callback:
                    self.log_callback("handshake", f"Trigger state synced before handshake: {current_trigger}", 
                                    {"trigger_state": current_trigger, "synced": True})
            except Exception as e:
                # If we can't read trigger state, keep current state (don't reset)
                if self.log_callback:
                    self.log_callback("error", f"Could not read trigger state before handshake: {e}", 
                                    {"error": str(e), "type": "read_error"})
            
            # Write measurement data to PLC (with timeout to prevent blocking)
            thickness = measurement_data.get('thickness')
            if thickness is not None:
                if self._safe_write(self.var_thickness, float(thickness), pyads.PLCTYPE_REAL):
                    if self.log_callback:
                        self.log_callback("write", f"Write {self.var_thickness} = {thickness:.3f}", 
                                        {"variable": self.var_thickness, "value": thickness})
            
            peak1 = measurement_data.get('peak1')
            if peak1 is not None:
                if self._safe_write(self.var_peak1, float(peak1), pyads.PLCTYPE_REAL):
                    if self.log_callback:
                        self.log_callback("write", f"Write {self.var_peak1} = {peak1:.3f}", 
                                        {"variable": self.var_peak1, "value": peak1})
            
            peak2 = measurement_data.get('peak2')
            if peak2 is not None:
                if self._safe_write(self.var_peak2, float(peak2), pyads.PLCTYPE_REAL):
                    if self.log_callback:
                        self.log_callback("write", f"Write {self.var_peak2} = {peak2:.3f}", 
                                        {"variable": self.var_peak2, "value": peak2})
            
            # Write measurement count
            count = measurement_data.get('measurement_count', 0)
            if self._safe_write(self.var_count, int(count), pyads.PLCTYPE_UDINT):
                if self.log_callback:
                    self.log_callback("write", f"Write {self.var_count} = {count}", 
                                    {"variable": self.var_count, "value": count})
            
            # Handshake: Clear busy flag and set ready flag
            # This signals to PLC that measurement is complete
            if self._safe_write(self.var_busy, False, pyads.PLCTYPE_BOOL):
                if self._safe_write(self.var_ready, True, pyads.PLCTYPE_BOOL):
                    if self.log_callback:
                        self.log_callback("handshake", "Measurement complete - Set busy=FALSE, ready=TRUE", 
                                        {"busy": False, "ready": True})
            
            # Clear any previous error (silently fail if variable doesn't exist)
            self._safe_write(self.var_error, '', pyads.PLCTYPE_STRING)
            
            # Reset measurement in progress flag AFTER syncing trigger state
            # This ensures trigger state is captured before we allow new triggers
            self.measurement_in_progress = False
            
            if self.log_callback:
                self.log_callback("handshake", "Measurement complete - Ready for next trigger", 
                                {"measurement_in_progress": False})
            
        except Exception as e:
            msg = f"Error writing measurement result to PLC: {e}"
            print(f"[ADS Interface] {msg}")
            if self.log_callback:
                self.log_callback("error", msg, {"error": str(e), "type": "write_result_error"})
            # Reset busy flag even on error
            self._safe_write(self.var_busy, False, pyads.PLCTYPE_BOOL)
            self.measurement_in_progress = False
            # Try to sync trigger state even on error
            try:
                if self.plc and self.plc.is_open:
                    current_trigger = self.plc.read_by_name(self.var_trigger, pyads.PLCTYPE_BOOL)
                    self.last_trigger_state = current_trigger
            except Exception:
                # If we can't read, reset to False (safe default)
                self.last_trigger_state = False
    
    def is_running(self) -> bool:
        """Check if polling is running."""
        return self.running
    
    def is_connected(self) -> bool:
        """Check if connected to PLC."""
        return self.plc is not None and self.plc.is_open


class BeckhoffADSNotificationInterface:
    """
    Advanced ADS interface using notifications for efficient real-time updates.
    This is more efficient than polling as it uses ADS notifications (callbacks).
    """
    
    def __init__(self, ams_netid: str = '127.0.0.1.1.1',
                 port: int = None,
                 callback: Optional[Callable] = None,
                 symbol_prefix: str = 'GVL_CHRocodile.'):
        """
        Initialize the ADS notification interface.
        
        Args:
            ams_netid: AMS NetID of the PLC
            port: ADS port
            callback: Callback function for measurement triggers
            symbol_prefix: Prefix for PLC variable names
        """
        if not PYADS_AVAILABLE:
            raise ImportError("pyads library is required. Install with: pip install pyads")
        
        self.ams_netid = ams_netid
        self.port = port if port is not None else pyads.PORT_TC3PLC1
        self.callback = callback
        self.symbol_prefix = symbol_prefix
        
        self.plc = None
        self.running = False
        self.notification_handles = []
        
        # Variable names
        self.var_trigger = f'{symbol_prefix}bTriggerMeasurement'
        self.var_thickness = f'{symbol_prefix}rThickness'
        
    def connect(self) -> bool:
        """Connect to PLC."""
        try:
            self.plc = pyads.Connection(self.ams_netid, self.port)
            self.plc.open()
            return True
        except Exception as e:
            print(f"[ADS Notification] Connection failed: {e}")
            return False
    
    def setup_notifications(self):
        """Setup ADS notifications for trigger variable."""
        if not self.plc or not self.plc.is_open:
            return False
        
        try:
            # Define callback for trigger variable
            def trigger_callback(notification, data):
                if data:
                    if self.callback:
                        self.callback("trigger_measurement")
            
            # Add notification for trigger variable
            handle = self.plc.add_device_notification(
                self.var_trigger,
                pyads.NotificationAttrib(1),  # 1 byte for BOOL
                trigger_callback
            )
            self.notification_handles.append(handle)
            return True
        except Exception as e:
            print(f"[ADS Notification] Failed to setup notifications: {e}")
            return False
    
    def write_measurement_result(self, measurement_data: dict):
        """Write measurement result to PLC."""
        if not self.plc or not self.plc.is_open:
            return
        
        try:
            thickness = measurement_data.get('thickness')
            if thickness is not None:
                self.plc.write_by_name(self.var_thickness, float(thickness), pyads.PLCTYPE_REAL)
        except Exception as e:
            print(f"[ADS Notification] Error writing to PLC: {e}")
    
    def disconnect(self):
        """Disconnect and cleanup."""
        if self.plc:
            # Remove notifications
            for handle in self.notification_handles:
                try:
                    self.plc.del_device_notification(handle)
                except:
                    pass
            self.notification_handles.clear()
            
            # Close connection
            try:
                self.plc.close()
            except:
                pass
            self.plc = None
