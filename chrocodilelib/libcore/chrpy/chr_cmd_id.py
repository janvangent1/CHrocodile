"""
This module contains command ids.

Copyright Precitec Optronik GmbH, 2022
"""
from ctypes import c_ubyte, c_byte, c_uint32, c_double, c_float, c_int32, c_int16, c_uint16
from enum import IntEnum


class ResLevel(IntEnum):
    SUCCESS = 0
    INFORMATION = 1
    WARNING = 2
    ERROR = 3


class OperationMode(IntEnum):
    SYNC = 0
    ASYNC = 1
    
    
class OutputDataMode(IntEnum):
    DOUBLE = 0
    RAW = 1


class DeviceType(IntEnum):
    CHR_1 = 0
    CHR_2 = 1
    CHR_MULTI_CHANNEL = 2
    CHR_COMPACT = 3


class LibFlag(IntEnum):
    RSP_DEACTIVATE_AUTO_BUFFER = 1
    AUTO_CHANGE_DATA_BUFFER_SIZE = 2
    CHECK_THREAD_ID = 4
    
    
class RspFlag(IntEnum):
    QUERY = 0x0001
    ERROR = 0X8000
    WARNING = 0X4000
    UPDATE = 0X2000


class RspParamType(IntEnum):
    INTEGER = 0
    FLOAT = 1
    STRING = 2
    BYTE_ARRAY = 4
    INTEGER_ARRAY = 254
    FLOAT_ARRAY = 255
    
    
class DataType(IntEnum):
    UNSIGNED_CHAR = 0
    SIGNED_CHAR = 1
    UNSIGNED_SHORT = 2
    SIGNED_SHORT = 3
    UNSIGNED_INT32 = 4
    SIGNED_INT32 = 5
    FLOAT = 6
    DOUBLE = 255
    
    @classmethod
    def to_numpy_dt_string(cls, lib_dt):
        if lib_dt == cls.UNSIGNED_CHAR:
            return 'u1'
        elif lib_dt == cls.SIGNED_CHAR:
            return 'i1'
        elif lib_dt == cls.UNSIGNED_SHORT:
            return 'u2'
        elif lib_dt == cls.SIGNED_SHORT:
            return 'i2'
        elif lib_dt == cls.UNSIGNED_INT32:
            return 'u4'
        elif lib_dt == cls.SIGNED_INT32:
            return 'i4'
        elif lib_dt == cls.FLOAT:
            return 'f4'
        elif lib_dt == cls.DOUBLE:
            return 'f8'

    @classmethod
    def to_ctype(cls, lib_dt):
        if lib_dt == cls.UNSIGNED_CHAR:
            return c_ubyte
        elif lib_dt == cls.SIGNED_CHAR:
            return c_byte
        elif lib_dt == cls.UNSIGNED_SHORT:
            return c_uint16
        elif lib_dt == cls.SIGNED_SHORT:
            return c_int16
        elif lib_dt == cls.UNSIGNED_INT32:
            return c_uint32
        elif lib_dt == cls.SIGNED_INT32:
            return c_int32
        elif lib_dt == cls.FLOAT:
            return c_float
        elif lib_dt == cls.DOUBLE:
            return c_double
        
    @classmethod
    def data_size_in_bytes(cls, lib_dt) -> int:
        if lib_dt == cls.UNSIGNED_CHAR:
            return 1
        elif lib_dt == cls.SIGNED_CHAR:
            return 1
        elif lib_dt == cls.UNSIGNED_SHORT:
            return 2
        elif lib_dt == cls.SIGNED_SHORT:
            return 2
        elif lib_dt == cls.UNSIGNED_INT32:
            return 4
        elif lib_dt == cls.SIGNED_INT32:
            return 4
        elif lib_dt == cls.FLOAT:
            return 4
        elif lib_dt == cls.DOUBLE:
            return 8
        

