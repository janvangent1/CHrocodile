# coding=utf-8

import os
import sys

from chr_connection import *
import chr_utils
import argparse
import timeit
import time

ACTIVE = True
DISABLED = False
signal_id = []
conn = Connection()


def connect(ip_address, device_type, connection_mode):
    # [in] device_type 0: CHR-1 1: CHR-2 2: multichannel 3: CHR-C
    # [in] connection_type 0: sync 1: async
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    parser = argparse.ArgumentParser()
    parser.add_argument('-a', help='Device address')
    parser.add_argument('-p', help='Path to chrocodile lib')

    config = ConnectionConfig()
    args = parser.parse_args()
    args.a = ip_address
    if args.a is not None:
        config.address = args.a
    if args.p is not None:
        print("Path to Client Lib: " + args.p)
        config.chr_dll_path = args.p

    config.device_type = device_type
    config.connection_mode = connection_mode
    error = conn.open(config)
    return error


def disconnect():
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    print('exiting', conn.config.address)
    # if conn.conn_handle.value > UINT(0).value:
    error = conn.close()
    return error


def stop_data_stream():
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    error = conn.stop_data_stream()
    return error


def start_data_stream():
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    error = conn.start_data_stream()
    return error


def set_optical_probe(n):
    # [in] n: integer number of probe index
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.OPTICAL_PROBE, [n])
    return rsp.error_code


def set_number_of_peak(n):
    # [in] n: integer number of peak
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.NUMBER_OF_PEAKS, [n])
    return rsp.error_code


def set_scan_rate(n):
    # [in] n: float number of scan rate
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.SCAN_RATE, [n])
    return rsp.error_code


def auto_adapt_led(n, level):
    # [in] n: int of enabling auto adapt led or not, 0: disable 1:enable
    # [in] level: float of CCD Exposure Level in percentage
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.LIGHT_SOURCE_AUTO_ADAPT, [n, level])
    return rsp.error_code


def set_lamp_intensity(n):
    # [in] n: float of lamp intensity in percentage
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.LAMP_INTENSITY, [n])
    return rsp.error_code


def set_data_average(n):
    # [in] n: integer number of samples average
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.DATA_AVERAGE, [n])
    return rsp.error_code


def set_peak_detection_threshold(n):
    # [in] n: integer number of threshold
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.CONFOCAL_DETECTION_THRESHOLD, [n])
    return rsp.error_code


def dark_reference():
    # [out] arg: float number of frequency scan rate in Hertz at which the CCD would be saturated by the
    #            stray light, only used for synchronous connection.
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    response = send_command_string("DRK")
    return response[0], response[2]


def set_output_signals(id_list):
    # [in] id_list: list of integers of signal id
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    rsp = conn.send_command(CmdId.OUTPUT_SIGNALS, [id_list])
    return rsp.error_code


def get_output_signals():
    # [out] signal_ids: list of integers
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    signal_ids, error = conn.get_device_output_signals()
    return signal_ids, error


def flush_connection_buffer():
    global conn
    error = conn.flush_connection_buffer()
    return error


def get_next_samples(sample_count):
    # [in] sample_count: User passes over the number of samples to be read,
    #                    lib gives back the actual number of read samples.
    # [out] data_list: two dimension list of float samples
    # [out] ret_sample_count: integer of actual number of read samples.
    # [out] err 0: present available samples is fewer than the required number.
    #           1: successfully read in required number of the samples
    #           2: command response has been received
    #           3 : library samples sample buffer is not large enough to store preset amount of samples
    #           negative: error or warning code
    global conn

    response = conn.get_next_samples(int(sample_count))
    if response:
        return response.samples.tolist(), response.samples.shape[0], response.error_code
    else:
        return [], 0, -1


def get_last_sample():
    # [out] data_list: two dimension list of float samples
    # [out] err 0: no sample has been received from device
    #           1: sample is available;
    #           negative: error or warning code.
    global conn
    response = conn.get_last_sample()
    return response.samples.tolist(), response.error_code


def get_single_output_sample_size():
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    sample_size, error = conn.get_single_output_sample_size()
    return sample_size, error


