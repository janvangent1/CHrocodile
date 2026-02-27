# coding=utf-8

"""
This module contains utility functions. For example, building
and populating command objects, obtaining response information
for a given response reference.

Copyright Precitec Optronik GmbH, 2022
"""
import math
from ctypes.wintypes import UINT
from typing import Union, Tuple, Callable

import numpy as np
from ctypes import byref, c_char_p, c_float, create_string_buffer, c_uint32, c_char, c_short, cast, sizeof, c_void_p, \
    POINTER, c_int, c_int32, c_int64, c_uint, CDLL

from chr_def import TResponseInfo, DLL_GEN_CALLBACK_FUNC, TErrorInfo
from chr_cmd_id import RspParamType, CmdId, OperationMode, ResLevel, DeviceType, DataType


def chr_success(res_code: int) -> bool:
    """
    Utility function to check if a result code is a success
    :param res_code: Result code to be checked
    :type res_code: int
    :return: True or False
    :rtype: bool
    """
    if res_code >= 0:
        return True
    else:
        return False


def chr_warning(res_code: int) -> bool:
    """
    Utility function to check if a result code is a warning
    :param res_code: Result code to be checked
    :type res_code: int
    :return: True or False
    :rtype: bool
    """
    ec = c_uint32(res_code)
    if ec.value >> 30 == ResLevel.WARNING:
        return True
    else:
        return False


def chr_error(res_code: int) -> bool:
    """
    Utility function to check if a result code is an error
    :param res_code: Result code to be checked
    :type res_code: int
    :return: True or False
    :rtype: bool
    """
    ec = c_uint32(res_code)
    if ec.value >> 30 == ResLevel.ERROR:
        return True
    else:
        return False


def chr_info(res_code: int) -> bool:
    """
    Utility function to check if a result code is an information
    :param res_code: Result code to be checked
    :type res_code: int
    :return: True or False
    :rtype: bool
    """
    ec = c_uint32(res_code)
    if ec.value >> 30 == ResLevel.INFORMATION:
        return True
    else:
        return False


def set_lib_log_file_directory(dll_h: CDLL, log_dir: str, max_file_sz: int, max_no_of_files: int) -> int:
    """
    Set the location of the directory for log files.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param log_dir: Name of the log directory
    :type log_dir: string
    :param max_file_sz: The maximum size of a log file
    :type max_file_sz: int
    :param max_no_of_files: The maximum number of log files
    :type max_no_of_files: int
    :return: Error code
    :rtype: int
    """
    dll_h.SetLibLogFileDirectory.argtypes = [c_void_p, c_int64, c_int32]
    dll_h.SetLibLogFileDirectory.restype = c_int
    b_log_dir = log_dir.encode('utf-16-le')
    err = dll_h.SetLibLogFileDirectory(b_log_dir, max_file_sz, max_no_of_files)
    return err


def set_lib_log_level(dll_h: CDLL, log_level: int) -> int:
    """
    Set the level for logging library messages. The message which has the higher or equal level will be logged.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param log_level: Log level
    :type log_level: int
    :return: error code
    :rtype: int
    """
    dll_h.SetLibLogLevel.argtypes = [c_int32]
    dll_h.SetLibLogLevel.restype = c_int
    err = dll_h.SetLibLogLevel(log_level)
    return err


def set_ini_file(dll_h: CDLL, ini_file: str) -> int:
    """
    Set the ini file to be used. If required, this call should be made right after loading the shared library.
    This operation will fail if an ini file has previously been loaded or a
    connection to the device has been opened.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param ini_file: The full path to the ini file including the ini file name.
    For example, C:/config/Chrocodile.ini.
    :type ini_file: string
    :return: error code
    :rtype: int
    """
    dll_h.SetIniFile.argtypes = [c_void_p]
    dll_h.SetIniFile.restype = c_int
    b_ini_file = ini_file.encode('utf-16-le')
    err = dll_h.SetIniFile(b_ini_file)
    return err


def set_lib_config_flags(dll_h: CDLL, flags: int) -> int:
    """
    LibFlag.RSP_DEACTIVATE_AUTO_BUFFER: set library to automatically quit auto buffer save mode
    when response/update has been received from the device, by default this flag is not set.
    LibFlag.AUTO_CHANGE_DATA_BUFFER_SIZE: set library to automatically resize data sample
    buffer to store preset number of data samples in RegisterSampleDataCallback() or GetNextSamples(), 
    by default this flag is set.
    LibFlag.CHECK_THREAD_ID: set library to check the thread ID of function call. When the connection is
    in synchronous mode, all the function calls related to command executing and data reading should be from 
    the same thread. When the connection is
    in asynchronous mode, all the function calls to ExecCommandAsync() should be in the same thread, 
    all the function calls to ProcessDeviceOutput() should also be in one thread.
    By default, this flag is set.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param flags: Libray configuration flag
    :type flags: int
    :return: error code
    :rtype: int
    """
    dll_h.SetLibConfigFlags.argtypes = [c_uint32]
    dll_h.SetLibConfigFlags.restype = c_int
    err = dll_h.SetLibConfigFlags(flags)
    return err


