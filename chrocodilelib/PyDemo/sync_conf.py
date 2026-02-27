# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for receiving conf responses
"""

import argparse

from context import chrpy
from chrpy.chr_connection import *
from chrpy.chr_cmd_id import *

keep_going = True


# Handler for catching signal ctl-c
def handler(signum, frame):
    global keep_going
    keep_going = False


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
        # Send conf and collect all responses.
        responses = conn.get_conf()
        for resp in responses:
            print(resp)
