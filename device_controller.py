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
    
    # Interferometric CHRocodile 2 peak/global signals (float format, already in engineering units)
    SIGNAL_SAMPLE_COUNTER = 83   # Sample counter (global)
    SIGNAL_THICKNESS = 256       # Thickness 1 float (µm, geometrical)
    SIGNAL_QUALITY = 257         # Quality 1 float (FFT peak quality in interferometric mode)
    SIGNAL_MEDIAN1 = 260         # Median 1 float (µm; PeakValue + 4 per MED command)
    SIGNAL_INTENSITY = 82        # InterferomIntensity global (% of full well)
    # Legacy int16 peak signals (confocal / compatibility fallbacks only)
    SIGNAL_PEAK1_VALUE = 16640
    SIGNAL_PEAK1_QI = 16641
    SIGNAL_ALT_QUALITY = 16648
    
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
        self._active_output_signals = []  # Signal IDs confirmed by device after SODX
        self._last_sample_counter = None  # Detect stale / repeated buffer reads
        self._buffer_backlog_warn_ts = 0.0  # Rate-limit backlog console messages
        self._last_recover_ts = 0.0  # Rate-limit full connection reopens
        self._recover_min_interval_s = 3.0
        # When True, avoid STO/full reopen so CHR native software can stay connected.
        self.coexist_with_chr_software = True

    def _data_single_sample(self, data, sample_no: int = 0):
        """Return a Data object containing only one sample row."""
        if data is None or data.sample_cnt <= 1:
            return data
        idx = min(max(sample_no, 0), data.sample_cnt - 1)
        return Data(
            data.samples[idx:idx + 1],
            1,
            data.gen_signal_info,
            data.signal_info,
            data.error_code,
            data._dll_h,
        )

    def _maybe_log_buffer_backlog(self, drained: int, sample_counter: Optional[int]):
        """Log buffer backlog occasionally (normal when device rate exceeds poll rate)."""
        if drained <= 20:
            return
        now = time.time()
        if now - self._buffer_backlog_warn_ts < 60.0:
            return
        self._buffer_backlog_warn_ts = now
        print(
            f"[BUFFER] Discarded {drained} queued sample(s) to use latest "
            f"(counter={sample_counter}). Normal if device rate is higher than GUI poll rate."
        )

    def _read_sample_counter(self, data) -> Optional[int]:
        """Read global sample counter (signal 83) when present in the stream."""
        value = self._extract_first_signal(data, self.SIGNAL_SAMPLE_COUNTER)
        if value is None:
            return None
        try:
            return int(value)
        except Exception:
            return None

    def _drain_stream_to_latest(
        self,
        batch_size: int = 512,
        max_batches: int = 64,
    ) -> Tuple[Optional[object], int]:
        """
        Drain the chrpy receive FIFO and return the newest sample.

        Uses batch reads (one DLL call per chunk) instead of one call per sample.
        Caps are high enough to catch up after multi-second trigger gaps at kHz rates.
        """
        if not self.connection:
            return None, 0

        latest_data = None
        latest_idx = 0
        drained = 0

        for _ in range(max_batches):
            try:
                data = self.connection.get_next_samples(batch_size, False)
            except APIException as exc:
                err_str = str(exc)
                if '-536580864' in err_str or '-536580859' in err_str or '-536580860' in err_str:
                    print("[STREAM] Sample stream out of sync — flushing buffer")
                    self._flush_stream_buffer()
                    continue
                raise

            if data is None or data.sample_cnt == 0:
                break

            drained += data.sample_cnt
            latest_data = data
            latest_idx = data.sample_cnt - 1

            if data.sample_cnt < batch_size:
                break

        if latest_data is None:
            return None, 0

        return self._data_single_sample(latest_data, latest_idx), drained

    def _read_fresh_sample(self, timeout_s: float, force_flush: bool = False) -> Tuple[Optional[object], int]:
        """
        Return the newest live sample, discarding FIFO backlog.

        Important: never flush away a sample we already successfully drained.
        Flushing after a large drain was discarding good data and then timing out
        waiting for the next packet — which also stresses chrpy into
        Internal thread error (-536451072).
        """
        drained_total = 0
        latest = None

        if force_flush:
            self._flush_stream_buffer()
            time.sleep(0.02)

        deadline = time.time() + max(0.2, timeout_s)
        while time.time() < deadline:
            try:
                batch, drained = self._drain_stream_to_latest(
                    batch_size=256,
                    max_batches=16,
                )
            except Exception as exc:
                if self._is_data_format_missing_error(exc) or self._is_stream_dead_error(exc):
                    raise
                print(f"[BUFFER] Drain failed: {exc}")
                time.sleep(0.02)
                continue

            drained_total += drained
            if batch is not None and batch.sample_cnt > 0:
                latest = batch
                # Short drain means the FIFO is caught up — use this sample now.
                if drained < (256 * 16):
                    return latest, drained_total
                # Still a huge backlog: keep draining for newer samples.
                continue

            # Buffer empty this pass.
            if latest is not None:
                return latest, drained_total
            time.sleep(0.01)

        return latest, drained_total

    @staticmethod
    def _sample_counter_stale(previous: Optional[int], current: Optional[int]) -> bool:
        """
        True when the sample counter did not advance.

        Signal 83 is typically uint16 and wraps at 65536 — that wrap must NOT be
        treated as a stale/repeated buffer read.
        """
        if previous is None or current is None:
            return False
        if current > previous:
            return False
        # uint16 wrap (e.g. 65137 -> 1649)
        if previous > 50000 and current < 10000:
            return False
        return True

    def _query_output_signals(self) -> list:
        """Return the active SODX signal list reported by the device."""
        if not self.connection:
            return []
        try:
            resp = self.connection.query(CmdId.OUTPUT_SIGNALS)
            if resp and resp.error_code == 0 and resp.args:
                return [int(x) for x in resp.args]
        except Exception:
            pass
        try:
            resp = self.connection.exec_from_string('SODX ?')
            if resp and resp.error_code == 0 and resp.args:
                return [int(x) for x in resp.args]
        except Exception:
            pass
        return []

    def _preferred_sodx_candidates(self) -> list:
        """Output signal layouts to try, best first."""
        return [
            [
                self.SIGNAL_SAMPLE_COUNTER,
                self.SIGNAL_INTENSITY,
                self.SIGNAL_THICKNESS,
                self.SIGNAL_QUALITY,
                self.SIGNAL_MEDIAN1,
            ],
            [
                self.SIGNAL_SAMPLE_COUNTER,
                self.SIGNAL_THICKNESS,
                self.SIGNAL_QUALITY,
                self.SIGNAL_MEDIAN1,
            ],
            [
                self.SIGNAL_THICKNESS,
                self.SIGNAL_QUALITY,
                self.SIGNAL_MEDIAN1,
            ],
            [self.SIGNAL_THICKNESS, self.SIGNAL_QUALITY],
            [self.SIGNAL_THICKNESS],
        ]

    def _sodx_meets_minimum(self, signals: list) -> bool:
        """True when the device already streams enough signals for measurements."""
        if not signals:
            return False
        sig_set = {int(x) for x in signals}
        if self.SIGNAL_THICKNESS not in sig_set:
            return False
        if (
            self.SIGNAL_QUALITY not in sig_set
            and self.SIGNAL_MEDIAN1 not in sig_set
        ):
            return False
        return True

    def _pick_matching_sodx_candidate(self, current: list) -> Optional[list]:
        """Return the best preferred SODX layout already satisfied by the device."""
        if not current:
            return None
        current_set = {int(x) for x in current}
        for candidate in self._preferred_sodx_candidates():
            if set(candidate).issubset(current_set):
                return candidate
        if self._sodx_meets_minimum(current):
            return list(current)
        return None

    def _try_join_existing_stream(self, prefix: str = "") -> bool:
        """
        Attach to an already-running stream without STO or SODX changes.

        Lets CHR native software and this app share the device.
        """
        signals = self._query_output_signals()
        if not self._sodx_meets_minimum(signals):
            print(f"{prefix}Passive join: SODX insufficient ({signals or 'none'})")
            return False

        self._active_output_signals = [int(x) for x in signals]
        print(f"{prefix}Passive join: keeping existing SODX {self._active_output_signals}")

        try:
            self._flush_stream_buffer()
            data, drained = self._drain_stream_to_latest(batch_size=64, max_batches=8)
            if data is not None:
                self._stream_started = True
                print(f"{prefix}Passive join: sample OK (drained={drained})")
                return True
        except Exception as exc:
            if self._is_data_format_missing_error(exc):
                print(f"{prefix}Passive join: format packet missing — need full setup")
            else:
                print(f"{prefix}Passive join read failed: {exc}")

        try:
            self.connection.start_data_stream()
            self._stream_started = True
            time.sleep(0.15)
            if self._probe_live_sample(timeout_s=2.0):
                print(f"{prefix}Passive join: STA without STO OK")
                return True
        except Exception as exc:
            print(f"{prefix}Passive join STA failed: {exc}")

        self._stream_started = False
        return False

    def _try_soft_stream_recovery(self) -> bool:
        """Recover a dead stream without STO or TCP reopen (coexist-friendly)."""
        if not self.connection:
            return False
        try:
            self._flush_stream_buffer()
            if self._probe_live_sample(timeout_s=2.0):
                return True
        except Exception as exc:
            print(f"[RECOVER] Soft flush/read failed: {exc}")

        try:
            self.connection.start_data_stream()
            self._stream_started = True
            time.sleep(0.1)
            if self._probe_live_sample(timeout_s=2.0):
                return True
        except Exception as exc:
            print(f"[RECOVER] Soft STA failed: {exc}")
            self._stream_started = False
        return False

    def _flush_stream_buffer(self):
        """Discard stale samples after the output signal layout changes."""
        if not self.connection:
            return
        try:
            self.connection.flush_connection_buffer()
        except Exception as ex:
            print(f"[STREAM] flush_connection_buffer: {ex}")

    def _resync_data_stream(self, stop_first: bool = True):
        """Restart streaming and flush stale samples after SODX changes."""
        if not self.connection:
            return
        if stop_first and not self.coexist_with_chr_software:
            try:
                self.connection.stop_data_stream()
            except Exception:
                pass
            self._stream_started = False
            time.sleep(0.05)
        elif stop_first and self.coexist_with_chr_software:
            print("[STREAM] Coexist mode — resync without STO")
            self._stream_started = False
        self._flush_stream_buffer()
        try:
            self.connection.start_data_stream()
            self._stream_started = True
        except Exception as ex:
            print(f"[STREAM] start_data_stream after resync FAILED: {ex}")
            self._stream_started = False
            return

        # Format packet can arrive slightly after STA — brief settle before drain.
        time.sleep(0.1)

        # Drop every queued packet so the next read is fresh (not pre-SODX layout).
        try:
            _, drained = self._drain_stream_to_latest()
            if drained:
                print(f"[STREAM] Drained {drained} stale sample(s) after resync")
        except Exception as ex:
            if self._is_data_format_missing_error(ex) or self._is_stream_dead_error(ex):
                print(f"[STREAM] Drain after resync hit stream error — will require probe/reopen: {ex}")
            else:
                print(f"[STREAM] Drain after resync failed: {ex}")
        self._last_sample_counter = None

    def _signal_is_active(self, signal_id: int) -> bool:
        return (not self._active_output_signals) or (signal_id in self._active_output_signals)

    def _extract_first_signal(self, data, signal_id: int) -> Optional[float]:
        """
        Read a signal value from the current sample.

        Float signals (256/257/260/82) are already in engineering units in the
        chrpy DOUBLE buffer — no manual scaling.
        """
        if not self._signal_is_active(signal_id):
            return None
        try:
            values = data.get_signal_values(signal_id, 0)
        except Exception:
            return None

        if values is None:
            return None
        if isinstance(values, (float, int, np.floating, np.integer)):
            value = float(values)
            return None if np.isnan(value) else value
        try:
            if len(values) == 0:
                return None
            value = float(values[0])
            return None if np.isnan(value) else value
        except Exception:
            return None

    def _describe_sample_layout(self, data) -> dict:
        """Describe how the current sample row maps to signal IDs (for debugging)."""
        layout = {
            "signal_info": [],
            "raw_row": None,
        }
        for idx, sig in enumerate(data.signal_info or []):
            try:
                sig_id = int(sig[1])
            except Exception:
                continue
            entry = {"idx": idx, "signal_id": sig_id}
            try:
                entry["value"] = self._format_signal_value_for_debug(
                    data.get_signal_values(sig_id, 0)
                )
            except Exception as exc:
                entry["error"] = str(exc)
            layout["signal_info"].append(entry)

        try:
            if data.samples is not None and data.sample_cnt > 0:
                row = data.samples[0]
                if isinstance(row, np.ndarray):
                    layout["raw_row"] = [
                        None if (isinstance(v, float) and np.isnan(v)) else float(v)
                        for v in row.tolist()
                    ]
        except Exception:
            pass

        try:
            if data.gen_signal_info is not None:
                layout["meta"] = {
                    "channel_cnt": int(getattr(data.gen_signal_info, "channel_cnt", 0)),
                    "global_sig_cnt": int(getattr(data.gen_signal_info, "global_sig_cnt", 0)),
                    "peak_sig_cnt": int(getattr(data.gen_signal_info, "peak_sig_cnt", 0)),
                }
        except Exception:
            pass
        return layout

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
            "layout": self._describe_sample_layout(data),
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
        
    def _is_data_format_missing_error(self, exc: Exception) -> bool:
        """True for chrpy ERR_DATAFMT_MISSING (-536450304)."""
        text = str(exc)
        return ('-536450304' in text) or ('data format packet is missing' in text.lower())

    def _is_stream_dead_error(self, exc_or_text) -> bool:
        """True for errors that mean the chrpy sample stream must be fully reopened."""
        text = str(exc_or_text)
        return any(
            code in text
            for code in (
                '-536451072',  # internal thread / stream dead
                '-536450304',  # data format missing
                '-536863232',  # unknown error often seen after dead stream
                '-536580864',  # stream out of sync
                '-536580859',
                '-536580860',
            )
        )

    def _probe_live_sample(self, timeout_s: float = 2.0) -> bool:
        """
        Confirm the sample stream is actually alive by reading one fresh sample.
        get_output_signal_infos() alone is not enough — stream can look configured
        but still be dead for GetNextSamples.
        """
        if not self.connection or not self._stream_started:
            return False
        try:
            data, drained = self._read_fresh_sample(timeout_s)
            ok = data is not None and getattr(data, 'sample_cnt', 0) > 0
            if ok:
                counter = self._read_sample_counter(data)
                print(
                    f"[CONNECT] Live sample OK "
                    f"(counter={counter}, drained={drained})"
                )
            else:
                print(f"[CONNECT] Live sample probe got no data within {timeout_s:.1f}s")
            return ok
        except Exception as exc:
            print(f"[CONNECT] Live sample probe failed: {exc}")
            return False

    def _safe_close_connection(self, stop_stream: Optional[bool] = None):
        """Best-effort close so a failed connect does not leave a stuck session."""
        if stop_stream is None:
            stop_stream = not self.coexist_with_chr_software

        conn = self.connection
        self.connection = None
        self._stream_started = False
        self._active_output_signals = []
        self._last_sample_counter = None
        if conn is None:
            return
        try:
            if stop_stream:
                try:
                    conn.stop_data_stream()
                except Exception:
                    pass
            try:
                conn.flush_connection_buffer()
            except Exception:
                pass
            handle = getattr(conn, 'conn_handle', None)
            if handle:
                try:
                    conn.close()
                except Exception as ex:
                    # Invalid handle after a failed OpenConnection is common.
                    print(f"[CONNECT] Close ignored: {ex}")
        except Exception as ex:
            print(f"[CONNECT] Close after failure: {ex}")

    def _open_and_configure(
        self,
        ip_address: str,
        attempt_label: str = "",
        force_full_setup: bool = False,
    ) -> None:
        """Open TCP connection, configure SODX, start stream (raises on failure)."""
        self.connection = connection_from_params(
            addr=ip_address,
            device_type=DeviceType.CHR_2,
            conn_mode=OperationMode.SYNC
        )
        self.connection.open()

        prefix = f"[CONNECT]{attempt_label} "

        if (
            not force_full_setup
            and self.coexist_with_chr_software
            and self._try_join_existing_stream(prefix)
            and self._probe_live_sample(timeout_s=2.5)
        ):
            print(f"{prefix}Connected via passive join (CHR software coexistence)")
            print(f"{prefix}Active SODX signals: {self._active_output_signals}")
            return

        if self.coexist_with_chr_software:
            print(
                f"{prefix}Passive join unavailable — full stream setup "
                f"(may interrupt CHR native software)"
            )
        else:
            print(f"{prefix}Stopping any existing stream ...")
            try:
                self.connection.stop_data_stream()
                print(f"{prefix}Stream stopped OK")
            except Exception as ex:
                print(f"{prefix}Stream stop (ignored): {ex}")
            time.sleep(0.15)

        print(f"{prefix}Running _setup_measurement ...")
        self._setup_measurement()
        print(f"{prefix}_setup_measurement done")

        print(f"{prefix}Starting data stream ...")
        self._resync_data_stream(stop_first=not self.coexist_with_chr_software)
        if not self._stream_started:
            raise Exception("Data stream failed to start")

        # Only declare success when a real sample can be read.
        if not self._probe_live_sample(timeout_s=2.5):
            raise Exception(
                "Connected but sample stream is not delivering data "
                "(device busy, leftover session, or stream dead)"
            )

        print(f"{prefix}Data stream started OK")
        print(f"{prefix}Active SODX signals: {self._active_output_signals}")

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

        last_error = None
        for attempt in range(1, 4):
            try:
                self._open_and_configure(ip_address, attempt_label=f" Attempt {attempt}:")
                self.state = ConnectionState.CONNECTED
                return True, "Connected successfully"

            except Exception as e:
                last_error = e
                print(f"[CONNECT] Attempt {attempt} failed: {e}")
                self._safe_close_connection()
                if attempt < 3:
                    time.sleep(0.5 * attempt)
                    continue
                break

        self.state = ConnectionState.ERROR
        self.error_message = str(last_error)
        hint = ""
        if last_error and self._is_stream_dead_error(last_error):
            hint = (
                " Close any other CHRocodileGUI.exe / Python GUI still connected "
                "to the device, then try again."
            )
        return False, f"Connection failed: {last_error}.{hint}"
    
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
                if not self.coexist_with_chr_software:
                    try:
                        self.connection.stop_data_stream()
                    except Exception:
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
        """Configure this client's output signal list (SODX). Device MMD/NOP/AVD are left as-is."""
        if not self.connection:
            return

        def _exec_checked(cmd: str, *args):
            try:
                resp = self.connection.exec(cmd, *args)
            except Exception as e:
                raise Exception(f"{cmd}{args}: transport/exec error: {e}")
            if resp.error_code != 0:
                raise Exception(f"{cmd}{args}: device error_code={resp.error_code}")
            return resp

        def _exec_verbose(cmd: str, *args):
            """Run command and print result to terminal."""
            args_str = ', '.join(str(a) for a in args)
            try:
                resp = _exec_checked(cmd, *args)
                print(f"[SETUP] {cmd}({args_str}) -> OK  (resp args={getattr(resp, 'args', None)})")
                return resp
            except Exception as e:
                print(f"[SETUP] {cmd}({args_str}) -> FAILED: {e}")
                raise

        try:
            before = self._query_output_signals()
            if before:
                print(f"[SETUP] Device SODX before connect setup: {before}")

            existing = self._pick_matching_sodx_candidate(before)
            if existing is not None:
                self._active_output_signals = before or existing
                print(
                    f"[SETUP] SODX already sufficient — skipping reconfigure: "
                    f"{self._active_output_signals}"
                )
                return

            # Globals first, then peak signals — matches chrpy DOUBLE buffer layout.
            # 256=Thickness1, 257=Quality1, 260=Median1 (interferometric float).
            sodx_candidates = self._preferred_sodx_candidates()

            sodx_signals = None
            last_error = None
            for candidate in sodx_candidates:
                try:
                    _exec_verbose('SODX', *candidate)
                    sodx_signals = candidate
                    print(f"[SETUP] SODX active signals: {sodx_signals}")
                    break
                except Exception as exc:
                    last_error = exc
                    print(f"[SETUP] SODX fallback failed for {candidate}: {exc}")

            if sodx_signals is None:
                raise Exception(f"Failed to configure SODX: {last_error}")

            confirmed = self._query_output_signals()
            self._active_output_signals = confirmed or list(sodx_signals)
            print(f"[SETUP] Device confirmed SODX: {self._active_output_signals}")

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

        rate_hz = max(1, int(self.measuring_rate_hz))
        avg_factor = max(1, int(self.data_average))
        # Floor at 2.0s: 0.5s was too tight and caused false "No data" timeouts
        # when the FIFO was briefly empty after a large drain.
        timeout_s = min(5.0, max(2.0, (avg_factor / rate_hz) * 8.0))

        try:
            return self._read_measurement_once(
                include_spectrum=include_spectrum,
                timeout_s=timeout_s,
            )

        except Exception as e:
            err_str = str(e)
            # Dead/corrupt sample stream — recover and retry once.
            if self._is_stream_dead_error(err_str):
                recovered = self._recover_data_stream()
                if recovered:
                    try:
                        return self._read_measurement_once(
                            include_spectrum=include_spectrum,
                            timeout_s=timeout_s,
                        )
                    except Exception as retry_exc:
                        return {'error': f'Measurement error after recover: {retry_exc}'}
            return {'error': f'Measurement error: {err_str}'}

        finally:
            self._measurement_lock.release()

    def _read_measurement_once(self, include_spectrum: bool, timeout_s: float) -> dict:
        """Read one measurement assuming the measurement lock is already held."""
        if not self._stream_started:
            self.start_data_stream()
            self._stream_started = True

        data, drained_total = self._read_fresh_sample(timeout_s)

        if data is None or data.sample_cnt == 0:
            # One gentle retry: short settle, drain again (no flush).
            print(f"[MEAS] No sample within {timeout_s:.1f}s — retrying once ...")
            time.sleep(0.05)
            data, drained_retry = self._read_fresh_sample(timeout_s)
            drained_total += drained_retry
            if data is None or data.sample_cnt == 0:
                return {'error': f'No data received (timeout {timeout_s:.2f}s)'}

        if data.error_code < 0:
            return {'error': f'Device error: {data.error_code}'}

        sample_counter = self._read_sample_counter(data)
        counter_stale = self._sample_counter_stale(
            self._last_sample_counter, sample_counter
        )
        if drained_total > 20:
            self._maybe_log_buffer_backlog(drained_total, sample_counter)
        if counter_stale:
            print(
                f"[BUFFER] Sample counter did not advance "
                f"({self._last_sample_counter} -> {sample_counter}); "
                f"draining again without flush"
            )
            # Do NOT force_flush here — that was killing the stream.
            data2, retry_drained = self._read_fresh_sample(timeout_s, force_flush=False)
            drained_total += retry_drained
            if data2 is not None and data2.sample_cnt > 0:
                data = data2
                sample_counter = self._read_sample_counter(data)
        if sample_counter is not None:
            self._last_sample_counter = sample_counter

        thickness = self._extract_first_signal(data, self.SIGNAL_THICKNESS)
        quality = self._extract_first_signal(data, self.SIGNAL_QUALITY)
        median1 = self._extract_first_signal(data, self.SIGNAL_MEDIAN1)
        intensity = self._extract_first_signal(data, self.SIGNAL_INTENSITY)

        print(
            f"[MEAS] counter={sample_counter} drained={drained_total} "
            f"thickness={thickness} median1={median1} "
            f"quality={quality} intensity={intensity}"
        )

        result = {
            'thickness': thickness,
            'median1': median1,
            'intensity': intensity,
            'quality': quality,
            'peak1': None,
            'peak2': None,
            'intensity_signal_id': self.SIGNAL_INTENSITY if intensity is not None else None,
            'quality_signal_id': self.SIGNAL_QUALITY if quality is not None else None,
            'sample_counter': sample_counter,
            'buffer_drained': drained_total,
            'signal_snapshot': self._collect_signal_snapshot(data),
            'timestamp': time.time()
        }

        if include_spectrum:
            spectrum_data = self.download_spectrum()
            if spectrum_data and 'error' not in spectrum_data:
                result['spectrum'] = spectrum_data.get('spectrum')

        return result

    def _recover_data_stream(self) -> bool:
        """
        Recover after chrpy stream death.

        In coexist mode, only soft recovery is attempted so CHR native software
        is not kicked off the device. Full TCP reopen is used otherwise.
        """
        now = time.time()
        if (now - self._last_recover_ts) < self._recover_min_interval_s:
            print(
                f"[RECOVER] Skipping recovery "
                f"(last attempt {now - self._last_recover_ts:.1f}s ago)"
            )
            return False
        self._last_recover_ts = now

        if self.coexist_with_chr_software:
            print("[RECOVER] Coexist mode — trying soft recovery (no STO/reopen)")
            if self._try_soft_stream_recovery():
                print("[RECOVER] Soft recovery OK")
                return True
            print(
                "[RECOVER] Soft recovery failed. CHR native software may hold "
                "the device — disconnect the other client or use Reconnect here."
            )
            self.state = ConnectionState.ERROR
            self.error_message = (
                "Stream lost while sharing with CHR native software. "
                "Close the other client or disconnect/reconnect from this app."
            )
            return False

        ip = self.ip_address
        if not ip:
            print("[RECOVER] No IP address available for reopen")
            self._stream_started = False
            return False

        print("[RECOVER] Internal stream error — full connection reopen ...")
        self._safe_close_connection(stop_stream=True)
        last_exc = None
        for attempt in range(1, 4):
            time.sleep(1.0 * attempt)
            try:
                self.state = ConnectionState.CONNECTING
                self._open_and_configure(
                    ip,
                    attempt_label=f" RECOVER {attempt}:",
                    force_full_setup=True,
                )
                self.state = ConnectionState.CONNECTED
                print("[RECOVER] Full reopen OK")
                return True
            except Exception as exc:
                last_exc = exc
                print(f"[RECOVER] Full reopen attempt {attempt} FAILED: {exc}")
                self._safe_close_connection(stop_stream=True)

        print(f"[RECOVER] Full reopen FAILED: {last_exc}")
        self.state = ConnectionState.ERROR
        self.error_message = str(last_exc)
        return False

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
        
        # Start data stream if not already running from connect().
        if not self._stream_started:
            print("[CONT] Stream not yet started — resyncing now ...")
            self._resync_data_stream()
            if self._stream_started:
                print("[CONT] Stream started")
            else:
                print("[CONT] Stream start FAILED — aborting continuous measurement")
                self.continuous_measurement_active = False
                return
        else:
            print("[CONT] Stream already running — reusing existing stream")

        print(f"[CONT] Starting measurement loop (interval={interval_ms}ms)")
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

        if not self.coexist_with_chr_software:
            self.stop_data_stream()
    
    def _continuous_measurement_loop(self):
        """Internal loop for continuous measurements."""
        consecutive_errors = 0
        max_consecutive_errors = 5
        measurement_count = 0

        while self.continuous_measurement_active and not self.stop_event.is_set():
            try:
                measurement = self.get_single_measurement(include_spectrum=self.include_spectrum_in_continuous)

                if measurement and 'error' not in measurement:
                    consecutive_errors = 0
                    measurement_count += 1

                    # Print first 10 measurements and then every 500 to terminal
                    if measurement_count <= 10 or measurement_count % 500 == 0:
                        snap = measurement.get('signal_snapshot', {})
                        print(
                            f"[MEAS #{measurement_count}] "
                            f"thickness={measurement.get('thickness')} "
                            f"median1={measurement.get('median1')} "
                            f"quality={measurement.get('quality')} "
                            f"intensity={measurement.get('intensity')} "
                            f"counter={measurement.get('sample_counter')} "
                            f"drained={measurement.get('buffer_drained')} "
                            f"active_sodx={self._active_output_signals}"
                        )

                    self.data_queue.put(measurement)

                    if self.data_callback:
                        try:
                            self.data_callback(measurement)
                        except Exception as e:
                            print(f"[MEAS] Callback error: {e}")
                else:
                    consecutive_errors += 1
                    if measurement:
                        error_msg = measurement.get('error', 'Unknown error')
                        print(f"[MEAS] Error: {error_msg}")

                    if consecutive_errors >= max_consecutive_errors:
                        print(f"[MEAS] {consecutive_errors} consecutive errors — stopping loop")
                        self.continuous_measurement_active = False
                        break

            except Exception as e:
                print(f"[MEAS] Exception in loop: {e}")
                consecutive_errors += 1
                if consecutive_errors >= max_consecutive_errors:
                    self.continuous_measurement_active = False
                    break

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