def error_code_to_string(error_code):
    # [in] error_code: integer number of error code
    # [out] error_str: error string corresponding to the input error code in python string format
    # [out] err: Zero on success. Negative error or warning code. This is the error code of error_code_to_string itself.
    global conn
    error_str, error = chr_utils.error_code_to_string(conn.dll_handle(), int(error_code))
    if error != 0:
        error = -1
    return error_str, error


def activate_auto_buffer_mode(sample_count, flush_buffer):
    # [in] sample_count: User passes over the number of samples to be read.
    # [in] flush_buffer: Bool, True or False
    # [out] err: Zero on success. Negative error or warning code.
    global conn, auto_buffer_data
    # get the minimum size of the buffer for saving expected number of samples.
    bufsz = conn.activate_auto_buffer_mode(sample_count, flush_buffer)
    auto_buffer_data = AutoBufferData(sample_count)
    auto_buffer_data.init_data()


def get_auto_buffer_status():
    # [out]
    # Auto_Buffer_Error = -1
    # Auto_Buffer_Saving = 0
    # Auto_Buffer_Finished = 1
    # Auto_Buffer_Received_Response = 2
    # Auto_Buffer_Deactivated = 3
    # Auto_Buffer_UnInit = 4
    global conn
    return conn.get_auto_buffer_status()


def get_auto_buffer_saved_sample_count():
    # [out] sample_count: integer number of sample count
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    sample_count, error = conn.get_auto_buffer_saved_sample_count()
    return sample_count, error


def deactivate_auto_buffer_mode():
    # [out] err: Zero on success. Negative error or warning code.
    global conn
    error = conn.deactivate_auto_buffer_mode()
    return error


class AutoBufferData:
    def __init__(self, max_samples_to_request=0):
        global conn
        self.gen_sig_info, self.sig_ids = None, None
        self.max_samples_to_request = max_samples_to_request
        self.last_sample_cnt = 0
        self.samples = None
        self.state = DISABLED

    def init_data(self):
        self.gen_sig_info, self.sig_ids = conn.get_output_signal_infos()
        print(self.gen_sig_info, self.sig_ids)
        sample_sz = conn.get_single_output_sample_size()
        # Reshape the whole buffer
        self.samples = conn.get_auto_buffer_samples(self.max_samples_to_request, sample_sz)
        self.state = ACTIVE


auto_buffer_data = AutoBufferData()


def get_sliced_auto_buffer_data():
    # [out] two dimension list of float samples
    #       If there is no new data in the buffer, an empty list will be returned.
    global auto_buffer_data
    cnt = conn.get_auto_buffer_saved_sample_count()
    if cnt == 0 or cnt <= auto_buffer_data.last_sample_cnt:
        return []
    sliced_data = Data(samples=auto_buffer_data.samples[auto_buffer_data.last_sample_cnt: cnt, :],
                       sample_cnt=cnt - auto_buffer_data.last_sample_cnt, gen_signal_info=auto_buffer_data.gen_sig_info,
                       signal_info=auto_buffer_data.sig_ids, err_code=0, dll_h=conn.dll_handle())
    auto_buffer_data.last_sample_cnt = cnt
    return sliced_data.samples.tolist()


def get_auto_buff_last_sample_cnt() -> int:
    # [out] [This function only for Auto Buffer Mode] return the last sample count number
    #       [If user did not active auto buffer mode first, will return -1]
    global auto_buffer_data
    if auto_buffer_data.state == ACTIVE:
        return auto_buffer_data.last_sample_cnt
    else:
        return -1


def get_buff_data() -> list:
    # [out] conn.auto_buffer.tolist(): two-dimension List of float samples from AutoBufferMode's buffer
    #                                  or empty list of no data.
    global conn
    sample_count = conn.get_auto_buffer_saved_sample_count()[0]
    sample_size = conn.get_single_output_sample_size()[0]
    if sample_count > 0:
        return conn.get_auto_buffer_samples(sample_count, sample_size).tolist()
    else:
        return []


def send_command_string(cmd_str):
    # [in] cmd_str: String of the command
    # [out] response.data: List of data. Type of the data inside the list will be various
    # [out] response.rsp_h: Integer number of response handle
    # [out] response.error_code: Zero on success. Negative error or warning code.
    global conn
    response = conn.send_command_string(cmd_str)
    return response.args, response.rsp_h, response.error_code


def auto_buffer_process(sample_cnt, flush_buffer):
    err = activate_auto_buffer_mode(sample_cnt, flush_buffer)
    while get_auto_buffer_status() != 1:
        pass
    data = get_buff_data()
    return data


