# -*- coding: utf-8 -*-

"""
This module contains Connection class and other utilities to communicate with a CHRocodile device.

Copyright Precitec Optronik GmbH, 2022
"""
from __future__ import annotations

import os
import signal
import sys
import threading
import time
import copy

from ctypes import (POINTER, byref, c_char_p, c_double, c_int, c_int32, c_int64, c_longlong, c_uint, c_void_p, sizeof,
                    c_char, cast, CDLL)
from math import gcd
from typing import Union, Tuple, Callable

import numpy as np

from chr_cmd_id import CmdId, AutoBuffer, DeviceType, OperationMode, OutputDataMode, DataType, ReadData
from chr_utils import (add_command_int_arg, new_command, response_to_string, get_response_info,
                       get_command_response, Response, Data, APIException, send_command, send_command_string,
                       GenSignalInfo, send_prepared_command, open_shared_connection, open_connection, set_ini_file,
                       last_errors, clear_errors, set_lib_log_file_directory, set_lib_log_level)

from chr_def import (DLL_GEN_CALLBACK_FUNC,
                     TSampleSignalGeneralInfo, TSampleSignalInfo, DLL_DATA_CALLBACK_FUNC)

from chr_dll import load_client_dll

from chr_plugins import PluginInfo


def _check_raise_exception(chr_dll, error_code: int, msg: str = None):
    if error_code < 0:
        raise APIException(chr_dll, error_code, msg)


def _set_callback(rsp: Response, gen_resp_cb, resp_cb, conn_mode, sent_tickets: {}):
    if conn_mode == OperationMode.ASYNC:
        if rsp.cmd_id == CmdId.START_DATA_STREAM or rsp.cmd_id == CmdId.STOP_DATA_STREAM:
            return

        if rsp.ticket in sent_tickets:
            sent_tickets[rsp.ticket][0] += 1
        else:
            sent_tickets[rsp.ticket] = [1, resp_cb if resp_cb is not None else gen_resp_cb]


class Plugin:
    """
    This is a utility class for plugins.
    """

    PLUGIN_TICKETS_SENT = {}

    def __init__(self, plugin_id: int, dll_h: CDLL, handle: int, conn_mode: OperationMode = OperationMode.SYNC,
                 resp_callback: Callable = None):
        """
        Initialize the plugin object
        :param plugin_id: ID of the plugin
        :type plugin_id: int
        :param dll_h: The C++ DLL handle
        :type dll_h: CDLL
        :param handle: This plugins handle
        :type handle: int
        :param conn_mode: Operation mode
        :type conn_mode: OperationMode
        :param resp_callback: Response callback
        :type resp_callback: function
        """
        self.plugin_id = plugin_id
        self._dll_h = dll_h
        self._handle = handle
        self._conn_mode = conn_mode
        self._resp_callback = resp_callback

    def exec(self, cmd_id: Union[str, CmdId], *args, resp_cb: Callable = None) -> Response:
        """
        Execute a command with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments
        :type args: Arguments
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used)
        :type resp_cb: function
        :return: Response ( if operation mode is async, this is a partial response, the complete response is delivered 
        when the reply is received)
        :rtype: Response
        """

        rsp = send_command(self._dll_h, self._handle, cmd_id, args, 0, self._conn_mode)
        _set_callback(rsp, self._resp_callback, resp_cb, self._conn_mode, Plugin.PLUGIN_TICKETS_SENT)
        _check_raise_exception(self._dll_h, rsp.error_code)
        return rsp

    def exec_from_string(self, cmd_str: str, resp_cb: Callable = None) -> Response:
        """
        Execute a command from the given string.
        :param cmd_str: Command string
        :type cmd_str: str
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used)
        :type resp_cb: function
        :return: Response ( if operation mode is async, this is a partial response, the complete response is delivered 
        when the reply is received)
        :rtype: Response
        """

        rsp = send_command_string(self._dll_h, self._handle, cmd_str, self._conn_mode)
        _set_callback(rsp, self._resp_callback, resp_cb, self._conn_mode, Plugin.PLUGIN_TICKETS_SENT)
        _check_raise_exception(self._dll_h, rsp.error_code)
        return rsp

    def query(self, cmd_id: Union[str, CmdId], *args, resp_cb: Callable = None) -> Response:
        """
        Execute a query with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments
        :type args: Arguments
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used)
        :type resp_cb: function
        :return: Response ( if operation mode is async, this is a partial response, the complete response is delivered 
        when the reply is received)
        :rtype: Response
        """

        rsp = send_command(self._dll_h, self._handle, cmd_id, args, 1, self._conn_mode)
        _set_callback(rsp, self._resp_callback, resp_cb, self._conn_mode, Plugin.PLUGIN_TICKETS_SENT)
        _check_raise_exception(self._dll_h, rsp.error_code)
        return rsp


class ConnectionConfig:
    """
    Contains the configuration for a connection.
    """
    buf_size_bytes: int

    def __init__(self):
        """
        Initialises by setting the default values of the Connection parameters.
        """
        #: str: IP Address of the device.
        self.address = '192.168.170.3'
        #: number: Type of device.
        self.device_type = DeviceType.CHR_MULTI_CHANNEL
        #: number: Connection mode.
        self.connection_mode = OperationMode.SYNC
        #: number: Maximum number of samples to be read, lib gives back actual number of read samples.
        self.sample_cnt = 32000
        self.timeout = 10
        """
        number: Timeout in ms for waiting for number of specified samples to be read.
        If set to 0, only available samples are read without waiting.
        """
        self.chr_dll_path = None
        """
        Full path to DLL (by default taken from this file's location). The path should include the
        name of the Chrocodile shared library, for example, C:/mypath/Chrocodile.dll.
        The default location for the shared library is the location of chrdll4 package root directory.
        """
        self.chr_ini_path = None
        """
        Path to the Chrocodile DLL ini file. If this is set, the connection will set it right after loading
        the DLL.
        """
        self.buf_size_bytes = 0
        """
        number: Size of  library internal buffer for storing device output. Only powers of two
        are allowed. If the specified size is less than or equal to zero, 32 MB is used.
        """
        #: int: library log-level (only relevant if chr_log_path is set)
        self.chr_log_level = 2
        #: str: Path to the log directory
        self.chr_log_path = None
        #: int: Maximum log file size in KiB
        self.chr_max_log_file_size = 1024
        #: int: Maximum number of log files
        self.chr_max_no_log_files = 10
        #: Flag to use auto buffer in async mode.
        self.async_auto_buffer = True
        #: Flag to automatically activate auto buffer in async mode when full.
        self.async_auto_activate_buffer = True


