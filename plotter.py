# -*- coding: utf-8 -*-
"""
Plotting module for CHRocodile measurements.
Handles live thickness plots and raw interferometric data visualization.
"""

import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk
from matplotlib.figure import Figure
from collections import deque
from typing import Optional
import time
import tkinter as tk


class MeasurementPlotter:
    """
    Manages matplotlib plots for thickness/intensity/quality and optional spectrum data.
    """
    
    def __init__(self, parent_frame, max_points: int = 1000):
        """
        Initialize the plotter.
        
        Args:
            parent_frame: Tkinter frame to embed plots in
            max_points: Maximum number of points to keep in rolling window
        """
        self.parent_frame = parent_frame
        self.max_points = max_points
        
        # Data storage for live plots
        self.timestamps = deque(maxlen=max_points)
        self.thickness_values = deque(maxlen=max_points)
        self.median1_values = deque(maxlen=max_points)
        self.thickness_rejected_flags = deque(maxlen=max_points)
        self.intensity_values = deque(maxlen=max_points)
        self.quality_values = deque(maxlen=max_points)
        
        # Create figures
        self.fig_thickness = Figure(figsize=(6, 4), dpi=100)
        self.fig_metrics = Figure(figsize=(6, 4), dpi=100)
        
        # Create subplots
        self.ax_thickness = self.fig_thickness.add_subplot(111)
        self.ax_metrics = self.fig_metrics.add_subplot(111)
        self.ax_metrics_right = None
        
        # Initialize plots
        self.line_thickness = None
        self.line_median1 = None
        self.line_thickness_rejected = None
        self.line_intensity = None
        self.line_quality = None
        self.last_spectrum = None
        self.last_peak1_pos = None
        self.last_peak2_pos = None
        self.spectrum_window = None
        self.spectrum_window_frame = None
        self.fig_spectrum = None
        self.ax_spectrum = None
        self.line_spectrum = None
        self.peak_markers = None
        
        # Setup plots
        self._setup_thickness_plot()
        self._setup_metrics_plot()
        
        # Create separate frames for each plot (to avoid pack/grid conflicts)
        self.thickness_frame = None
        self.metrics_frame = None
        
        # Canvas and toolbars will be created when frames are provided
        self.canvas_thickness = None
        self.canvas_metrics = None
        self.toolbar_thickness = None
        self.toolbar_metrics = None
        self.canvas_spectrum = None
        self.toolbar_spectrum = None
        
    def _setup_thickness_plot(self):
        """Setup the thickness vs measurement number plot."""
        self.ax_thickness.set_title('Thickness Measurement (Live)')
        self.ax_thickness.set_xlabel('Measurement Number')
        self.ax_thickness.set_ylabel('Thickness (μm)')
        self.ax_thickness.grid(True, alpha=0.3)
        self.ax_thickness.set_xlim(0.5, 10.5)
        self.ax_thickness.set_ylim(50, 130)  # Initial range; runtime updates auto-scale
        
        # Initialize empty line
        self.line_thickness, = self.ax_thickness.plot([], [], 'b-', linewidth=1.5, marker='o', markersize=4, label='Thickness')
        self.line_median1, = self.ax_thickness.plot(
            [], [], color='orange', linestyle='--', linewidth=1.3, marker='x', markersize=4, label='Median 1'
        )
        self.line_thickness_rejected, = self.ax_thickness.plot(
            [], [], 'ro', linewidth=0, markersize=7, label='Below quality threshold', zorder=5
        )
        self.ax_thickness.legend()
        
    def _setup_metrics_plot(self):
        """Setup the intensity/quality live plot."""
        self.ax_metrics_right = self.ax_metrics.twinx()

        self.ax_metrics.set_title('Intensity and Quality (Live)')
        self.ax_metrics.set_xlabel('Measurement Number')
        self.ax_metrics.set_ylabel('Intensity', color='m')
        self.ax_metrics_right.set_ylabel('Quality', color='g')
        self.ax_metrics.grid(True, alpha=0.3)
        self.ax_metrics.set_xlim(0.5, 10.5)
        self.ax_metrics.set_ylim(0, 1)
        self.ax_metrics_right.set_ylim(0, 1)
        self.ax_metrics.tick_params(axis='y', colors='m')
        self.ax_metrics_right.tick_params(axis='y', colors='g')

        self.line_intensity, = self.ax_metrics.plot(
            [], [], 'm-', linewidth=1.5, marker='o', markersize=3, label='Intensity'
        )
        self.line_quality, = self.ax_metrics_right.plot(
            [], [], 'g-', linewidth=1.5, marker='o', markersize=3, label='Quality'
        )
        self.ax_metrics.legend(
            [self.line_intensity, self.line_quality],
            ['Intensity', 'Quality'],
            loc='upper right'
        )

    def _setup_spectrum_plot(self):
        """Setup the raw interferometric spectrum plot (separate window)."""
        if self.ax_spectrum is None:
            return

        self.ax_spectrum.clear()
        self.ax_spectrum.set_title('Raw Interferometric Spectrum')
        self.ax_spectrum.set_xlabel('Pixel Number')
        self.ax_spectrum.set_ylabel('Intensity')
        self.ax_spectrum.grid(True, alpha=0.3)

        self.line_spectrum, = self.ax_spectrum.plot(
            [], [], 'b-', linewidth=1.2, antialiased=True, label='Spectrum'
        )
        self.peak_markers = None
        self.ax_spectrum.legend()
        
    def update_thickness_plot(
        self,
        timestamp: float,
        thickness: Optional[float],
        median1: Optional[float] = None,
        below_quality_threshold: bool = False
    ):
        """
        Update the live thickness plot with a new measurement.
        
        Args:
            timestamp: Measurement timestamp (stored but not used for x-axis)
            thickness: Thickness value in microns
            median1: Median 1 value in microns
            below_quality_threshold: True if this measurement fails quality threshold
        """
        if (thickness is None or np.isnan(thickness)) and (median1 is None or np.isnan(median1)):
            return
        
        # Add new data point
        self.timestamps.append(timestamp)
        self.thickness_values.append(np.nan if thickness is None else thickness)
        self.median1_values.append(np.nan if median1 is None else median1)
        self.thickness_rejected_flags.append(bool(below_quality_threshold))
        
        if len(self.timestamps) == 0:
            return
        
        # Convert to numpy arrays for plotting
        values = np.array(self.thickness_values, dtype=np.float64)
        median_values = np.array(self.median1_values, dtype=np.float64)
        
        # Use measurement number (1, 2, 3, ...) for x-axis instead of time
        measurement_numbers = np.arange(1, len(values) + 1)
        
        flags = np.array(self.thickness_rejected_flags, dtype=bool)
        thickness_plot = values.copy()
        median_plot = median_values.copy()
        thickness_plot[flags] = np.nan
        median_plot[flags] = np.nan

        # Update line data (hide rejected points on main series; show as red markers)
        self.line_thickness.set_data(measurement_numbers, thickness_plot)
        self.line_median1.set_data(measurement_numbers, median_plot)
        rejected_x = measurement_numbers[flags]
        rejected_y = np.where(np.isnan(median_values[flags]), values[flags], median_values[flags])
        self.line_thickness_rejected.set_data(rejected_x, rejected_y)
        
        # Auto-scale axes
        if len(measurement_numbers) > 0:
            # Show all collected measurements
            x_min = 0.5
            x_max = max(len(measurement_numbers) + 0.5, 10.5)
            self.ax_thickness.set_xlim(x_min, x_max)

            # Auto-scale y-axis with dynamic margin (no fixed 50..130 clamp)
            combined = np.concatenate([values[~np.isnan(values)], median_values[~np.isnan(median_values)]])
            if len(combined) > 0:
                y_data_min = float(np.min(combined))
                y_data_max = float(np.max(combined))
                y_span = y_data_max - y_data_min
                y_margin = max(1.0, y_span * 0.1)
                if y_span == 0:
                    y_margin = max(1.0, abs(y_data_max) * 0.05)
                self.ax_thickness.set_ylim(y_data_min - y_margin, y_data_max + y_margin)
        
        # Redraw canvas
        if self.canvas_thickness is not None:
            self.canvas_thickness.draw_idle()

    def update_signal_plot(self, _timestamp: float, intensity: Optional[float], quality: Optional[float]):
        """
        Update the live intensity/quality plot.

        Args:
            _timestamp: Unused placeholder for API consistency
            intensity: Intensity value in raw/device units
            quality: Quality value in raw/device units
        """
        self.intensity_values.append(np.nan if intensity is None else float(intensity))
        self.quality_values.append(np.nan if quality is None else float(quality))

        if len(self.intensity_values) == 0:
            return

        x = np.arange(1, len(self.intensity_values) + 1)
        intensity_arr = np.array(self.intensity_values, dtype=np.float64)
        quality_arr = np.array(self.quality_values, dtype=np.float64)
        self.line_intensity.set_data(x, intensity_arr)
        self.line_quality.set_data(x, quality_arr)

        self.ax_metrics.set_xlim(0.5, max(len(x) + 0.5, 10.5))
        self.ax_metrics_right.set_xlim(0.5, max(len(x) + 0.5, 10.5))

        def _set_axis_limits(axis, values):
            valid = values[~np.isnan(values)]
            if len(valid) > 0:
                y_min = float(np.min(valid))
                y_max = float(np.max(valid))
                y_span = y_max - y_min
                margin = max(1.0, y_span * 0.15)
                if y_span == 0:
                    margin = max(1.0, abs(y_max) * 0.05)
                axis.set_ylim(y_min - margin, y_max + margin)
            else:
                axis.set_ylim(0, 1)

        _set_axis_limits(self.ax_metrics, intensity_arr)
        _set_axis_limits(self.ax_metrics_right, quality_arr)

        if self.canvas_metrics is not None:
            self.canvas_metrics.draw_idle()
    
    def update_raw_data_plot(self, spectrum: np.ndarray, peak1_pos: Optional[int] = None, 
                           peak2_pos: Optional[int] = None):
        """
        Update the raw interferometric spectrum plot.
        
        Args:
            spectrum: Spectrum data array
            peak1_pos: Optional position of first peak (pixel number)
            peak2_pos: Optional position of second peak (pixel number)
        """
        if spectrum is None or len(spectrum) == 0:
            return

        self.last_spectrum = spectrum
        self.last_peak1_pos = peak1_pos
        self.last_peak2_pos = peak2_pos
        if self.canvas_spectrum is None or self.ax_spectrum is None or self.line_spectrum is None:
            return
        
        # Create x-axis (pixel numbers)
        x = np.arange(len(spectrum))

        # Choose an x-window that contains all detected peaks plus some margin.
        x_min = 0
        x_max = len(spectrum) - 1
        valid_peaks = [
            p for p in (peak1_pos, peak2_pos)
            if p is not None and 0 <= p < len(spectrum)
        ]
        if valid_peaks:
            p_min = min(valid_peaks)
            p_max = max(valid_peaks)
            base_span = max(10, p_max - p_min)
            margin = max(5, int(base_span * 0.5))
            x_min = max(0, p_min - margin)
            x_max = min(len(spectrum) - 1, p_max + margin)

        visible_x = x[x_min:x_max + 1]
        visible_y = spectrum[x_min:x_max + 1].astype(np.float64)

        # Smooth visible data to reduce "gritty" appearance, then densify display.
        if len(visible_y) >= 7:
            window_size = 7 if len(visible_y) >= 7 else 3
            kernel = np.ones(window_size, dtype=np.float64) / window_size
            display_y = np.convolve(visible_y, kernel, mode='same')
        else:
            display_y = visible_y

        if len(visible_x) >= 2:
            # Keep rendering responsive by capping display points.
            target_points = min(2500, max(len(visible_x) * 8, 600))
            dense_x = np.linspace(visible_x[0], visible_x[-1], target_points)
            dense_y = np.interp(dense_x, visible_x, display_y)
            self.line_spectrum.set_data(dense_x, dense_y)
        else:
            self.line_spectrum.set_data(visible_x, display_y)
        
        # Remove old peak markers if they exist
        if self.peak_markers:
            for marker in self.peak_markers:
                marker.remove()
            self.peak_markers = None
        
        # Add peak markers if positions provided
        if peak1_pos is not None or peak2_pos is not None:
            self.peak_markers = []

            # Refine peak marker location to nearest local maximum in displayed window.
            def _refine_peak_position(peak_pos: int) -> int:
                if peak_pos is None or not (0 <= peak_pos < len(spectrum)):
                    return peak_pos
                local_radius = 8
                lo = max(x_min, peak_pos - local_radius)
                hi = min(x_max, peak_pos + local_radius)
                if hi <= lo:
                    return int(peak_pos)
                local_segment = display_y[(lo - x_min):(hi - x_min + 1)]
                if len(local_segment) == 0:
                    return int(peak_pos)
                return int(lo + np.argmax(local_segment))

            if peak1_pos is not None and 0 <= peak1_pos < len(spectrum):
                peak1_refined = _refine_peak_position(peak1_pos)
                marker1 = self.ax_spectrum.axvline(x=peak1_refined, color='r', linestyle='--', 
                                                   linewidth=1.5, label='Peak 1', alpha=0.7)
                self.peak_markers.append(marker1)
            
            if peak2_pos is not None and 0 <= peak2_pos < len(spectrum):
                peak2_refined = _refine_peak_position(peak2_pos)
                marker2 = self.ax_spectrum.axvline(x=peak2_refined, color='g', linestyle='--', 
                                                   linewidth=1.5, label='Peak 2', alpha=0.7)
                self.peak_markers.append(marker2)
            
            if self.peak_markers:
                self.ax_spectrum.legend()
        
        # Auto-scale x-axis so both peaks and some margin are visible
        if len(visible_x) > 0:
            self.ax_spectrum.set_xlim(float(visible_x[0]), float(visible_x[-1]))
        if len(display_y) > 0:
            y_min = 0
            y_max = np.max(display_y) * 1.1
            self.ax_spectrum.set_ylim(y_min, y_max)
        
        # Redraw canvas
        if self.canvas_spectrum is not None:
            self.canvas_spectrum.draw_idle()
    
    def clear_plots(self):
        """Clear all plot data."""
        self.timestamps.clear()
        self.thickness_values.clear()
        self.median1_values.clear()
        self.thickness_rejected_flags.clear()
        self.intensity_values.clear()
        self.quality_values.clear()
        
        # Reset plot lines
        self.line_thickness.set_data([], [])
        self.line_median1.set_data([], [])
        self.line_thickness_rejected.set_data([], [])
        self.line_intensity.set_data([], [])
        self.line_quality.set_data([], [])
        if self.line_spectrum is not None:
            self.line_spectrum.set_data([], [])
        
        # Remove peak markers
        if self.peak_markers:
            for marker in self.peak_markers:
                marker.remove()
            self.peak_markers = None
        
        # Reset axes
        self.ax_thickness.set_xlim(0.5, 10.5)
        self.ax_thickness.set_ylim(50, 130)
        self.ax_metrics.set_xlim(0.5, 10.5)
        self.ax_metrics.set_ylim(0, 1)
        if self.ax_metrics_right is not None:
            self.ax_metrics_right.set_xlim(0.5, 10.5)
            self.ax_metrics_right.set_ylim(0, 1)
        if self.ax_spectrum is not None:
            self.ax_spectrum.set_xlim(0, 100)
            self.ax_spectrum.set_ylim(0, 10000)
        
        # Redraw
        if self.canvas_thickness is not None:
            self.canvas_thickness.draw_idle()
        if self.canvas_metrics is not None:
            self.canvas_metrics.draw_idle()
        if self.canvas_spectrum is not None:
            self.canvas_spectrum.draw_idle()
    
    def setup_canvases(self, thickness_frame, metrics_frame):
        """
        Setup canvas widgets in the provided frames.
        This must be called after the frames are created to avoid pack/grid conflicts.
        
        Args:
            thickness_frame: Frame for thickness plot
            metrics_frame: Frame for intensity/quality plot
        """
        self.thickness_frame = thickness_frame
        self.metrics_frame = metrics_frame
        
        # Create canvas widgets in their respective frames
        self.canvas_thickness = FigureCanvasTkAgg(self.fig_thickness, thickness_frame)
        self.canvas_metrics = FigureCanvasTkAgg(self.fig_metrics, metrics_frame)
        
        # Create toolbars in their respective frames (toolbars use pack internally)
        self.toolbar_thickness = NavigationToolbar2Tk(self.canvas_thickness, thickness_frame)
        self.toolbar_metrics = NavigationToolbar2Tk(self.canvas_metrics, metrics_frame)
        
        # Pack the canvas and toolbar in their frames
        self.canvas_thickness.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        self.toolbar_thickness.pack(side=tk.BOTTOM, fill=tk.X)
        
        self.canvas_metrics.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        self.toolbar_metrics.pack(side=tk.BOTTOM, fill=tk.X)

    def setup_spectrum_window(self, parent_window: tk.Toplevel):
        """Create spectrum plot canvas in a separate toplevel window."""
        self.spectrum_window = parent_window
        self.spectrum_window_frame = tk.Frame(parent_window)
        self.spectrum_window_frame.pack(fill=tk.BOTH, expand=True)

        self.fig_spectrum = Figure(figsize=(7, 4), dpi=100)
        self.ax_spectrum = self.fig_spectrum.add_subplot(111)
        self._setup_spectrum_plot()

        self.canvas_spectrum = FigureCanvasTkAgg(self.fig_spectrum, self.spectrum_window_frame)
        self.toolbar_spectrum = NavigationToolbar2Tk(self.canvas_spectrum, self.spectrum_window_frame)
        self.canvas_spectrum.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        self.toolbar_spectrum.pack(side=tk.BOTTOM, fill=tk.X)

        if self.last_spectrum is not None:
            self.update_raw_data_plot(self.last_spectrum, self.last_peak1_pos, self.last_peak2_pos)

    def close_spectrum_window(self):
        """Release spectrum window plotting resources."""
        self.spectrum_window = None
        self.spectrum_window_frame = None
        self.canvas_spectrum = None
        self.toolbar_spectrum = None
        self.fig_spectrum = None
        self.ax_spectrum = None
        self.line_spectrum = None
        self.peak_markers = None
    
    def get_canvas_widgets(self):
        """
        Get the canvas widgets for embedding in GUI.
        Note: This method is deprecated. Use setup_canvases() instead.
        
        Returns:
            Tuple of (thickness_canvas, spectrum_canvas, thickness_toolbar, spectrum_toolbar)
        """
        if self.canvas_thickness is None:
            raise RuntimeError("setup_canvases() must be called first")
        return (
            self.canvas_thickness.get_tk_widget(),
            self.canvas_metrics.get_tk_widget(),
            self.toolbar_thickness,
            self.toolbar_metrics
        )
    
    def export_data(self, filename: str):
        """
        Export measurement data to CSV file.
        
        Args:
            filename: Output filename
        """
        import csv
        
        with open(filename, 'w', newline='') as csvfile:
            writer = csv.writer(csvfile)
            writer.writerow(['Timestamp', 'Thickness (μm)'])
            
            times = list(self.timestamps)
            values = list(self.thickness_values)
            
            # Calculate relative times
            if len(times) > 0:
                base_time = times[0]
                for t, v in zip(times, values):
                    writer.writerow([t - base_time, v])

