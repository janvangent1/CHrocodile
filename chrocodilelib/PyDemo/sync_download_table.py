# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for downloading tables with the TABL command.
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
    with connection_from_params(addr=addr, dll_path=path_to_lib) as conn, open(
        "./confocal_calib_sen_0.csv", "wb"
    ) as calib_file:
        sen_idx = 0
        # Download confocal calibration table
        print("Starting table download")
        # Download the first chunk.
        resp = conn.query('TABL', TableType.CHR3_CONFOCAL_CALIBRATION_RAW_DATA, sen_idx, 0, -1)
        if resp.error_code != 0:
            print("Failed to download table: ", resp.get_error_string())
            exit(1)

        total_sz = resp.args[3]
        pos = resp.args[0]
        table_data = resp.args[4]
        current_sz = len(table_data)
        calib_file.write(table_data)
        print("Downloaded chunk: ", current_sz, total_sz)

        # Download the rest in a loop
        while current_sz < total_sz:
            resp = conn.query(
                "TABL",
                TableType.CHR3_CONFOCAL_CALIBRATION_RAW_DATA,
                sen_idx,
                current_sz,
                -1,
            )
            if resp.error_code < 0:
                print("Failed to download table: ", resp.get_error_string())
                exit(1)
            table_data = resp.args[4]
            chunk_sz = len(table_data)
            calib_file.write(table_data)
            current_sz = current_sz + chunk_sz
            print("Downloaded chunk: ", current_sz, total_sz)

    print("Table download done")