def general_example(max_samples_to_request):
    # Do it oneself, without waiting
    gen_sig_info, sig_ids = conn.get_output_signal_infos()
    print(gen_sig_info, sig_ids)
    _, err = conn.activate_auto_buffer_mode(max_samples_to_request, True)
    if err != 0:
        raise APIException(conn.dll_handle(), err)
    sample_sz, err = conn.get_single_output_sample_size()
    if err != 0:
        raise APIException(conn.dll_handle(), err)
    # Reshape the whole buffer
    samples = conn.get_auto_buffer_samples(max_samples_to_request, sample_sz)
    last_sample_cnt = 0
    while last_sample_cnt < max_samples_to_request:
        if conn.get_auto_buffer_status() == AutoBuffer.ERROR:
            raise Exception("Auto buffer error")
        cnt, err = conn.get_auto_buffer_saved_sample_count()
        if err != 0:
            raise APIException(conn.dll_handle(), err)
        if cnt == 0 or cnt <= last_sample_cnt:
            continue
        # Now create the Data object with the new slice
        # print("last, curr", last_sample_cnt, cnt)
        data = Data(samples=samples[last_sample_cnt: cnt, :], sample_cnt=cnt - last_sample_cnt,
                    gen_signal_info=gen_sig_info, signal_info=sig_ids, err_code=0, dll_h=conn.dll_handle())
        # print(data)
        last_sample_cnt = cnt
    conn.deactivate_auto_buffer_mode()
    print("Finished getting samples without waiting")


def labview_auto_buffer_mode_example(sample_count):
    # This is an example for how LabVIEW application can get the sample data a little by little when the sensor is
    # scanning and filling the buffer.
    # data_list = []
    err = activate_auto_buffer_mode(sample_count, True)
    while get_auto_buff_last_sample_cnt() < sample_count:
        s_data = get_sliced_auto_buffer_data()
        # print(get_auto_buff_last_sample_cnt(), ' ', len(s_data))
        # data_list.extend(s_data)
        time.sleep(0.1)
    print("Finished getting samples without waiting")


if __name__ == '__main__':
    # error_code_to_string(-536580864)  # --> Client samples stream reading out of sync with CHRocodie Lib, please flush connection buffer.
    # error_code_to_string(-536450304)  # --> Device samples format packet is missing.
    connect(ip_address="192.168.170.3", device_type=2, connection_mode=0)
    send_command_string("SODX 83 16640 16641")
    # data, rsp_h, err = send_command_string("SODX ?")
    # data, rsp_h, err = send_command_string("DNLD 0 1")
    # d, err = get_command_response(conn.dll_handle(), rsp_h)
    # freq, err = dark_reference()
    stop_data_stream()
    set_optical_probe(0)
    set_number_of_peak(1)
    set_scan_rate(7900)
    # send_command_string("CRA 10 190")
    # auto_adapt_led(0, 30.0)
    set_lamp_intensity(90)
    set_data_average(1)
    set_peak_detection_threshold(500)
    # set_output_signals([83, 16640, 16641])
    # id_list, err_code = get_output_signals()
    start_data_stream()

    # flush_connection_buffer()
    # get_single_output_sample_size()
    # for i in range(10):
    #     data_l, err_code = get_last_sample()
    #     print('error_code: ', err_code, ', data_l: ', data_l)
    #     # time.sleep(0.01)
    # for i in range(10):
    #     data_l, ret_sp_cnt, err_code = get_next_samples(1000)
    #     print('ret_sp_cnt: ', ret_sp_cnt, ', error_code: ', err_code, ', data_l: ', data_l)
    #     # time.sleep(0.01)

    # data_list = []
    # flush_buff = True
    # for i in range(1):
    #     raw_data = auto_buffer_process(76500, flush_buff)
    #     data_list.append(raw_data)

    # standard_example(76500)

    print(timeit.timeit(stmt='labview_auto_buffer_mode_example(76500)', globals=globals(), number=1), ' seconds')
    # labview_auto_buffer_mode_example(sample_count=76500)

    print(timeit.timeit(stmt='auto_buffer_process(76500,True)', globals=globals(), number=1), ' seconds')

    stop_data_stream()
    disconnect()


