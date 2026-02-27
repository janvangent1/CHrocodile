# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for downloading and displaying signal profiles from the device.
"""

import argparse
import matplotlib.pyplot as plt

from context import chrpy
from chrpy.chr_connection import *
from chrpy.chr_cmd_id import *

keep_going = True


def on_close(event):
    global keep_going
    keep_going = False


def connection_parameters():
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')
    parser.add_argument('-s', help='signal IDs')

    # Default CLS2
    addr = '192.168.170.3'
    # Default expected in the chrpy module location
    path_to_lib = None
    sig_id = 16640
    args = parser.parse_args()
    if args.a is not None:
        addr = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        path_to_lib = args.p
    if args.s is not None:
        sig_id = n = int(args.s)
    return addr, sig_id, path_to_lib


def prepare_chart(sig_id):
    plt.ion()
    fig = plt.figure()
    fig.canvas.mpl_connect('close_event', on_close)
    ax = fig.add_subplot(111)

    plt.title('Profile: sig=' + str(sig_id))
    plt.xlabel('channel no')
    plt.ylabel('pix val')
    plt.grid()
    return fig, ax


if __name__ == '__main__':

    addr, sig_id, lib_path = connection_parameters()
    fig, ax = prepare_chart(sig_id)

    # Connections support the python context manager protocol:
    # When used in a "with" clause, the connection is opened and closed automatically.
    # By default, the connection is opened in synchronous mode.
    # Since synchronous mode is implicitly set, the connection type is SynchronousConnection
    with connection_from_params(addr=addr, dll_path=lib_path, device_type=DeviceType.CHR_MULTI_CHANNEL) as conn:
        conn.exec('SODX', sig_id)
        resp = conn.query('SHZ')  # exec and query return response objects
        shz = resp.args[0]

        last_time = time.time()
        max_val = 0
        min_val = 0
        avg_val = 0
        line = None

        # Now start reading incoming samples till stopped.
        while keep_going:
            data = conn.get_next_samples(100, True)
            if data is None:
                continue

            # Get the middle sample
            # "get_signal_values" returns an ndarray view containing only sig_id values:
            profile = data.get_signal_values(sig_id, int(data.sample_cnt / 2))

            # Simulate some work.
            sig_data = data.get_signal_values_all(sig_id)
            max_val = np.max(sig_data)
            min_val = np.min(sig_data)
            avg_val = np.average(sig_data)

            curr_time = time.time()
            if curr_time - last_time < 0.1:
                continue

            last_time = curr_time

            print("Avg Min Max", avg_val, min_val, max_val)

            if line is None:
                x = np.arange(0, data.gen_signal_info.channel_cnt)
                line, = ax.plot(x, profile)

            line.set_ydata(profile)
            ax.relim()
            ax.autoscale_view()
            fig.canvas.draw()

            # to flush the GUI events
            fig.canvas.flush_events()