def get_handle_type(dll_h: CDLL, handle: int) -> int:
    """
    Get the handle type.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param handle: Handle whose type has to be checked
    :type handle: int
    :return: Handle type
    :rtype: int
    """
    dll_h.GetHandleType.argtypes = [c_uint32]
    dll_h.GetHandleType.restype = c_int32
    return dll_h.GetHandleType(c_uint32(handle))


def destroy_handle(dll_h: CDLL, handle: int) -> int:
    """
    Get the handle type.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param handle: Handle to be released
    :type handle: int
    :return: Error code
    :rtype: int
    """
    dll_h.DestroyHandle.argtypes = [c_uint32]
    dll_h.DestroyHandle.restype = None
    return dll_h.DestroyHandle(c_uint32(handle))


def open_connection(dll_h: CDLL, conn_info: str, device_type: DeviceType, conn_mode: OperationMode,
                    buf_sz_bytes: int) -> Tuple[int, int]:
    """
    Open a connection to the device with the supplied parameters.
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL 
    :param conn_info: Connection information (IP address etc.)
    :type conn_info: string
    :param device_type: Device type
    :type device_type: DeviceType
    :param conn_mode: Operation mode
    :type conn_mode: OperationMode
    :param buf_sz_bytes: Buffer size in bytes
    :type buf_sz_bytes: int
    :return: Connection handle, error code
    :rtype: int, int
    """
    dll_h.OpenConnection.argtypes = [c_char_p, c_int, c_int, c_int, POINTER(c_uint)]
    dll_h.OpenConnection.restype = c_int
    b_str_url = conn_info.encode('utf-8')
    conn_h = c_uint(0)
    err = dll_h.OpenConnection(b_str_url, device_type, conn_mode, buf_sz_bytes, byref(conn_h))
    return conn_h.value, err


def open_shared_connection(dll_h: CDLL, conn_h: int, conn_mode: OperationMode) -> Tuple[int, int]:
    """
    Open a shared connection for the given physical connection
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param conn_h: Existing connection handle
    :type conn_h: int
    :param conn_mode: Operation mode
    :type conn_mode: OperationMode
    :return: Shared connection handle, error code
    :rtype: int, int
    """
    dll_h.OpenSharedConnection.argtypes = [c_uint, c_int32, POINTER(c_uint)]
    dll_h.OpenSharedConnection.restype = c_int
    c_shared_conn_h = c_uint()
    err = dll_h.OpenSharedConnection(conn_h, c_int32(conn_mode), byref(c_shared_conn_h))
    return c_shared_conn_h.value, err


def get_response_info(dll_h: CDLL, rsp_h: int) -> Tuple[TResponseInfo, int]:
    """
    Obtain the response info of a given reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type: int
    :return: Response info, error code
    :rtype: TResponseInfo, int
    """
    dll_h.GetResponseInfo.argtypes = [
        c_uint, POINTER(TResponseInfo)]
    dll_h.GetResponseInfo.restype = c_int
    rsp_info = TResponseInfo()
    err = dll_h.GetResponseInfo(rsp_h, rsp_info)
    return rsp_info, err


