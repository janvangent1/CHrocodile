# -*- coding: utf-8 -*-
"""
Settings Manager for CHRocodile GUI Application
Handles loading and saving of application settings to JSON file.
Works with both script execution and PyInstaller frozen executables.
"""

import os
import json
import sys
from typing import Dict, Any


class SettingsManager:
    """Manages application settings stored in JSON file."""
    
    DEFAULT_SETTINGS = {
        "ads": {
            "ams_netid": "127.0.0.1.1.1",
            "port": None,  # None = use default PORT_TC3PLC1
            "symbol_prefix": "GVL_CHRocodile.",
            "poll_interval": 0.1,
            "write_timeout": 1.0,  # Timeout for PLC write operations in seconds (default: 1.0s for slow networks)
            "variables": {
                "trigger": "bTriggerMeasurement",
                "start_continuous": "bStartContinuous",
                "stop_continuous": "bStopContinuous",
                "interval": "nIntervalMs",
                "busy": "bMeasurementBusy",
                "ready": "bMeasurementReady",
                "ack": "bMeasurementAck",
                "thickness": "rThickness",
                "peak1": "rPeak1",
                "peak2": "rPeak2",
                "count": "nMeasurementCount",
                "error": "sError"
            }
        },
        "device": {
            "default_ip": "192.168.170.3"
        }
    }
    
    def __init__(self, settings_file: str = None):
        """
        Initialize settings manager.
        
        Args:
            settings_file: Path to settings JSON file. If None, uses default location.
        """
        if settings_file is None:
            settings_file = self._get_default_settings_path()
        
        self.settings_file = settings_file
        self.settings = self.DEFAULT_SETTINGS.copy()
        self.load()
    
    def _get_default_settings_path(self) -> str:
        """
        Get default path for settings file.
        Works for both script execution and PyInstaller frozen executables.
        """
        if getattr(sys, 'frozen', False):
            # Running as compiled executable
            # Store settings next to executable
            base_path = os.path.dirname(sys.executable)
        else:
            # Running as script
            base_path = os.path.dirname(os.path.abspath(__file__))
        
        return os.path.join(base_path, 'chrocodile_settings.json')
    
    def load(self) -> Dict[str, Any]:
        """
        Load settings from JSON file.
        If file doesn't exist, uses default settings.
        
        Returns:
            Dictionary with loaded settings
        """
        if os.path.exists(self.settings_file):
            try:
                with open(self.settings_file, 'r', encoding='utf-8') as f:
                    loaded_settings = json.load(f)
                
                # Merge with defaults (ensures new settings are added)
                self.settings = self._merge_settings(self.DEFAULT_SETTINGS, loaded_settings)
                return self.settings
            except Exception as e:
                print(f"Warning: Could not load settings from {self.settings_file}: {e}")
                print("Using default settings.")
                self.settings = self.DEFAULT_SETTINGS.copy()
        else:
            # File doesn't exist, use defaults and save them
            self.save()
        
        return self.settings
    
    def save(self) -> bool:
        """
        Save current settings to JSON file.
        
        Returns:
            True if saved successfully, False otherwise
        """
        try:
            # Create directory if it doesn't exist
            settings_dir = os.path.dirname(self.settings_file)
            if settings_dir and not os.path.exists(settings_dir):
                os.makedirs(settings_dir, exist_ok=True)
            
            with open(self.settings_file, 'w', encoding='utf-8') as f:
                json.dump(self.settings, f, indent=4, ensure_ascii=False)
            
            return True
        except Exception as e:
            print(f"Error saving settings to {self.settings_file}: {e}")
            return False
    
    def get(self, key_path: str, default: Any = None) -> Any:
        """
        Get setting value using dot-notation path (e.g., 'ads.ams_netid').
        
        Args:
            key_path: Dot-separated path to setting (e.g., 'ads.ams_netid')
            default: Default value if setting not found
        
        Returns:
            Setting value or default
        """
        keys = key_path.split('.')
        value = self.settings
        
        try:
            for key in keys:
                value = value[key]
            return value
        except (KeyError, TypeError):
            return default
    
    def set(self, key_path: str, value: Any) -> bool:
        """
        Set setting value using dot-notation path.
        
        Args:
            key_path: Dot-separated path to setting (e.g., 'ads.ams_netid')
            value: Value to set
        
        Returns:
            True if set successfully, False otherwise
        """
        keys = key_path.split('.')
        settings = self.settings
        
        try:
            # Navigate to parent dict
            for key in keys[:-1]:
                if key not in settings:
                    settings[key] = {}
                settings = settings[key]
            
            # Set value
            settings[keys[-1]] = value
            return True
        except Exception as e:
            print(f"Error setting {key_path}: {e}")
            return False
    
    def get_ads_settings(self) -> Dict[str, Any]:
        """Get ADS settings dictionary."""
        return self.settings.get('ads', {}).copy()
    
    def set_ads_settings(self, ads_settings: Dict[str, Any]) -> bool:
        """
        Set ADS settings.
        
        Args:
            ads_settings: Dictionary with ADS settings
        
        Returns:
            True if set successfully
        """
        self.settings['ads'] = ads_settings
        return True
    
    def _merge_settings(self, default: Dict, loaded: Dict) -> Dict:
        """
        Recursively merge loaded settings with defaults.
        Ensures all default keys exist, but loaded values take precedence.
        """
        result = default.copy()
        
        for key, value in loaded.items():
            if key in result and isinstance(result[key], dict) and isinstance(value, dict):
                result[key] = self._merge_settings(result[key], value)
            else:
                result[key] = value
        
        return result
    
    def reset_to_defaults(self) -> bool:
        """
        Reset all settings to defaults.
        
        Returns:
            True if reset successfully
        """
        self.settings = self.DEFAULT_SETTINGS.copy()
        return self.save()
