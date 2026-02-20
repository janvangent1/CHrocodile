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
    Manages matplotlib plots for thickness measurements and raw interferometric data.
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
        
        # Data storage for live plot
        self.timestamps = deque(maxlen=max_points)
        self.thickness_values = deque(maxlen=max_points)
        
        # Create figures
        self.fig_thickness = Figure(figsize=(6, 4), dpi=100)
        self.fig_spectrum = Figure(figsize=(6, 4), dpi=100)
        
        # Create subplots
        self.ax_thickness = self.fig_thickness.add_subplot(111)
        self.ax_spectrum = self.fig_spectrum.add_subplot(111)
        
        # Initialize plots
        self.line_thickness = None
        self.line_spectrum = None
        self.peak_markers = None
        
        # Setup plots
        self._setup_thickness_plot()
        self._setup_spectrum_plot()
        
        # Create separate frames for each plot (to avoid pack/grid conflicts)
        self.thickness_frame = None
        self.spectrum_frame = None
        
        # Canvas and toolbars will be created when frames are provided
        self.canvas_thickness = None
        self.canvas_spectrum = None
        self.toolbar_thickness = None
        self.toolbar_spectrum = None
        
    def _setup_thickness_plot(self):
        """Setup the thickness vs measurement number plot."""
        self.ax_thickness.set_title('Thickness Measurement (Live)')
        self.ax_thickness.set_xlabel('Measurement Number')
        self.ax_thickness.set_ylabel('Thickness (μm)')
        self.ax_thickness.grid(True, alpha=0.3)
        self.ax_thickness.set_ylim(50, 130)  # Typical range for 60-120 microns
        
        # Initialize empty line
        self.line_thickness, = self.ax_thickness.plot([], [], 'b-', linewidth=1.5, marker='o', markersize=4, label='Thickness')
        self.ax_thickness.legend()
        
    def _setup_spectrum_plot(self):
        """Setup the raw interferometric spectrum plot."""
        self.ax_spectrum.set_title('Raw Interferometric Spectrum')
        self.ax_spectrum.set_xlabel('Pixel Number')
        self.ax_spectrum.set_ylabel('Intensity')
        self.ax_spectrum.grid(True, alpha=0.3)
        
        # Initialize empty line
        self.line_spectrum, = self.ax_spectrum.plot([], [], 'b-', linewidth=1.0, label='Spectrum')
        self.peak_markers = None
        self.ax_spectrum.legend()
        
    def update_thickness_plot(self, timestamp: float, thickness: float):
        """
        Update the live thickness plot with a new measurement.
        
        Args:
            timestamp: Measurement timestamp (stored but not used for x-axis)
            thickness: Thickness value in microns
        """
        if thickness is None or np.isnan(thickness):
            return
        
        # Add new data point
        self.timestamps.append(timestamp)
        self.thickness_values.append(thickness)
        
        if len(self.timestamps) == 0:
            return
        
        # Convert to numpy arrays for plotting
        values = np.array(self.thickness_values)
        
        # Use measurement number (1, 2, 3, ...) for x-axis instead of time
        measurement_numbers = np.arange(1, len(values) + 1)
        
        # Update line data
        self.line_thickness.set_data(measurement_numbers, values)
        
        # Auto-scale axes
        if len(measurement_numbers) > 0:
            # Show all measurements with some margin
            x_min = 0.5
            x_max = max(len(measurement_numbers) + 0.5, 10.5)  # At least show 10 measurements
            
            # If we have many measurements, show a rolling window of the last 50
            if len(measurement_numbers) > 50:
                x_min = len(measurement_numbers) - 49.5
                x_max = len(measurement_numbers) + 0.5
            
            self.ax_thickness.set_xlim(x_min, x_max)
            
            # Auto-scale y-axis with some margin
            if len(values) > 0:
                y_min = max(50, np.min(values) - 5)
                y_max = min(130, np.max(values) + 5)
                self.ax_thickness.set_ylim(y_min, y_max)
        
        # Redraw canvas
        if self.canvas_thickness is not None:
            self.canvas_thickness.draw_idle()
    
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
        
        # Create x-axis (pixel numbers)
        x = np.arange(len(spectrum))
        
        # Update line data
        self.line_spectrum.set_data(x, spectrum)
        
        # Remove old peak markers if they exist
        if self.peak_markers:
            for marker in self.peak_markers:
                marker.remove()
            self.peak_markers = None
        
        # Add peak markers if positions provided
        if peak1_pos is not None or peak2_pos is not None:
            self.peak_markers = []
            if peak1_pos is not None and 0 <= peak1_pos < len(spectrum):
                marker1 = self.ax_spectrum.axvline(x=peak1_pos, color='r', linestyle='--', 
                                                   linewidth=1.5, label='Peak 1', alpha=0.7)
                self.peak_markers.append(marker1)
            
            if peak2_pos is not None and 0 <= peak2_pos < len(spectrum):
                marker2 = self.ax_spectrum.axvline(x=peak2_pos, color='g', linestyle='--', 
                                                   linewidth=1.5, label='Peak 2', alpha=0.7)
                self.peak_markers.append(marker2)
            
            if self.peak_markers:
                self.ax_spectrum.legend()
        
        # Auto-scale axes
        self.ax_spectrum.set_xlim(0, len(spectrum))
        if len(spectrum) > 0:
            y_min = 0
            y_max = np.max(spectrum) * 1.1
            self.ax_spectrum.set_ylim(y_min, y_max)
        
        # Redraw canvas
        if self.canvas_spectrum is not None:
            self.canvas_spectrum.draw_idle()
    
    def clear_plots(self):
        """Clear all plot data."""
        self.timestamps.clear()
        self.thickness_values.clear()
        
        # Reset plot lines
        self.line_thickness.set_data([], [])
        self.line_spectrum.set_data([], [])
        
        # Remove peak markers
        if self.peak_markers:
            for marker in self.peak_markers:
                marker.remove()
            self.peak_markers = None
        
        # Reset axes
        self.ax_thickness.set_xlim(0.5, 10.5)
        self.ax_thickness.set_ylim(50, 130)
        self.ax_spectrum.set_xlim(0, 1200)
        self.ax_spectrum.set_ylim(0, 10000)
        
        # Redraw
        if self.canvas_thickness is not None:
            self.canvas_thickness.draw_idle()
        if self.canvas_spectrum is not None:
            self.canvas_spectrum.draw_idle()
    
    def setup_canvases(self, thickness_frame, spectrum_frame):
        """
        Setup canvas widgets in the provided frames.
        This must be called after the frames are created to avoid pack/grid conflicts.
        
        Args:
            thickness_frame: Frame for thickness plot
            spectrum_frame: Frame for spectrum plot
        """
        self.thickness_frame = thickness_frame
        self.spectrum_frame = spectrum_frame
        
        # Create canvas widgets in their respective frames
        self.canvas_thickness = FigureCanvasTkAgg(self.fig_thickness, thickness_frame)
        self.canvas_spectrum = FigureCanvasTkAgg(self.fig_spectrum, spectrum_frame)
        
        # Create toolbars in their respective frames (toolbars use pack internally)
        self.toolbar_thickness = NavigationToolbar2Tk(self.canvas_thickness, thickness_frame)
        self.toolbar_spectrum = NavigationToolbar2Tk(self.canvas_spectrum, spectrum_frame)
        
        # Pack the canvas and toolbar in their frames
        self.canvas_thickness.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        self.toolbar_thickness.pack(side=tk.BOTTOM, fill=tk.X)
        
        self.canvas_spectrum.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        self.toolbar_spectrum.pack(side=tk.BOTTOM, fill=tk.X)
    
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
            self.canvas_spectrum.get_tk_widget(),
            self.toolbar_thickness,
            self.toolbar_spectrum
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