# pylint: disable=R0904
class Connection:
    _nd_pointer_1 = np.ctypeslib.ndpointer(dtype=np.float64,
                                           ndim=1,
                                           flags="C")
    """
    This class handles the communication with a device.
    """

    # def __init__(self, conn_config: ConnectionConfig = None, resp_callback=None, data_callback=None) -> None:
    def __init__(self, *, config: ConnectionConfig = None, resp_callback: Callable = None,
                 data_callback: Callable = None, dll_h: CDLL = None) -> None:
        """
        Initialises the Connection object.
        :param config: Contains the connection configuration.
        :type config: ConnectionConfig:
        :param resp_callback: The general response callback function in async mode
        :type resp_callback: function | None
        :param data_callback: The samples' callback function in async mode
        :type data_callback: function | None
        :param dll_h: User supplied handle to chrocodile client lib
        :type dll_h: CDLL
        """
        #: ConnectionConfig: Connection configuration.
        if config is not None:
            self.config = copy.deepcopy(config)
        else:
            self.config = ConnectionConfig()
        #: int: Connection handle.
        self.conn_handle = -1
        #: int: Parent connection handle for shared connection.
        self.parent_conn_handle = -1
        #: array: Data buffer for auto buffer mode.
        self.auto_buffer = np.empty(1)
        #: Response callback function in async mode.
        self.resp_callback = resp_callback
        #: Data callback function in async mode.
        self.data_callback = data_callback
        self._auto_recvd_sample_cnt = 0
        self._data_size = 0
        self._gen_info = TSampleSignalGeneralInfo()
        self._chr_dll = dll_h
        self._gen_sig_info = GenSignalInfo()
        self._sig_info = None
        self._gen_cb = None
        self._data_cb = None
        if not (sys.version_info.major >= 3 and sys.version_info.minor >= 7):
            raise SystemExit("Minimum supported python version is 3.7")
        self._sent_tickets = {}
        self._output_data_format = OutputDataMode.DOUBLE
        self._numpy_dt = None

    def __enter__(self):
        print('Entering', self.config.address)
        self.open()
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        print('Exiting', self.config.address)
        if self.conn_handle >= 0:
            self.close()

    def dll_handle(self) -> CDLL:
        """
        Obtain the handle to the C++ client DLL.
        :return: DLL handle
        :rtype: CDLL
        """
        return self._chr_dll

    def _data_callback(self, user, state, sample_count, sample_buffer, sample_size,
                       gen_signal_info: TSampleSignalGeneralInfo, signal_info):
        try:
            if state == ReadData.FORMAT_CHANGE:
                if self.config.async_auto_buffer:
                    self.activate_auto_buffer_mode(self.config.sample_cnt)

            if state < 0 or sample_count == 0 or sample_size == 0:
                self.data_callback(
                    Data(samples=None, sample_cnt=0, gen_signal_info=None, signal_info=None, err_code=state,
                         dll_h=self._chr_dll))
                return

            if gen_signal_info.InfoIndex != self._gen_info.InfoIndex or state == ReadData.FORMAT_CHANGE:
                self._gen_info = gen_signal_info
                self._update_signal_info(signal_info, sample_size)
                if self.config.async_auto_buffer:
                    self.activate_auto_buffer_mode(self.config.sample_cnt)

            data_list = self._get_output_ndarray(sample_count, sample_buffer) if sample_count > 0 else None
            self.data_callback(
                Data(data_list, sample_count, self._gen_sig_info, self._sig_info, state, self._chr_dll))

            if state == ReadData.BUFFER_FULL:
                self.reset_async_auto_buffer()
            else:
                self._auto_recvd_sample_cnt += sample_count

        except Exception as ex:
            print("Caught data callback exception:\n", ex)
            signal.raise_signal(signal.SIGABRT)

    def reset_async_auto_buffer(self):
        """
        This method is used to manually reset the async auto buffer.
        """
        if self.config.async_auto_buffer and self.config.async_auto_activate_buffer:
            self._auto_recvd_sample_cnt = 0
            self._chr_dll.ActivateAsyncDataUserBuffer.argtypes = [
                c_uint, Connection._nd_pointer_1, c_int64, POINTER(c_int64)]
            err = self._chr_dll.ActivateAsyncDataUserBuffer(
                self.conn_handle, self.auto_buffer, self.config.sample_cnt, None)

    def open(self, config: ConnectionConfig = None, resp_callback: Callable = None,
             data_callback: Callable = None) -> int:
        """
        Opens connection to a device.
        :param config: Configuration for the connection
        :type config: ConnectionConfig
        :param resp_callback: General response callback in async mode
        :type resp_callback: function | None
        :param data_callback: Data callback function in async mode
        :type data_callback: Function
        :return: Error code
        :rtype: int
        """
        if self.conn_handle >= 0:
            print("Connection already open")
            return 0

        if config is not None:
            self.config = copy.deepcopy(config)
        if self.config is None:
            raise Exception('No configuration defined for the connection')
        if self._chr_dll is None:
            self.config.chr_dll_path, self._chr_dll = load_client_dll(
                self.config.chr_dll_path)
            if self.config.chr_ini_path is not None:
                if not os.path.exists(self.config.chr_ini_path) or not os.path.isfile(self.config.chr_ini_path):
                    raise Exception("Invalid ini file: " + self.config.chr_ini_path)
                err = set_ini_file(self._chr_dll, self.config.chr_ini_path)
                if err < 0:
                    raise APIException(self._chr_dll, err, "Failed to set ini file to: " + self.config.chr_ini_path)
            if self.config.chr_log_path is not None:
                if not os.path.exists(self.config.chr_log_path) or not os.path.isdir(self.config.chr_log_path):
                    raise Exception("Invalid log path: " + self.config.chr_log_path)
                max_sz = self.config.chr_max_log_file_size
                max_files = self.config.chr_max_no_log_files
                err = set_lib_log_level(self._chr_dll, self.config.chr_log_level)
                if err < 0:
                    raise APIException(self._chr_dll, err, "Unable to set log level to: " + self.config.chr_log_level)

                err = set_lib_log_file_directory(self._chr_dll, self.config.chr_log_path,
                                                 max_sz if max_sz is not None else 1024,
                                                 max_files if max_files is not None else 10)
                if err < 0:
                    raise APIException(self._chr_dll, err, "Failed to set log path to: " + self.config.chr_log_path)

        self._chr_dll.GetNextSamples.argtypes = [c_uint, POINTER(c_longlong), POINTER(POINTER(c_double)),
                                                 POINTER(c_int),
                                                 POINTER(
                                                     TSampleSignalGeneralInfo),
                                                 POINTER(POINTER(TSampleSignalInfo))]
        self._chr_dll.GetNextSamples.restype = c_int

        if self.parent_conn_handle >= 0:
            print("Opening shared conn", self.config.connection_mode, self.resp_callback, self.data_callback)
            self.conn_handle, err = open_shared_connection(self._chr_dll, self.parent_conn_handle,
                                                           self.config.connection_mode)
        else:
            print("Opening conn", self.config.connection_mode)
            self.conn_handle, err = open_connection(self._chr_dll, self.config.address, self.config.device_type,
                                                    self.config.connection_mode,
                                                    self.config.buf_size_bytes)

        _check_raise_exception(self._chr_dll, err)

        if self.config.connection_mode == OperationMode.ASYNC:
            if resp_callback is not None:
                self.resp_callback = resp_callback

            if data_callback is not None:
                self.data_callback = data_callback

            def general_callback(rsp_callback_info, rsp_ref):
                try:
                    rsp_info, err_code = get_response_info(
                        self._chr_dll, rsp_ref)
                    response = Response(rsp_info.CmdID, self._chr_dll)

                    # if rsp_info.CmdID == CmdId.SODX and self.config.async_auto_buffer:
                    #    self.activate_auto_buffer_mode(self.config.sample_cnt)

                    response.ticket = rsp_info.Ticket
                    response.error_code = err_code
                    if err_code == 0:
                        _, error = get_command_response(
                            self._chr_dll, rsp_ref, response)
                        if error < 0:
                            response.error_code = error
                    else:
                        response.args = response_to_string(
                            self._chr_dll, rsp_ref)
                    if response.ticket in self._sent_tickets:
                        self._sent_tickets[response.ticket][1](response)
                        self._sent_tickets[response.ticket][0] -= 1
                        if self._sent_tickets[response.ticket][0] == 0:
                            self._sent_tickets.pop(response.ticket, None)
                    elif response.ticket in Plugin.PLUGIN_TICKETS_SENT:
                        Plugin.PLUGIN_TICKETS_SENT[response.ticket][1](response)
                        Plugin.PLUGIN_TICKETS_SENT[response.ticket][0] -= 1
                        # Seem to be getting multiple updates for one command.
                        # if Plugin.PLUGIN_TICKETS_SENT[response.ticket][0] == 0:
                        #    Plugin.PLUGIN_TICKETS_SENT.pop(response.ticket, None)
                    else:
                        self.resp_callback(response)
                except Exception as ex:
                    print("Caught response callback exception:\n", ex)
                    signal.raise_signal(signal.SIGABRT)

            def data_callback(user, state, sample_count, sample_buffer, sample_size,
                              gen_signal_info: TSampleSignalGeneralInfo, signal_info):
                self._data_callback(user, state, sample_count, sample_buffer, sample_size, gen_signal_info, signal_info)

            self._gen_cb = DLL_GEN_CALLBACK_FUNC(general_callback)
            self._data_cb = DLL_DATA_CALLBACK_FUNC(data_callback)
            if self.resp_callback:
                self._chr_dll.RegisterGeneralResponseAndUpdateCallback(self.conn_handle, None, self._gen_cb)
            if self.data_callback:
                self._chr_dll.RegisterSampleDataCallback(
                    self.conn_handle, self.config.sample_cnt, self.config.timeout, None, self._data_cb)
            self._chr_dll.StartAutomaticDeviceOutputProcessing(
                self.conn_handle)

        print("OpenConnection: ", err, self.conn_handle)
        _check_raise_exception(self._chr_dll, err)
        return err

    def open_shared_connection(self, conn_mode: OperationMode, resp_callback: Callable = None,
                               data_callback=None) -> Union[AsynchronousConnection, SynchronousConnection]:
        """
        Creates and opens a shared connection from this connection.
        :param conn_mode: Operation mode for the new connection
        :type conn_mode: OperationMode
        :param resp_callback: General response callback in async mode
        :type resp_callback: function | None
        :param data_callback: Data callback function in async mode
        :type data_callback: function | None
        :return: Shared connection object
        :rtype: AsynchronousConnection | SynchronousConnection
        """
        new_config = copy.deepcopy(self.config)
        new_config.connection_mode = conn_mode
        conn = connection_from_config(config=new_config, resp_callback=resp_callback, data_callback=data_callback)
        conn.parent_conn_handle = self.conn_handle
        conn.open()
        return conn

    def close(self) -> int:
        """
        Closes the connection to the device.
        :return: Error code
        :rtype: int
        """
        cntr = 0
        while self.config.connection_mode == OperationMode.ASYNC and self._sent_tickets:
            time.sleep(0.5)
            cntr = cntr + 1
            if cntr > 10:
                raise Exception(
                    "Timed out waiting for outstanding commands to complete:", self._sent_tickets)

        self._chr_dll.CloseConnection.argtypes = [c_int]
        self._chr_dll.CloseConnection.restype = c_int
        err = self._chr_dll.CloseConnection(self.conn_handle)
        print("CloseConnection: ", err)
        _check_raise_exception(self._chr_dll, err)
        return err

    def send_command_string(self, cmd_str: str, resp_cb: Callable = None) -> Response:
        """
        Parses the supplied string as a command and sends it to device. The commands are 
        executed synchronously or asynchronously depending on the operation mode.
        :param cmd_str: The command string, for example, SODX 83 16640
        :type cmd_str: str
        :param resp_cb: Response callback for this specific command id
        :type resp_cb: function
        :return: Response
        :rtype: Response
        """
        rsp = send_command_string(self._chr_dll, self.conn_handle, cmd_str, self.config.connection_mode)
        _set_callback(rsp, self.resp_callback, resp_cb,
                      self.config.connection_mode, self._sent_tickets)
        _check_raise_exception(self._chr_dll, rsp.error_code)
        return rsp

    def send_command(self, cmd_id: Union[str, CmdId, int], args: Union[list, tuple] = None, query: int = 0,
                     resp_cb: Callable = None) -> Response:
        """
        Create and send a command with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId | int
        :param args: Arguments for the command
        :type args: list | tuple
        :param query: 1 = query, 0 = not a query
        :type query: int
        :param resp_cb: Response callback for this specific command id
        :type resp_cb: function
        :return: Response
        :rtype: Response
        """
        rsp = send_command(self._chr_dll, self.conn_handle, cmd_id, args, query, self.config.connection_mode)
        _set_callback(rsp, self.resp_callback, resp_cb,
                      self.config.connection_mode, self._sent_tickets)
        _check_raise_exception(self._chr_dll, rsp.error_code)
        return rsp

    def send_query(self, cmd_id: Union[str, CmdId], args: Union[list, tuple] = None,
                   resp_cb: Callable = None) -> Response:
        """
        Create and send a query command.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments for the query
        :type args: list | tuple
        :param resp_cb: Response callback for this specific command id
        :type resp_cb: function
        :return: Response
        :rtype: Response
        """
        rsp = send_command(self._chr_dll, self.conn_handle, cmd_id, args, 1, self.config.connection_mode)
        _set_callback(rsp, self.resp_callback, resp_cb,
                      self.config.connection_mode, self._sent_tickets)
        _check_raise_exception(self._chr_dll, rsp.error_code)
        return rsp

    def start_data_stream(self) -> int:
        """
        Start the samples stream
        :return: Error code
        :rtype: int
        """
        rsp = self.send_command(CmdId.START_DATA_STREAM)
        _check_raise_exception(self._chr_dll, rsp.error_code)
        return rsp.error_code

    def get_output_signal_infos(self, max_buf_sz: int = 1024) -> Tuple[GenSignalInfo, list]:
        """
        Get the output signal information.
        :param max_buf_sz: Max buffer size for the signal array.
        :type max_buf_sz: int
        :return: General signal info, list of signals
        :rtype: GenSignalInfo, list
        """
        self._chr_dll.GetConnectionOutputSignalInfos.argtypes = [c_uint, POINTER(TSampleSignalGeneralInfo),
                                                                 POINTER(c_char), POINTER(c_int64)]
        self._chr_dll.GetConnectionOutputSignalInfos.restype = c_int
        buf_sz = c_int64(max_buf_sz)
        sig_info_buf = (c_char * max_buf_sz)()
        err = self._chr_dll.GetConnectionOutputSignalInfos(self.conn_handle, byref(self._gen_info), sig_info_buf,
                                                           byref(buf_sz))
        _check_raise_exception(self._chr_dll, err)
        
        self._gen_sig_info = GenSignalInfo(self._gen_info.ChannelCount,
                                           self._gen_info.GlobalSignalCount,
                                           self._gen_info.InfoIndex,
                                           self._gen_info.PeakSignalCount)
        signal_ids = []
        num_signals = int(buf_sz.value / sizeof(TSampleSignalInfo))
        for idx in range(num_signals):
            sig_info = TSampleSignalInfo.from_buffer(sig_info_buf, idx * sizeof(TSampleSignalInfo))
            signal_ids.append((sig_info.DataType, sig_info.SignalID))
        self._sig_info = signal_ids

        return self._gen_sig_info, signal_ids

    def conn_last_errors(self, max_buf_size: int = 2048) -> list:
        """
        Get the last errors for this connection.
        :param max_buf_size: Max buffer size for the error array.
        :type max_buf_size: int
        :return: List of ErrorInfo
        :rtype: List
        """
        error_infos, err = last_errors(self._chr_dll, self.conn_handle, max_buf_size)
        _check_raise_exception(self._chr_dll, err)
        return error_infos

    def conn_clear_errors(self):
        """
        Clear the errors for this connection.
        :return: Error code
        :rtype:
        """
        err = clear_errors(self._chr_dll, self.conn_handle)
        _check_raise_exception(self._chr_dll, err)

    def stop_data_stream(self):
        """
        Stop the samples stream
        :return: Error code
        :rtype:
        """
        rsp = self.send_command(CmdId.STOP_DATA_STREAM)
        _check_raise_exception(self._chr_dll, rsp.error_code)

    def flush_connection_buffer(self) -> int:
        """
        Flush all the received samples and command response, move the read pointer to the 
        last valid position in the samples stream from the device.
        :return: Error code
        :rtype: c_int
        """
        self._chr_dll.FlushConnectionBuffer.argtypes = [c_uint]
        self._chr_dll.GetDeviceOutputSignals.restype = c_int
        err = self._chr_dll.FlushConnectionBuffer(self.conn_handle)
        _check_raise_exception(self._chr_dll, err)
        return err

    def activate_auto_buffer_mode(self, sample_cnt: int, flush_buffer: bool = False) -> int:
        """
        Automatically save samples from device to the target buffer.
        :param sample_cnt: Number of samples to be written to the buffer
        :type sample_cnt: int
        :param flush_buffer: Set to True to flush the buffer to read the newest args
        :type flush_buffer: Boolean
        :return: Auto buffer size in bytes
        :rtype: int
        """
        self._auto_recvd_sample_cnt = 0
        self.config.sample_cnt = sample_cnt
        bufsz = c_int64()

        if self.config.connection_mode == OperationMode.SYNC:
            self._chr_dll.ActivateAutoBufferMode.argtypes = [
                c_uint, c_void_p, c_int64, POINTER(c_int64)]
            self._chr_dll.ActivateAutoBufferMode.restype = c_int

            err = self._chr_dll.ActivateAutoBufferMode(
                self.conn_handle, None, sample_cnt, byref(bufsz))
        else:
            self._chr_dll.ActivateAsyncDataUserBuffer.argtypes = [
                c_uint, c_void_p, c_int64, POINTER(c_int64)]
            self._chr_dll.ActivateAsyncDataUserBuffer.restype = c_int

            err = self._chr_dll.ActivateAsyncDataUserBuffer(
                self.conn_handle, None, sample_cnt, byref(bufsz))

        # TODO - the APIs give a pointer not assigned error here, which is wrong.
        # if err < 0:
        #    raise APIException(self._chr_dll, err)

        tmp = bufsz.value
        if self._output_data_format == OutputDataMode.RAW:
            tmp = (tmp + 7) & (-8)
        # tmp = int(tmp / sizeof(c_int64))

        if self.auto_buffer.size != tmp:
            if self.config.connection_mode == OperationMode.SYNC:
                self._gen_sig_info, self._sig_info = self.get_output_signal_infos()
            if self._output_data_format == OutputDataMode.RAW:
                comp_dt = self._get_numpy_dt()
                comp_dt_sz = comp_dt.itemsize
                gd = gcd(comp_dt_sz, 8)
                tmp = int((comp_dt_sz * 8 / gd) * sample_cnt)
                tmp = (tmp + 7) & (-8)
                if tmp < bufsz.value:
                    raise Exception("Incorrect auto buffer size")
            self.auto_buffer = np.empty(tmp, dtype=np.int8)
        # This what gets passed to the DLL.
        self.auto_buffer.dtype = np.float64

        if flush_buffer and self.config.connection_mode == OperationMode.SYNC:
            self.flush_connection_buffer()

        if self.config.connection_mode == OperationMode.SYNC:
            self._chr_dll.ActivateAutoBufferMode.argtypes = [
                c_uint, Connection._nd_pointer_1, c_int64, POINTER(c_int64)]

            err = self._chr_dll.ActivateAutoBufferMode(
                self.conn_handle, self.auto_buffer, sample_cnt, byref(bufsz))
        else:
            self._chr_dll.ActivateAsyncDataUserBuffer.argtypes = [
                c_uint, Connection._nd_pointer_1, c_int64, POINTER(c_int64)]

            err = self._chr_dll.ActivateAsyncDataUserBuffer(
                self.conn_handle, self.auto_buffer, sample_cnt, byref(bufsz))

        # Adjust the numpy data type
        if self._output_data_format == OutputDataMode.RAW:
            self.auto_buffer.dtype = self._get_numpy_dt()

        _check_raise_exception(self._chr_dll, err)
        return bufsz.value

    def deactivate_auto_buffer_mode(self):
        """
        Manually quit automatic buffer samples save mode.
        :return:
        :rtype:
        """
        if self.config.connection_mode == OperationMode.SYNC:
            self._chr_dll.DeactivateAutoBufferMode.argtypes = [c_uint]
            self._chr_dll.DeactivateAutoBufferMode.restype = c_int
            err = self._chr_dll.DeactivateAutoBufferMode(
                self.conn_handle)
        else:
            self._chr_dll.DeactivateAsyncDataUserBuffer.argtypes = [c_uint]
            self._chr_dll.DeactivateAsyncDataUserBuffer.restype = c_int
            err = self._chr_dll.DeactivateAsyncDataUserBuffer(
                self.conn_handle)

        _check_raise_exception(self._chr_dll, err)

    def get_auto_buffer_saved_sample_count(self) -> int:
        """
        Get the number of the sample already saved into the buffer in the automatic buffer samples save
        mode.
        :return: Sample count
        :rtype: int
        """
        sample_cnt = c_int64()

        if self.config.connection_mode == OperationMode.SYNC:
            self._chr_dll.GetAutoBufferSavedSampleCount.argtypes = [
                c_uint, POINTER(c_int64)]
            self._chr_dll.GetAutoBufferSavedSampleCount.restype = c_int
            err = self._chr_dll.GetAutoBufferSavedSampleCount(
                self.conn_handle, byref(sample_cnt))
        else:
            self._chr_dll.GetAsyncDataUserBufferSavedSampleCount.argtypes = [
                c_uint, POINTER(c_int64)]
            self._chr_dll.GetAsyncDataUserBufferSavedSampleCount.restype = c_int
            err = self._chr_dll.GetAsyncDataUserBufferSavedSampleCount(
                self.conn_handle, byref(sample_cnt))

        _check_raise_exception(self._chr_dll, err)
        return sample_cnt.value

    def get_auto_buffer_status(self) -> int:
        """
        Get the status of automatic buffer samples save.
        :return: Status
        :rtype: int
        """
        self._chr_dll.GetAutoBufferStatus.argtypes = [c_uint]
        self._chr_dll.GetAutoBufferStatus.restype = c_int32
        status = self._chr_dll.GetAutoBufferStatus(self.conn_handle)
        return status

    @staticmethod
    def _sample_size_in_float64(sample_sz):
        return int(sample_sz / 8)

    def get_auto_buffer_samples(self, sample_cnt: int, sample_sz: int) -> np.ndarray:
        """
        Get the complete reshaped auto buffer array for easy access. Please note this is the buffer
        being used in auto buffer mode. This call does not fill the buffer. It may or may not contain
        any data samples. The purpose of this call is to obtain the auto buffer in the correct shape
        at the start of an auto buffer sample collection.
        :param sample_cnt: Number of samples that fit in buffer
        :type sample_cnt: int
        :param sample_sz: Size of one sample in bytes
        :type sample_sz: int
        :return: Reshaped auto buffer
        :rtype: ndarray
        """
        return self.auto_buffer.reshape(sample_cnt, Connection._sample_size_in_float64(sample_sz)) \
            if self._output_data_format == OutputDataMode.DOUBLE else self.auto_buffer

    def get_auto_buffer_new_samples(self, sample_size_in_bytes: int = 0) -> Union[Data, None]:
        """
        This method can be used to fetch the newly available samples in auto buffer mode.
        :param sample_size_in_bytes: Optional maximum number of bytes per sample size. If zero, the
        value will be obtained from the library
        :type sample_size_in_bytes: int
        :return: The newly added samples to the buffer or None.
        :rtype: Data | None
        """
        if sample_size_in_bytes == 0:
            sample_size_in_bytes = self.get_single_output_sample_size()

        if self.get_auto_buffer_status() == AutoBuffer.ERROR:
            raise Exception("Auto buffer error")

        cnt = self.get_auto_buffer_saved_sample_count()
        if cnt == 0:
            return None

        samples = self.get_auto_buffer_samples(self.config.sample_cnt, sample_size_in_bytes)
        if self._output_data_format == OutputDataMode.DOUBLE:
            data = Data(samples[self._auto_recvd_sample_cnt: cnt, :], cnt - self._auto_recvd_sample_cnt,
                        self._gen_sig_info,
                        self._sig_info, 0, self.dll_handle())
        else:
            data = Data(samples[self._auto_recvd_sample_cnt: cnt], cnt - self._auto_recvd_sample_cnt, self._gen_sig_info,
                        self._sig_info, 0, self.dll_handle())

        self._auto_recvd_sample_cnt += data.sample_cnt
        return data

    def dark_reference(self) -> Response:
        """
        Carry out a dark reference.
        :return: float number of frequency scan rate in Hertz at which the CCD would be saturated by the
        stray light, error code
        :rtype: Response
        """
        rsp = self.send_command(CmdId.DARK_REFERENCE)
        return rsp

    def get_device_output_signals(self) -> list:
        """
        Get the requested output signals.
        :return: List of signals, error code
        :rtype: list
        """
        self._chr_dll.GetDeviceOutputSignals.argtypes = [
            c_uint, POINTER(c_int), POINTER(c_int)]
        self._chr_dll.GetDeviceOutputSignals.restype = c_int
        # signal_ids = c_int()
        signal_ids = (c_int * 99)()
        signal_count = c_int(99)
        err = self._chr_dll.GetDeviceOutputSignals(
            self.conn_handle, signal_ids, byref(signal_count))
        _check_raise_exception(self._chr_dll, err)
        return list(signal_ids[:signal_count.value])

    def get_device_channel_count(self) -> int:
        """
        Get device measuring channel count.
        :return: Number of channels or error code
        :rtype: int
        """
        self._chr_dll.GetDeviceChannelCount.argtypes = [c_uint]
        self._chr_dll.GetDeviceChannelCount.restype = c_int
        err = self._chr_dll.GetDeviceChannelCount(self.conn_handle)
        return err

    def get_single_output_sample_size(self) -> int:
        """
        Get the size of each samples sample output by the library.
        :return: Sample size
        :rtype: int
        """
        self._chr_dll.GetSingleOutputSampleSize.argtypes = [
            c_uint, POINTER(c_longlong)]
        self._chr_dll.GetSingleOutputSampleSize.restype = c_int
        ret_sample_size = c_int64()
        err = self._chr_dll.GetSingleOutputSampleSize(
            self.conn_handle, byref(ret_sample_size))
        _check_raise_exception(self._chr_dll, err)
        return ret_sample_size.value

    def _get_numpy_dt(self):
        fmts = []
        peaks_fmt = []
        for idx, sig in enumerate(self._sig_info):
            if idx < self._gen_sig_info.global_sig_cnt:
                fmts.append(('g' + str(idx), DataType.to_numpy_dt_string(sig[0])))
            else:
                peaks_fmt.append(DataType.to_numpy_dt_string(sig[0]))
        if peaks_fmt:
            delim = ','
            fmts.append(('p', delim.join(peaks_fmt), self._gen_sig_info.channel_cnt))
        return np.dtype(fmts)

    def _update_signal_info(self, signal_info: POINTER(TSampleSignalInfo), sample_size: int):
        self._gen_sig_info = GenSignalInfo(self._gen_info.ChannelCount,
                                           self._gen_info.GlobalSignalCount,
                                           self._gen_info.InfoIndex,
                                           self._gen_info.PeakSignalCount)
        num_signals = self._gen_info.PeakSignalCount + self._gen_info.GlobalSignalCount
        signal_ids = []
        for idx in range(num_signals):
            signal_ids.append(
                (signal_info[idx].DataType, signal_info[idx].SignalID))
        self._sig_info = signal_ids

        # Handle raw output data format
        if self._output_data_format == OutputDataMode.DOUBLE:
            self._data_size = self._gen_info.ChannelCount * self._gen_info.PeakSignalCount + \
                              self._gen_info.GlobalSignalCount
        else:
            self._data_size = sample_size
            self._numpy_dt = self._get_numpy_dt()

    def _get_output_ndarray(self, sample_cnt: int, pp_data: POINTER(c_double)):
        if self._output_data_format == OutputDataMode.DOUBLE:
            return np.ctypeslib.as_array(pp_data, (sample_cnt, self._data_size))
        else:
            buffer_as_ctypes_array = cast(pp_data, POINTER(c_char * self._data_size * sample_cnt))[0]
            return np.frombuffer(buffer_as_ctypes_array, self._numpy_dt)

    def get_next_samples(self, sample_count: int, flush_buffer_on_error: bool = False) -> Union[Data, None]:
        """
        Get the next available samples from the device.
        :param sample_count: Number of samples to be read
        :type sample_count:  int
        :param flush_buffer_on_error: If this is set to true, in case of an error the buffer is flushed and
        retired once.
        :type flush_buffer_on_error: True if the buffer should be flushed and retried, otherwise false
        :return: Sample metadata and args or None
        :rtype: Data | None
        """
        info_idx = self._gen_info.InfoIndex
        pp_data = POINTER(c_double)()
        signal_info = POINTER(TSampleSignalInfo)()
        ret_sp_cnt = c_int64(sample_count)
        size_per_sample = c_int(0)
        err = self._chr_dll.GetNextSamples(self.conn_handle, byref(ret_sp_cnt), byref(pp_data),
                                           byref(size_per_sample),
                                           byref(self._gen_info), byref(signal_info))
        if err < 0:
            if not flush_buffer_on_error:
                raise APIException(self._chr_dll, err)

            self.flush_connection_buffer()
            print("Flushed data")
            err = self._chr_dll.GetNextSamples(self.conn_handle, byref(ret_sp_cnt), byref(pp_data),
                                               byref(size_per_sample),
                                               byref(self._gen_info), byref(signal_info))
            _check_raise_exception(self._chr_dll, err)

        if ret_sp_cnt.value > 0:
            if info_idx != self._gen_info.InfoIndex:
                self._update_signal_info(signal_info, size_per_sample.value)
            data_list = self._get_output_ndarray(ret_sp_cnt.value, pp_data)
            return Data(data_list, ret_sp_cnt.value, self._gen_sig_info, self._sig_info, err, self._chr_dll)
        else:
            if info_idx != self._gen_info.InfoIndex:
                self._update_signal_info(signal_info, size_per_sample.value)
            return None

    def get_last_sample(self) -> Union[Data, None]:
        """
        Get the last sample received from device
        :return: Sample metadata and args
        :rtype: Data | None
        """
        info_idx = self._gen_info.InfoIndex
        self._chr_dll.GetLastSample.argtypes = [c_uint, POINTER(POINTER(c_double)),
                                                POINTER(c_int), POINTER(TSampleSignalGeneralInfo),
                                                POINTER(POINTER(TSampleSignalInfo))]
        self._chr_dll.GetLastSample.restype = c_int
        pp_data = POINTER(c_double)()
        signal_info = POINTER(TSampleSignalInfo)()
        size_per_sample = c_int(0)
        err = self._chr_dll.GetLastSample(self.conn_handle, byref(pp_data), byref(size_per_sample),
                                          byref(self._gen_info), byref(signal_info))
        _check_raise_exception(self._chr_dll, err)

        if info_idx != self._gen_info.InfoIndex:
            self._update_signal_info(signal_info, size_per_sample.value)

        if err == 0 or not pp_data.contents:
            return None

        data_list = self._get_output_ndarray(1, pp_data)
        return Data(data_list, 1, self._gen_sig_info, self._sig_info, err, self._chr_dll)

    def download_spectrum(self, spec_type: int, first_channel: int = 0, last_channel: int = 1,
                          resp_cb: Callable = None) -> Response:
        """
        Download spectrum from a device
        :param spec_type: Type of spectrum
        :type spec_type: int
        :param first_channel: First channel
        :type first_channel: int
        :param last_channel: Last channel
        :type last_channel: int
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used)
        :type resp_cb: function
        :return: Response
        :rtype: Response
        """

        cmd_h, err = new_command(self._chr_dll, CmdId.DOWNLOAD_SPECTRUM, 1)
        if err != 0:
            rsp = Response(CmdId.DOWNLOAD_SPECTRUM, self._chr_dll)
            rsp.error_code = err
            return rsp
        err = add_command_int_arg(self._chr_dll, cmd_h, spec_type)
        if err != 0:
            rsp = Response(CmdId.DOWNLOAD_SPECTRUM, self._chr_dll)
            rsp.error_code = err
            return rsp
        err = add_command_int_arg(self._chr_dll, cmd_h, first_channel)
        if err != 0:
            rsp = Response(CmdId.DOWNLOAD_SPECTRUM, self._chr_dll)
            rsp.error_code = err
            return rsp
        err = add_command_int_arg(self._chr_dll, cmd_h, last_channel)
        if err != 0:
            rsp = Response(CmdId.DOWNLOAD_SPECTRUM, self._chr_dll)
            rsp.error_code = err
            return rsp

        rsp = send_prepared_command(self._chr_dll, self.conn_handle, CmdId.DOWNLOAD_SPECTRUM, cmd_h,
                                    self.config.connection_mode)
        if self.config.connection_mode == OperationMode.ASYNC:
            _set_callback(rsp, self.resp_callback, resp_cb,
                          self.config.connection_mode, self._sent_tickets)

        _check_raise_exception(self._chr_dll, rsp.error_code)
        return rsp

    def add_plugin(self, plugin_info: PluginInfo) -> Plugin:
        """
        Add a plugin with the given name.
        :param plugin_info: Information about the plugin to be added
        :type plugin_info: PluginInfo
        :return: Plugin
        :rtype: Plugin
        """
        self._chr_dll.AddConnectionPlugIn.argtypes = [
            c_uint, c_char_p, POINTER(c_uint)]
        self._chr_dll.AddConnectionPlugIn.restype = c_int
        b_name = plugin_info.name.encode('utf-8')
        plugin_h = c_uint()
        err = self._chr_dll.AddConnectionPlugIn(
            self.conn_handle, b_name, byref(plugin_h))

        _check_raise_exception(self._chr_dll, err)
        return Plugin(plugin_info.id, self._chr_dll, plugin_h.value, self.config.connection_mode, self.resp_callback)

    def add_plugin_with_id(self, plugin_info: PluginInfo) -> Plugin:
        """
        Add a plugin with the given ID.
        :param plugin_info: Information about the plugin to be added
        :type plugin_info: PluginInfo
        :return: Plugin
        :rtype: Plugin
        """
        self._chr_dll.AddConnectionPlugInByID.argtypes = [
            c_uint, c_uint, POINTER(c_uint)]
        self._chr_dll.AddConnectionPlugInByID.restype = c_int
        plugin_h = c_uint()
        err = self._chr_dll.AddConnectionPlugInByID(self.conn_handle, plugin_info.id, byref(plugin_h))

        _check_raise_exception(self._chr_dll, err)
        return Plugin(plugin_info.id, self._chr_dll, plugin_h.value, self.config.connection_mode, self.resp_callback)

    def set_sample_buffer_size(self, buffer_size: int) -> int:
        """
        Set the size of buffer in bytes to store data samples.
        :param buffer_size: Library config flags.
        :type buffer_size: int
        :return: error code
        :rtype: int
        """
        self._chr_dll.SetSampleDataBufferSize.argtypes = [c_uint, c_int64]
        self._chr_dll.SetSampleDataBufferSize.restype = c_int
        c_buf_sz = c_int64(buffer_size)
        err = self._chr_dll.SetSampleDataBufferSize(self.conn_handle, c_buf_sz)
        _check_raise_exception(self._chr_dll, err)
        return err

    def get_conf(self) -> list:
        """
        Executes the CONF command and returns a list of responses that are generated as a result.
        :return: List of all responses as a result of the CONF command
        :rtype: list
        """
        responses = []
        event = threading.Event()

        def sodx_callback(rsp: Response):
            responses.append(rsp)
            event.set()

        def conf_resp_callback(rsp: Response):
            if rsp.cmd_id == CmdId.SODX:
                return

            responses.append(rsp)
            if rsp.cmd_id == CmdId.CONF:
                event.set()

        def conf_data_callback(data: Data):
            pass

        if self.config.connection_mode == OperationMode.ASYNC:
            self.send_query(CmdId.SODX, resp_cb=sodx_callback)
            no_time_out = event.wait(5.0)
            if not no_time_out:
                raise Exception("Timed out waiting for SODX response in get_conf")
        else:
            resp = self.send_query(CmdId.SODX)
            responses.append(resp)

        sh_conn = self.open_shared_connection(conn_mode=OperationMode.ASYNC,
                                              resp_callback=conf_resp_callback, data_callback=conf_data_callback)
        with sh_conn:
            sh_conn.exec('CONF')
            no_time_out = event.wait(5.0)
            if not no_time_out:
                raise Exception("Timed out in get_conf")
        return responses

    def set_output_data_format_mode(self, output_mode: OutputDataMode) -> int:
        """
        Set the output data format mode.
        :param output_mode: The output format mode to set
        :type output_mode: OutputDataMode
        :return: Error code
        :rtype: int
        """
        self._chr_dll.SetOutputDataFormatMode.argtypes = [c_uint, c_int32]
        self._chr_dll.SetOutputDataFormatMode.restype = c_int
        err = self._chr_dll.SetOutputDataFormatMode(self.conn_handle, output_mode)
        if err < 0:
            raise APIException(self._chr_dll, err)

        self._output_data_format = output_mode
        self.auto_buffer = np.empty(1)
        _check_raise_exception(self._chr_dll, err)
        return err

    def get_output_data_format_mode(self) -> OutputDataMode:
        """
        Get the data output format mode.
        :return: Output data format mode
        :rtype: OutputDataMode
        """
        self._chr_dll.GetOutputDataFormatMode.argtypes = [c_uint]
        self._chr_dll.GetOutputDataFormatMode.restype = c_int
        err = self._chr_dll.GetOutputDataFormatMode(self.conn_handle)
        if err < 0:
            raise APIException(self._chr_dll, err)

        self._output_data_format = OutputDataMode.DOUBLE if err == 0 else OutputDataMode.RAW
        return self._output_data_format


