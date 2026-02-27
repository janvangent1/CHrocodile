# -*- coding: utf-8 -*-

"""
This script demonstrates the use of auto buffer functionality in sync mode for receiving device args.
The data is used after the expected number of samples have been collected.
"""

import argparse

from context import chrpy

from chrpy.chr_cmd_id import *
from chrpy.chr_connection import *

keep_going = True


# ctl-c signal handler.
def handler(signum, frame):
    global keep_going
    keep_going = False


if __name__ == '__main__':
    # The connection configuration. Instead of specifying connection parameters individually, one can also use
    # a ConnectionConfig object
    config = ConnectionConfig()
    config.address = '192.168.170.3'

    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')
    parser.add_argument('-o', help='Output data format')

    # Set the signal handler.
    signal.signal(signal.SIGINT, handler)

    output_format = OutputDataMode.DOUBLE
    args = parser.parse_args()
    if args.a is not None:
        config.address = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        config.chr_dll_path = args.p
    if args.o is not None and int(args.o) == 1:
        output_format = OutputDataMode.RAW

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    with connection_from_config(config=config) as conn:
        conn.set_output_data_format_mode(output_format)
        # Set the signals.
        resp = conn.exec('SODX', 83, 16640, 16641)
        print(resp)

        signals = conn.get_device_output_signals()
        print("Signals:", signals)

        max_samples_to_request = 10
        while keep_going:
            # Get the next max_samples_to_request samples, with a maximum wait of 1 second.
            # The samples are saved in a buffer managed by the connection instead of an internal DLL buffer.
            # The args object is of type Data (chr_utils.py) and it contains
            # metadata and the sensor args. The samples are returned as numpy array - samples member in Data.
            # This is a utility function.
            data = conn.wait_get_auto_buffer_samples(max_samples_to_request, 10.0, flush_buffer=False)
            if data is not None:
                print(data)
                if data.sample_cnt < max_samples_to_request:
                    print("Not able to fetch all samples with waiting, expected, received", max_samples_to_request,
                          data.sample_cnt)
                    
            print("Finished getting samples with waiting")
