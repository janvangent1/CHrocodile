# -*- coding: utf-8 -*-
"""
Simulation module for CHRocodile device.
Generates synthetic data for testing when the physical device is not available.
"""

import numpy as np
import time
from typing import Tuple, Optional
from dataclasses import dataclass


@dataclass
class SimulatedData:
    """Simulated data structure matching the Data class interface"""
    thickness: float  # Thickness in micrometers (signal 256 in real device)
    peak1: float      # Peak 1 position (extracted from spectrum)
    peak2: float      # Peak 2 position (extracted from spectrum)
    spectrum: np.ndarray  # Raw interferometric pattern
    timestamp: float


class CHRocodileSimulator:
    """
    Simulates CHRocodile 2 LR device behavior.
    Generates realistic thickness measurements and interferometric patterns.
    """
    
    def __init__(self, base_thickness: float = 90.0):
        """
        Initialize the simulator.
        
        Args:
            base_thickness: Base thickness value in microns (default: 90, in range 60-120)
        """
        self.base_thickness = base_thickness
        self.measurement_count = 0
        self.rng = np.random.default_rng()
        
    def generate_thickness(self) -> float:
        """
        Generate a simulated thickness measurement.
        
        Returns:
            Thickness value in microns (60-120 range with small variations)
        """
        # Add small random variations around base thickness
        # Use Gaussian noise with std dev of 2 microns
        noise = self.rng.normal(0, 2.0)
        thickness = self.base_thickness + noise
        
        # Keep within reasonable bounds (60-120 microns)
        thickness = np.clip(thickness, 60.0, 120.0)
        
        self.measurement_count += 1
        return float(thickness)
    
    def generate_peak_signals(self, thickness: float) -> Tuple[float, float]:
        """
        Generate simulated peak signals based on thickness.
        The peaks represent the interference pattern from the two film interfaces.
        
        Args:
            thickness: Thickness value in microns
            
        Returns:
            Tuple of (peak1, peak2) signal values
        """
        # Peak values are related to thickness
        # Higher thickness generally correlates with larger peak separation
        # Add some realistic variation
        base_peak1 = 5000.0 + (thickness - 60) * 50
        base_peak2 = 6000.0 + (thickness - 60) * 50
        
        peak1 = base_peak1 + self.rng.normal(0, 100)
        peak2 = base_peak2 + self.rng.normal(0, 100)
        
        return float(peak1), float(peak2)
    
    def generate_spectrum(self, thickness: float, num_points: int = 1200) -> np.ndarray:
        """
        Generate a synthetic interferometric spectrum pattern.
        This simulates the raw CCD data showing the interference pattern.
        
        Args:
            thickness: Thickness value in microns
            num_points: Number of points in the spectrum (default: 1200, typical for CHRocodile)
            
        Returns:
            NumPy array representing the spectrum
        """
        x = np.arange(num_points)
        
        # Create two peaks representing the interference pattern
        # Peak positions are related to thickness
        peak1_pos = int(num_points * 0.3)
        peak2_pos = int(num_points * 0.7)
        
        # Peak separation is related to thickness
        peak_separation = int((thickness - 60) / 60 * 200)  # Scale with thickness
        peak2_pos = peak1_pos + peak_separation
        
        # Ensure peaks stay within bounds
        peak2_pos = min(peak2_pos, num_points - 50)
        
        # Generate Gaussian peaks with some background
        spectrum = np.zeros(num_points)
        
        # Background level
        spectrum += 1000 + self.rng.normal(0, 50, num_points)
        
        # First peak (from first interface)
        peak1_width = 20
        peak1_amplitude = 8000 + (thickness - 60) * 30
        spectrum += peak1_amplitude * np.exp(-((x - peak1_pos) / peak1_width) ** 2)
        
        # Second peak (from second interface)
        peak2_width = 20
        peak2_amplitude = 7000 + (thickness - 60) * 30
        spectrum += peak2_amplitude * np.exp(-((x - peak2_pos) / peak2_width) ** 2)
        
        # Add some noise
        spectrum += self.rng.normal(0, 100, num_points)
        
        # Ensure non-negative values
        spectrum = np.maximum(spectrum, 0)
        
        return spectrum.astype(np.uint16)  # Typical CCD data type
    
    def simulate_measurement(self) -> SimulatedData:
        """
        Simulate a complete measurement including thickness and raw spectrum.
        
        Returns:
            SimulatedData object containing all measurement data
        """
        thickness = self.generate_thickness()
        peak1, peak2 = self.generate_peak_signals(thickness)
        spectrum = self.generate_spectrum(thickness)
        
        return SimulatedData(
            thickness=thickness,
            peak1=peak1,
            peak2=peak2,
            spectrum=spectrum,
            timestamp=time.time()
        )
    
    def set_base_thickness(self, thickness: float):
        """
        Set the base thickness for simulation.
        
        Args:
            thickness: Base thickness in microns (should be in 60-120 range)
        """
        self.base_thickness = np.clip(thickness, 60.0, 120.0)
    
    def reset(self):
        """Reset the measurement counter."""
        self.measurement_count = 0