class SynchronousConnection(Connection):
    """
    This class is derived from Connection and handles the communication with a device in
    synchronous operation mode. Although the connection class can be directly used,
    it is recommended to use this class when synchronous operation is desired.
    """

    def __init__(self, *, config: ConnectionConfig, dll_h=None):
        super().__init__(config=config, resp_callback=None, data_callback=None, dll_h=dll_h)

    def __enter__(self):
        Connection.__enter__(self)
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        Connection.__exit__(self, exc_type, exc_val, exc_tb)

    def exec(self, cmd_id: Union[str, CmdId], *args) -> Response:
        """
        Execute a command with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments
        :type args: Arguments
        :return: Response
        :rtype: Response
        """
        return super().send_command(cmd_id, args, 0)

    def exec_from_string(self, cmd_str: str) -> Response:
        """
        Execute a command from the given string
        :param cmd_str: Command string
        :type cmd_str: str
        :return: Response
        :rtype: Response
        """
        return super().send_command_string(cmd_str)

    def query(self, cmd_id: Union[str, CmdId], *args) -> Response:
        """
        Execute a query with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: CmdId | str
        :param args: Arguments
        :type args: Arguments
        :return: Response
        :rtype: Response
        """
        return super().send_query(cmd_id, args)

    def wait_get_auto_buffer_samples(self, sample_cnt: int, timeout: float = 0.0, pause_sec: float = 0.005,
                                     flush_buffer: bool = True) -> Union[Data, None]:
        """
        Obtain samples in an external buffer managed by the connection object as opposed to an internal DLL buffer.
        The auto buffer functionality of the DLL is used for this purpose. Since the connection object is the owner
        of the args buffer, it will not be overwritten by the DLL. This method waits till all the expected samples
        have been collected or a timeout occurs.
        :param sample_cnt: Maximum number of samples to get
        :type sample_cnt: int
        :param timeout: Maximum amount of time in seconds to wait for the sample buffer to be filled. This value
        can be a fraction. If in the given timeout, sufficient samples are not available, no samples are returned.
        If timeout is zero, the function returns with available samples.
        :type timeout: float
        :param pause_sec: To avoid busy looping while waiting for the samples to be filled, the wait loop periodically
        sleeps for this time interval in seconds between checks. This value can be a fraction.
        :type pause_sec: float
        :param flush_buffer: In case only the latest samples are desired, this can be set to true
        :type flush_buffer: True if old samples should be ignored, False otherwise
        :return: Samples or None if no samples are available
        :rtype: Data | None
        """
        self.auto_recvd_sample_cnt = 0

        self.activate_auto_buffer_mode(sample_cnt, flush_buffer)

        sample_sz = self.get_single_output_sample_size()

        if timeout > 0.0:
            start_time = time.time()
            while self.get_auto_buffer_status() != AutoBuffer.FINISHED:
                time.sleep(pause_sec)
                check_time = time.time()
                if check_time - start_time > timeout:
                    break
                if self.get_auto_buffer_status() == AutoBuffer.ERROR:
                    raise Exception("Auto buffer error")

        self.auto_recvd_sample_cnt = self.get_auto_buffer_saved_sample_count()

        if sample_cnt == 0:
            return None

        if sample_cnt > self.config.sample_cnt:
            raise Exception("Sample count mismatch: " +
                            str(self.config.sample_cnt) + ' : ' + str(sample_cnt))

        samples = self.get_auto_buffer_samples(self.config.sample_cnt, sample_sz)
        return Data(samples, self.auto_recvd_sample_cnt, self._gen_sig_info, self._sig_info, 0, self._chr_dll)