def get_response_arg_type(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[int, int]:
    """
    Obtain the type of argument for a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type: int
    :param index: Index of the argument
    :type: int
    :return: Type of argument, error code
    :rtype: int, int
    """
    dll_h.GetResponseArgType.argtypes = [
        c_uint, c_uint, POINTER(c_int32)]
    dll_h.GetResponseArgType.restype = c_int
    idx = c_uint(index)
    argtype = c_int32()
    err = dll_h.GetResponseArgType(rsp_h, idx, byref(argtype))
    return argtype.value, err


def get_response_int_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[int, int]:
    """
    Obtain int argument of response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type: int
    :param index: Index of the argument
    :type: int
    :return: Argument value, error code
    :rtype: int, int
    """
    dll_h.GetResponseIntArg.argtypes = [c_uint, c_uint32, POINTER(c_int32)]
    dll_h.GetResponseIntArg.restype = c_int
    arg = c_int32()
    err = dll_h.GetResponseIntArg(rsp_h, index, arg)
    return arg.value, err


def get_response_int_array_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[Union[np.ndarray, None], int, int]:
    """
    Obtain the int array arg of response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type: int
    :param index: Index of the argument
    :type index: int
    :return: Argument value, error code
    :rtype: ndarray | None, int, int
    """
    dll_h.GetResponseIntArrayArg.argtypes = [c_uint, c_uint, POINTER(POINTER(c_int32)), POINTER(c_int32)]
    dll_h.GetResponseIntArrayArg.restype = c_int
    arg = POINTER(c_int32)()
    length = c_int32()
    err = dll_h.GetResponseIntArrayArg(rsp_h, index, byref(arg), byref(length))
    data_array = np.ctypeslib.as_array(arg, (length.value,)) if length.value > 0 else None
    return data_array, length.value, err


def get_response_string_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[Union[np.ndarray, None], int, int]:
    """
    Obtain the string arg for a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :param index: Index of the argument
    :type index: int 
    :return: Argument value, length of array, error code
    :rtype: ndarray | None, int, int
    """
    dll_h.GetResponseStringArg.argtypes = [c_uint, c_uint, POINTER(POINTER(c_char)), POINTER(c_int32)]
    dll_h.GetResponseStringArg.restype = c_int
    arg = POINTER(c_char)()
    length = c_int32()
    err = dll_h.GetResponseStringArg(rsp_h, index, byref(arg), byref(length))
    data_array = np.ctypeslib.as_array(arg, (length.value,)) if length.value > 0 else None
    return data_array, length.value, err


def get_response_float_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[float, int]:
    """
    Obtain float argument of a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type rsp_h: int
    :param index: Index of the argument
    :type index: int
    :return: Argument value, error
    :rtype: float, int
    """
    dll_h.GetResponseFloatArg.argtypes = [
        c_uint, c_uint, POINTER(c_float)]
    dll_h.GetResponseFloatArg.restype = c_int
    arg = c_float()
    err = dll_h.GetResponseFloatArg(rsp_h, index, arg)
    return arg.value, err


def get_response_float_array_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[Union[np.ndarray, None], int, int]:
    """
    Obtain float array argument of a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type rsp_h: int
    :param index: Index of the argument
    :type index: int
    :return: Array of argument values, length of array, error code
    :rtype: ndarray | None, int, int
    """
    dll_h.GetResponseFloatArrayArg.argtypes = [
        c_uint, c_uint, POINTER(POINTER(c_float)), POINTER(c_int32)]
    dll_h.GetResponseFloatArrayArg.restype = c_int
    arg = POINTER(c_float)()
    length = c_int32()
    err = dll_h.GetResponseFloatArrayArg(
        rsp_h, index, byref(arg), byref(length))
    data_array = np.ctypeslib.as_array(arg, (length.value,)) if length.value > 0 else None
    return data_array, length.value, err


def get_response_blob_arg(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[Union[np.ndarray, None], int, int]:
    """
    Obtain the blob argument of a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h:  Reference to response
    :type rsp_h: int
    :param index: Index of the argument
    :type index: int
    :return: Blob args array, length of array, error code
    :rtype: ndarray | None, int, int
    """
    dll_h.GetResponseBlobArg.argtypes = [c_uint, c_uint, POINTER(POINTER(c_char)), POINTER(c_int32)]
    dll_h.GetResponseBlobArg.restype = c_int
    arg = POINTER(c_char)()
    length = c_int32()
    err = dll_h.GetResponseBlobArg(rsp_h, index, byref(arg), byref(length))
    # short_arr = cast(arg, POINTER(c_short))
    # data_array = np.ctypeslib.as_array(short_arr, (int(length.value/2),)) if length.value > 0 else None
    data_array = np.ctypeslib.as_array(arg, (length.value,)) if length.value > 0 else None
    return data_array, length.value, err


def get_response_blob_arg_as_short_array(dll_h: CDLL, rsp_h: int, index: int) -> Tuple[
        Union[np.ndarray, None], int, int]:
    """
    Obtain the blob argument of a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h:  Reference to response
    :type rsp_h: int
    :param index: Index of the argument
    :type index: int
    :return: Short args array, length of array, error code
    :rtype: ndarray | None, int, int
    """
    dll_h.GetResponseBlobArg.argtypes = [c_uint, c_uint, POINTER(POINTER(c_char)), POINTER(c_int32)]
    dll_h.GetResponseBlobArg.restype = c_int
    arg = POINTER(c_char)()
    length = c_int32()
    err = dll_h.GetResponseBlobArg(rsp_h, index, byref(arg), byref(length))
    short_arr = cast(arg, POINTER(c_short))
    sz = int(length.value / 2)
    data_array = np.ctypeslib.as_array(short_arr, (sz,)) if length.value > 0 else None
    return data_array, sz, err


def new_command(dll_h: CDLL, cmd_id: CmdId, query: int = 0) -> Tuple[int, int]:
    """
    Create a new command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_id: Command ID
    :type cmd_id: CmdId
    :param query: If the command is a query (0 - no, 1 - yes)
    :type query: int
    :return: Command object reference, error code
    :rtype: int, int
    """
    dll_h.NewCommand.argtypes = [c_uint, c_int, POINTER(UINT)]
    dll_h.NewCommand.restype = c_int
    cmd_h = UINT()
    err = dll_h.NewCommand(cmd_id, query, cmd_h)
    return cmd_h.value, err


def new_command_from_string(dll_h: CDLL, cmd_str: str) -> Tuple[int, int]:
    """
    Create a new command object from a command string
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_str: Command string
    :type cmd_str: str
    :return: Command object reference, error code
    :rtype: int, int
    """
    dll_h.NewCommandFromString.argtypes = [c_char_p, POINTER(c_uint)]
    dll_h.NewCommandFromString.restype = c_int
    b_cmd_str = cmd_str.encode('utf-8')
    cmd_h = c_uint()
    err = dll_h.NewCommandFromString(b_cmd_str, cmd_h)
    return cmd_h.value, err


def add_command_int_arg(dll_h: CDLL, cmd_h: int, val: int) -> int:
    """
    Add an int argument to a command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_h: Reference to command object
    :type cmd_h: int
    :param val: Value of the argument
    :type val: int
    :return: Error code
    :rtype: int
    """
    dll_h.AddCommandIntArg.argtypes = [c_uint, c_int]
    dll_h.AddCommandIntArg.restype = c_int
    err = dll_h.AddCommandIntArg(cmd_h, val)
    return err


def add_command_string_arg(dll_h: CDLL, cmd_h: int, val: str) -> int:
    """
    Add a string argument to a command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_h:  Reference to command object
    :type cmd_h: int
    :param val: Value of the argument
    :type val: str
    :return: int code
    :rtype: c_int
    """
    dll_h.AddCommandStringArg.argtypes = [c_uint, c_char_p, c_int32]
    dll_h.AddCommandStringArg.restype = c_int
    b_str_val = val.encode('utf-8')
    arg_len = c_int32(len(b_str_val))
    err = dll_h.AddCommandStringArg(cmd_h, b_str_val, arg_len)
    return err


def add_command_float_arg(dll_h: CDLL, cmd_h: int, val: float) -> int:
    """
    Add a float argument to a command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_h: Reference to command object
    :type cmd_h: int
    :param val: Value of the argument
    :type val: float
    :return: Error code
    :rtype: int
    """
    dll_h.AddCommandFloatArg.argtypes = [c_uint, c_float]
    dll_h.AddCommandFloatArg.restype = c_int
    err = dll_h.AddCommandFloatArg(cmd_h, val)
    return err


def add_command_int_array_arg(dll_h: CDLL, cmd_h: int, vals: list) -> int:
    """
    Add an int array argument to a command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_h: Reference to command object
    :type cmd_h: int
    :param vals: Array of argument values
    :type vals: list
    :return: Error code
    :rtype: int
    """
    # allocates memory for an equivalent array in C and populates it with values from `list_num`
    arr_c = (c_int * len(vals))(*vals)
    dll_h.AddCommandIntArrayArg.argtypes = [
        c_int, POINTER(c_int), c_int]
    dll_h.AddCommandIntArrayArg.restype = c_int
    err = dll_h.AddCommandIntArrayArg(
        cmd_h, arr_c, c_int(len(vals)))
    return err


def add_command_float_array_arg(dll_h: CDLL, cmd_h: int, vals: list) -> int:
    """
    Add an float array argument to a command object
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param cmd_h: Reference to command object
    :type cmd_h: int
    :param vals: Array of argument values
    :type vals: list
    :return: Error code
    :rtype: int
    """
    # allocates memory for an equivalent array in C and populates it with values from `list_num`
    arr_c = (c_float * len(vals))(*vals)
    dll_h.AddCommandFloatArrayArg.argtypes = [
        c_int, POINTER(c_float), c_int]
    dll_h.AddCommandFloatArrayArg.restype = c_int
    err = dll_h.AddCommandFloatArrayArg(
        cmd_h, arr_c, c_int(len(vals)))
    return err


def add_command_blob_arg(dll_h: CDLL, cmd_h: int, data: bytes) -> int:
    """
    Add a blob argument to a command object
    :param dll_h: 
    :type dll_h: 
    :param cmd_h: 
    :type cmd_h: 
    :param data: 
    :type data: 
    :return: Error code
    :rtype: int
    """
    dll_h.AddCommandBlobArg.argtypes = [c_uint, c_char_p, c_int32]
    dll_h.AddCommandBlobArg.restype = c_int
    blob = create_string_buffer(bytes(data))
    arg_len = c_int32(len(data))
    err = dll_h.AddCommandBlobArg(cmd_h, blob, arg_len)
    return err


def response_to_string(dll_h: CDLL, rsp_h: int, max_buf_sz: int = 1024) -> Tuple[str, int]:
    """
    Obtain response string
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type rsp_h: int
    :param max_buf_sz: Max size of the buffer to create for response string
    :type max_buf_sz: int
    :return: Response string, error code
    :rtype: str, int
    """
    dll_h.ResponseToString.argtypes = [
        c_uint, c_char_p, POINTER(c_int64)]
    dll_h.ResponseToString.restype = c_int
    s64 = c_int64(max_buf_sz)
    psz = POINTER(c_int64)(s64)
    dest = create_string_buffer(max_buf_sz)
    err = dll_h.ResponseToString(rsp_h, dest, psz)
    resp_str = dest.value.decode('utf-8')
    return resp_str, err


def error_code_to_string(dll_h: CDLL, error_code: int, max_buf_sz: int = 1024):
    """
    Obtain string for the given error code
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param error_code: Error code
    :type error_code: int
    :param max_buf_sz: Max size of the buffer to create for response string
    :type max_buf_sz: int
    :return: Error string, error code
    :rtype: str, int
    """
    dll_h.ErrorCodeToString.argtypes = [c_int, POINTER(c_char), POINTER(c_int)]
    dll_h.ErrorCodeToString.restype = c_int
    error_str = create_string_buffer(max_buf_sz)
    lib_size = c_int(max_buf_sz)
    err = dll_h.ErrorCodeToString(error_code, error_str, byref(lib_size))
    error_string = error_str.value.decode('utf-8')
    return error_string, err


def cmd_id_from_str(cmd_id_str: str) -> Union[CmdId, int]:
    """
    Convert a command ID string to command ID.
    :param cmd_id_str: Command ID string
    :type cmd_id_str: str
    :return: cmd_id
    :rtype: CmdId | int
    """
    cmd_id = 0
    for i in range(len(cmd_id_str)):
        cmd_id = cmd_id + pow(16, i * 2) * ord(cmd_id_str[i])

    return cmd_id


def cmd_str_from_id(cmd_id: int) -> str:
    """
    Converts a command ID to its string representation
    :param cmd_id: Command ID
    :type cmd_id: int 
    :return: Command ID string
    :rtype: str
    """
    length = math.ceil(math.log(cmd_id) / math.log(256))
    return cmd_id.to_bytes(length, 'little').decode('utf-8')


class Response:
    """
    Contains a command response meta args and args. The argument args is available as a list.
    """

    def __init__(self, cmd_id, dll_h: CDLL = None, rsp_h: int = None, ticket: int = None, flag: int = None,
                 err: int = 0, param_count: int = None, args: list = None):
        self.cmd_id = cmd_id
        self._dll_h = dll_h
        self.rsp_h = rsp_h
        self.ticket = ticket
        self.flag = flag
        self.param_count = param_count
        self.error_code = err
        self.args = args

    def __str__(self):
        cmd_str = cmd_str_from_id(self.cmd_id)
        return (f"cmd={cmd_str},rsp_h={self.rsp_h},ticket={self.ticket},flag={self.flag},"
                f"param_count={self.param_count},error_code={self.error_code}\nargs={self.args}")

    def get_error_string(self) -> Union[str, None]:
        if self.error_code != 0:
            return "Cannot determine error as DLL handle is None" if self._dll_h is None else error_code_to_string(
                self._dll_h, self.error_code)
        else:
            return None


def get_command_response(dll_h: CDLL, rsp_h: int, response: Response = None) -> Tuple[list, int]:
    """
    Obtain the response samples for a response reference
    :param dll_h: Handle to chrocodile client lib
    :type dll_h: CDLL
    :param rsp_h: Reference to response
    :type rsp_h: int
    :param response: Response object
    :type response: Response
    :return: Response samples list, error code
    :rtype: list, int
    """
    rsp_info, err = get_response_info(dll_h, rsp_h)
    rsp_data_list = []
    if err == 0:
        for i in range(rsp_info.ParamCount):
            argtype, err = get_response_arg_type(dll_h, rsp_h, i)
            if argtype == RspParamType.INTEGER:
                data, err = get_response_int_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data)
            elif argtype == RspParamType.FLOAT:
                data, err = get_response_float_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data)
            elif argtype == RspParamType.STRING:
                data, length, err = get_response_string_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data.tostring().decode('utf-8') if data is not None else data)
            elif argtype == RspParamType.BYTE_ARRAY:
                data_array, length, err = get_response_blob_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data_array)
            elif argtype == RspParamType.INTEGER_ARRAY:
                data_array, length, err = get_response_int_array_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data_array.tolist() if data_array is not None else data_array)
            elif argtype == RspParamType.FLOAT_ARRAY:
                data_array, length, err = get_response_float_array_arg(dll_h, rsp_h, i)
                rsp_data_list.append(data_array.tolist() if data_array is not None else data_array)

    if response is not None:
        response.rsp_h = rsp_h
        response.ticket = rsp_info.Ticket
        response.flag = rsp_info.Flag
        response.param_count = rsp_info.ParamCount
        response.args = rsp_data_list

    return rsp_data_list, err


