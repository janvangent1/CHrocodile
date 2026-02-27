# -*- coding: utf-8 -*-

"""
This script demonstrates the use of FSS plugin in async mode.
"""

import argparse
from pathlib import Path

from context import chrpy
from chrpy.chr_cmd_id import CmdId
from chrpy.chr_dll import *
from chrpy.chr_plugins import FSSPluginShapeData, FlyingSpot_Plugin, FlyingSpotConsts

from chrpy.chr_connection import *


# The general purpose callback for receiving responses asynchronously.
def gen_callback(cb: Response):
    pass
    # print("General response\n", cb)
    # print('First argument in response=', cb.args[0] if cb.args is not None else None)


# The args callback for receiving args asynchronously. The samples are returned as numpy array - samples member in Data.
def data_callback(cb: Data):
    # print(cb)
    pass


file_num = 1
shapes = []


# saves the scanned shapes into a list for future processing
def response_callback(rsp: Response):
    global shapes
    # print("Response\n", rsp)
    if rsp.error_code < 0:
        raise Exception(rsp.get_error_string())

    if rsp.param_count < 1:
        return False

    shape = FSSPluginShapeData(rsp)
    if shape.data_type == FlyingSpotConsts.PluginRecipeTerminate:
        print("\nReceived recipe terminate...")
        return True

    shapes.append(shape)
    return False


def saveToCSV(shape: FSSPluginShapeData):
    print(shape)
    global file_num
    fname = f"{file_num}_{shape.label}_{shape.shape_counter}.csv"

    with open(fname, "w") as f:
        if shape.data_type == FlyingSpotConsts.PluginInterpolated2D:
            f.write(f"bitmap ; {shape.image_w}x{shape.image_h}\n")
        else:
            f.write("raw data\n")

        try:
            for key, buf in shape.data.items():
                f.write(f"signal {key} ;\n")

                if shape.data_type == FlyingSpotConsts.PluginInterpolated2D:
                    # np.savetxt('bla.csv', buf, delimiter=';')
                    # str = np.array2string(buf, separator=';', formatter={'float_kind':lambda x: "%.4f" % x})
                    for row in buf:
                        for val in row:
                            f.write(f"{val} ; ".replace(".", ","))
                        f.write("\n")
                else:  # Raw args
                    for val in buf:
                        f.write(f"{val} ; ".replace(".", ","))
                    f.write("\n")

        except Exception as e:
            print("Error:   ", e)
        except:
            print("Exception")

        file_num += 1


class ExecWaiter:
    """
    Wait for an asynchronous command response
    """

    def __init__(self, plugin_obj: Plugin):
        self.plugin = plugin_obj
        self.event = threading.Event()
        self.userCB = None  # user callback if set
        self.curr_resp = None

    def callback(self, resp: Response):
        print(f"Exec response ..")
        self.curr_resp = resp
        if self.userCB(resp) if self.userCB else True:
            self.event.set()

    def exec(self, cid, *cargs):
        print(f"Exec waiting ..")
        self.plugin.exec(cid, cargs, resp_cb=self.callback)
        self.event.wait()
        self.event.clear()
        return self.curr_resp


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-a", help="Device address")
    parser.add_argument("-p", help="Path to chrocodile lib")

    # FSS
    addr = "192.168.170.2"
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
    bin_dir = chr_dll_dir

    # Connections support the python context manager protocol.
    # When used in a "with" clause, the connection is opened and closed automatically.
    # The responses and args are received in the callback functions above.
    # Since asynchronous mode is set the connection type is AsynchronousConnection
    with connection_from_params(
        addr=addr,
        conn_mode=OperationMode.ASYNC,
        device_type=DeviceType.CHR_2,
        # log_path=".", log_level=4,
        resp_callback=gen_callback,
        data_callback=data_callback,
        dll_path=path_to_lib,
    ) as conn:
        # NOTE NOTE: FSS Plugin now supports 'double' as well as 'raw' data mode (default is 'double')
        # conn.set_output_data_format_mode(OutputDataMode.RAW)
        conn.start_data_stream()

        # Activate the FSS plugin
        plugin = conn.add_plugin(FlyingSpot_Plugin)

        rs_file = (Path(__file__).parent / "LoopFigures.rs").absolute()

        exec_waiter = ExecWaiter(plugin)
        exec_waiter.exec("CFG", bin_dir + "/ScannerGlobalConfig.cfg")
        conn.exec("SODX", 65, 66, 69, 256, 68, 82, 83)

        id1 = exec_waiter.exec(
            "PROG", FlyingSpotConsts.PROG_InputFile, str(rs_file)
        ).args[0]
        print(f"id1 = {id1}")

        exec_waiter.userCB = response_callback
        exec_waiter.exec("EXEC", id1)

        print(f"shapes collected: {len(shapes)}")
        for shape in shapes:
            saveToCSV(shape)

        # Wait for q or ctl-c to exit
        # try:
        #     if input() == 'q':
        #         sys.exit(1)
        # except KeyboardInterrupt:
        #     sys.exit(1)