class CmdId(IntEnum):
    OUTPUT_SIGNALS = 0X58444F53
    FIRMWARE_VERSION = 0X00524556
    MEASURING_METHOD = 0X00444D4D
    FULL_SCALE = 0X00414353
    SCAN_RATE = 0X005A4853
    DATA_AVERAGE = 0X00445641
    SPECTRUM_AVERAGE = 0X00535641
    SERIAL_DATA_AVERAGE = 0X53445641
    REFRACTIVE_INDICES = 0X00495253
    ABBE_NUMBERS = 0X00454241
    REFRACTIVE_INDEX_TABLES = 0X00545253
    LAMP_INTENSITY = 0X0049414C
    OPTICAL_PROBE = 0X004E4553
    CONFOCAL_DETECTION_THRESHOLD = 0X00524854
    PEAK_SEPARATION_MIN = 0X004D4350
    INTERFEROMETRIC_QUALITY_THRESHOLD = 0X00485451
    DUTY_CYCLE = 0X00594344
    DETECTION_WINDOW_ACTIVE = 0X00414D4C
    DETECTION_WINDOW = 0X00445744
    NUMBER_OF_PEAKS = 0X00504F4E
    PEAK_ORDERING = 0X00444F50
    DARK_REFERENCE = 0X004B5244
    CONTINUOUS_DARK_REFERENCE = 0X4B445243
    START_DATA_STREAM = 0X00415453
    STOP_DATA_STREAM = 0X004F5453
    LIGHT_SOURCE_AUTO_ADAPT = 0X004C4141
    CCD_RANGE = 0X00415243
    MEDIAN = 0X5844454D
    ANALOG_OUTPUT = 0X58414E41
    ENCODER_COUNTER = 0X53504525
    ENCODER_COUNTER_SOURCE = 0X53434525
    ENCODER_PRELOAD_FUNCTION = 0X46504525
    ENCODER_TRIGGER_ENABLED = 0X45544525
    ENCODER_TRIGGER_PROPERTY = 0X50544525
    DEVICE_TRIGGER_MODE = 0X4D525425
    DOWNLOAD_SPECTRUM = 0X444C4E44
    SAVE_SETTINGS = 0X00555353
    DOWNLOAD_UPLOAD_TABLE = 0X4C424154
    FLTC = 0X43544C46
    SODX = 0x58444f53
    CONF = 0x464E4F43
    ULFW = 0x57464c55


class SpectrumType(IntEnum):
    RAW = 0
    CONFOCAL = 1
    FT = 2
    TWO_D_IMAGE = 3


class TableType(IntEnum):
    CONFOCAL_CALIBRATION = 1
    WAVELENGTH = 2
    REFRACTIVE_INDEX = 3
    DARK_CORRECTION = 4
    CONFOCAL_CALIBRATION_MULTI_CHANNEL = 5
    CLS_MASK = 6
    MPS_MASK = 8
    CHR3_CONST_DARK = 16
    CHR3_SCALED_DARK = 17
    CHR3_CONFOCAL_CALIBRATION_RAW_DATA = 18
    CHR3_CONFOCAL_CALIBRATION_X_RAW_DATA = 19
    CHR3_WHITE_GAIN = 20
    CHR3_WHITE_GAIN_RAW = 21


class DeviceTriggerMode(IntEnum):
    FREE_RUN = 0
    WAIT_TRIGGER = 1
    TRIGGER_EACH = 2
    TRIGGER_WINDOW = 3


class EncoderCounterTriggerSource(IntEnum):
    A0 = 0
    B0 = 1
    A1 = 2
    B1 = 3
    A2 = 4
    B2 = 5
    A3 = 6
    B3 = 7
    A4 = 8
    B4 = 9
    SYNCIN = 1
    QUADRATURE = 15
    IMMEDIATE = 15


class EncoderPreloadConfig(IntEnum):
    ONCE = 0
    EACHTIME = 1
    TRIGGER_RISINGEDGE = 0
    TRIGGER_FALLINGEDGE = 2
    TRIGGER_ONEDGE = 0
    TRIGGER_ONLEVEL = 4
    ENCODER_PRELOAD_CONFIG_ACTIVE = 8


class AutoBuffer(IntEnum):
    ERROR = -1
    SAVING = 0
    FINISHED = 1
    RECEIVED_RESPONSE = 2
    DEACTIVATED = 3
    UNINIT = 4


class ReadData(IntEnum):
    NOT_ENOUGH = 0
    SUCCESS = 1
    RESPONSE = 2
    BUFFER_SMALL = 3
    BUFFER_FULL = 4
    FORMAT_CHANGE = 5
