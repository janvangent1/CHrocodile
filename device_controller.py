# -*- coding: utf-8 -*-
"""
Device controller for CHRocodile 2 LR.
Handles connection, measurement, and data acquisition from the device.
"""

import sys
import os
import io
import contextlib
import threading
import time
import queue
import numpy as np
from typing import Optional, Callable, Tuple
from enum import Enum

# Add path to chrpy library (similar to PyDemo/context.py)
# Support for PyInstaller frozen executable
if getattr(sys, 'frozen', False):
    # Running as compiled executable
    base_path = os.path.dirname(sys.executable)
    chrpy_dir = os.path.join(base_path, 'chrocodilelib', 'libcore')
    if not os.path.exists(chrpy_dir):
        # Try in _MEIPASS (PyInstaller temp directory)
        chrpy_dir = os.path.join(sys._MEIPASS, 'chrocodilelib', 'libcore')
else:
    # Running as script
    chrpy_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), 'chrocodilelib', 'libcore'))

if os.path.exists(chrpy_dir):
    sys.path.insert(0, chrpy_dir)

try:
    from chrpy.chr_connection import connection_from_params, DeviceType, OperationMode, APIException
    from chrpy.chr_cmd_id import CmdId, SpectrumType
    from chrpy.chr_utils import Data, Response
    CHR_LIBRARY_AVAILABLE = True
except ImportError as e:
    print(f"Warning: CHRocodile library not available: {e}")
    CHR_LIBRARY_AVAILABLE = False


class ConnectionState(Enum):
    """Connection state enumeration"""
    DISCONNECTED = "disconnected"
    CONNECTING = "connecting"
    CONNECTED = "connected"
    ERROR = "error"


