"""
This module contains modules and related definitions and utilities.

Copyright Precitec Optronik GmbH, 2022
"""
from enum import IntEnum

from collections import namedtuple

from ctypes import (POINTER, Structure, c_double, c_int32, c_ubyte, c_uint32, cast, c_char_p, sizeof)

import numpy

from chr_def import TSampleSignalInfo

from chr_utils import Response
from chr_cmd_id import DataType


class PluginCmdId(IntEnum):
    FLYINGSPOT_CFG = 0x474643  # CFG
    FLYINGSPOT_COMPILE = 0x474f5250  # PROG
    FLYINGSPOT_EXEC = 0x43455845  # EXEC
    FLYINGSPOT_STOP = 0x504f5453  # STOP
    FLYINGSPOT_BLOB = 0x424f4c42  # BLOB
    FLYINGSPOT_SDCARD = 0x44524143  # CARD


class FlyingSpotConsts(IntEnum):
    PROG_InputString = 0
    PROG_InputFile = 1
    PluginRawData = 0
    PluginInterpolated2D = 1
    PluginRecipeTerminate = 2


PluginInfo = namedtuple('PluginInfo', 'name id')
FlyingSpot_Plugin = PluginInfo("FlyingSpotPlugin", 0x3594C46)  # FLY
Test_Plugin = PluginInfo("TestPlugin", 0x545354)  # TST


class TFSSPluginShapeData(Structure):
    _pack_ = 1
    _fields_ = [
        ("Label", c_char_p),
        ("Info", POINTER(TSampleSignalInfo)),
        ("Data", POINTER(POINTER(c_ubyte))),
        ("DataType", c_int32),
        ("ShapeCounter", c_uint32),
        ("NumSignals", c_uint32),
        ("NumSamples", c_uint32),
        ("x0", c_double),
        ("y0", c_double),
        ("x1", c_double),
        ("y1", c_double),
        ("ImageW", c_uint32),
        ("ImageH", c_uint32)
    ]

    def get_data_array(self):
        return None if not self.Data or self.NumSamples == 0 or self.NumSignals == 0 else \
            cast(self.Data, POINTER(POINTER(c_ubyte) * self.NumSignals))[0]

    def get_info_array(self):
        return None if not self.Info or self.NumSignals == 0 else \
            cast(self.Info, POINTER(TSampleSignalInfo * self.NumSignals))[0]

class FSSPluginShapeData:
    def __init__(self, rsp: Response):
        if rsp.args:
            recv_sz = len(rsp.args[0])
            expected_sz = sizeof(TFSSPluginShapeData)
            if recv_sz != expected_sz:
                raise Exception(f"Unexpected response blob size!")

            shape = TFSSPluginShapeData.from_buffer(rsp.args[0])
            self.label = shape.Label.decode("utf-8") if shape.Label else None
            self.data_type = shape.DataType
            self.shape_counter = shape.ShapeCounter
            self.num_signals = shape.NumSignals
            self.num_samples = shape.NumSamples
            self.x0 = shape.x0
            self.y0 = shape.y0
            self.x1 = shape.x1
            self.y1 = shape.y1
            self.image_w = shape.ImageW
            self.image_h = shape.ImageH
            self._fill_signal_data(shape)
        else:
            self.data = dict()
            self.num_samples = 0
            self.num_signals = 0

    def _fill_signal_data(self, shape : TFSSPluginShapeData):
        self.data = dict()
        pinfo = shape.get_info_array()
        parray = shape.get_data_array()
        if pinfo and parray:
            for info,buf in zip(pinfo, parray):
                dt = DataType.to_numpy_dt_string(info.DataType)
                sd = cast(buf, POINTER(DataType.to_ctype(info.DataType) * self.num_samples))[0]
                buf = numpy.frombuffer(sd, dt, self.num_samples)

                if self.data_type == FlyingSpotConsts.PluginInterpolated2D:
                    buf = numpy.reshape(buf, (self.image_w,self.image_h))
                self.data[info.SignalID] = buf

    def __str__(self):
        return (f'label={self.label}\nbuffers={self.data.keys()}\ndata_type={self.data_type}'
                f'\nshape_counter={self.shape_counter}'
                f'\nnum_signals={self.num_signals}'
                f'\nnum_samples={self.num_samples}'
                f'\nx0={self.x0}\ny0={self.y0}\nx1={self.x1}\ny1={self.y1}'
                f'\nimage_w={self.image_w}\nimage_h={self.image_h}')
