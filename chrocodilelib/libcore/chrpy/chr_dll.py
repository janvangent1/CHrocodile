# -*- coding: utf-8 -*-

"""
This module contains the global variables and utility functions for CHRocodile lib.

Copyright Precitec Optronik GmbH, 2022
"""

import os
import sys
import platform
from ctypes import CDLL
from typing import Tuple


def get_abs_dll_path(dll_path=None) -> str:
    """
    Get the absolute path for the CHRocodile DLL
    Supports both script execution and PyInstaller frozen executables.
    :return: Full default path to the dll
    :rtype: str
    """
    if dll_path is None:
        # Support for PyInstaller frozen executable
        if getattr(sys, 'frozen', False):
            # Running as compiled executable
            # Try executable directory first
            base_path = os.path.dirname(sys.executable)
            p_dir = os.path.join(base_path, 'chrocodilelib', 'libcore', 'chrpy')
            if not os.path.exists(p_dir):
                # Try _MEIPASS (PyInstaller temp directory)
                base_path = sys._MEIPASS
                p_dir = os.path.join(base_path, 'chrocodilelib', 'libcore', 'chrpy')
        else:
            # Running as script
            p_dir = os.path.dirname(__file__)
        
        pp_dir = os.path.dirname(p_dir)
        dll_dir_name = os.path.join(pp_dir, "lib")
    
        if "Windows" == platform.system():
            dll_dir_name = os.path.join(dll_dir_name, "win")
            dll_name = "CHRocodile.dll"
            if "64bit" == platform.architecture()[0]:
                dll_dir_name = os.path.join(dll_dir_name, "x64")
            else:
                dll_dir_name = os.path.join(dll_dir_name, "Win32")
        else:
            dll_dir_name = os.path.join(dll_dir_name, "linux")
            dll_name = "libCHRocodile.so"
            if "64bit" == platform.architecture()[0]:
                dll_dir_name = os.path.join(dll_dir_name, "x64")
            else:
                dll_dir_name = os.path.join(dll_dir_name, "x32")
    
        chr_dll_path = os.path.join(dll_dir_name, dll_name)
        chr_dll_path = os.path.abspath(chr_dll_path)
    else:
        chr_dll_path = os.path.abspath(dll_path)
    
    return chr_dll_path
    

def load_client_dll(dll_path=None) -> Tuple[str, CDLL]:
    """
    Load the chrocodile library from the specified path. If the full path is not specified,
    the library is loaded from the default location.
    :param dll_path: Full path to the client DLL including the library name
    :type dll_path: str
    :return: Path the chrocodile client library was loaded from, handle to the client library
    :rtype: str, CDLL
    """
    chr_dll_path = get_abs_dll_path(dll_path)
    chr_dll = CDLL(os.path.abspath(chr_dll_path))
    return chr_dll_path, chr_dll
