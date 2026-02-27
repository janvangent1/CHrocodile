# -*- coding: utf-8 -*-
"""
This script demonstrates the use of sync mode for downloading and displaying signal profiles as a surface.
"""

import argparse
import matplotlib.pyplot as plt

from context import chrpy
from chrpy.chr_connection import *
from chrpy.chr_cmd_id import *


# Handler for catching signal ctl-c
def handler(signum, frame):
    sys.exit(1)
    
    
def surface_plot (matrix, **kwargs):
    # acquire the cartesian coordinate matrices from the matrix
    # x is cols, y is rows
    y = np.arange(matrix.shape[0])
    x = np.arange(matrix.shape[1])
    (x, y) = np.meshgrid(x, y)
    figure = plt.figure()
    axis = figure.add_subplot(111, projection='3d')
    surf = axis.plot_surface(x, y, matrix, **kwargs)
    return figure, axis, surf
    

if __name__ == '__main__':
    # Set the signal handler for ctl-c
    signal.signal(signal.SIGINT, handler)
    
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')
    parser.add_argument('-s', help='signal no.')

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
        sig_id = int(args.s)

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    with connection_from_params(addr=addr, dll_path=path_to_lib) as conn:
        # Now start reading incoming samples till stopped.
        conn.exec('SODX', 83, 16640, 16641)
        resp = conn.query('SHZ')
        shz = resp.args[0]

        # Currently a hack to get signal info metadata
        last_data = conn.get_last_sample()
        print(last_data)
        
        while True:
            data = conn.wait_get_auto_buffer_samples(100, 2, 0.1, flush_buffer=True)
            if data is None:
                raise Exception("No data received")
            
            data.signal_info = last_data.signal_info
            data.gen_signal_info = last_data.gen_signal_info

            sig_data = data.get_signal_values_all(sig_id)
            
            (fig, ax, sf) = surface_plot(sig_data, cmap=plt.cm.coolwarm)
            ax.set_ylabel('sample no')
            ax.set_xlabel('channel no')
            ax.set_zlabel('sig value')
            plt.title("Signal ID = " + str(sig_id))
            plt.show()

            print("q enter for exit, or, enter to continue")
            
            try:
                if input() == 'q':
                    sys.exit(1)
            except KeyboardInterrupt:
                sys.exit(1)
