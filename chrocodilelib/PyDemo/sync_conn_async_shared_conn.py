# -*- coding: utf-8 -*-
"""
This script demonstrates the use of async mode for a shared connection for receiving data. The shared connection
is created using a sync physical connection.
"""

import argparse

from context import chrpy
from chrpy.chr_connection import *
from chrpy.chr_cmd_id import *
from chrpy.chr_utils import *

keep_going = True


# Handler for catching signal ctl-c
def handler(signum, frame):
    global keep_going
    keep_going = False


def shared_resp_callback(resp: Response):
    print("Shared resp:\n", resp)
    pass


def shared_data_callback(data: Data):
    print("Shared data:\n", data)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')

    # Set the signal handler for ctl-c
    signal.signal(signal.SIGINT, handler)

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

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    # By default, the connection is opened in synchronous mode.
    # Since synchronous mode is implicitly set, the connection type is SynchronousConnection
    with connection_from_params(addr=addr, dll_path=path_to_lib) as conn:
        # Send some commands  and queries and print the response object. It is of type Response (chr_utils.py)

        resp = conn.exec("NOP", 1)
        print(resp)
        print("Num peaks=", resp.args[0])

        resp = conn.exec_from_string('NOP 1')
        print(resp)

        resp = conn.query('ENC', 0, 0)
        print(resp)

        resp = conn.query("FLTC")
        print(resp)

        resp = conn.query(CmdId.FLTC)
        print(resp)

        resp = conn.exec_from_string('FLTC ?')
        print(resp)

        err = conn.set_sample_buffer_size(64 * 1024 * 1024)
        if err != 0:
            raise APIException(conn.dll_handle(), err)

        err = set_lib_log_level(conn.dll_handle(), 1)
        if err != 0:
            raise APIException(conn.dll_handle(), err)

        shared_conn = conn.open_shared_connection(conn_mode=OperationMode.ASYNC, resp_callback=shared_resp_callback,
                                                  data_callback=shared_data_callback)
        with shared_conn:
            shared_conn.exec('SODX', 83, 16640, 16641, resp_cb=shared_resp_callback) # pylint: disable=unexpected-keyword-arg

            # Now do callbacks.
            while keep_going:
                time.sleep(0.5)

            shared_conn.get_conf()
            time.sleep(2)

        conn.exec('SODX', 83, 16640, 16641)
        conf_resp = conn.get_conf()
        print("Conf response:")
        for resp in conf_resp:
            print(resp)
