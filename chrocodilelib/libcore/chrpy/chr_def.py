# coding=utf-8

"""
This module contains common definitions.

Copyright Precitec Optronik GmbH, 2022
"""

from ctypes import (CFUNCTYPE, POINTER, Structure, c_double, c_int, c_int32, c_int64, c_short,
                    c_uint, c_uint32, c_ushort, c_void_p)


class TResponseInfo(Structure):
    """
    Container for response info.
    """
    _fields_ = [("CmdID", c_uint),
                ("Ticket", c_int),
                ("Flag", c_uint),
                ("ParamCount", c_uint)]


class TResponseCallbackInfo(Structure):
    """
    Container for response info used by callbacks.
    """
    _fields_ = [("User", c_void_p), ("State", POINTER(c_int32)),
                ("Ticket", c_int32), ("Handle", c_uint32)]


class TSampleSignalGeneralInfo(Structure):
    """
    Container for general signal info.
    """
    _fields_ = [("InfoIndex", c_uint),
                ("PeakSignalCount", c_int),
                ("GlobalSignalCount", c_int),
                ("ChannelCount", c_int)]


class TSampleSignalInfo(Structure):
    """
    Container for signal info
    """
    _pack_ = 1
    _fields_ = [("SignalID", c_ushort),
                ("DataType", c_short)]
    
    
class TErrorInfo(Structure):
    """
    Information of each single error stored in the buffer returned by last_errors
    """
    _fields_ = [("SourceHandle", c_uint32), ("ErrorCode", c_int32)]
    
    
class TPluginInfo(Structure):
    """
    Contains information about a plugin.
    """
    _fields_ = [("PluginHandle", c_uint32), ("TypeID", c_int32)]


DLL_GEN_CALLBACK_FUNC = CFUNCTYPE(c_void_p, TResponseCallbackInfo, c_uint32)
DLL_DATA_CALLBACK_FUNC = CFUNCTYPE(c_void_p, c_void_p, c_int32, c_int64, POINTER(
    c_double), c_int64, TSampleSignalGeneralInfo, POINTER(TSampleSignalInfo))
