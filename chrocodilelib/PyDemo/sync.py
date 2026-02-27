# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for receiving device data.
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


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-a", help="Device address")
    parser.add_argument("-p", help="Path to chrocodile lib")

    # Set the signal handler for ctl-c
    signal.signal(signal.SIGINT, handler)

    # Default CLS2
    addr = "192.168.170.3"
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

        resp = conn.exec_from_string("MMD 0")
        print(resp)

        resp = conn.exec_from_string("NOP 1")
        print(resp)

        resp = conn.exec_from_string("SHZ 40000")
        print(resp)

        resp = conn.exec_from_string("AVD 100")
        print(resp)

        resp = conn.query("ENC", 0, 0)
        print(resp)

        # conn.set_output_signals([83, 16640, 16641])
        resp = conn.exec_from_string("SODX 83 16640 16641")
        print(resp)

        resp = conn.exec("SODX", [83, 16640, 16641])
        print(resp)

        resp = conn.query("FLTC")
        print(resp)

        resp = conn.query(CmdId.FLTC)
        print(resp)

        resp = conn.exec_from_string("FLTC ?")
        print(resp)

        resp = conn.query("SODX")
        print(resp)
        print("First signal id:", resp.args[0])

        err = conn.set_sample_buffer_size(64 * 1024 * 1024)
        if err != 0:
            raise APIException(conn.dll_handle(), err)

        # Counter is used to reduce the frequency of print statement below.
        ctr = 0
        # Now start reading incoming samples till stopped.
        while keep_going:
            # The sample_data object is of type Data (chr_utils.py) and it contains
            # metadata and the sensor args. The samples are returned as numpy array - samples member in Data.
            sample_data = conn.get_next_samples(10, False)

            if sample_data is None:
                continue

            if sample_data.error_code < 0:
                raise APIException(sample_data.error_code)

            # If samples are available do something with the args.
            if sample_data.sample_cnt > 0:
                tmp = 0
                # Just find the maximum value in each sample args row and sum it.
                for i in range(sample_data.sample_cnt):
                    max_val = np.amax(sample_data.samples[i])
                    tmp = tmp + max_val

                # Print some info.
                ctr = ctr + 1
                if ctr > 10:
                    # If the args rate is high, print less.
                    # print(sample_data.sample_cnt, sample_data.error_code, tmp)
                    print(sample_data)
                    ctr = 0