class GenSignalInfo:
    """
    This contains general information about updates.
    """

    def __init__(self, channel_cnt: int = 0, global_sig_cnt: int = 0, info_index: int = 0, peak_sig_cnt: int = 0):
        #: int: Channel count
        self.channel_cnt = channel_cnt
        #: int: Global signals count
        self.global_sig_cnt = global_sig_cnt
        #: int: Information index. A change in this value indicates a change in format of data.
        self.info_index = info_index
        #: int: Peak signals count
        self.peak_sig_cnt = peak_sig_cnt

    def __str__(self):
        return (f"channel_cnt={self.channel_cnt},global_sig_cnt={self.global_sig_cnt},"
                f"info_index={self.info_index},peak_sig_cnt={self.peak_sig_cnt}")


class SignalInfo:
    """
    This contains information about a signal.
    """

    def __init__(self, sig_id: int, data_type: DataType):
        # int: Signal ID
        self.sig_id = sig_id
        # DataType: Type of signal data
        self.data_type = data_type


class Data:
    """
    Contains received samples with the related metadata. The args is numpy array with each row representing
    one sample args. The samples format is, global signals in the beginning followed by peak signals for each channel. 
    Please refer to the CHRocodile command reference for more details.
    """

    def __init__(self, samples: np.ndarray = None, sample_cnt: int = 0, gen_signal_info: GenSignalInfo = None,
                 signal_info: list = None,
                 err_code: int = -1, dll_h: CDLL = None):
        self.samples = samples
        """
        2D Numpy sample array: Contains args for samples as rows.
        """
        #: int: Number of samples
        self.sample_cnt = sample_cnt
        #: dict: Contains general information about signals
        self.gen_signal_info = gen_signal_info
        #: dict: Contains information about signals.
        self.signal_info = signal_info
        #: int: Error code.
        self.error_code = err_code
        self._dll_h = dll_h

    def __str__(self):
        return (f"sample_cnt={self.sample_cnt}\ngen_signal_info={self.gen_signal_info}"
                f"\nsignal_info={self.signal_info}\nerror_code={self.error_code}\nsamples={self.samples}")

    def _gen_data_slicing_info(self, sig_id: int):
        num_signals = self.gen_signal_info.global_sig_cnt + self.gen_signal_info.peak_sig_cnt
        idx = -1
        for num, sig in enumerate(self.signal_info):
            if sig[1] == sig_id:
                idx = num
                break

        if idx == -1:
            raise Exception("Invalid signal id")

        step_size = num_signals - self.gen_signal_info.global_sig_cnt

        return idx, step_size

    def get_error_string(self):
        if self.error_code != 0:
            return "Cannot determine error as DLL handle is None" if self._dll_h is None else error_code_to_string(
                self._dll_h, self.error_code)[0]
        else:
            return None

    def _get_raw_signal_values(self, idx: int, sig: int, sample_no: int = 0) -> Union[np.ndarray, np.array]:
        if idx < self.gen_signal_info.global_sig_cnt:
            g_idx_str = 'g' + str(idx)
            return self.samples[g_idx_str][sample_no] if sample_no >= 0 else self.samples[g_idx_str]

        peak_idx_str = 'f' + str(idx - self.gen_signal_info.global_sig_cnt)
        return self.samples['p'][sample_no][peak_idx_str] if sample_no >= 0 else self.samples['p'][peak_idx_str]

    def get_signal_values(self, sig_id: int, sample_no: int = 0) -> Union[np.ndarray, np.array]:
        """
        Obtain the data for the specified signal ID and the sample number.
        :param sig_id: Signal ID
        :type sig_id: int
        :param sample_no: Sample number
        :type sample_no: int
        :return: Data array
        :rtype: np.ndarray | np.array
        """
        if self.samples is None or sample_no >= self.sample_cnt:
            raise Exception("No data" if self.samples is None else "Invalid sample number")

        idx, step_size = self._gen_data_slicing_info(sig_id)

        if self.samples.dtype != np.float64:
            return self._get_raw_signal_values(idx, sig_id, sample_no)

        if idx < self.gen_signal_info.global_sig_cnt:
            return self.samples[sample_no][idx]

        return self.samples[sample_no][idx::step_size]

    def get_signal_values_all(self, sig_id: int) -> Union[np.ndarray, np.array]:
        """
        Obtain the data for the specified signal ID from all samples. 
        :param sig_id: Signal ID
        :type sig_id: 
        :return: Data array
        :rtype: np.ndarray | np.array 
        """
        if self.samples is None:
            raise Exception("No data")

        idx, step_size = self._gen_data_slicing_info(sig_id)

        if self.samples.dtype != np.float64:
            return self._get_raw_signal_values(idx, sig_id, -1)

        if idx < self.gen_signal_info.global_sig_cnt:
            return self.samples[:, idx]

        return self.samples[:, idx::step_size]


