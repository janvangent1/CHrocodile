# -*- coding: utf-8 -*-

"""
This script demonstrates the use of async mode for receiving device args using the auto buffer functionality.
"""

import argparse

from context import chrpy
from chrpy.chr_cmd_id import CmdId

from chrpy.chr_utils import chr_warning, chr_error

from chrpy.chr_connection import *


# The general purpose callback for receiving responses asynchronously.
def gen_callback(cb: Response):
    print("Gen callback\n", cb)
    if cb.args:
        print('First argument in response=', cb.args[0])


# This is a different callback for SODX command below
def sodx_callback(cb: Response):
    print("Special SODX callback\n", cb)


# The args callback for receiving args asynchronously. The samples are returned as numpy array - samples member in Data.
def data_callback(cb: Data):
    print(cb)
    if cb is None:
        return
    
    if chr_warning(cb.error_code):
        print("Lib auto flushed buffer")
        # Just for debugging
        signal.raise_signal(signal.SIGABRT)
    elif chr_error(cb.error_code):
        print("Lib error in processing device output")
        # Just for debugging
        signal.raise_signal(signal.SIGABRT)

    if cb.error_code == ReadData.BUFFER_FULL:
        print("In case the config async_auto_activate_buffer flag is set,"
              " after this callback the auto buffer will be internally reset,"
              " copy data if needed later."
              " If not, please make sure that the after the data has been"
              " worked on the auto buffer is reactivated with the connection"
              " reset_async_auto_buffer method to continue receiving data.")


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')
    parser.add_argument('-o', help='Output data format')

    # Default CLS2
    addr = '192.168.170.3'
    # Default expected in the chrpy module location
    path_to_lib = None
    output_format = OutputDataMode.DOUBLE
    args = parser.parse_args()
    if args.a is not None:
        addr = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        path_to_lib = args.p
    if args.o is not None and int(args.o) == 1:
        output_format = OutputDataMode.RAW

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    # The callbacks can be class methods too.
    # The responses and args are received in the callback functions above.
    # Since asynchronous mode is set the connection type is AsynchronousConnection
    with connection_from_params(addr=addr, conn_mode=OperationMode.ASYNC, resp_callback=gen_callback, sample_cnt=16000,
                                data_callback=data_callback, dll_path=path_to_lib, async_auto_buffer=True) as conn:
        conn.set_output_data_format_mode(output_format)
        # Request some signals.
        # conn.set_output_signals([83, 16640, 16641])
        conn.exec('SODX', [83, 16640, 16641], resp_callback=sodx_callback) # pylint: disable=unexpected-keyword-arg

        # Set the number of peaks to 1
        # Different ways to do it
        conn.exec('NOP', 1)
        conn.exec_from_string(cmd_str='NOP 1')
        conn.exec(CmdId.NUMBER_OF_PEAKS, 1)

        # Query FLTC, different ways to do it.
        conn.query('FLTC')
        conn.query(CmdId.FLTC)
        conn.exec_from_string('FLTC ?')

        # This will throw an exception as it is not supported in async mode.
        # conn.get_next_samples(10)

        # Wait for q or ctl-c to exit
        try:
            if input() == 'q':
                sys.exit(1)
        except KeyboardInterrupt:
            sys.exit(1)
