# -*- coding: utf-8 -*-

"""
This script demonstrates the use of a test plugin in async mode.
"""

import argparse
import os.path

from context import chrpy
from chrpy.chr_cmd_id import CmdId

from chrpy.chr_connection import *
from chrpy.chr_dll import *
from chrpy.chr_plugins import Test_Plugin


# The general purpose callback for receiving responses asynchronously.
def gen_callback(cb: Response):
    print(cb)
    print('First argument in response=', cb.args[0] if cb.args else None)


# The args callback for receiving args asynchronously. The samples are returned as numpy array - samples member in Data.
def data_callback(cb: Data):
    # print(cb)
    pass


# Define a separate callback for plugin command response
def plugin_callback(cb: Response):
    print('Plugin response\n', cb)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')

    # Default CLS2
    addr = '192.168.170.3'
    # Default expected in the chrpy module location
    path_to_lib = None
    args = parser.parse_args()
    if args.a is not None:
        addr = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        path_to_lib = args.p

    # The plugins are found by default relative to the location of the chrocodile lib.
    chr_dll_dir = os.path.dirname(get_abs_dll_path(path_to_lib))
    os.chdir(chr_dll_dir)

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    # The responses and args are received in the callback functions above.
    # Since asynchronous mode is set the connection type is AsynchronousConnection
    with connection_from_params(addr=addr, conn_mode=OperationMode.ASYNC, resp_callback=gen_callback,
                                data_callback=data_callback, dll_path=path_to_lib) as conn:
        # Request some signals.
        # conn.set_output_signals([83, 16640, 16641])
        conn.exec('SODX', 83, 16640, 16641)

        plugin = conn.add_plugin_with_id(Test_Plugin)
        
        plugin.exec('TSTA', resp_cb=plugin_callback)
        plugin.exec('TSTB', resp_cb=plugin_callback)
        plugin.exec('TSTC', resp_cb=plugin_callback)
        plugin.exec('TSTD', resp_cb=plugin_callback)
        plugin.exec('TSTE', resp_cb=plugin_callback)
        
        # Wait for q or ctl-c to exit
        try:
            if input() == 'q':
                sys.exit(1)
        except KeyboardInterrupt:
            sys.exit(1)