class ErrorInfo:
    """
    Contains error information about a particular handle.
    """

    def __init__(self, dll_h: CDLL, handle: int, error_code: int):
        self._dll_h = dll_h
        #: int: The handle of the connection
        self.handle = handle
        #: int: The error code
        self.error_code = error_code

    def __str__(self):
        err_str = error_code_to_string(self._dll_h, self.error_code) if self._dll_h is not None else "unknown error"
        return f"handle={self.handle}, error_code={err_str}\nerror_string={err_str}"


def send_prepared_command(chr_dll: CDLL, handle: int, cmd_id: Union[CmdId, int], cmd_h: int,
                          conn_mode: OperationMode = OperationMode.SYNC,
                          resp_cb: Callable = None) -> Response:
    """
    Sends a command to the device. The commands are executed synchronously or
    asynchronously depending on the operation mode.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: Execution context handle.
    :type handle: int
    :param cmd_id: Command ID
    :type cmd_id: CmdId | int
    :param cmd_h: Reference to the command object
    :type cmd_h: int
    :param conn_mode: Connection operation mode - SYNC or ASYNC
    :type conn_mode: OperationMode
    :param resp_cb: This is a ctypes callback
    :type resp_cb: function
    :return: Response
    :rtype: Response
    """
    response_info = Response(cmd_id, chr_dll)
    if conn_mode == OperationMode.SYNC:
        rsp_h, response_info.error_code = exec_command(chr_dll, handle, cmd_h)
        _, error = get_command_response(chr_dll, rsp_h, response_info)
        if error < 0:
            response_info.error_code = error
    else:
        response_info.ticket, response_info.error_code = exec_command_async(chr_dll, handle, cmd_h, resp_cb)
    return response_info


