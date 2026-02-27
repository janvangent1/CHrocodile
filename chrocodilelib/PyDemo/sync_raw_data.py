# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for receiving device data in raw output mode.
The advantage of this mode is smaller memory usage depending on the data type of signals. The data type
of sample is the data type delivered by the device and not double as default.
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
        # Send some commands  and queries and print the response object. It is of type Response (chr_utils.py)

        err = conn.set_output_data_format_mode(OutputDataMode.RAW)
        if err != 0:
            raise APIException(conn.dll_handle(), err)

        resp = conn.exec("NOP", 2)
        print(resp)
        print("Num peaks=", resp.args[0])

        resp = conn.exec('SODX', 83, 76, 16640, 16641, 16648)
        print(resp)

        resp = conn.query('SODX')
        print(resp)
        print('First signal id:', resp.args[0])

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

            ctr += 1
            if ctr < 1:
                continue
            ctr = 0

            # print(sample_data.sample_cnt, '\n', sample_data.samples['p']['f1'])
            # print("copy=", True if sample_data.samples['p'][0]['f1'].base is None else False )
            # print(sample_data)
            peak_sig = 16640
            peak_sig2 = 16641
            print(sample_data.sample_cnt, peak_sig, 'Middle sample\n', 
                  sample_data.get_signal_values(peak_sig, int(sample_data.sample_cnt/2)))
            global_sig = 83
            print(sample_data.sample_cnt, global_sig, '\n', sample_data.get_signal_values_all(global_sig))
            print(sample_data.sample_cnt, peak_sig2, 'All\n', sample_data.get_signal_values_all(peak_sig2))

        resp = conn.exec("NOP", 1)
        print(resp)
        print("Num peaks=", resp.args[0])
            