class CHRocodileController:
    """
    Controller for CHRocodile 2 LR device.
    Handles connection, measurement setup, and data acquisition.
    """
    
    # Signal IDs for thickness measurement
    SIGNAL_SAMPLE_COUNTER = 83  # Sample counter (global signal, optional)
    SIGNAL_THICKNESS = 256      # Thickness 1 in float format (interferometric mode)
    SIGNAL_MEDIAN1 = 260        # Median 1 measurement value (configured in device)
    SIGNAL_INTENSITY_PERCENT = 76  # Not used for intensity in current CHR2 setup
    SIGNAL_INTENSITY = 257      # Often intensity in confocal/single-peak mode; may differ by mode
    SIGNAL_PEAK1_VALUE = 16640  # Peak 1 value (commonly used in confocal examples)
    SIGNAL_PEAK1_QI = 16641     # Peak 1 quality/intensity (device/mode dependent)
    SIGNAL_ALT_QUALITY = 16648  # Additional quality/intensity-like signal on some setups
    # Note: For interferometric mode, signal 256 already includes refractive index correction
    # Peak signals 16640/16641 are for confocal mode, not needed for interferometric thickness
    
    def __init__(self, data_callback: Optional[Callable] = None):
        """
        Initialize the controller.
        
        Args:
            data_callback: Optional callback function for received data
        """
        self.connection = None
        self.state = ConnectionState.DISCONNECTED
        self.ip_address = None
        self.data_callback = data_callback
        self.measurement_thread = None
        self.continuous_measurement_active = False
        self.measurement_interval_ms = 100
        self.stop_event = threading.Event()
        self.data_queue = queue.Queue()
        self.error_message = None
        self.refractive_index = 1.5  # Default refractive index
        self.measuring_rate_hz = 1000  # Default measuring rate
        self.data_average = 1  # Default data averaging
        self.spectrum_average = 1  # Default spectrum averaging
        self.measuring_mode = 1  # 0=confocal, 1=interferometric
        self.lamp_intensity = 50  # Default lamp intensity (0-100)
        self.lamp_control_supported = None  # Unknown until confirmed by query/set
        self.include_spectrum_in_continuous = False  # Whether to download spectrum in continuous mode
        self._measurement_lock = threading.Lock()  # Serializes get_next_samples calls across threads
        self._stream_started = False  # True once start_data_stream() succeeds

    def _format_signal_value_for_debug(self, value):
        """Convert a signal value to a debug-friendly Python type."""
        if value is None:
            return None

        if isinstance(value, np.ndarray):
            if value.size == 0:
                return []
            if value.size == 1:
                scalar = float(value.item())
                return None if np.isnan(scalar) else scalar
            # Prevent huge terminal spam for multi-channel arrays.
            preview_len = min(12, int(value.size))
            preview = value[:preview_len].tolist()
            return {
                "len": int(value.size),
                "preview": preview,
                "truncated": bool(value.size > preview_len),
            }

        if isinstance(value, (float, int, np.floating, np.integer)):
            scalar = float(value)
            return None if np.isnan(scalar) else scalar

        return str(value)

    def _collect_signal_snapshot(self, data) -> dict:
        """
        Collect all available streamed signal IDs and current values for debugging.
        """
        snapshot = {
            "signal_ids": [],
            "signals": {},
        }
        try:
            if data.gen_signal_info is not None:
                snapshot["meta"] = {
                    "channel_cnt": int(getattr(data.gen_signal_info, "channel_cnt", 0)),
                    "global_sig_cnt": int(getattr(data.gen_signal_info, "global_sig_cnt", 0)),
                    "peak_sig_cnt": int(getattr(data.gen_signal_info, "peak_sig_cnt", 0)),
                }
        except Exception:
            pass

        seen = set()
        for sig in (data.signal_info or []):
            try:
                sig_id = int(sig[1])
            except Exception:
                continue
            if sig_id in seen:
                continue
            seen.add(sig_id)
            snapshot["signal_ids"].append(sig_id)
            try:
                value = data.get_signal_values(sig_id, 0)
                snapshot["signals"][str(sig_id)] = self._format_signal_value_for_debug(value)
            except Exception as e:
                snapshot["signals"][str(sig_id)] = {"error": str(e)}

        return snapshot
        
    def connect(self, ip_address: str) -> Tuple[bool, str]:
        """
        Connect to the CHRocodile device.
        
        Args:
            ip_address: IP address of the device (e.g., '192.168.170.2')
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not CHR_LIBRARY_AVAILABLE:
            return False, "CHRocodile library not available"
        
        if self.state == ConnectionState.CONNECTED:
            return False, "Already connected"
        
        self.state = ConnectionState.CONNECTING
        self.ip_address = ip_address
        self.error_message = None
        
        try:
            # Create connection (synchronous mode for simplicity)
            self.connection = connection_from_params(
                addr=ip_address,
                device_type=DeviceType.CHR_2,
                conn_mode=OperationMode.SYNC
            )
            
            # Open the connection
            self.connection.open()

            # Stop any stream left active from a previous session.
            # If the device is still streaming, the first command will fail with
            # "Device data format packet is missing" (-536450304).
            try:
                self.connection.stop_data_stream()
            except Exception:
                pass
            # Brief pause so the device processes the stop before we send setup commands.
            time.sleep(0.1)

            # Setup measurement signals
            self._setup_measurement()

            # Pre-start the stream so it is ready before the first measurement.
            # Doing this once here avoids racing start_data_stream() calls from
            # multiple threads later.
            try:
                self.connection.start_data_stream()
                self._stream_started = True
            except Exception:
                self._stream_started = False

            self.state = ConnectionState.CONNECTED
            return True, "Connected successfully"
            
        except Exception as e:
            self.state = ConnectionState.ERROR
            self.error_message = str(e)
            self.connection = None
            return False, f"Connection failed: {str(e)}"
    
    def disconnect(self) -> Tuple[bool, str]:
        """
        Disconnect from the device.
        
        Returns:
            Tuple of (success: bool, message: str)
        """
        if self.state == ConnectionState.DISCONNECTED:
            return True, "Already disconnected"
        
        # Stop continuous measurement if active
        self.stop_continuous_measurement()
        
        try:
            if self.connection:
                try:
                    self.connection.stop_data_stream()
                except:
                    pass
                self._stream_started = False

                # Close connection
                self.connection.close()
                self.connection = None

            self.state = ConnectionState.DISCONNECTED
            return True, "Disconnected successfully"
            
        except Exception as e:
            self.state = ConnectionState.ERROR
            self.error_message = str(e)
            return False, f"Disconnect error: {str(e)}"
    
    def _setup_measurement(self):
        """Configure the device for thickness measurement."""
        if not self.connection:
            return
        
        def _exec_checked(cmd: str, *args):
            """Execute device command and raise a clear error on failure."""
            try:
                resp = self.connection.exec(cmd, *args)
            except Exception as e:
                raise Exception(f"{cmd}{args}: transport/exec error: {e}")
            if resp.error_code != 0:
                raise Exception(f"{cmd}{args}: device error_code={resp.error_code}")
            return resp

        try:
            # Set measuring mode FIRST (interferometric for thickness)
            # This must be set before configuring signals
            _exec_checked('MMD', self.measuring_mode)
            
            # Set number of peaks to 2 (for film thickness measurement)
            _exec_checked('NOP', 2)
            
            # Set output signals with compatibility fallbacks.
            # Signal 256 = Thickness 1 in float format (already includes refractive index correction)
            # 16641/16648 may provide quality/intensity-like values on some setups.
            # We keep 257 as additional candidate for intensity.
            try:
                _exec_checked(
                    'SODX',
                    self.SIGNAL_SAMPLE_COUNTER,
                    self.SIGNAL_THICKNESS,
                    self.SIGNAL_MEDIAN1,
                    self.SIGNAL_PEAK1_QI,
                    self.SIGNAL_ALT_QUALITY,
                    self.SIGNAL_INTENSITY
                )
            except Exception:
                try:
                    _exec_checked(
                        'SODX',
                        self.SIGNAL_SAMPLE_COUNTER,
                        self.SIGNAL_THICKNESS,
                        self.SIGNAL_MEDIAN1,
                        self.SIGNAL_PEAK1_QI,
                        self.SIGNAL_ALT_QUALITY,
                        self.SIGNAL_INTENSITY
                    )
                except Exception:
                    try:
                        _exec_checked(
                            'SODX',
                            self.SIGNAL_SAMPLE_COUNTER,
                            self.SIGNAL_PEAK1_VALUE,
                            self.SIGNAL_PEAK1_QI,
                            self.SIGNAL_MEDIAN1
                        )
                    except Exception:
                        try:
                            _exec_checked('SODX', self.SIGNAL_THICKNESS, self.SIGNAL_MEDIAN1, self.SIGNAL_INTENSITY)
                        except Exception:
                            _exec_checked('SODX', self.SIGNAL_THICKNESS)
            # Do not force-write SHZ/AVD/AVS/LIA/SRI on connect.
            # Users requested to keep device settings as-is and read them first.
            
        except Exception as e:
            raise Exception(f"Failed to setup measurement: {str(e)}")
    
    def set_refractive_index(self, n: float) -> Tuple[bool, str]:
        """
        Set the refractive index for thickness calculation.
        
        Args:
            n: Refractive index value (typically 1.3-2.0 for common materials)
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            # Store for later when connected
            self.refractive_index = n
            return True, "Refractive index will be set on connection"
        
        try:
            # SRI command sets refractive indices (for each layer/peak)
            # For interferometric film thickness, we set the same value for both interfaces
            # Format: SRI <n1> <n2> ... (one value per layer, which is NOP - 1)
            # In interferometric mode, only one refractive index value is applied
            # Using string command format (SRI, not RIRS)
            resp = self.connection.exec_from_string(f'SRI {n} {n}')
            if resp.error_code != 0:
                # Try with CmdId enum
                resp = self.connection.exec(CmdId.REFRACTIVE_INDICES, n, n)
                if resp.error_code != 0:
                    return False, f"Failed to set refractive index: {resp.error_code}"
            
            self.refractive_index = n
            return True, f"Refractive index set to {n}"
            
        except Exception as e:
            return False, f"Error setting refractive index: {str(e)}"
    
    def get_refractive_index(self) -> float:
        """Get current refractive index setting."""
        return self.refractive_index
    
    def start_data_stream(self) -> Tuple[bool, str]:
        """
        Start the data stream from the device.
        
        Returns:
            Tuple of (success: bool, message: str)
        """
        if self.state != ConnectionState.CONNECTED:
            return False, "Not connected"
        
        try:
            self.connection.start_data_stream()
            return True, "Data stream started"
        except Exception as e:
            return False, f"Failed to start data stream: {str(e)}"
    
    def stop_data_stream(self) -> Tuple[bool, str]:
        """
        Stop the data stream.
        
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection:
            return True, "Not connected"
        
        try:
            self.connection.stop_data_stream()
            return True, "Data stream stopped"
        except Exception as e:
            return False, f"Failed to stop data stream: {str(e)}"
    
    def get_single_measurement(self, include_spectrum: bool = False) -> Optional[dict]:
        """
        Get a single thickness measurement.
        
        Args:
            include_spectrum: If True, also download and include spectrum data
            
        Returns:
            Dictionary with measurement data or None if error:
            {
                'thickness': float,  # Thickness in micrometers (already corrected for refractive index)
                'median1': float,    # Median 1 value in micrometers (if configured)
                'intensity': float,  # Measurement intensity (raw/device units)
                'quality': float,    # Measurement quality (raw/device units)
                'peak1': float,      # Peak 1 position (from spectrum, if available)
                'peak2': float,      # Peak 2 position (from spectrum, if available)
                'spectrum': np.ndarray (if include_spectrum=True),
                'timestamp': float,
                'error': str (if error occurred)
            }
            
        Note:
            - Thickness value from signal 256 is already in micrometers and includes
              refractive index correction (geometrical thickness)
            - No manual calculation needed for float format signals
            - Peak positions are extracted from spectrum data when available
        """
        if self.state != ConnectionState.CONNECTED:
            return {'error': 'Not connected'}

        # Prevent concurrent get_next_samples calls across threads (continuous loop +
        # PLC-triggered single shot).  Non-blocking: if another measurement is already
        # in flight, skip this one rather than queuing behind it.
        if not self._measurement_lock.acquire(blocking=False):
            return {'error': 'Measurement already in progress'}

        try:
            # Ensure stream is running (pre-started in connect(); only fall back to
            # starting it here if that failed or it has since been stopped).
            if not self._stream_started:
                self.start_data_stream()
                self._stream_started = True

            # With higher averaging (AVD/AVS), the first valid sample can take longer.
            # Retry for a bounded time instead of failing immediately.
            rate_hz = max(1, int(self.measuring_rate_hz))
            avg_factor = max(1, int(self.data_average))
            # Estimated sample period scales with averaging; add safety margin.
            estimated_period_s = avg_factor / rate_hz
            timeout_s = min(5.0, max(0.5, estimated_period_s * 8.0))

            data = None
            start_wait = time.time()
            while (time.time() - start_wait) < timeout_s:
                # Non-blocking fetch from current stream buffer.
                data = self.connection.get_next_samples(1, False)
                if data is not None and data.sample_cnt > 0:
                    break
                time.sleep(0.01)

            if data is None or data.sample_cnt == 0:
                return {'error': f'No data received (timeout {timeout_s:.2f}s)'}
            
            if data.error_code < 0:
                return {'error': f'Device error: {data.error_code}'}
            
            def _extract_first_signal(signal_id: int) -> Optional[float]:
                """
                Robustly read a signal value from current sample.
                Handles scalar and array-like return variants.
                """
                try:
                    values = data.get_signal_values(signal_id, 0)
                except Exception:
                    return None

                if values is None:
                    return None
                if isinstance(values, (float, int, np.floating)):
                    value = float(values)
                    if np.isnan(value):
                        return None
                    return value
                try:
                    if len(values) == 0:
                        return None
                    value = float(values[0])
                    if np.isnan(value):
                        return None
                    return value
                except Exception:
                    return None

            # Extract thickness (signal 256)
            # Signal 256 in float format already includes refractive index correction
            # Value is already geometrical thickness in micrometers - NO manual correction needed!
            thickness = _extract_first_signal(self.SIGNAL_THICKNESS)
            median1 = _extract_first_signal(self.SIGNAL_MEDIAN1)
            available_ids = set()
            try:
                if data.signal_info:
                    available_ids = {int(sig[1]) for sig in data.signal_info if len(sig) > 1}
            except Exception:
                available_ids = set()

            def _extract_from_candidates(candidates, excluded_ids=None):
                excluded_ids = excluded_ids or set()
                for sig_id in candidates:
                    if sig_id in excluded_ids:
                        continue
                    if sig_id in available_ids:
                        value = _extract_first_signal(sig_id)
                        if value is not None:
                            return value, sig_id
                return None, None

            def _normalize_intensity(raw_value: Optional[float], sig_id: Optional[int]) -> Optional[float]:
                """
                Normalize intensity to a human-friendly percent-like value.
                CHR signals such as 16648/16641 are often fixed-point with scale 1/16.
                """
                if raw_value is None or sig_id is None:
                    return raw_value
                if sig_id in (self.SIGNAL_ALT_QUALITY, self.SIGNAL_PEAK1_QI):
                    # Apply fixed-point conversion when clearly out of percent range.
                    if raw_value > 100.0:
                        return raw_value / 16.0
                return raw_value

            # Keep intensity and quality strictly separate to avoid duplicated values.
            # Observed CHR2 behavior: quality aligns with 257/16641 raw values,
            # while 16648 behaves like intensity-like fixed-point value.
            intensity, intensity_sig = _extract_from_candidates(
                [self.SIGNAL_ALT_QUALITY, self.SIGNAL_PEAK1_QI]
            )
            intensity = _normalize_intensity(intensity, intensity_sig)
            quality, _quality_sig = _extract_from_candidates(
                [self.SIGNAL_INTENSITY, self.SIGNAL_PEAK1_QI, 258],
                excluded_ids={intensity_sig} if intensity_sig is not None else set()
            )
            
            # For interferometric mode, peak signals are not the same as confocal mode
            # We can try to get peak positions from spectrum if needed, but for now
            # we'll just return None for peaks (they're not directly available as signals)
            # Peak positions would need to be extracted from the spectrum data
            peak1 = None
            peak2 = None
            
            result = {
                'thickness': thickness,
                'median1': median1,
                'intensity': intensity,
                'quality': quality,
                'peak1': peak1,
                'peak2': peak2,
                'intensity_signal_id': intensity_sig,
                'quality_signal_id': _quality_sig,
                'signal_snapshot': self._collect_signal_snapshot(data),
                'timestamp': time.time()
            }
            
            # Download spectrum if requested
            if include_spectrum:
                spectrum_data = self.download_spectrum()
                if spectrum_data and 'error' not in spectrum_data:
                    result['spectrum'] = spectrum_data.get('spectrum')
                    # Peak positions can be extracted from spectrum in the GUI
                    # using the _detect_peaks method
            
            return result

        except Exception as e:
            err_str = str(e)
            # -536451072: chrpy internal thread error — stream is dead, needs restart.
            if '-536451072' in err_str:
                self._recover_data_stream()
            return {'error': f'Measurement error: {err_str}'}

        finally:
            self._measurement_lock.release()

    def _recover_data_stream(self):
        """Stop and restart the data stream after an internal thread error."""
        self._stream_started = False
        try:
            self.connection.stop_data_stream()
        except Exception:
            pass
        time.sleep(0.1)
        try:
            self.connection.start_data_stream()
            self._stream_started = True
        except Exception:
            pass

    def start_continuous_measurement(self, interval_ms: int = 100, include_spectrum: bool = False):
        """
        Start continuous measurements at specified interval.
        
        Args:
            interval_ms: Measurement interval in milliseconds
            include_spectrum: If True, download spectrum for each measurement
        """
        if self.continuous_measurement_active:
            return
        
        self.measurement_interval_ms = interval_ms
        self.continuous_measurement_active = True
        self.stop_event.clear()
        self.include_spectrum_in_continuous = include_spectrum
        
        # Start data stream (only once, keep it running)
        success, msg = self.start_data_stream()
        if not success:
            self.continuous_measurement_active = False
            return
        
        # Start measurement thread
        self.measurement_thread = threading.Thread(
            target=self._continuous_measurement_loop,
            daemon=True
        )
        self.measurement_thread.start()
    
    def stop_continuous_measurement(self):
        """Stop continuous measurements."""
        self.continuous_measurement_active = False
        self.stop_event.set()
        
        if self.measurement_thread:
            self.measurement_thread.join(timeout=2.0)
            self.measurement_thread = None
        
        self.stop_data_stream()
    
    def _continuous_measurement_loop(self):
        """Internal loop for continuous measurements."""
        consecutive_errors = 0
        max_consecutive_errors = 5
        
        while self.continuous_measurement_active and not self.stop_event.is_set():
            try:
                # Get measurement (with spectrum if requested)
                measurement = self.get_single_measurement(include_spectrum=self.include_spectrum_in_continuous)
                
                if measurement and 'error' not in measurement:
                    # Reset error counter on success
                    consecutive_errors = 0
                    
                    # Put measurement in queue for GUI thread
                    self.data_queue.put(measurement)
                    
                    # Call callback if provided
                    if self.data_callback:
                        try:
                            self.data_callback(measurement)
                        except Exception as e:
                            print(f"Error in data callback: {e}")
                else:
                    # Handle error
                    consecutive_errors += 1
                    if measurement:
                        error_msg = measurement.get('error', 'Unknown error')
                        print(f"Measurement error: {error_msg}")
                    
                    # If too many consecutive errors, stop continuous measurement
                    if consecutive_errors >= max_consecutive_errors:
                        print(f"Too many consecutive errors ({consecutive_errors}), stopping continuous measurement")
                        self.continuous_measurement_active = False
                        break
                
            except Exception as e:
                print(f"Exception in continuous measurement loop: {e}")
                consecutive_errors += 1
                if consecutive_errors >= max_consecutive_errors:
                    self.continuous_measurement_active = False
                    break
            
            # Wait for next measurement interval
            self.stop_event.wait(self.measurement_interval_ms / 1000.0)
    
    def download_spectrum(self) -> Optional[dict]:
        """
        Download raw interferometric spectrum from the device.
        
        Returns:
            Dictionary with spectrum data or None if error:
            {
                'spectrum': np.ndarray,
                'timestamp': float,
                'error': str (if error occurred)
            }
        """
        if self.state != ConnectionState.CONNECTED:
            return {'error': 'Not connected'}
        
        try:
            # Download spectrum (raw interferometric data)
            # Note: SpectrumType.CONFOCAL refers to the raw detector data format,
            # not the measurement mode. This works for both confocal and interferometric modes.
            resp = self.connection.download_spectrum(SpectrumType.CONFOCAL, 0)
            
            if resp.error_code != 0:
                return {'error': f'Spectrum download failed: {resp.error_code}'}
            
            # Extract spectrum data from response
            # The spectrum is typically in the last parameter as byte array
            if resp.param_count > 0:
                spectrum_bytes = resp.args[resp.param_count - 1]
                # Convert to numpy array (typically uint16 for CCD data)
                spectrum = np.frombuffer(spectrum_bytes, dtype=np.uint16)
                
                return {
                    'spectrum': spectrum,
                    'timestamp': time.time()
                }
            else:
                return {'error': 'No spectrum data in response'}
                
        except Exception as e:
            return {'error': f'Spectrum download error: {str(e)}'}
    
    def get_state(self) -> ConnectionState:
        """Get current connection state."""
        return self.state
    
    def is_connected(self) -> bool:
        """Check if device is connected."""
        return self.state == ConnectionState.CONNECTED
    
    def perform_dark_reference(self) -> Tuple[bool, str]:
        """
        Perform a dark reference measurement.
        This is essential for accurate measurements and should be done:
        - After device warm-up
        - After environmental changes
        - Periodically during long measurements
        
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            return False, "Not connected"
        
        try:
            resp = self.connection.dark_reference()
            if resp.error_code != 0:
                return False, f"Dark reference failed: {resp.error_code}"
            
            # Dark reference returns a frequency value (stray light saturation frequency)
            if resp.args and len(resp.args) > 0:
                freq = resp.args[0]
                return True, f"Dark reference completed (saturation freq: {freq:.1f} Hz)"
            else:
                return True, "Dark reference completed"
                
        except Exception as e:
            return False, f"Error performing dark reference: {str(e)}"
    
    def set_measuring_rate(self, rate_hz: int) -> Tuple[bool, str]:
        """
        Set the measuring rate (sample frequency).
        
        Args:
            rate_hz: Measuring rate in Hz (typically 1-70000)
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            self.measuring_rate_hz = rate_hz
            return True, "Measuring rate will be set on connection"
        
        try:
            resp = self.connection.exec('SHZ', rate_hz)
            if resp.error_code != 0:
                return False, f"Failed to set measuring rate: {resp.error_code}"
            
            self.measuring_rate_hz = rate_hz
            return True, f"Measuring rate set to {rate_hz} Hz"
            
        except Exception as e:
            return False, f"Error setting measuring rate: {str(e)}"
    
    def set_averaging(self, data_avg: int, spectrum_avg: int) -> Tuple[bool, str]:
        """
        Set averaging parameters.
        
        Args:
            data_avg: Data averaging (number of distance results averaged, 1-1000)
            spectrum_avg: Spectrum averaging (number of spectral exposures averaged, 1-1000)
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            self.data_average = data_avg
            self.spectrum_average = spectrum_avg
            return True, "Averaging will be set on connection"
        
        try:
            resp = self.connection.exec('AVD', data_avg)
            if resp.error_code != 0:
                return False, f"Failed to set data averaging: {resp.error_code}"
            
            resp = self.connection.exec('AVS', spectrum_avg)
            if resp.error_code != 0:
                return False, f"Failed to set spectrum averaging: {resp.error_code}"
            
            self.data_average = data_avg
            self.spectrum_average = spectrum_avg
            return True, f"Averaging set: Data={data_avg}, Spectrum={spectrum_avg}"
            
        except Exception as e:
            return False, f"Error setting averaging: {str(e)}"
    
    def set_measuring_mode(self, mode: int) -> Tuple[bool, str]:
        """
        Set the measuring mode.
        
        Args:
            mode: 0 = Chromatic Confocal, 1 = Interferometric
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            self.measuring_mode = mode
            return True, "Measuring mode will be set on connection"
        
        try:
            resp = self.connection.exec('MMD', mode)
            if resp.error_code != 0:
                return False, f"Failed to set measuring mode: {resp.error_code}"
            
            self.measuring_mode = mode
            mode_name = "Chromatic Confocal" if mode == 0 else "Interferometric"
            return True, f"Measuring mode set to {mode_name}"
            
        except Exception as e:
            return False, f"Error setting measuring mode: {str(e)}"
    
    def set_lamp_intensity(self, intensity: int) -> Tuple[bool, str]:
        """
        Set the lamp intensity.
        
        Args:
            intensity: Lamp intensity (0-100, typically 0-100%)
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            self.lamp_intensity = intensity
            return True, "Lamp intensity will be set on connection"

        # If firmware was detected as not supporting lamp control, skip silently.
        if self.lamp_control_supported is False:
            self.lamp_intensity = intensity
            return True, "Lamp control not supported by this device firmware (skipped)"
        
        try:
            resp = self.connection.exec('LIA', intensity)
            if resp.error_code != 0:
                # Some CHRocodile firmware variants reject LIA.
                # Treat this as non-fatal to keep settings workflow usable.
                self.lamp_control_supported = False
                self.lamp_intensity = intensity
                return True, (
                    f"Warning: Lamp intensity command not accepted by device "
                    f"(error code {resp.error_code})."
                )
            
            self.lamp_control_supported = True
            self.lamp_intensity = intensity
            return True, f"Lamp intensity set to {intensity}%"
            
        except Exception as e:
            err = str(e)
            # Known non-fatal behavior on some devices: command response error for LIA.
            if "-536250368" in err or "Error in command response" in err:
                self.lamp_control_supported = False
                self.lamp_intensity = intensity
                return True, (
                    "Lamp intensity command not accepted by device firmware; "
                    "skipping lamp update."
                )
            return False, f"Error setting lamp intensity: {err}"
    
    def get_configuration(self) -> Optional[dict]:
        """
        Get current device configuration.
        
        Returns:
            Dictionary with configuration parameters or None if error
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            return None
        
        try:
            # Suppress noisy stdout prints from wrapper internals (shared connection info).
            with contextlib.redirect_stdout(io.StringIO()):
                responses = self.connection.get_conf()
            config = {}
            for resp in responses:
                if resp.cmd_id == CmdId.SODX:
                    config['output_signals'] = resp.args
                elif resp.cmd_id == CmdId.SCAN_RATE:
                    config['measuring_rate'] = resp.args[0] if resp.args else None
                elif resp.cmd_id == CmdId.DATA_AVERAGE:
                    config['data_average'] = resp.args[0] if resp.args else None
                elif resp.cmd_id == CmdId.SPECTRUM_AVERAGE:
                    config['spectrum_average'] = resp.args[0] if resp.args else None
                elif resp.cmd_id == CmdId.MEASURING_METHOD:
                    config['measuring_mode'] = resp.args[0] if resp.args else None
                elif resp.cmd_id == CmdId.LAMP_INTENSITY:
                    config['lamp_intensity'] = resp.args[0] if resp.args else None
                elif resp.cmd_id == CmdId.REFRACTIVE_INDICES:
                    config['refractive_indices'] = resp.args
            
            return config
            
        except Exception as e:
            return {'error': f"Failed to get configuration: {str(e)}"}

    def read_current_settings(self) -> Optional[dict]:
        """
        Read current settings from the connected device and normalize them.

        Returns:
            Dictionary with normalized setting values or error dict.
        """
        config = self.get_configuration()
        if config is None or 'error' in config:
            return config if isinstance(config, dict) else {'error': 'No configuration available'}

        def _first_scalar(value):
            """Return first scalar from nested list/tuple/ndarray values."""
            current = value
            while isinstance(current, (list, tuple, np.ndarray)) and len(current) > 0:
                current = current[0]
            return current

        settings = {
            'measuring_rate': config.get('measuring_rate'),
            'data_average': config.get('data_average'),
            'spectrum_average': config.get('spectrum_average'),
            'measuring_mode': config.get('measuring_mode'),
            # Prefer direct query for lamp intensity; get_conf can be inconsistent.
            'lamp_intensity': None,
            'refractive_index': None,
        }

        def _query_first_arg(*query_variants):
            """
            Try query variants and return the first response argument.
            Variants can be tuples like ('query', 'LIA') or ('exec_from_string', 'LIA ?').
            """
            for method_name, arg in query_variants:
                try:
                    if method_name == 'query':
                        resp = self.connection.query(arg)
                    else:
                        resp = self.connection.exec_from_string(arg)
                    if resp and getattr(resp, 'error_code', -1) == 0 and getattr(resp, 'args', None):
                        return _first_scalar(resp.args)
                except Exception:
                    continue
            return None

        # Refractive indices are returned as a list/tuple (often n1, n2).
        refractive_indices = config.get('refractive_indices')
        if refractive_indices is not None:
            settings['refractive_index'] = _first_scalar(refractive_indices)

        # Keep controller cache aligned with what the device reports.
        if settings['measuring_rate'] is not None:
            settings['measuring_rate'] = _first_scalar(settings['measuring_rate'])
            self.measuring_rate_hz = int(settings['measuring_rate'])
        if settings['data_average'] is not None:
            settings['data_average'] = _first_scalar(settings['data_average'])
            self.data_average = int(settings['data_average'])
        if settings['spectrum_average'] is not None:
            settings['spectrum_average'] = _first_scalar(settings['spectrum_average'])
            self.spectrum_average = int(settings['spectrum_average'])
        if settings['measuring_mode'] is not None:
            settings['measuring_mode'] = _first_scalar(settings['measuring_mode'])
            self.measuring_mode = int(settings['measuring_mode'])
        # Read lamp intensity via direct query only; treat missing value as unsupported.
        lamp_value = _query_first_arg(('query', 'LIA'), ('exec_from_string', 'LIA ?'))
        if lamp_value is not None:
            lamp_value = _first_scalar(lamp_value)
            try:
                lamp_value = int(lamp_value)
                if 0 <= lamp_value <= 100:
                    settings['lamp_intensity'] = lamp_value
                    self.lamp_intensity = lamp_value
                    self.lamp_control_supported = True
                else:
                    self.lamp_control_supported = False
            except Exception:
                self.lamp_control_supported = False
        else:
            self.lamp_control_supported = False
        if settings['refractive_index'] is not None:
            self.refractive_index = float(settings['refractive_index'])

        return settings
    
    def set_device_ip_address(self, ip_address: str, subnet_mask: str = "255.255.255.0", 
                              gateway: str = "192.168.170.1") -> Tuple[bool, str]:
        """
        Set the device IP address using the IPCN command.
        
        WARNING: This will change the device's network configuration. After changing the IP,
        you will need to reconnect using the new IP address. Ensure your PC's network
        adapter is configured to be on the same subnet.
        
        Args:
            ip_address: New IP address (e.g., '192.168.170.4')
            subnet_mask: Subnet mask (default: '255.255.255.0')
            gateway: Gateway address (default: '192.168.170.1')
            
        Returns:
            Tuple of (success: bool, message: str)
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            return False, "Not connected. Connect to device first to change IP address."
        
        # Validate IP address format
        try:
            parts = ip_address.split('.')
            if len(parts) != 4:
                return False, "Invalid IP address format. Use format: xxx.xxx.xxx.xxx"
            for part in parts:
                num = int(part)
                if num < 0 or num > 255:
                    return False, "Invalid IP address. Each octet must be 0-255"
        except ValueError:
            return False, "Invalid IP address format. Use format: xxx.xxx.xxx.xxx"
        
        try:
            # IPCN command format: $IPCN <IP> <SubnetMask> <Gateway>
            # Using string command format as IPCN may not be in CmdId enum
            cmd_str = f"$IPCN {ip_address} {subnet_mask} {gateway}"
            resp = self.connection.exec_from_string(cmd_str)
            
            if resp.error_code != 0:
                # Try alternative format without $ prefix
                cmd_str = f"IPCN {ip_address} {subnet_mask} {gateway}"
                resp = self.connection.exec_from_string(cmd_str)
                if resp.error_code != 0:
                    return False, f"Failed to set IP address. Error code: {resp.error_code}. " \
                                 f"Note: IPCN command may not be supported on this device model."
            
            # IP change takes effect after device restart or reconnection
            return True, f"IP address set to {ip_address}. " \
                        f"Device will use new IP after reconnection. " \
                        f"Update your connection settings and reconnect."
            
        except Exception as e:
            return False, f"Error setting IP address: {str(e)}"
    
    def get_device_ip_address(self) -> Optional[str]:
        """
        Query the current device IP address.
        
        Note: This may not be supported on all device models. The IP address
        is typically only available through network configuration queries.
        
        Returns:
            Current IP address as string, or None if not available
        """
        if not self.connection or self.state != ConnectionState.CONNECTED:
            return None
        
        try:
            # Try to query IP configuration - this may not be available
            # Some devices support querying network settings via CONF or specific commands
            # For now, return the IP we're connected to
            return self.ip_address
            
        except Exception as e:
            print(f"Error querying IP address: {e}")
            return None

