# -*- coding: utf-8 -*-
"""
CHRocodile Film Thickness Measurement GUI Application
Main application window for controlling CHRocodile 2 LR device.
"""

import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import threading
import queue
import time
import numpy as np
from typing import Optional
import logging
import os
import sys
from datetime import datetime

from device_controller import CHRocodileController, ConnectionState
from simulator import CHRocodileSimulator
from plotter import MeasurementPlotter
from settings_manager import SettingsManager

# Import ADS interface (requires pyads and TwinCAT DLL)
# This import is safe - beckhoff_ads_interface handles missing DLL gracefully
try:
    from beckhoff_ads_interface import BeckhoffADSInterface, PYADS_AVAILABLE
    import pyads
except Exception as e:
    # Fallback if import completely fails
    PYADS_AVAILABLE = False
    BeckhoffADSInterface = None
    pyads = None
    print(f"Warning: Could not import Beckhoff ADS interface: {e}")


class CHRocodileGUI:
    """
    Main GUI application for CHRocodile thickness measurement.
    """
    
    def __init__(self, root):
        """
        Initialize the GUI application.
        
        Args:
            root: Tkinter root window
        """
        self.root = root
        self.root.title("CHRocodile Film Thickness Measurement")
        self.root.geometry("1400x800")
        
        # Device controller and simulator
        self.controller = CHRocodileController(data_callback=self.on_measurement_data)
        self.simulator = CHRocodileSimulator()
        self.simulation_mode = False
        
        # Data queue for thread-safe communication
        self.data_queue = queue.Queue()
        
        # Measurement state
        self.continuous_measurement_active = False
        self.measurement_count = 0
        self.simulation_stop_event = None
        self.simulation_thread = None
        
        # Settings manager
        self.settings_manager = SettingsManager()
        
        # Setup file logging
        self._setup_file_logging()
        
        # Beckhoff PLC interface (ADS only)
        self.beckhoff_ads_interface = None
        self.beckhoff_enabled = False
        self.beckhoff_monitor_window = None
        self.beckhoff_monitor_window = None
        
        # Load ADS settings from file
        ads_settings = self.settings_manager.get_ads_settings()
        self.beckhoff_ams_netid = ads_settings.get('ams_netid', '127.0.0.1.1.1')
        
        # Setup GUI
        self._create_widgets()
        
        # Display log file location in status (after GUI is ready)
        self._log_status(f"Logging to: {self.log_file_path}")
        
        # Start queue processing
        self.root.after(100, self._process_queue)
        
        # Start Beckhoff interface if enabled
        self._init_beckhoff_interface()
        
    def _create_widgets(self):
        """Create and layout all GUI widgets."""
        # Main container
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        self.root.columnconfigure(0, weight=1)
        self.root.rowconfigure(0, weight=1)
        
        # Top section: Connection and controls
        top_frame = ttk.LabelFrame(main_frame, text="Connection & Controls", padding="10")
        top_frame.grid(row=0, column=0, columnspan=2, sticky=(tk.W, tk.E), pady=(0, 10))
        top_frame.columnconfigure(0, weight=1)
        top_frame.columnconfigure(1, weight=1)
        
        # Connection panel (Row 0 - full width)
        conn_frame = ttk.Frame(top_frame)
        conn_frame.grid(row=0, column=0, columnspan=2, sticky=(tk.W, tk.E), pady=(0, 10))
        
        ttk.Label(conn_frame, text="IP Address:").grid(row=0, column=0, padx=(0, 5))
        self.ip_entry = ttk.Entry(conn_frame, width=15)
        self.ip_entry.insert(0, "192.168.170.3")
        self.ip_entry.grid(row=0, column=1, padx=(0, 5))
        
        self.config_ip_btn = ttk.Button(conn_frame, text="Configure Device IP", 
                                       command=self.on_configure_device_ip, state=tk.DISABLED)
        self.config_ip_btn.grid(row=0, column=2, padx=(0, 10))
        
        self.connect_btn = ttk.Button(conn_frame, text="Connect", command=self.on_connect)
        self.connect_btn.grid(row=0, column=3, padx=(0, 5))
        
        self.disconnect_btn = ttk.Button(conn_frame, text="Disconnect", command=self.on_disconnect, state=tk.DISABLED)
        self.disconnect_btn.grid(row=0, column=4, padx=(0, 10))
        
        # Connection status
        self.status_label = ttk.Label(conn_frame, text="Disconnected", foreground="red")
        self.status_label.grid(row=0, column=5, padx=(10, 0))
        
        # Simulation mode checkbox
        self.sim_check = ttk.Checkbutton(conn_frame, text="Simulation Mode", 
                                         command=self.on_simulation_toggle)
        self.sim_check.grid(row=0, column=6, padx=(20, 0))
        
        # Beckhoff PLC Communication section (Row 1 - full width, below connection)
        beckhoff_section = ttk.LabelFrame(top_frame, text="Beckhoff PLC Communication", padding="10")
        beckhoff_section.grid(row=1, column=0, columnspan=2, sticky=(tk.W, tk.E), pady=(0, 10))
        
        beckhoff_frame = ttk.Frame(beckhoff_section)
        beckhoff_frame.pack(fill=tk.X)
        
        if PYADS_AVAILABLE:
            self.beckhoff_check = ttk.Checkbutton(beckhoff_frame, text="Enable ADS Interface", 
                                                   command=self.on_beckhoff_toggle)
            self.beckhoff_check.pack(side=tk.LEFT, padx=(0, 10))
        else:
            # Show disabled checkbox with warning tooltip
            self.beckhoff_check = ttk.Checkbutton(beckhoff_frame, text="Enable ADS Interface", 
                                                   command=self.on_beckhoff_toggle, state=tk.DISABLED)
            self.beckhoff_check.pack(side=tk.LEFT, padx=(0, 10))
            # Tooltip would require additional library, so we'll show status in label instead
        
        # Settings button - always visible (allows configuration even without DLL)
        self.beckhoff_settings_btn = ttk.Button(beckhoff_frame, text="⚙ Settings", width=12,
                                                command=self.on_beckhoff_settings)
        self.beckhoff_settings_btn.pack(side=tk.LEFT, padx=(0, 10))
        
        # Communication monitor button
        self.beckhoff_monitor_btn = ttk.Button(beckhoff_frame, text="📊 Monitor", width=12,
                                               command=self.on_beckhoff_monitor)
        self.beckhoff_monitor_btn.pack(side=tk.LEFT, padx=(0, 10))
        
        # Beckhoff status label
        if PYADS_AVAILABLE:
            self.beckhoff_status_label = ttk.Label(beckhoff_frame, text="Status: Off", foreground="gray")
        else:
            self.beckhoff_status_label = ttk.Label(beckhoff_frame, text="Status: DLL missing (TwinCAT Runtime required)", 
                                                   foreground="orange")
        self.beckhoff_status_label.pack(side=tk.LEFT)
        
        # Measurement controls - split into two rows for better visibility
        measure_frame = ttk.LabelFrame(top_frame, text="Measurement Controls", padding="5")
        measure_frame.grid(row=2, column=0, sticky=(tk.W, tk.E), padx=(0, 10), pady=(0, 10))
        
        # Row 1: Single measurement and continuous controls
        row1_frame = ttk.Frame(measure_frame)
        row1_frame.grid(row=0, column=0, sticky=(tk.W, tk.E), pady=(0, 5))
        
        self.single_measure_btn = ttk.Button(row1_frame, text="Single Measurement", 
                                             command=self.on_single_measurement, state=tk.DISABLED)
        self.single_measure_btn.grid(row=0, column=0, padx=(0, 10))
        
        ttk.Label(row1_frame, text="Interval (ms):").grid(row=0, column=1, padx=(0, 5))
        self.interval_var = tk.StringVar(value="100")
        self.interval_entry = ttk.Entry(row1_frame, textvariable=self.interval_var, width=8)
        self.interval_entry.grid(row=0, column=2, padx=(0, 10))
        
        self.continuous_btn = ttk.Button(row1_frame, text="▶ Start Continuous", 
                                        command=self.on_continuous_toggle, state=tk.DISABLED)
        self.continuous_btn.grid(row=0, column=3, padx=(0, 10))
        
        # Row 2: Refractive index and spectrum download
        row2_frame = ttk.Frame(measure_frame)
        row2_frame.grid(row=1, column=0, sticky=(tk.W, tk.E), pady=(0, 5))
        
        ttk.Label(row2_frame, text="Refractive Index (n):").grid(row=0, column=0, padx=(0, 5))
        self.refractive_index_var = tk.StringVar(value="1.5")
        self.refractive_index_entry = ttk.Entry(row2_frame, textvariable=self.refractive_index_var, width=8)
        self.refractive_index_entry.grid(row=0, column=1, padx=(0, 5))
        
        self.set_refractive_btn = ttk.Button(row2_frame, text="Set n", 
                                            command=self.on_set_refractive_index, state=tk.DISABLED)
        self.set_refractive_btn.grid(row=0, column=2, padx=(0, 10))
        
        self.dark_ref_btn = ttk.Button(row2_frame, text="Dark Reference", 
                                       command=self.on_dark_reference, state=tk.DISABLED)
        self.dark_ref_btn.grid(row=0, column=3, padx=(0, 10))
        
        self.download_spectrum_btn = ttk.Button(row2_frame, text="Download Spectrum", 
                                               command=self.on_download_spectrum, state=tk.DISABLED)
        self.download_spectrum_btn.grid(row=0, column=4)
        
        # Device Settings Panel
        settings_frame = ttk.LabelFrame(top_frame, text="Device Settings", padding="5")
        settings_frame.grid(row=2, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=(0, 10))
        
        # Row 1: Measuring rate and mode
        settings_row1 = ttk.Frame(settings_frame)
        settings_row1.grid(row=0, column=0, sticky=(tk.W, tk.E), pady=(0, 5))
        
        ttk.Label(settings_row1, text="Rate (Hz):").grid(row=0, column=0, padx=(0, 5))
        self.measuring_rate_var = tk.StringVar(value="1000")
        self.measuring_rate_entry = ttk.Entry(settings_row1, textvariable=self.measuring_rate_var, width=10)
        self.measuring_rate_entry.grid(row=0, column=1, padx=(0, 10))
        
        ttk.Label(settings_row1, text="Mode:").grid(row=0, column=2, padx=(0, 5))
        self.measuring_mode_var = tk.StringVar(value="Interferometric")
        self.measuring_mode_combo = ttk.Combobox(settings_row1, textvariable=self.measuring_mode_var, 
                                                 values=["Chromatic Confocal", "Interferometric"], 
                                                 state="readonly", width=18)
        self.measuring_mode_combo.grid(row=0, column=3, padx=(0, 10))
        self.measuring_mode_combo.bind("<<ComboboxSelected>>", self.on_measuring_mode_change)
        
        ttk.Label(settings_row1, text="Lamp Intensity (%):").grid(row=0, column=4, padx=(0, 5))
        self.lamp_intensity_var = tk.StringVar(value="50")
        self.lamp_intensity_entry = ttk.Entry(settings_row1, textvariable=self.lamp_intensity_var, width=8)
        self.lamp_intensity_entry.grid(row=0, column=5, padx=(0, 10))
        
        self.apply_settings_btn = ttk.Button(settings_row1, text="Apply Settings", 
                                            command=self.on_apply_settings, state=tk.DISABLED)
        self.apply_settings_btn.grid(row=0, column=6, padx=(0, 10))
        
        self.view_config_btn = ttk.Button(settings_row1, text="View Config", 
                                          command=self.on_view_config, state=tk.DISABLED)
        self.view_config_btn.grid(row=0, column=7)
        
        # Row 2: Averaging
        settings_row2 = ttk.Frame(settings_frame)
        settings_row2.grid(row=1, column=0, sticky=(tk.W, tk.E))
        
        ttk.Label(settings_row2, text="Data Avg:").grid(row=0, column=0, padx=(0, 5))
        self.data_avg_var = tk.StringVar(value="1")
        self.data_avg_entry = ttk.Entry(settings_row2, textvariable=self.data_avg_var, width=8)
        self.data_avg_entry.grid(row=0, column=1, padx=(0, 10))
        
        ttk.Label(settings_row2, text="Spectrum Avg:").grid(row=0, column=2, padx=(0, 5))
        self.spectrum_avg_var = tk.StringVar(value="1")
        self.spectrum_avg_entry = ttk.Entry(settings_row2, textvariable=self.spectrum_avg_var, width=8)
        self.spectrum_avg_entry.grid(row=0, column=3)
        
        # Middle section: Display and plots
        middle_frame = ttk.Frame(main_frame)
        middle_frame.grid(row=1, column=0, columnspan=2, sticky=(tk.W, tk.E, tk.N, tk.S), pady=(0, 10))
        main_frame.rowconfigure(1, weight=1)
        
        # Left panel: Current measurement display
        left_panel = ttk.LabelFrame(middle_frame, text="Current Measurement", padding="10")
        left_panel.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S), padx=(0, 10))
        middle_frame.columnconfigure(1, weight=1)
        middle_frame.rowconfigure(0, weight=1)
        
        # Thickness display
        ttk.Label(left_panel, text="Thickness:", font=("Arial", 12)).grid(row=0, column=0, sticky=tk.W, pady=5)
        self.thickness_label = ttk.Label(left_panel, text="-- μm", font=("Arial", 16, "bold"))
        self.thickness_label.grid(row=0, column=1, sticky=tk.W, padx=(10, 0), pady=5)
        
        # Peak signals
        ttk.Label(left_panel, text="Peak 1:", font=("Arial", 10)).grid(row=1, column=0, sticky=tk.W, pady=5)
        self.peak1_label = ttk.Label(left_panel, text="--")
        self.peak1_label.grid(row=1, column=1, sticky=tk.W, padx=(10, 0), pady=5)
        
        ttk.Label(left_panel, text="Peak 2:", font=("Arial", 10)).grid(row=2, column=0, sticky=tk.W, pady=5)
        self.peak2_label = ttk.Label(left_panel, text="--")
        self.peak2_label.grid(row=2, column=1, sticky=tk.W, padx=(10, 0), pady=5)
        
        # Measurement count
        ttk.Label(left_panel, text="Measurements:", font=("Arial", 10)).grid(row=3, column=0, sticky=tk.W, pady=5)
        self.count_label = ttk.Label(left_panel, text="0")
        self.count_label.grid(row=3, column=1, sticky=tk.W, padx=(10, 0), pady=5)
        
        # Separator
        ttk.Separator(left_panel, orient=tk.HORIZONTAL).grid(row=4, column=0, columnspan=2, sticky=(tk.W, tk.E), pady=10)
        
        # Control buttons
        self.clear_plots_btn = ttk.Button(left_panel, text="Clear Plots", command=self.on_clear_plots)
        self.clear_plots_btn.grid(row=5, column=0, columnspan=2, pady=5, sticky=(tk.W, tk.E))
        
        self.export_btn = ttk.Button(left_panel, text="Export Data", command=self.on_export_data)
        self.export_btn.grid(row=6, column=0, columnspan=2, pady=5, sticky=(tk.W, tk.E))
        
        # Right panel: Plots
        right_panel = ttk.Frame(middle_frame)
        right_panel.grid(row=0, column=1, sticky=(tk.W, tk.E, tk.N, tk.S))
        right_panel.columnconfigure(0, weight=1)
        right_panel.rowconfigure(0, weight=1)
        right_panel.rowconfigure(1, weight=1)
        
        # Create plotter
        self.plotter = MeasurementPlotter(right_panel)
        
        # Thickness plot frame (toolbar uses pack, so we need separate frames)
        thickness_frame = ttk.Frame(right_panel)
        thickness_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S), pady=(0, 5))
        thickness_frame.columnconfigure(0, weight=1)
        thickness_frame.rowconfigure(0, weight=1)
        
        # Spectrum plot frame
        spectrum_frame = ttk.Frame(right_panel)
        spectrum_frame.grid(row=1, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        spectrum_frame.columnconfigure(0, weight=1)
        spectrum_frame.rowconfigure(0, weight=1)
        
        # Setup canvases in their frames (this handles the pack/grid conflict)
        self.plotter.setup_canvases(thickness_frame, spectrum_frame)
        
        # Bottom: Status bar
        status_frame = ttk.Frame(main_frame)
        status_frame.grid(row=2, column=0, columnspan=2, sticky=(tk.W, tk.E))
        
        self.status_text = tk.Text(status_frame, height=3, wrap=tk.WORD, state=tk.DISABLED)
        self.status_text.grid(row=0, column=0, sticky=(tk.W, tk.E), pady=(0, 5))
        status_frame.columnconfigure(0, weight=1)
        
        scrollbar = ttk.Scrollbar(status_frame, orient=tk.VERTICAL, command=self.status_text.yview)
        scrollbar.grid(row=0, column=1, sticky=(tk.N, tk.S))
        self.status_text.configure(yscrollcommand=scrollbar.set)
        
    def _setup_file_logging(self):
        """Setup file logging for the application."""
        # Determine log file path (next to executable or in project directory)
        if getattr(sys, 'frozen', False):
            # Running as compiled executable
            log_dir = os.path.dirname(sys.executable)
        else:
            # Running as script
            log_dir = os.path.dirname(os.path.abspath(__file__))
        
        # Create log file with timestamp in name
        log_filename = f"chrocodile_{datetime.now().strftime('%Y%m%d')}.log"
        self.log_file_path = os.path.join(log_dir, log_filename)
        
        # Setup logging
        self.logger = logging.getLogger('CHRocodile')
        self.logger.setLevel(logging.DEBUG)
        
        # Remove existing handlers to avoid duplicates
        self.logger.handlers.clear()
        
        # File handler with rotation (max 10MB, keep 5 backup files)
        from logging.handlers import RotatingFileHandler
        file_handler = RotatingFileHandler(
            self.log_file_path,
            maxBytes=10*1024*1024,  # 10MB
            backupCount=5,
            encoding='utf-8'
        )
        file_handler.setLevel(logging.DEBUG)
        
        # Format: timestamp, level, message
        formatter = logging.Formatter(
            '%(asctime)s [%(levelname)s] %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        file_handler.setFormatter(formatter)
        
        self.logger.addHandler(file_handler)
        
        # Log application start
        self.logger.info("=" * 80)
        self.logger.info("CHRocodile Application Started")
        self.logger.info(f"Log file: {self.log_file_path}")
        self.logger.info("=" * 80)
    
    def _log_status(self, message: str):
        """Add message to status text area and log file."""
        # Add to GUI status text (if GUI is initialized)
        if hasattr(self, 'status_text') and self.status_text is not None:
            try:
                self.status_text.config(state=tk.NORMAL)
                self.status_text.insert(tk.END, f"[{time.strftime('%H:%M:%S')}] {message}\n")
                self.status_text.see(tk.END)
                self.status_text.config(state=tk.DISABLED)
            except (tk.TclError, AttributeError):
                # GUI widget may not be ready yet or was destroyed
                pass
        
        # Also log to file
        if hasattr(self, 'logger'):
            self.logger.info(message)
        
    def _process_queue(self):
        """Process data from queue (called periodically from main thread)."""
        try:
            while True:
                data = self.data_queue.get_nowait()
                self._handle_measurement_data(data)
        except queue.Empty:
            pass
        
        # Schedule next check
        self.root.after(100, self._process_queue)
    
    def _handle_measurement_data(self, data: dict):
        """Handle measurement data in main thread."""
        if 'error' in data:
            error_msg = data['error']
            self._log_status(f"Error: {error_msg}")
            if hasattr(self, 'logger'):
                self.logger.error(f"Measurement error: {error_msg}")
            return
        
        # Update display
        thickness = data.get('thickness')
        if thickness is not None:
            self.thickness_label.config(text=f"{thickness:.2f} μm")
        
        peak1 = data.get('peak1')
        if peak1 is not None:
            self.peak1_label.config(text=f"{peak1:.1f}")
        
        peak2 = data.get('peak2')
        if peak2 is not None:
            self.peak2_label.config(text=f"{peak2:.1f}")
        
        # Update plot
        timestamp = data.get('timestamp', time.time())
        if thickness is not None:
            self.plotter.update_thickness_plot(timestamp, thickness)
        
        # Update spectrum plot if spectrum data is available
        spectrum = data.get('spectrum')
        if spectrum is not None:
            # Use peak positions from measurement data if available
            peak1_pos = None
            peak2_pos = None
            # Try to find peak positions from spectrum
            peak1_pos, peak2_pos = self._detect_peaks(spectrum)
            self.plotter.update_raw_data_plot(spectrum, peak1_pos, peak2_pos)
        
        # Update count
        self.measurement_count += 1
        self.count_label.config(text=str(self.measurement_count))
        
        # Log measurement to file
        if hasattr(self, 'logger'):
            thickness = data.get('thickness')
            peak1 = data.get('peak1')
            peak2 = data.get('peak2')
            self.logger.info(f"Measurement #{self.measurement_count}: Thickness={thickness:.3f} μm, Peak1={peak1:.1f}, Peak2={peak2:.1f}")
        
        # Send measurement result to Beckhoff PLC via ADS if interface is active
        # This ensures PLC-triggered measurements complete the handshake properly
        if self.beckhoff_ads_interface and self.beckhoff_ads_interface.is_running():
            plc_data = {
                "thickness": data.get('thickness'),
                "peak1": data.get('peak1'),
                "peak2": data.get('peak2'),
                "timestamp": data.get('timestamp'),
                "measurement_count": self.measurement_count
            }
            self.beckhoff_ads_interface.write_measurement_result(plc_data)
    
    def on_measurement_data(self, data: dict):
        """Callback for measurement data (called from device thread)."""
        # Put data in queue for processing in main thread
        # PLC write will be handled by _handle_measurement_data() to ensure
        # both device-triggered and PLC-triggered measurements complete handshake
        self.data_queue.put(data)
    
    def on_connect(self):
        """Handle connect button click."""
        ip_address = self.ip_entry.get().strip()
        
        if not ip_address:
            messagebox.showerror("Error", "Please enter an IP address")
            return
        
        # Disable connect button
        self.connect_btn.config(state=tk.DISABLED)
        self.status_label.config(text="Connecting...", foreground="orange")
        
        # Connect in separate thread
        def connect_thread():
            if self.simulation_mode:
                success = True
                message = "Simulation mode enabled"
            else:
                success, message = self.controller.connect(ip_address)
            
            # Update UI in main thread
            self.root.after(0, lambda: self._on_connect_complete(success, message))
        
        threading.Thread(target=connect_thread, daemon=True).start()
    
    def _on_connect_complete(self, success: bool, message: str):
        """Handle connection completion."""
        if success:
            self.status_label.config(text="Connected", foreground="green")
            self.connect_btn.config(state=tk.DISABLED)
            self.disconnect_btn.config(state=tk.NORMAL)
            self.config_ip_btn.config(state=tk.NORMAL)
            self.single_measure_btn.config(state=tk.NORMAL)
            self.continuous_btn.config(state=tk.NORMAL)
            self.download_spectrum_btn.config(state=tk.NORMAL)
            self.set_refractive_btn.config(state=tk.NORMAL)
            self.dark_ref_btn.config(state=tk.NORMAL)
            self.apply_settings_btn.config(state=tk.NORMAL)
            self.view_config_btn.config(state=tk.NORMAL)
            self._log_status(f"Connected: {message}")
            if hasattr(self, 'logger'):
                ip_address = self.ip_entry.get().strip()
                self.logger.info(f"Device connected: IP={ip_address}, Message={message}")
        else:
            self.status_label.config(text="Connection Failed", foreground="red")
            self.connect_btn.config(state=tk.NORMAL)
            messagebox.showerror("Connection Error", message)
            self._log_status(f"Connection failed: {message}")
            if hasattr(self, 'logger'):
                ip_address = self.ip_entry.get().strip()
                self.logger.error(f"Device connection failed: IP={ip_address}, Error={message}")
    
    def on_disconnect(self):
        """Handle disconnect button click."""
        success, message = self.controller.disconnect()
        
        if success:
            self.status_label.config(text="Disconnected", foreground="red")
            self.connect_btn.config(state=tk.NORMAL)
            self.disconnect_btn.config(state=tk.DISABLED)
            self.config_ip_btn.config(state=tk.DISABLED)
            self.single_measure_btn.config(state=tk.DISABLED)
            self.continuous_btn.config(state=tk.DISABLED)
            self.download_spectrum_btn.config(state=tk.DISABLED)
            self.set_refractive_btn.config(state=tk.DISABLED)
            self.dark_ref_btn.config(state=tk.DISABLED)
            self.apply_settings_btn.config(state=tk.DISABLED)
            self.view_config_btn.config(state=tk.DISABLED)
            self._log_status("Disconnected")
            if hasattr(self, 'logger'):
                self.logger.info("Device disconnected")
        else:
            messagebox.showerror("Error", message)
            if hasattr(self, 'logger'):
                self.logger.error(f"Disconnect error: {message}")
    
    def on_configure_device_ip(self):
        """Handle configure device IP button click."""
        if not self.controller.is_connected():
            messagebox.showwarning("Not Connected", 
                                  "Please connect to the device first before configuring its IP address.")
            return
        
        # Get current IP (the one we're connected to)
        current_ip = self.controller.ip_address if hasattr(self.controller, 'ip_address') else self.ip_entry.get()
        
        # Create dialog window
        dialog = tk.Toplevel(self.root)
        dialog.title("Configure Device IP Address")
        dialog.geometry("500x300")
        dialog.transient(self.root)
        dialog.grab_set()
        
        # Warning message
        warning_frame = ttk.Frame(dialog, padding="10")
        warning_frame.pack(fill=tk.X, padx=10, pady=10)
        
        warning_text = (
            "WARNING: Changing the device IP address will disconnect you from the device.\n\n"
            "After changing the IP:\n"
            "1. Update your PC's network adapter to be on the same subnet\n"
            "2. Reconnect using the new IP address\n"
            "3. The device may need to be restarted for changes to take effect\n\n"
            f"Current IP: {current_ip}\n"
            "Default IP for CHRocodile 2 LR: 192.168.170.3"
        )
        
        warning_label = ttk.Label(warning_frame, text=warning_text, 
                                  foreground="red", justify=tk.LEFT, wraplength=450)
        warning_label.pack()
        
        # Input fields
        input_frame = ttk.Frame(dialog, padding="10")
        input_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Label(input_frame, text="New IP Address:").grid(row=0, column=0, sticky=tk.W, pady=5)
        new_ip_entry = ttk.Entry(input_frame, width=20)
        new_ip_entry.insert(0, current_ip)
        new_ip_entry.grid(row=0, column=1, sticky=tk.W, pady=5, padx=(5, 0))
        
        ttk.Label(input_frame, text="Subnet Mask:").grid(row=1, column=0, sticky=tk.W, pady=5)
        subnet_entry = ttk.Entry(input_frame, width=20)
        subnet_entry.insert(0, "255.255.255.0")
        subnet_entry.grid(row=1, column=1, sticky=tk.W, pady=5, padx=(5, 0))
        
        ttk.Label(input_frame, text="Gateway:").grid(row=2, column=0, sticky=tk.W, pady=5)
        gateway_entry = ttk.Entry(input_frame, width=20)
        gateway_entry.insert(0, "192.168.170.1")
        gateway_entry.grid(row=2, column=1, sticky=tk.W, pady=5, padx=(5, 0))
        
        # Buttons
        button_frame = ttk.Frame(dialog, padding="10")
        button_frame.pack(fill=tk.X, padx=10, pady=10)
        
        def apply_ip_config():
            new_ip = new_ip_entry.get().strip()
            subnet = subnet_entry.get().strip()
            gateway = gateway_entry.get().strip()
            
            if not new_ip:
                messagebox.showerror("Error", "Please enter a new IP address")
                return
            
            # Confirm action
            confirm_msg = (
                f"Are you sure you want to change the device IP address to {new_ip}?\n\n"
                "This will disconnect you from the device. You will need to:\n"
                "1. Configure your PC's network adapter\n"
                "2. Reconnect using the new IP address"
            )
            
            if not messagebox.askyesno("Confirm IP Change", confirm_msg):
                return
            
            # Apply IP configuration
            success, message = self.controller.set_device_ip_address(new_ip, subnet, gateway)
            
            if success:
                messagebox.showinfo("Success", 
                                  f"{message}\n\n"
                                  f"Please update the IP address in the connection field to {new_ip} "
                                  f"and reconnect after configuring your network adapter.")
                # Update the IP entry field
                self.ip_entry.delete(0, tk.END)
                self.ip_entry.insert(0, new_ip)
                # Disconnect since IP changed
                self.on_disconnect()
                dialog.destroy()
            else:
                messagebox.showerror("Error", f"Failed to set IP address:\n{message}")
        
        ttk.Button(button_frame, text="Apply", command=apply_ip_config).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=5)
    
    def on_simulation_toggle(self):
        """Handle simulation mode toggle."""
        self.simulation_mode = not self.simulation_mode
        
        if self.simulation_mode:
            self._log_status("Simulation mode enabled")
            if hasattr(self, 'logger'):
                self.logger.info("Simulation mode enabled")
            # Enable controls even without connection
            self.single_measure_btn.config(state=tk.NORMAL)
            self.continuous_btn.config(state=tk.NORMAL)
            self.download_spectrum_btn.config(state=tk.NORMAL)
            self.set_refractive_btn.config(state=tk.DISABLED)  # Still need connection for this
            self.dark_ref_btn.config(state=tk.DISABLED)
            self.apply_settings_btn.config(state=tk.DISABLED)
            self.view_config_btn.config(state=tk.DISABLED)
        else:
            self._log_status("Simulation mode disabled")
            if hasattr(self, 'logger'):
                self.logger.info("Simulation mode disabled")
            # Disable controls if not connected
            if not self.controller.is_connected():
                self.single_measure_btn.config(state=tk.DISABLED)
                self.continuous_btn.config(state=tk.DISABLED)
                self.download_spectrum_btn.config(state=tk.DISABLED)
                self.set_refractive_btn.config(state=tk.DISABLED)
                self.dark_ref_btn.config(state=tk.DISABLED)
                self.apply_settings_btn.config(state=tk.DISABLED)
                self.view_config_btn.config(state=tk.DISABLED)
    
    def on_single_measurement(self):
        """Handle single measurement button click."""
        def measure_thread():
            if self.simulation_mode:
                data = self.simulator.simulate_measurement()
                result = {
                    'thickness': data.thickness,
                    'peak1': data.peak1,
                    'peak2': data.peak2,
                    'spectrum': data.spectrum,  # Include spectrum by default
                    'timestamp': data.timestamp
                }
            else:
                # Always include spectrum for single measurements
                result = self.controller.get_single_measurement(include_spectrum=True)
            
            self.root.after(0, lambda: self._handle_measurement_data(result))
        
        threading.Thread(target=measure_thread, daemon=True).start()
    
    def on_continuous_toggle(self):
        """Handle continuous measurement toggle."""
        if self.continuous_measurement_active:
            # Stop continuous measurement
            if self.simulation_mode:
                # Stop simulation thread
                self.continuous_measurement_active = False
                if self.simulation_stop_event:
                    self.simulation_stop_event.set()
                if self.simulation_thread:
                    self.simulation_thread.join(timeout=1.0)
            else:
                self.controller.stop_continuous_measurement()
            self.continuous_measurement_active = False
            self.continuous_btn.config(text="▶ Start Continuous")
            self._log_status("Continuous measurement stopped")
        else:
            # Start continuous measurement
            try:
                interval_ms = int(self.interval_var.get())
                if interval_ms < 10:
                    messagebox.showerror("Error", "Interval must be at least 10 ms")
                    return
            except ValueError:
                messagebox.showerror("Error", "Invalid interval value")
                return
            
            # Set flag BEFORE starting thread so thread sees it
            self.continuous_measurement_active = True
            
            if self.simulation_mode:
                self._start_simulation_continuous(interval_ms)
            else:
                # Start continuous measurement with spectrum download enabled by default
                self.controller.start_continuous_measurement(interval_ms, include_spectrum=True)
            
            self.continuous_btn.config(text="⏸ Stop Continuous")
            self._log_status(f"Continuous measurement started (interval: {interval_ms} ms, with spectrum)")
    
    def _start_simulation_continuous(self, interval_ms: int):
        """Start continuous simulation measurements."""
        # Use a stop event for better control
        self.simulation_stop_event = threading.Event()
        
        def simulation_loop():
            measurement_count = 0
            while self.continuous_measurement_active and not self.simulation_stop_event.is_set():
                try:
                    data = self.simulator.simulate_measurement()
                    result = {
                        'thickness': data.thickness,
                        'peak1': data.peak1,
                        'peak2': data.peak2,
                        'spectrum': data.spectrum,  # Include spectrum by default
                        'timestamp': data.timestamp
                    }
                    self.data_queue.put(result)
                    measurement_count += 1
                    
                    # Debug: print every 10th measurement
                    if measurement_count % 10 == 0:
                        print(f"Simulation: {measurement_count} measurements sent, active={self.continuous_measurement_active}")
                    
                except Exception as e:
                    print(f"Error in simulation loop: {e}")
                    import traceback
                    traceback.print_exc()
                
                # Wait for interval or stop event (whichever comes first)
                if self.simulation_stop_event.wait(interval_ms / 1000.0):
                    # Event was set, stop
                    break
            
            print(f"Simulation loop ended. Active={self.continuous_measurement_active}, count={measurement_count}")
        
        self.simulation_thread = threading.Thread(target=simulation_loop, daemon=True)
        self.simulation_thread.start()
        print(f"Simulation thread started, interval={interval_ms}ms")
    
    def on_download_spectrum(self):
        """Handle download spectrum button click."""
        def download_thread():
            if self.simulation_mode:
                data = self.simulator.simulate_measurement()
                result = {
                    'spectrum': data.spectrum,
                    'peak1': data.peak1,
                    'peak2': data.peak2,
                    'timestamp': data.timestamp
                }
            else:
                result = self.controller.download_spectrum()
            
            self.root.after(0, lambda: self._handle_spectrum_data(result))
        
        threading.Thread(target=download_thread, daemon=True).start()
    
    def _handle_spectrum_data(self, data: dict):
        """Handle spectrum data."""
        if 'error' in data:
            messagebox.showerror("Error", data['error'])
            self._log_status(f"Spectrum download error: {data['error']}")
            return
        
        spectrum = data.get('spectrum')
        if spectrum is not None:
            # Improved peak detection: find two distinct peaks
            peak1_pos, peak2_pos = self._detect_peaks(spectrum)
            
            self.plotter.update_raw_data_plot(spectrum, peak1_pos, peak2_pos)
            self._log_status("Spectrum downloaded and displayed")
        else:
            self._log_status("No spectrum data received")
    
    def _detect_peaks(self, spectrum: np.ndarray) -> tuple:
        """
        Detect two peaks in the interferometric spectrum.
        Uses a robust algorithm to find two distinct peaks with proper separation.
        
        Args:
            spectrum: Spectrum data array
            
        Returns:
            Tuple of (peak1_pos, peak2_pos) or (None, None) if not found
        """
        if len(spectrum) < 10:
            return None, None
        
        # Convert to float for processing
        spectrum_float = spectrum.astype(np.float64)
        
        # Apply stronger smoothing to reduce noise (larger window)
        window_size = 7
        if len(spectrum_float) > window_size:
            kernel = np.ones(window_size) / window_size
            spectrum_smooth = np.convolve(spectrum_float, kernel, mode='same')
        else:
            spectrum_smooth = spectrum_float
        
        # Calculate baseline (median of lower 20% of values)
        sorted_vals = np.sort(spectrum_smooth)
        baseline = np.median(sorted_vals[:len(sorted_vals)//5])
        max_val = np.max(spectrum_smooth)
        
        # Dynamic threshold: at least 20% above baseline and 15% of max
        threshold = max(baseline * 1.2, max_val * 0.15)
        
        # Find all local maxima (peaks) with sufficient height
        peaks = []
        for i in range(2, len(spectrum_smooth) - 2):
            # Check if it's a local maximum (higher than neighbors on both sides)
            if (spectrum_smooth[i] > spectrum_smooth[i-1] and 
                spectrum_smooth[i] > spectrum_smooth[i+1] and
                spectrum_smooth[i] > spectrum_smooth[i-2] and
                spectrum_smooth[i] > spectrum_smooth[i+2]):
                
                # Check if it's significantly above threshold
                if spectrum_smooth[i] > threshold:
                    peaks.append((i, spectrum_smooth[i]))
        
        if len(peaks) == 0:
            # No peaks found, use simple max approach
            max_idx = np.argmax(spectrum_smooth)
            estimated_separation = 300
            if max_idx < len(spectrum) / 2:
                peak2 = max_idx + estimated_separation
            else:
                peak2 = max_idx - estimated_separation
            peak2 = np.clip(peak2, 0, len(spectrum) - 1)
            return int(max_idx), int(peak2)
        
        # Sort peaks by intensity (highest first)
        peaks.sort(key=lambda x: x[1], reverse=True)
        
        # Find two peaks with good separation
        min_separation = 80  # Increased minimum separation for better detection
        max_separation = 600  # Maximum reasonable separation
        
        # Strategy 1: Find two highest peaks with good separation
        for i in range(min(15, len(peaks))):  # Check top 15 peaks
            for j in range(i+1, min(15, len(peaks))):
                pos1, intensity1 = peaks[i]
                pos2, intensity2 = peaks[j]
                separation = abs(pos1 - pos2)
                
                if min_separation <= separation <= max_separation:
                    # Found two well-separated peaks
                    if pos1 < pos2:
                        return int(pos1), int(pos2)
                    else:
                        return int(pos2), int(pos1)
        
        # Strategy 2: If no well-separated peaks found, use two highest with any separation
        if len(peaks) >= 2:
            pos1, _ = peaks[0]
            pos2, _ = peaks[1]
            # Ensure they're in order
            if pos1 < pos2:
                return int(pos1), int(pos2)
            else:
                return int(pos2), int(pos1)
        
        # Strategy 3: Only one peak found, estimate second peak
        if len(peaks) == 1:
            peak1 = peaks[0][0]
            # Try to find second peak in the other half of spectrum
            if peak1 < len(spectrum) / 2:
                # First peak in left half, look for second in right half
                search_start = len(spectrum) // 2
                search_end = len(spectrum)
            else:
                # First peak in right half, look for second in left half
                search_start = 0
                search_end = len(spectrum) // 2
            
            # Find maximum in the other half
            search_region = spectrum_smooth[search_start:search_end]
            if len(search_region) > 0:
                local_max_idx = np.argmax(search_region)
                peak2 = search_start + local_max_idx
                
                # Verify it's a reasonable peak
                if spectrum_smooth[peak2] > threshold:
                    if peak1 < peak2:
                        return int(peak1), int(peak2)
                    else:
                        return int(peak2), int(peak1)
            
            # Fallback: estimate second peak position
            estimated_separation = 300
            if peak1 < len(spectrum) / 2:
                peak2 = peak1 + estimated_separation
            else:
                peak2 = peak1 - estimated_separation
            peak2 = np.clip(peak2, 0, len(spectrum) - 1)
            return int(peak1), int(peak2)
        
        return None, None
    
    def on_set_refractive_index(self):
        """Handle set refractive index button click."""
        try:
            n = float(self.refractive_index_var.get())
            if n < 1.0 or n > 3.0:
                messagebox.showerror("Error", "Refractive index should be between 1.0 and 3.0")
                return
            
            if self.simulation_mode:
                # Update simulator with new refractive index
                self.simulator.set_refractive_index(n)
                self._log_status(f"Simulation mode: Refractive index set to {n} (affects thickness calculation)")
                messagebox.showinfo("Refractive Index", 
                                  f"Refractive index set to {n} in simulation mode.\n\n"
                                  f"Note: Changing refractive index affects the thickness calculation.\n"
                                  f"Higher n values will result in different measured thicknesses.")
                return
            
            def set_thread():
                success, message = self.controller.set_refractive_index(n)
                self.root.after(0, lambda: self._on_refractive_index_set(success, message))
            
            threading.Thread(target=set_thread, daemon=True).start()
            
        except ValueError:
            messagebox.showerror("Error", "Invalid refractive index value")
    
    def _on_refractive_index_set(self, success: bool, message: str):
        """Handle refractive index setting completion."""
        if success:
            self._log_status(message)
        else:
            messagebox.showerror("Error", message)
            self._log_status(f"Failed to set refractive index: {message}")
    
    def on_dark_reference(self):
        """Handle dark reference button click."""
        if self.simulation_mode:
            # Simulate dark reference with progress
            self._simulate_dark_reference()
            return
        
        # Disable button and show progress
        self.dark_ref_btn.config(state=tk.DISABLED, text="Performing...")
        self._log_status("Starting dark reference measurement...")
        
        # Create progress window
        progress_window = tk.Toplevel(self.root)
        progress_window.title("Dark Reference")
        progress_window.geometry("400x150")
        progress_window.transient(self.root)
        progress_window.grab_set()
        
        # Center the window
        progress_window.update_idletasks()
        x = (progress_window.winfo_screenwidth() // 2) - (progress_window.winfo_width() // 2)
        y = (progress_window.winfo_screenheight() // 2) - (progress_window.winfo_height() // 2)
        progress_window.geometry(f"+{x}+{y}")
        
        info_label = ttk.Label(progress_window, text="Performing dark reference measurement...", 
                              font=("Arial", 10))
        info_label.pack(pady=10)
        
        status_label = ttk.Label(progress_window, text="This may take 5-30 seconds", 
                                font=("Arial", 9), foreground="gray")
        status_label.pack(pady=5)
        
        # Progress bar
        progress_var = tk.DoubleVar()
        progress_bar = ttk.Progressbar(progress_window, variable=progress_var, 
                                       maximum=100, length=300, mode='indeterminate')
        progress_bar.pack(pady=10)
        progress_bar.start(10)
        
        time_label = ttk.Label(progress_window, text="", font=("Arial", 9))
        time_label.pack(pady=5)
        
        start_time = time.time()
        
        def update_progress():
            elapsed = time.time() - start_time
            time_label.config(text=f"Elapsed time: {elapsed:.1f} seconds")
            progress_window.after(100, update_progress)
        
        update_progress()
        
        def dark_ref_thread():
            try:
                success, message = self.controller.perform_dark_reference()
                elapsed_time = time.time() - start_time
                self.root.after(0, lambda: self._on_dark_reference_complete(
                    success, message, progress_window, elapsed_time))
            except Exception as e:
                elapsed_time = time.time() - start_time
                self.root.after(0, lambda: self._on_dark_reference_complete(
                    False, f"Error: {str(e)}", progress_window, elapsed_time))
        
        threading.Thread(target=dark_ref_thread, daemon=True).start()
    
    def _simulate_dark_reference(self):
        """Simulate dark reference with progress feedback."""
        # Create progress window
        progress_window = tk.Toplevel(self.root)
        progress_window.title("Dark Reference (Simulation)")
        progress_window.geometry("400x150")
        progress_window.transient(self.root)
        progress_window.grab_set()
        
        # Center the window
        progress_window.update_idletasks()
        x = (progress_window.winfo_screenwidth() // 2) - (progress_window.winfo_width() // 2)
        y = (progress_window.winfo_screenheight() // 2) - (progress_window.winfo_height() // 2)
        progress_window.geometry(f"+{x}+{y}")
        
        info_label = ttk.Label(progress_window, text="Simulating dark reference...", 
                              font=("Arial", 10))
        info_label.pack(pady=10)
        
        status_label = ttk.Label(progress_window, text="Simulation mode", 
                                font=("Arial", 9), foreground="gray")
        status_label.pack(pady=5)
        
        progress_var = tk.DoubleVar()
        progress_bar = ttk.Progressbar(progress_window, variable=progress_var, 
                                       maximum=100, length=300, mode='determinate')
        progress_bar.pack(pady=10)
        
        time_label = ttk.Label(progress_window, text="", font=("Arial", 9))
        time_label.pack(pady=5)
        
        start_time = time.time()
        duration = 3.0  # Simulate 3 seconds
        
        def simulate_progress():
            elapsed = time.time() - start_time
            progress = min(100, (elapsed / duration) * 100)
            progress_var.set(progress)
            time_label.config(text=f"Elapsed: {elapsed:.1f}s / Estimated: {duration:.1f}s")
            
            if elapsed < duration:
                progress_window.after(50, simulate_progress)
            else:
                progress_window.destroy()
                self._log_status("Dark reference completed (simulation)")
                messagebox.showinfo("Dark Reference", 
                                  f"Dark reference completed successfully (simulation)\n"
                                  f"Duration: {elapsed:.1f} seconds")
        
        simulate_progress()
    
    def _on_dark_reference_complete(self, success: bool, message: str, 
                                    progress_window: tk.Toplevel, elapsed_time: float):
        """Handle dark reference completion."""
        # Close progress window
        progress_window.destroy()
        
        # Re-enable button
        self.dark_ref_btn.config(state=tk.NORMAL, text="Dark Reference")
        
        if success:
            full_message = f"{message}\nDuration: {elapsed_time:.1f} seconds"
            self._log_status(f"Dark reference completed in {elapsed_time:.1f}s: {message}")
            messagebox.showinfo("Dark Reference", full_message)
        else:
            self._log_status(f"Dark reference failed after {elapsed_time:.1f}s: {message}")
            messagebox.showerror("Dark Reference Error", 
                               f"{message}\nDuration: {elapsed_time:.1f} seconds")
    
    def on_measuring_mode_change(self, event=None):
        """Handle measuring mode change."""
        # Mode is set when Apply Settings is clicked
        pass
    
    def on_apply_settings(self):
        """Handle apply settings button click."""
        if self.simulation_mode:
            self._log_status("Simulation mode: Settings would be applied")
            return
        
        def apply_thread():
            errors = []
            
            # Measuring rate
            try:
                rate = int(self.measuring_rate_var.get())
                if rate < 1 or rate > 70000:
                    errors.append("Measuring rate must be between 1 and 70000 Hz")
                else:
                    success, msg = self.controller.set_measuring_rate(rate)
                    if not success:
                        errors.append(f"Rate: {msg}")
            except ValueError:
                errors.append("Invalid measuring rate value")
            
            # Measuring mode
            mode_str = self.measuring_mode_var.get()
            mode = 0 if mode_str == "Chromatic Confocal" else 1
            success, msg = self.controller.set_measuring_mode(mode)
            if not success:
                errors.append(f"Mode: {msg}")
            
            # Lamp intensity
            try:
                intensity = int(self.lamp_intensity_var.get())
                if intensity < 0 or intensity > 100:
                    errors.append("Lamp intensity must be between 0 and 100%")
                else:
                    success, msg = self.controller.set_lamp_intensity(intensity)
                    if not success:
                        errors.append(f"Lamp: {msg}")
            except ValueError:
                errors.append("Invalid lamp intensity value")
            
            # Averaging
            try:
                data_avg = int(self.data_avg_var.get())
                spectrum_avg = int(self.spectrum_avg_var.get())
                if data_avg < 1 or data_avg > 1000:
                    errors.append("Data average must be between 1 and 1000")
                elif spectrum_avg < 1 or spectrum_avg > 1000:
                    errors.append("Spectrum average must be between 1 and 1000")
                else:
                    success, msg = self.controller.set_averaging(data_avg, spectrum_avg)
                    if not success:
                        errors.append(f"Averaging: {msg}")
            except ValueError:
                errors.append("Invalid averaging values")
            
            self.root.after(0, lambda: self._on_settings_applied(errors))
        
        threading.Thread(target=apply_thread, daemon=True).start()
    
    def _on_settings_applied(self, errors: list):
        """Handle settings application completion."""
        if errors:
            messagebox.showerror("Settings Error", "\n".join(errors))
            self._log_status(f"Settings application errors: {', '.join(errors)}")
        else:
            self._log_status("Device settings applied successfully")
            messagebox.showinfo("Success", "Device settings applied successfully")
    
    def on_view_config(self):
        """Handle view configuration button click."""
        if self.simulation_mode:
            messagebox.showinfo("Configuration", "Simulation mode: No device configuration available")
            return
        
        def get_config_thread():
            config = self.controller.get_configuration()
            self.root.after(0, lambda: self._display_config(config))
        
        threading.Thread(target=get_config_thread, daemon=True).start()
    
    def _display_config(self, config: Optional[dict]):
        """Display device configuration in a dialog."""
        if config is None or 'error' in config:
            messagebox.showerror("Error", config.get('error', 'Failed to get configuration') if config else 'No configuration available')
            return
        
        # Create configuration display window
        config_window = tk.Toplevel(self.root)
        config_window.title("Device Configuration")
        config_window.geometry("500x400")
        
        # Create text widget with scrollbar
        frame = ttk.Frame(config_window, padding="10")
        frame.pack(fill=tk.BOTH, expand=True)
        
        text_widget = tk.Text(frame, wrap=tk.WORD, state=tk.DISABLED)
        scrollbar = ttk.Scrollbar(frame, orient=tk.VERTICAL, command=text_widget.yview)
        text_widget.configure(yscrollcommand=scrollbar.set)
        
        text_widget.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Format configuration
        text_widget.config(state=tk.NORMAL)
        text_widget.insert(tk.END, "Device Configuration\n")
        text_widget.insert(tk.END, "=" * 50 + "\n\n")
        
        for key, value in config.items():
            text_widget.insert(tk.END, f"{key.replace('_', ' ').title()}: {value}\n")
        
        text_widget.config(state=tk.DISABLED)
        
        # Close button
        ttk.Button(config_window, text="Close", command=config_window.destroy).pack(pady=10)
    
    def on_clear_plots(self):
        """Handle clear plots button click."""
        self.plotter.clear_plots()
        self.measurement_count = 0
        self.count_label.config(text="0")
        self._log_status("Plots cleared")
    
    def on_export_data(self):
        """Handle export data button click."""
        filename = filedialog.asksaveasfilename(
            defaultextension=".csv",
            filetypes=[("CSV files", "*.csv"), ("All files", "*.*")]
        )
        
        if filename:
            try:
                self.plotter.export_data(filename)
                self._log_status(f"Data exported to {filename}")
                messagebox.showinfo("Success", f"Data exported to {filename}")
            except Exception as e:
                messagebox.showerror("Error", f"Failed to export data: {str(e)}")
                self._log_status(f"Export error: {str(e)}")
    
    def _init_beckhoff_interface(self):
        """Initialize Beckhoff PLC interface (ADS only)."""
        if not PYADS_AVAILABLE or not BeckhoffADSInterface:
            self._log_status("pyads library not available. Install with: pip install pyads")
            return
        
        try:
            # Load ADS settings
            ads_settings = self.settings_manager.get_ads_settings()
            ams_netid = ads_settings.get('ams_netid', '127.0.0.1.1.1')
            port = ads_settings.get('port')
            symbol_prefix = ads_settings.get('symbol_prefix', 'GVL_CHRocodile.')
            write_timeout = ads_settings.get('write_timeout', 1.0)  # Default 1.0s for slow networks
            
            # Check if AMS NetID is changing - if so, ensure old connection is fully closed
            old_ams_netid = getattr(self, 'beckhoff_ams_netid', None)
            if old_ams_netid and old_ams_netid != ams_netid:
                # AMS NetID is changing - ensure old connection is fully closed
                if self.beckhoff_ads_interface:
                    if self.beckhoff_ads_interface.is_running():
                        self.beckhoff_ads_interface.stop_polling()
                    # Force disconnect to ensure port is released
                    self.beckhoff_ads_interface.disconnect()
                    # Additional delay to ensure port is released when switching connections
                    import time
                    time.sleep(0.3)
                    if self.log_callback:
                        self.log_callback("connection", f"Switching from {old_ams_netid} to {ams_netid} - old connection closed", {})
            
            # Update AMS NetID
            self.beckhoff_ams_netid = ams_netid
            
            # Get variable names from settings
            variables = ads_settings.get('variables', {})
            
            # Create combined log callback (monitor + file logging)
            # Store as instance variable so it can be reused when monitor window opens/closes
            def create_combined_log_callback():
                def combined_log_callback(event_type: str, message: str, data: dict = None):
                    """Log callback that writes to both monitor and file."""
                    # Write to monitor if open
                    if self.beckhoff_monitor_window:
                        try:
                            if self.beckhoff_monitor_window.window.winfo_exists():
                                self.beckhoff_monitor_window.log_event(event_type, message, data)
                        except Exception:
                            pass  # Monitor window might be closed
                    
                    # Write to log file
                    if hasattr(self, 'logger'):
                        # Format log message with event type
                        log_level = {
                            'error': logging.ERROR,
                            'connection': logging.INFO,
                            'read': logging.DEBUG,
                            'write': logging.DEBUG,
                            'trigger': logging.INFO,
                            'handshake': logging.INFO,
                        }.get(event_type.lower(), logging.INFO)
                        
                        log_message = f"[ADS {event_type.upper()}] {message}"
                        if data:
                            # Add relevant data to log message
                            relevant_data = {k: v for k, v in data.items() 
                                           if k not in ['type'] and v is not None}
                            if relevant_data:
                                log_message += f" | Data: {relevant_data}"
                        
                        self.logger.log(log_level, log_message)
                return combined_log_callback
            
            # Store the callback creator function
            self._create_combined_log_callback = create_combined_log_callback
            combined_log_callback = create_combined_log_callback()
            
            self.beckhoff_ads_interface = BeckhoffADSInterface(
                ams_netid=ams_netid,
                port=port if port is not None else (pyads.PORT_TC3PLC1 if pyads else None),
                callback=self._beckhoff_command_callback,
                symbol_prefix=symbol_prefix,
                log_callback=combined_log_callback,
                write_timeout=write_timeout
            )
            
            # Update monitor window if it exists
            if self.beckhoff_monitor_window:
                self.beckhoff_monitor_window.ads_interface = self.beckhoff_ads_interface
                # Update callback to use combined callback
                self.beckhoff_ads_interface.log_callback = combined_log_callback
            
            # Override variable names if custom ones are provided
            if variables:
                if 'trigger' in variables:
                    self.beckhoff_ads_interface.var_trigger = f"{symbol_prefix}{variables['trigger']}"
                if 'start_continuous' in variables:
                    self.beckhoff_ads_interface.var_start_cont = f"{symbol_prefix}{variables['start_continuous']}"
                if 'stop_continuous' in variables:
                    self.beckhoff_ads_interface.var_stop_cont = f"{symbol_prefix}{variables['stop_continuous']}"
                if 'interval' in variables:
                    self.beckhoff_ads_interface.var_interval = f"{symbol_prefix}{variables['interval']}"
                if 'busy' in variables:
                    self.beckhoff_ads_interface.var_busy = f"{symbol_prefix}{variables['busy']}"
                if 'ready' in variables:
                    self.beckhoff_ads_interface.var_ready = f"{symbol_prefix}{variables['ready']}"
                if 'ack' in variables:
                    self.beckhoff_ads_interface.var_ack = f"{symbol_prefix}{variables['ack']}"
                if 'thickness' in variables:
                    self.beckhoff_ads_interface.var_thickness = f"{symbol_prefix}{variables['thickness']}"
                if 'peak1' in variables:
                    self.beckhoff_ads_interface.var_peak1 = f"{symbol_prefix}{variables['peak1']}"
                if 'peak2' in variables:
                    self.beckhoff_ads_interface.var_peak2 = f"{symbol_prefix}{variables['peak2']}"
                if 'count' in variables:
                    self.beckhoff_ads_interface.var_count = f"{symbol_prefix}{variables['count']}"
                if 'error' in variables:
                    self.beckhoff_ads_interface.var_error = f"{symbol_prefix}{variables['error']}"
            
            # Get poll interval from settings
            poll_interval = ads_settings.get('poll_interval', 0.1)
            
            if self.beckhoff_enabled:
                if self.beckhoff_ads_interface.start_polling(poll_interval=poll_interval):
                    self._log_status(f"Beckhoff ADS interface started (AMS: {self.beckhoff_ams_netid})")
                    if hasattr(self, 'logger'):
                        port_display = port if port is not None else (pyads.PORT_TC3PLC1 if pyads else None)
                        self.logger.info(f"Beckhoff ADS interface started: AMS={ams_netid}, Port={port_display}, PollInterval={poll_interval}s")
                else:
                    self._log_status("Failed to start Beckhoff ADS interface")
                    if hasattr(self, 'logger'):
                        last_error = self.beckhoff_ads_interface.get_last_error() if self.beckhoff_ads_interface else None
                        self.logger.error(f"Failed to start Beckhoff ADS interface: {last_error}")
        except Exception as e:
            self._log_status(f"Error initializing Beckhoff ADS interface: {e}")
            if hasattr(self, 'logger'):
                self.logger.error(f"Error initializing Beckhoff ADS interface: {e}", exc_info=True)
    
    def _beckhoff_command_callback(self, command: str, **kwargs) -> dict:
        """
        Callback function for Beckhoff PLC commands.
        
        Args:
            command: Command string ('trigger_measurement', 'get_status', etc.)
            **kwargs: Additional arguments (e.g., interval_ms for start_continuous)
            
        Returns:
            Dictionary with command result
        """
        if command == "trigger_measurement":
            # Trigger a single measurement from PLC
            self.root.after(0, self.on_single_measurement)
            return {
                "triggered": True,
                "message": "Measurement triggered"
            }
        
        elif command == "get_status":
            # Return current application status
            return {
                "connected": self.controller.is_connected(),
                "simulation_mode": self.simulation_mode,
                "continuous_active": self.continuous_measurement_active,
                "measurement_count": self.measurement_count
            }
        
        elif command == "start_continuous":
            # Start continuous measurement
            interval_ms = kwargs.get('interval_ms', 100)
            self.interval_var.set(str(interval_ms))
            self.root.after(0, self.on_continuous_toggle)
            return {
                "started": True,
                "interval_ms": interval_ms
            }
        
        elif command == "stop_continuous":
            # Stop continuous measurement
            if self.continuous_measurement_active:
                self.root.after(0, self.on_continuous_toggle)
            return {
                "stopped": True
            }
        
        else:
            return {
                "error": f"Unknown command: {command}"
            }
    
    def on_beckhoff_toggle(self):
        """Handle Beckhoff ADS interface enable/disable toggle."""
        if not PYADS_AVAILABLE:
            error_msg = (
                "pyads library is required for Beckhoff integration.\n\n"
                "Install pyads: pip install pyads\n\n"
                "Note: On Windows, pyads also requires TwinCAT ADS DLL (TcAdsDll.dll).\n"
                "This DLL is installed with TwinCAT. If TwinCAT is not installed on this PC,\n"
                "you may need to install TwinCAT or copy the DLL to a location in PATH."
            )
            messagebox.showwarning("pyads Required", error_msg)
            if hasattr(self, 'beckhoff_check'):
                self.beckhoff_check.state(['!selected'])
            return
        
        self.beckhoff_enabled = not self.beckhoff_enabled
        
        if self.beckhoff_enabled:
            # Enable ADS interface
            if not self.beckhoff_ads_interface:
                self._init_beckhoff_interface()
            
            if self.beckhoff_ads_interface and not self.beckhoff_ads_interface.is_running():
                # Get poll interval from settings
                ads_settings = self.settings_manager.get_ads_settings()
                poll_interval = ads_settings.get('poll_interval', 0.1)
                
                if self.beckhoff_ads_interface.start_polling(poll_interval=poll_interval):
                    self._log_status(f"Beckhoff ADS interface enabled (AMS: {self.beckhoff_ams_netid})")
                    self.beckhoff_status_label.config(text=f"Status: On ({self.beckhoff_ams_netid})", foreground="green")
                    if hasattr(self, 'logger'):
                        self.logger.info(f"Beckhoff ADS interface enabled: AMS={self.beckhoff_ams_netid}")
                else:
                    self.beckhoff_enabled = False
                    if hasattr(self, 'beckhoff_check'):
                        self.beckhoff_check.state(['!selected'])
                    
                    # Get detailed error message
                    error_msg = "Failed to start Beckhoff ADS interface"
                    last_error = self.beckhoff_ads_interface.get_last_error() if self.beckhoff_ads_interface else None
                    if last_error:
                        error_msg += f": {last_error}"
                        # Show error in messagebox
                        messagebox.showerror(
                            "ADS Interface Error",
                            f"Failed to start Beckhoff ADS interface.\n\n{last_error}"
                        )
                    
                    self._log_status(error_msg)
                    self.beckhoff_status_label.config(text="Status: Error - See log", foreground="red")
                    if hasattr(self, 'logger'):
                        self.logger.error(f"Failed to start Beckhoff ADS interface: {last_error}")
            elif self.beckhoff_ads_interface and self.beckhoff_ads_interface.is_running():
                self.beckhoff_status_label.config(text=f"Status: On ({self.beckhoff_ams_netid})", foreground="green")
        else:
            # Disable ADS interface
            if self.beckhoff_ads_interface:
                self.beckhoff_ads_interface.stop_polling()
                self._log_status("Beckhoff ADS interface disabled")
                if hasattr(self, 'logger'):
                    self.logger.info("Beckhoff ADS interface disabled")
            self.beckhoff_status_label.config(text="Status: Off", foreground="gray")
    
    def on_beckhoff_settings(self):
        """Open Beckhoff ADS settings dialog."""
        # Allow settings configuration even if pyads DLL is missing
        # This allows users to configure settings before installing TwinCAT
        if not PYADS_AVAILABLE:
            # Show info message but still allow configuration
            response = messagebox.askyesno(
                "Settings Available",
                "pyads DLL (TcAdsDll.dll) is not available.\n\n"
                "You can still configure ADS settings, but the interface will not work until TwinCAT Runtime is installed.\n\n"
                "Continue to settings?",
                icon='question'
            )
            if not response:
                return
        
        # Create settings dialog
        settings_window = tk.Toplevel(self.root)
        settings_window.title("Beckhoff ADS Settings")
        settings_window.geometry("600x700")
        settings_window.transient(self.root)
        settings_window.grab_set()
        
        # Main frame with padding
        main_frame = ttk.Frame(settings_window, padding="15")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Load current settings
        ads_settings = self.settings_manager.get_ads_settings()
        variables = ads_settings.get('variables', {})
        
        # Variables to store input values
        ams_netid_var = tk.StringVar(value=ads_settings.get('ams_netid', '127.0.0.1.1.1'))
        port_var = tk.StringVar(value=str(ads_settings.get('port', '')) if ads_settings.get('port') else '')
        symbol_prefix_var = tk.StringVar(value=ads_settings.get('symbol_prefix', 'GVL_CHRocodile.'))
        poll_interval_var = tk.StringVar(value=str(ads_settings.get('poll_interval', 0.1)))
        write_timeout_var = tk.StringVar(value=str(ads_settings.get('write_timeout', 1.0)))
        
        # Variable name variables
        var_vars = {}
        for var_name in ['trigger', 'start_continuous', 'stop_continuous', 'interval',
                        'busy', 'ready', 'ack', 'thickness', 'peak1', 'peak2', 'count', 'error']:
            var_vars[var_name] = tk.StringVar(value=variables.get(var_name, ''))
        
        # Connection settings section
        conn_frame = ttk.LabelFrame(main_frame, text="Connection Settings", padding="10")
        conn_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Label(conn_frame, text="AMS NetID (IP Address):").grid(row=0, column=0, sticky=tk.W, pady=5)
        ams_entry = ttk.Entry(conn_frame, textvariable=ams_netid_var, width=30)
        ams_entry.grid(row=0, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=5)
        ttk.Label(conn_frame, text="(e.g., 127.0.0.1.1.1 for localhost)", 
                 font=("Arial", 8), foreground="gray").grid(row=0, column=2, sticky=tk.W, padx=(5, 0))
        
        ttk.Label(conn_frame, text="ADS Port:").grid(row=1, column=0, sticky=tk.W, pady=5)
        port_entry = ttk.Entry(conn_frame, textvariable=port_var, width=30)
        port_entry.grid(row=1, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=5)
        ttk.Label(conn_frame, text="(Leave empty for default: 851)", 
                 font=("Arial", 8), foreground="gray").grid(row=1, column=2, sticky=tk.W, padx=(5, 0))
        
        ttk.Label(conn_frame, text="Symbol Prefix:").grid(row=2, column=0, sticky=tk.W, pady=5)
        prefix_entry = ttk.Entry(conn_frame, textvariable=symbol_prefix_var, width=30)
        prefix_entry.grid(row=2, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=5)
        ttk.Label(conn_frame, text="(e.g., GVL_CHRocodile.)", 
                 font=("Arial", 8), foreground="gray").grid(row=2, column=2, sticky=tk.W, padx=(5, 0))
        
        ttk.Label(conn_frame, text="Poll Interval (seconds):").grid(row=3, column=0, sticky=tk.W, pady=5)
        poll_entry = ttk.Entry(conn_frame, textvariable=poll_interval_var, width=30)
        poll_entry.grid(row=3, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=5)
        ttk.Label(conn_frame, text="(Default: 0.1 = 100ms)", 
                 font=("Arial", 8), foreground="gray").grid(row=3, column=2, sticky=tk.W, padx=(5, 0))
        
        ttk.Label(conn_frame, text="Write Timeout (seconds):").grid(row=4, column=0, sticky=tk.W, pady=5)
        write_timeout_entry = ttk.Entry(conn_frame, textvariable=write_timeout_var, width=30)
        write_timeout_entry.grid(row=4, column=1, sticky=(tk.W, tk.E), padx=(10, 0), pady=5)
        ttk.Label(conn_frame, text="(Default: 1.0s, increase for slow networks)", 
                 font=("Arial", 8), foreground="gray").grid(row=4, column=2, sticky=tk.W, padx=(5, 0))
        
        conn_frame.columnconfigure(1, weight=1)
        
        # Variable names section
        vars_frame = ttk.LabelFrame(main_frame, text="PLC Variable Names", padding="10")
        vars_frame.pack(fill=tk.BOTH, expand=True, pady=(0, 10))
        
        # Create scrollable frame for variables
        canvas = tk.Canvas(vars_frame, height=300)
        scrollbar = ttk.Scrollbar(vars_frame, orient=tk.VERTICAL, command=canvas.yview)
        scrollable_frame = ttk.Frame(canvas)
        
        scrollable_frame.bind(
            "<Configure>",
            lambda e: canvas.configure(scrollregion=canvas.bbox("all"))
        )
        
        canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        
        # Variable name entries
        row = 0
        var_labels = {
            'trigger': 'Trigger Measurement',
            'start_continuous': 'Start Continuous',
            'stop_continuous': 'Stop Continuous',
            'interval': 'Interval (ms)',
            'busy': 'Measurement Busy',
            'ready': 'Measurement Ready',
            'ack': 'Acknowledgment',
            'thickness': 'Thickness Result',
            'peak1': 'Peak 1 Result',
            'peak2': 'Peak 2 Result',
            'count': 'Measurement Count',
            'error': 'Error Message'
        }
        
        for var_name in ['trigger', 'start_continuous', 'stop_continuous', 'interval',
                        'busy', 'ready', 'ack', 'thickness', 'peak1', 'peak2', 'count', 'error']:
            ttk.Label(scrollable_frame, text=var_labels.get(var_name, var_name) + ":").grid(
                row=row, column=0, sticky=tk.W, padx=(0, 10), pady=3)
            entry = ttk.Entry(scrollable_frame, textvariable=var_vars[var_name], width=25)
            entry.grid(row=row, column=1, sticky=(tk.W, tk.E), padx=(0, 10), pady=3)
            row += 1
        
        scrollable_frame.columnconfigure(1, weight=1)
        canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Buttons
        button_frame = ttk.Frame(main_frame)
        button_frame.pack(fill=tk.X, pady=(10, 0))
        
        def save_settings():
            """Save settings and close dialog."""
            try:
                # Validate inputs
                ams_netid = ams_netid_var.get().strip()
                if not ams_netid:
                    messagebox.showerror("Error", "AMS NetID cannot be empty.")
                    return
                
                port_str = port_var.get().strip()
                port = int(port_str) if port_str else None
                
                symbol_prefix = symbol_prefix_var.get().strip()
                if not symbol_prefix:
                    messagebox.showerror("Error", "Symbol prefix cannot be empty.")
                    return
                
                try:
                    poll_interval = float(poll_interval_var.get())
                    if poll_interval <= 0:
                        raise ValueError("Poll interval must be positive")
                except ValueError:
                    messagebox.showerror("Error", "Poll interval must be a positive number.")
                    return
                
                try:
                    write_timeout = float(write_timeout_var.get())
                    if write_timeout <= 0:
                        raise ValueError("Write timeout must be positive")
                except ValueError:
                    messagebox.showerror("Error", "Write timeout must be a positive number.")
                    return
                
                # Collect variable names
                variables_dict = {}
                for var_name in var_vars:
                    var_value = var_vars[var_name].get().strip()
                    if var_value:
                        variables_dict[var_name] = var_value
                
                # Save settings
                new_ads_settings = {
                    'ams_netid': ams_netid,
                    'port': port,
                    'symbol_prefix': symbol_prefix,
                    'poll_interval': poll_interval,
                    'write_timeout': write_timeout,
                    'variables': variables_dict
                }
                
                self.settings_manager.set_ads_settings(new_ads_settings)
                self.settings_manager.save()
                
                # Update current AMS NetID
                self.beckhoff_ams_netid = ams_netid
                
                # If interface is running, update settings without full restart if possible
                was_running = False
                if self.beckhoff_ads_interface and self.beckhoff_ads_interface.is_running():
                    was_running = True
                    # Stop polling but keep connection and state
                    self.beckhoff_ads_interface.stop_polling()
                    # Note: stop_polling() disconnects, so we need to reconnect
                    # Add delay to ensure port is fully released before reconnecting
                    import time
                    time.sleep(0.3)
                
                # Reinitialize interface with new settings (creates new instance)
                # This resets state, but start_polling will read current trigger state
                self._init_beckhoff_interface()
                
                if was_running and self.beckhoff_ads_interface:
                    ads_settings = self.settings_manager.get_ads_settings()
                    poll_interval = ads_settings.get('poll_interval', 0.1)
                    # start_polling will read current trigger state to avoid false trigger
                    self.beckhoff_ads_interface.start_polling(poll_interval=poll_interval)
                    self.beckhoff_status_label.config(text=f"Status: On ({ams_netid})", foreground="green")
                
                self._log_status("Beckhoff ADS settings saved and applied")
                settings_window.destroy()
                
            except Exception as e:
                messagebox.showerror("Error", f"Failed to save settings: {e}")
        
        def reset_to_defaults():
            """Reset settings to defaults."""
            if messagebox.askyesno("Reset Settings", 
                                  "Reset all ADS settings to defaults?\n\nThis will close the settings window."):
                self.settings_manager.reset_to_defaults()
                self._log_status("Beckhoff ADS settings reset to defaults")
                settings_window.destroy()
                # Reinitialize interface
                if self.beckhoff_ads_interface:
                    self.beckhoff_ads_interface.stop_polling()
                self._init_beckhoff_interface()
        
        ttk.Button(button_frame, text="Save", command=save_settings).pack(side=tk.RIGHT, padx=(5, 0))
        ttk.Button(button_frame, text="Reset to Defaults", command=reset_to_defaults).pack(side=tk.RIGHT)
        ttk.Button(button_frame, text="Cancel", command=settings_window.destroy).pack(side=tk.RIGHT, padx=(0, 5))
    
    def on_beckhoff_monitor(self):
        """Open Beckhoff ADS communication monitor window."""
        if not PYADS_AVAILABLE:
            messagebox.showwarning("Monitor", "pyads library is required for ADS communication monitor.")
            return
        
        # Create or show monitor window
        if self.beckhoff_monitor_window is None or not self.beckhoff_monitor_window.window.winfo_exists():
            self.beckhoff_monitor_window = BeckhoffADSMonitor(self.root, self.beckhoff_ads_interface, self)
            # Update ADS interface with combined log callback (monitor + file)
            if self.beckhoff_ads_interface:
                if hasattr(self, '_create_combined_log_callback'):
                    self.beckhoff_ads_interface.log_callback = self._create_combined_log_callback()
                else:
                    # Fallback if callback creator doesn't exist yet
                    self.beckhoff_ads_interface.log_callback = self.beckhoff_monitor_window.log_event
        else:
            # Bring existing window to front
            self.beckhoff_monitor_window.window.lift()
            self.beckhoff_monitor_window.window.focus()


class BeckhoffADSMonitor:
    """Beckhoff ADS Communication Monitor Window."""
    
    def __init__(self, parent, ads_interface, gui_instance=None):
        """
        Initialize the ADS communication monitor.
        
        Args:
            parent: Parent window (Tkinter root)
            ads_interface: BeckhoffADSInterface instance (can be None)
            gui_instance: CHRocodileGUI instance (for cleanup reference)
        """
        self.parent = parent
        self.gui_instance = gui_instance  # Store reference to GUI instance for cleanup
        self.ads_interface = ads_interface
        self.log_entries = []
        self.max_entries = 1000
        
        # Create window
        self.window = tk.Toplevel(parent)
        self.window.title("Beckhoff ADS Communication Monitor")
        self.window.geometry("900x600")
        self.window.transient(parent)
        
        # Main frame
        main_frame = ttk.Frame(self.window, padding="10")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Status frame
        status_frame = ttk.LabelFrame(main_frame, text="Connection Status", padding="10")
        status_frame.pack(fill=tk.X, pady=(0, 10))
        
        self.status_label = ttk.Label(status_frame, text="Not connected", foreground="gray")
        self.status_label.pack(side=tk.LEFT, padx=(0, 20))
        
        if ads_interface:
            ams_netid = ads_interface.ams_netid
            port = ads_interface.port if ads_interface.port else "851 (default)"
            connected = ads_interface.is_connected()
            running = ads_interface.is_running() if ads_interface else False
            
            status_text = f"AMS NetID: {ams_netid} | Port: {port}"
            if connected:
                status_text += " | Connected"
                self.status_label.config(text=status_text, foreground="green")
            elif running:
                status_text += " | Connecting..."
                self.status_label.config(text=status_text, foreground="orange")
            else:
                status_text += " | Disconnected"
                self.status_label.config(text=status_text, foreground="gray")
        
        # Filter frame
        filter_frame = ttk.Frame(main_frame)
        filter_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Label(filter_frame, text="Filter:").pack(side=tk.LEFT, padx=(0, 5))
        self.filter_var = tk.StringVar(value="All")
        filter_combo = ttk.Combobox(filter_frame, textvariable=self.filter_var, 
                                    values=["All", "Read", "Write", "Trigger", "Handshake", "Connection", "Error"],
                                    state="readonly", width=15)
        filter_combo.pack(side=tk.LEFT, padx=(0, 10))
        filter_combo.bind("<<ComboboxSelected>>", lambda e: self._update_display())
        
        ttk.Button(filter_frame, text="Clear Log", command=self.clear_log).pack(side=tk.LEFT, padx=(0, 10))
        self.autoscroll_var = tk.BooleanVar(value=True)
        ttk.Checkbutton(filter_frame, text="Auto-scroll", variable=self.autoscroll_var).pack(side=tk.LEFT, padx=(0, 10))
        
        # Reset disabled variables button
        if ads_interface:
            ttk.Button(filter_frame, text="Reset Disabled Variables", 
                      command=lambda: self.reset_disabled_vars()).pack(side=tk.LEFT)
        
        # Log display
        log_frame = ttk.LabelFrame(main_frame, text="Communication Log", padding="5")
        log_frame.pack(fill=tk.BOTH, expand=True)
        
        # Text widget with scrollbar
        text_frame = ttk.Frame(log_frame)
        text_frame.pack(fill=tk.BOTH, expand=True)
        
        self.log_text = tk.Text(text_frame, wrap=tk.WORD, state=tk.DISABLED, font=("Consolas", 9))
        self.scrollbar = ttk.Scrollbar(text_frame, orient=tk.VERTICAL, command=self.log_text.yview)
        self.log_text.configure(yscrollcommand=self.scrollbar.set)
        
        self.log_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        self.scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Configure text colors
        self.log_text.tag_config("read", foreground="blue")
        self.log_text.tag_config("write", foreground="green")
        self.log_text.tag_config("trigger", foreground="purple", font=("Consolas", 9, "bold"))
        self.log_text.tag_config("handshake", foreground="orange")
        self.log_text.tag_config("connection", foreground="darkgreen", font=("Consolas", 9, "bold"))
        self.log_text.tag_config("error", foreground="red", font=("Consolas", 9, "bold"))
        
        # Update status periodically
        self._update_status()
        self.window.after(1000, self._update_status_loop)
        
        # Handle window close
        self.window.protocol("WM_DELETE_WINDOW", self.on_close)
    
    def log_event(self, event_type: str, message: str, data: dict = None):
        """
        Log a communication event.
        
        Args:
            event_type: Type of event ('read', 'write', 'trigger', 'handshake', 'connection', 'error')
            message: Event message
            data: Optional event data dictionary
        """
        # Check if window still exists before logging
        if not self.window.winfo_exists():
            return
        
        import datetime
        timestamp = datetime.datetime.now().strftime("%H:%M:%S.%f")[:-3]
        
        entry = {
            "timestamp": timestamp,
            "type": event_type,
            "message": message,
            "data": data or {}
        }
        
        self.log_entries.append(entry)
        
        # Limit log size
        if len(self.log_entries) > self.max_entries:
            self.log_entries.pop(0)
        
        # Update display (will check window existence internally)
        self._update_display()
    
    def _update_display(self):
        """Update the log display."""
        # Check if window still exists before updating
        if not self.window.winfo_exists():
            return
        
        # Check if widgets still exist
        try:
            if not hasattr(self, 'log_text') or not self.log_text.winfo_exists():
                return
        except tk.TclError:
            return
        
        # Save current scroll position if autoscroll is off
        autoscroll_enabled = self.autoscroll_var.get()
        if not autoscroll_enabled:
            # Get current scrollbar position (returns tuple: (top, bottom))
            try:
                scroll_pos = self.scrollbar.get()
                saved_scroll_top = scroll_pos[0] if scroll_pos else 0.0
            except Exception:
                saved_scroll_top = 0.0
        else:
            saved_scroll_top = None
        
        try:
            self.log_text.config(state=tk.NORMAL)
            self.log_text.delete(1.0, tk.END)
        except tk.TclError:
            # Widget was destroyed, ignore
            return
        
        filter_type = self.filter_var.get()
        
        for entry in self.log_entries:
            if filter_type != "All" and entry["type"] != filter_type.lower():
                continue
            
            timestamp = entry["timestamp"]
            event_type = entry["type"].upper()
            message = entry["message"]
            
            line = f"[{timestamp}] [{event_type}] {message}\n"
            
            self.log_text.insert(tk.END, line, entry["type"])
        
        # Restore scroll position or auto-scroll to bottom
        try:
            if autoscroll_enabled:
                self.log_text.see(tk.END)
            elif saved_scroll_top is not None:
                # Restore scroll position using yview_moveto
                try:
                    self.log_text.yview_moveto(saved_scroll_top)
                except Exception:
                    pass  # If restore fails, just leave it at current position
            
            self.log_text.config(state=tk.DISABLED)
        except tk.TclError:
            # Widget was destroyed, ignore
            pass
    
    def clear_log(self):
        """Clear the log."""
        self.log_entries.clear()
        self._update_display()
    
    def _update_status(self):
        """Update connection status."""
        if self.ads_interface:
            connected = self.ads_interface.is_connected()
            running = self.ads_interface.is_running()
            
            if connected:
                ams_netid = self.ads_interface.ams_netid
                port = self.ads_interface.port if self.ads_interface.port else "851 (default)"
                self.status_label.config(text=f"Connected to {ams_netid}:{port}", 
                                       foreground="green")
            elif running:
                self.status_label.config(text="Connecting...", foreground="orange")
            else:
                self.status_label.config(text="Disconnected", foreground="gray")
    
    def _update_status_loop(self):
        """Periodically update status."""
        if self.window.winfo_exists():
            self._update_status()
            self.window.after(1000, self._update_status_loop)
    
    def reset_disabled_vars(self):
        """Reset disabled variables in ADS interface."""
        if self.ads_interface:
            disabled = self.ads_interface.get_disabled_variables()
            if disabled:
                self.ads_interface.reset_disabled_variables()
                self.log_event("connection", f"Reset {len(disabled)} disabled variables: {', '.join(disabled)}")
            else:
                self.log_event("connection", "No disabled variables to reset")
    
    def on_close(self):
        """Handle window close."""
        # Remove log callback from ADS interface to prevent errors after window is closed
        if self.ads_interface:
            self.ads_interface.log_callback = None
        
        # Clear reference in parent GUI if it exists
        if self.gui_instance and hasattr(self.gui_instance, 'beckhoff_monitor_window'):
            if self.gui_instance.beckhoff_monitor_window is self:
                self.gui_instance.beckhoff_monitor_window = None
        
        self.window.destroy()

def main():
    """Main entry point."""
    root = tk.Tk()
    app = CHRocodileGUI(root)
    
    # Cleanup on window close
    def on_closing():
        if hasattr(app, 'logger'):
            app.logger.info("=" * 80)
            app.logger.info("CHRocodile Application Shutting Down")
            app.logger.info("=" * 80)
        if app.beckhoff_ads_interface:
            app.beckhoff_ads_interface.stop_polling()
        app.controller.disconnect()
        root.destroy()
    
    root.protocol("WM_DELETE_WINDOW", on_closing)
    root.mainloop()


if __name__ == "__main__":
    main()