def send_command_string(chr_dll: CDLL, handle: int, cmd_str: str, conn_mode: OperationMode = OperationMode.SYNC,
                        resp_cb: Callable = None) -> Response:
    """
    Parses the supplied string as a command and sends it to device. The commands are 
    executed synchronously or asynchronously depending on the operation mode.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: Execution context handle.
    :type handle: int
    :param cmd_str: The command string, for example, SODX 83 16640
    :type cmd_str: str
    :param conn_mode: Connection operation mode - SYNC or ASYNC
    :type conn_mode: OperationMode
    :param resp_cb: This is a ctypes callback
    :type resp_cb: function
    :return: Response
    :rtype: Response
    """
    cmd_elems = cmd_str.split()
    cmd_id = cmd_id_from_str(cmd_elems[0])

    cmd_h, err = new_command_from_string(chr_dll, cmd_str)
    if err != 0:
        rsp = Response(cmd_id, chr_dll)
        rsp.error_code = err
        return rsp

    return send_prepared_command(chr_dll, handle, cmd_id, cmd_h, conn_mode, resp_cb)


def _add_arg(chr_dll, cmd_h, arg):
    err = 0

    if arg is None:
        return err

    if isinstance(arg, int):
        err = add_command_int_arg(chr_dll, cmd_h, arg)
    elif isinstance(arg, float):
        err = add_command_float_arg(chr_dll, cmd_h, arg)
    elif isinstance(arg, str):
        err = add_command_string_arg(chr_dll, cmd_h, arg)
    elif isinstance(arg, bytes):
        err = add_command_blob_arg(chr_dll, cmd_h, arg)
    elif isinstance(arg, tuple):
        for par in arg:
            err = _add_arg(chr_dll, cmd_h, par)
            if err != 0:
                return err
    elif isinstance(arg, list):
        if arg:
            if isinstance(arg[0], int):
                err = add_command_int_array_arg(chr_dll, cmd_h, arg)
            elif isinstance(arg[0], float):
                err = add_command_float_array_arg(chr_dll, cmd_h, arg)
    else:
        err = add_command_string_arg(chr_dll, cmd_h, arg)

    return err