class AsynchronousConnection(Connection):
    """
    This class is derived from Connection and handles the communication with a device in
    asynchronous operation mode. Although the connection class can be directly used,
    it is recommended to use this class when asynchronous operation is desired.
    """

    def __init__(self, *, config: ConnectionConfig, resp_callback, data_callback, dll_h=None):
        super().__init__(config=config, resp_callback=resp_callback, data_callback=data_callback, dll_h=dll_h)
        #if resp_callback is None or data_callback is None:
        #    raise Exception('Callbacks cannot be None for an async connection')

    def __enter__(self):
        Connection.__enter__(self)
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        Connection.__exit__(self, exc_type, exc_val, exc_tb)

    def get_next_samples(self, sample_count, flush_buffer_on_error=False) -> Data:
        """
        This method is not allowed in async mode and will raise an exception.
        """
        raise Exception('Not supported in async mode')

    def get_last_sample(self) -> Data:
        """
        This method is not allowed in async mode and will raise an exception.
        """
        raise Exception('Not supported in async mode')

    def get_auto_buffer_status(self):
        """
        This method is not allowed in async mode and will raise an exception.
        """
        raise Exception('Not supported in async mode')

    def exec(self, cmd_id: Union[str, CmdId], *args, resp_cb: Callable = None) -> Response:
        """
        Execute a command with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments
        :type args: Arguments
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used if set)
        :type resp_cb: function
        :return: Response (this is a partial response, the complete response is delivered when the reply is received)
        :rtype: Response
        """
        return super().send_command(cmd_id, args, 0, resp_cb)

    def exec_from_string(self, cmd_str: str, resp_cb: Callable = None) -> Response:
        """
        Execute a command from the given string.
        :param cmd_str: Command string
        :type cmd_str: str
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used if set)
        :type resp_cb: function
        :return: Response (this is a partial response, the complete response is delivered when the reply is received)
        :rtype: Response
        """
        return super().send_command_string(cmd_str, resp_cb)

    def query(self, cmd_id: Union[str, CmdId], *args, resp_cb: Callable = None) -> Response:
        """
        Execute a query with the given arguments.
        :param cmd_id: Command ID
        :type cmd_id: str | CmdId
        :param args: Arguments
        :type args: Arguments
        :param resp_cb: Response callback in async mode (if not specified, the general callback is used if set)
        :type resp_cb: function
        :return: Response (this is a partial response, the complete response is delivered when the reply is received)
        :rtype: Response
        """
        return super().send_command(cmd_id, args, 1, resp_cb)


