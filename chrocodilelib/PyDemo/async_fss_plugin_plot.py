import argparse
import math
from context import chrpy
from chrpy.chr_cmd_id import CmdId
from chrpy.chr_dll import *
from chrpy.chr_plugins import FSSPluginShapeData, FlyingSpot_Plugin, FlyingSpotConsts
from chrpy.chr_utils import SignalInfo
from chrpy.chr_connection import *
import numpy as np
import matplotlib.pyplot as plt
from pathlib import Path


# The general purpose callback for receiving responses asynchronously.
def gen_callback(cb: Response):
    pass
    # print("General response\n", cb)
    # print('First argument in response=', cb.args[0] if cb.args is not None else None)


# The args callback for receiving args asynchronously. The samples are returned as numpy array - samples member in Data.
def data_callback(cb: Data):
    # print(cb)
    pass


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


shapes = []


def exec_callback(rsp: Response):
    global shapes

    if rsp.error_code < 0:
        print(f"execution error: {rsp.get_error_string()}")
        return True

    if rsp.param_count < 1:
        return False

    shape = FSSPluginShapeData(rsp)
    if shape.data_type == FlyingSpotConsts.PluginRecipeTerminate:
        print("\nReceived plugin terminate, press q or ctl-c to exit")
        return True

    if shape.data:
        shapes.append(shape)
    return False


def plot_shape(shape: FSSPluginShapeData):
    if not shape.data:
        print("shape.data is empty!")
        return

    isInterp = shape.data_type == FlyingSpotConsts.PluginInterpolated2D
    num = shape.num_signals if isInterp else shape.num_signals - 2
    nX = math.ceil(math.sqrt(num))  # try to make the plot square
    nY = (num + nX - 1) // nX
    print(f"nx = {nX}; ny = {nY}")

    fig = plt.figure(f"{shape.label} #{shape.shape_counter}", figsize=(nX * 3, nY * 3))
    axes = fig.subplots(nY, nX).flatten()

    for ax in axes:  # set off the remaining empty plots
        ax.axis("off")

    if isInterp:
        for (key, img), ax in zip(shape.data.items(), axes):
            ax.imshow(img, cmap="plasma")
            ax.set_title(f"sig #{key}")
    else:
        Xs = shape.data[65]
        Ys = shape.data[66]
        g = (item for item in shape.data.items() if item[0] not in [65, 66])

        for (key, img), ax in zip(g, axes):
            ax.scatter(Xs, Ys, marker=".", c=img, cmap="plasma", s=2)
            ax.set_title(f"sig #{key}")

    plt.tight_layout(pad=0)
    plt.show(block=True)


if __name__ == "__main__":
    addr = "192.168.170.2"

    # The plugins are found by default relative to the location of the chrocodile lib.
    chr_dll_dir = os.path.dirname(get_abs_dll_path(None))
    os.chdir(chr_dll_dir)
    bin_dir = chr_dll_dir

    with connection_from_params(
        addr=addr,
        conn_mode=OperationMode.ASYNC,
        device_type=DeviceType.CHR_2,
        resp_callback=gen_callback,
        data_callback=data_callback,
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

        exec_waiter.userCB = exec_callback
        exec_waiter.exec("EXEC", id1)

        print(f"#shapes collected: {len(shapes)}")
        for shape in shapes:
            plot_shape(shape)