def send_command(chr_dll: CDLL, handle: int, cmd_id: Union[str, CmdId, int], args: Union[list, tuple] = None,
                 query: int = 0,
                 conn_mode: OperationMode = OperationMode.SYNC, resp_cb: Callable = None,
                 ) -> Response:
    """
    Create and send a command with the given arguments.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: Execution context handle.
    :type handle: int
    :param cmd_id: Command ID
    :type cmd_id: str | CmdId | int
    :param args: Arguments for the command
    :type args: list | tuple
    :param query: 1 = query, 0 = not a query
    :type query: int
    :param conn_mode: Connection operation mode - SYNC or ASYNC
    :type conn_mode: OperationMode
    :param resp_cb: This is a ctypes callback
    :type resp_cb: function
    :return: Response
    :rtype: Response
    """
    if isinstance(cmd_id, str):
        cmd_id = cmd_id_from_str(cmd_id)

    cmd_h, err = new_command(chr_dll, cmd_id, query)
    if err != 0:
        rsp = Response(cmd_id, chr_dll)
        rsp.error_code = err
        return rsp

    if args:
        for arg in args:
            err = _add_arg(chr_dll, cmd_h, arg)
            if err != 0:
                rsp = Response(cmd_id, chr_dll)
                rsp.error_code = err
                return rsp

    return send_prepared_command(chr_dll, handle, cmd_id, cmd_h, conn_mode, resp_cb)


