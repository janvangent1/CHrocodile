# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode downloading and displaying a spectrum from the device.
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


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')
    parser.add_argument('-c', help='channel no.')

    # Default CLS2
    addr = '192.168.170.3'
    # Default expected in the chrpy module location
    path_to_lib = None
    start_channel = 0
    args = parser.parse_args()
    if args.a is not None:
        addr = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        path_to_lib = args.p
    if args.c is not None:
        start_channel = int(args.c)

    plt.ion()
    fig = plt.figure()
    fig.canvas.mpl_connect('close_event', on_close)
    ax = fig.add_subplot(111)
    line = None
    x = None

    plt.title('Spectrum: channel=' + str(start_channel))
    plt.xlabel('pix no')
    plt.ylabel('pix val')
    plt.grid()

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    with connection_from_params(addr=addr, dll_path=path_to_lib) as conn:
        # Now start reading incoming samples till stopped.
        while keep_going:
            # Download and display a spectrum every second.
            resp = conn.download_spectrum(SpectrumType.CONFOCAL, start_channel)

            if resp.error_code != 0:
                raise APIException(conn.dll_handle(), resp.error_code)

            par = resp.args[resp.param_count - 1]
            # Convert byte args to short values for the spectrum.
            par = np.frombuffer(par, dtype=np.short)
            if line is None:
                x = np.arange(0, par.size)
                line, = ax.plot(x, par)

            line.set_ydata(par)
            ax.relim()
            ax.autoscale_view()
            fig.canvas.draw()

            # to flush the GUI events
            fig.canvas.flush_events()
            time.sleep(0.05)