def connection_from_config(config: ConnectionConfig, resp_callback: Callable = None, data_callback: Callable = None,
                           dll_h: CDLL = None) -> Union[SynchronousConnection, AsynchronousConnection]:
    """
    A factory function to create a connection object from the given configuration that handles
    communication with a CHRocodile device. Depending on the conn_mode a SynchronousConnection 
    or an AsynchronousConnection will be returned.
    :param config: Connection configuration
    :type config: ConnectionConfig
    :param resp_callback: General response callback in async mode. If None general response callback will not be set.
    :type resp_callback: function | None
    :param data_callback: Data callback in async mode
    :type data_callback: function
    :param dll_h: User supplied handle to chrocodile client lib
    :type dll_h: CDLL
    :return: A connection object
    :rtype: SynchronousConnection or AsynchronousConnection
    """
    if config.connection_mode == OperationMode.SYNC:
        return SynchronousConnection(config=config, dll_h=dll_h)
    else:
        return AsynchronousConnection(config=config, resp_callback=resp_callback, data_callback=data_callback,
                                      dll_h=dll_h)


def connection_from_params(addr: str, device_type: DeviceType = DeviceType.CHR_MULTI_CHANNEL,
                           conn_mode: OperationMode = OperationMode.SYNC, buf_size_bytes: int = 0,
                           sample_cnt: int = 32000, timeout: int = 10,
                           resp_callback: Callable = None, data_callback: Callable = None, dll_path: str = None,
                           dll_h: CDLL = None, ini_file: str = None,
                           log_level : int = 2,
                           log_path: str = None, max_log_file_sz: int = None, max_no_log_files: int = None,
                           async_auto_buffer: bool = True,
                           async_auto_activate_buffer: bool = True) -> Union[
    SynchronousConnection, AsynchronousConnection]:
    """
    A factory function to create a connection object from the given parameters that handles
    communication with a CHRocodile device. Depending on the conn_mode a SynchronousConnection 
    or an AsynchronousConnection will be returned.
    :param addr: Address of the device, for example, '192.168.170.3' or '192.168.170.3:7891, Port: 7891'
    :type addr: str
    :param device_type: The type of device, for example, ConnectionConfig.CHR_MULTI_CHANNEL_DEVICE.
    :type device_type: Refer ConnectionConfig
    :param conn_mode: Mode of operation, sync or async.
    :type conn_mode: Refer ConnectionConfig
    :param buf_size_bytes:  The size of library internal buffer for storing device output 
    (only powers of 2 allowed (e.g. 16x1024x1024)). By default (when <=0 ), 32MB.
    :type buf_size_bytes: int
    :param sample_cnt: Maximum samples to retrieve in one args call
    :type sample_cnt: int
    :param timeout: Timeout in ms for waiting for number of specified samples to be read.
    If set to 0, only available samples are read without waiting.
    :type timeout: int
    :param resp_callback: General response callback in async mode. If None general response callback will not be set.
    :type resp_callback: function | None
    :param data_callback: Data callback in async mode
    :type data_callback: function
    :param dll_path: Full path to DLL (by default taken from this file's location). The path should include the
    name of the Chrocodile shared library, for example, C:/mypath/Chrocodile.dll.
    The default location for the shared library is the location of chrdll4 package root directory.
    :type dll_path: str
    :param dll_h: User supplied handle to chrocodile client lib
    :type dll_h: CDLL
    :param ini_file: Path to the DLL ini file
    :type ini_file: str
    :param log_level: chrocodile client lib log level
    :type log_level: int
    :param log_path: Path to log directory
    :type log_path: str
    :param max_log_file_sz:  Maximum size of one log file in KiB
    :type max_log_file_sz: int
    :param max_no_log_files: Maximum number of log files in the directory. Older files will be deleted. 
    :type max_no_log_files: int
    :param async_auto_buffer: Flag to use auto buffer in async operation mode.
    :type async_auto_buffer: bool
    :param async_auto_activate_buffer: Flag to automatically auto buffer in async operation mode when full.
    :type async_auto_activate_buffer: bool
    :return: A connection object
    :rtype: SynchronousConnection or AsynchronousConnection
    """
    config = ConnectionConfig()
    config.address = addr
    config.device_type = device_type
    config.connection_mode = conn_mode
    config.buf_size_bytes = buf_size_bytes
    config.sample_cnt = sample_cnt
    config.timeout = timeout
    config.chr_dll_path = dll_path
    config.chr_ini_path = ini_file
    config.chr_log_level = log_level
    config.chr_log_path = log_path
    config.chr_max_log_file_size = max_log_file_sz
    config.chr_max_no_log_files = max_no_log_files
    config.async_auto_buffer = async_auto_buffer
    config.async_auto_activate_buffer = async_auto_activate_buffer
    return connection_from_config(config=config, resp_callback=resp_callback, data_callback=data_callback, dll_h=dll_h)