def exec_command(chr_dll: CDLL, handle: int, cmd_h: int) -> Tuple[int, int]:
    """
    Execute a command synchronously.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: Execution context handle.
    :type handle: int
    :param cmd_h: Reference to the command object
    :type cmd_h: int
    :param handle: Execution context handle.
    :type handle: int
    :return: Response handle, error code
    :rtype: int, int
    """
    chr_dll.ExecCommand.argtypes = [
        c_uint, c_uint, POINTER(c_uint)]
    chr_dll.ExecCommand.restype = c_int
    rsp_h = c_uint()
    err = chr_dll.ExecCommand(handle, cmd_h, rsp_h)
    return rsp_h.value, err


def exec_command_async(chr_dll: CDLL, handle: int, cmd_h: int, callback: Callable) -> Tuple[int, int]:
    """
    Execute a command asynchronously.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: Execution context handle.
    :type handle: int
    :param cmd_h: Reference to the command object
    :type cmd_h: int
    :param callback: Callback function for the command response
    :type callback: 
    :return: Ticket number, Error code
    :rtype: int, int
    """
    chr_dll.ExecCommandAsync.restype = c_int
    ticket = c_int32()
    if callback is None:
        chr_dll.ExecCommandAsync.argtypes = [
            c_uint, c_uint, c_void_p, c_void_p, POINTER(c_int32)]
    else:
        chr_dll.ExecCommandAsync.argtypes = [
            c_uint, c_uint, c_void_p, DLL_GEN_CALLBACK_FUNC, POINTER(c_int32)]
    err = chr_dll.ExecCommandAsync(handle, cmd_h, None, callback, byref(ticket))
    return ticket.value, err


def last_errors(chr_dll: CDLL, handle: int, max_buf_sz: int = 2048) -> Tuple[list, int]:
    """
    Get the last errors for the given handle.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: The handle to get the errors for
    :type handle: int
    :param max_buf_sz: Max buffer size for the error array
    :type max_buf_sz: int
    :return: List of ErrorInfo, error code
    :rtype: List, int
    """
    chr_dll.LastErrors.restype = c_int
    chr_dll.LastErrors.argtypes = [c_uint, POINTER(c_char), POINTER(c_int64)]
    buf_sz = c_int64(max_buf_sz)
    info_buf = (c_char * max_buf_sz)()
    err = chr_dll.LastErrors(handle, info_buf, byref(buf_sz))

    error_infos = []
    num_errors = int(buf_sz.value / sizeof(TErrorInfo))
    for idx in range(num_errors):
        err_info = TErrorInfo.from_buffer(info_buf, idx * sizeof(TErrorInfo))
        error_infos.append(ErrorInfo(chr_dll, err_info.SourceHandle, err_info.ErrorCode))

    return error_infos, err


def clear_errors(chr_dll: CDLL, handle: int):
    """
    Clears errors for the given handle.
    :param chr_dll: Handle to chrocodile client lib
    :type chr_dll: CDLL
    :param handle: The handle to clear the errors for
    :type handle: int
    :return: Error code
    :rtype: int
    """
    chr_dll.ClearErrors.restype = c_int
    chr_dll.ClearErrors.argtypes = [c_uint]
    return chr_dll.ClearErrors(handle)


class APIException(Exception):
    """
    API specific exceptions.
    """

    def __init__(self, dll_h, error_code, msg: str = None):
        super().__init__(msg)
        self.error_code = error_code
        self.error_string = error_code_to_string(dll_h, error_code)

    def __str__(self):
        return f'{super().__str__()},{self.error_code}={self.error_string}'
