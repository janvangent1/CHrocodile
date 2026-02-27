# Introduction
This documentation refers to the Python wrapper/bindings for the Chrocodile C++ client DLL version 5.x. The Python wrapper is written using the ``ctypes`` Python module. For details regarding the C++ DLL functionality, the reader is referred to the C++ DLL documentation.

The motivation for this package is to provide  the users a way to deploy the C++ client DLL in a Python environment for rapid development, prototyping, analysis and testing. An effort has been made to provide good performance by minimizing copying of data and the use of numpy. However, for high performance applications, it may be better to use the C++ API directly. If in doubt, one can quickly find out if the usage of the Python wrapper meets the performance expectations or not.

The supplied examples are a good source to get familiar with the basic usage of the the Python bindings. A brief description of the usage is given in a later section.

# Requirements
* Python 3.7 and greater.
* numpy (if the wheel package is installed, numpy will be installed automatically)


# Basic usage

The user creates a connection object and sends commands and receives commands responses and data. There are factory functions available for creating connection objects. The Python bindings support two operation modes - synchronous and asynchronous. In synchronous mode, the command execution calls are blocking and the data is explicit fetched. On the contrary in asynchronous mode, callbacks are specified for command responses and data. These callbacks are then called by the library when a command response or data is available.


A synchronous connection one can be created as follows (not all arguments are shown):


```python
#
# Connections support the python context manager protocol
# and used in a "with" clause, the connection is opened 
# and closed automatically.
    
with connection_from_params(addr='192.168.170.3', 
    device_type=DeviceType.CHR_MULTI_CHANNEL,) as conn:
#
```

A connection in asynchronous operation mode can be created as follows (not all arguments are shown):


```python
#
# Connections support the python context manager protocol
# and used in a "with" clause, the connection is opened 
# and closed automatically.
# The responses and data are received 
# in the supplied callback functions.
# The callbacks can be free functions or class methods.
    
with connection_from_params(addr='192.168.170.3', 
    conn_mode=OperationMode.ASYNC,  
    device_type=DeviceType.CHR_2,
    resp_callback=gen_callback, 
    data_callback=data_callback, 
    dll_path=path_to_lib) as conn:
#
```


After a connection to the sensor device has been opened, one can send commands and queries like:


```python
#
response = conn.exec('SODX', 83, 16640, 16641)

response = conn.query("FLTC")

response = conn.exec_from_string("SODX 83 16640 16641")
#
```


In case of synchronous connections, the returned **Response** object is completely filled. But in case of asynchronous connections a partial response is delivered with a ticket ID and the full response is received in the specified callback.

For synchronous connections there are methods available to fetch data. For example,


```python
#
sample_data = conn.get_next_samples(10, False)
#
```


The returned **Data** object contains metadata and sample data.

For asynchronous connections the responses and data are received in the specified response and data callback functions. For example,

```python
# Response callback
def resp_callback(resp: Response):
    print(resp)

# Data callback
def data_callback(data: Data):
    print(data)
#
```

Please note that in the case of asynchronous operation mode, it is also possible to specify a callback for a particular command execution. For example,

```python
#
def my_sodx_callback(cb: Response):
    print("Special SODX callback\n", cb)

# Specify my_sodx_callback function as the response callback for the following command execution.
conn.exec('SODX', 83, 16640, 16641, resp_cb=my_sodx_callback)
#
```

Example Python scripts are supplied demonstrating the implementation of many common use cases.

# Command Response and Data objects

Command responses are delivered as objects of type **Response**. In addition to metadata it has a member **args** which is a list of response arguments.

The data is delivered as objects of type **Data**. In addition to metadata it has a member **samples** which is a numpy ndarray containing the data for one or more samples. The **Data** class provides utility functions for accessing signals values.


# Data Format

The received sample data contains global signals followed by channel signals. For example,

**Global Signal 1  
Global Signal 2  
Channel 1  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 1  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 2  
Channel 2  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 1  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 2  
...  
...  
Channel N  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 1  
&nbsp;&nbsp;&nbsp;&nbsp;PeakSignal 2**

By default this data is delivered as float64 values even if the underlying type of the received signal requirer fewer bytes such as an uint16. If one were to visualize the ***samples*** member of type ndarray of the **Data** object with a print statement one would see something like (in this case one global signal 83 and tw peak signals 16640 and 16641 were requested):

```
sample_cnt=2
gen_signal_info=channel_cnt=1200,global_sig_cnt=1,info_index=2,peak_sig_cnt=2
signal_info=[(255, 83), (255, 16640), (255, 16641)]
error_code=0
samples=[[15600.  5027.   402. ...   313.  6852.   311.]
 [15601.  5035.   405. ...   312.  6859.   310.]]
```

However, the Python wrapper can be configured to work with raw sensor data. For example,

```python
#
conn.set_output_data_format_mode(OutputDataMode.RAW)
#
```

This maybe desirable when one wants to reduce the memory usage. In this case the raw data buffer is used in conjunction with a structured array to present data as global signals followed by tuples of channel signals. The type of the elements is same as the underlying type delivered by the sensor. So one can have signals with different data types next to each other. If one were to visualize the ***samples*** member of type ndarray of the **Data** object with a print statement one would see something like (here two global signals 83 and 75, and, two peak signals 16640 and 16641 were requested)::

```
sample_cnt=1
gen_signal_info=channel_cnt=1200,global_sig_cnt=2,info_index=3,peak_sig_cnt=2
signal_info=[(2, 83), (2, 76), (3, 16640), (3, 16641)]
error_code=0
samples=[(48000, 1264, [(5032, 409), (5034, 403), (5030, 420), (5032, 427), ..., (8935, 604), (8951, 572), (8962, 575)])]
```

Please note that the **Data** class provides utility functions for obtaining signals values for a particular signal ID. These normally provide a view of the underlying data without making copies.

```python
#
signal_data = data.get_signal_values(sig_id, sample_no)

signal_data = data.get_signal_values_all(sig_id)
#
```

# Error handling

Usually the Python wrapper connection methods throw an exception if a C++ function call results in an error code being returned. The exception contains the error code and the string representation of the error code. However, when operating in asynchronous mode, the user is expected to evaluate the error codes and react accordingly in the callbacks. ***Please handle all the exceptions in the asynchronous callbacks, as currently an unhandled exception will result in an abort signal.***


# Auto buffer mode

Normally the memory for the data buffers that are forwarded to the client is managed internally by the C++ client DLL. However, the C++ client DLL supports auto buffer mode in which a client can provide a user allocated buffer to the C++ client DLL for receiving data. The Python wrapper supports this functionality by providing a numpy buffer to the C++ client DLL for receiving data. The user just has to specify how many maximum number of data samples should be saved in the auto buffer. The rest is handled by the Python wrapper. This functionality is available in both synchronous and asynchronous operations modes.

In asynchronous mode the auto buffer functionality is used by default but can be turned off. From a user point of view there is no difference in the data callback. Please note that if the application is slow in processing the data, there will be data loss. The DLL generates special logs for this and the error code in the Data object can be checked for warning and error. For example,

```python
#
def data_callback(cb: Data):
    print(cb)
    
    if chr_warning(cb.error_code):
        print("Lib auto flushed buffer")
        # Just for debugging
        signal.raise_signal(signal.SIGABRT)
    elif chr_error(cb.error_code):
        print("Lib error in processing device output")
        # Just for debugging
        signal.raise_signal(signal.SIGABRT)

    if cb.error_code == ReadData.BUFFER_FULL:
        print("In case the config async_auto_activate_buffer flag is set,"
              " after this callback the auto buffer will be internally reset,"
              " copy data if needed later."
              " If not, please make sure that the after the data has been"
              " worked on the auto buffer is reactivated with the connection"
              " reset_async_auto_buffer method to continue receiving data.")
#
```

In synchronous mode one has to explicitly enable the auto buffer functionality. Also, the user can wait for the buffer to be filled before processing the data, or alternatively fetch the newly added samples to the buffer and process these while the DLL is receiving the remaining samples. The latter approach is more efficient as a client DLL thread is filling the buffer and the Python interpreter thread is processing the already received data. The following information is specific to synchronous mode. 

If one want to collect all the samples, before processing these, one could use the provided utility method as show in the code example that follows.

```python
#
# In this case 10.0 is the timeout for collecting data. Please look at 
# the documentation of this method for other arguments. With flush buffer
# one can control if one wants to discard older data or not. 
# Default is False, to keep reading from where one left off.
max_samples_to_request = 75000
data = conn.wait_get_auto_buffer_samples(max_samples_to_request, 10.0, flush_buffer=False)
#
```

In case one would rather process the samples as they are coming in, one could implement it as shown in the sample code that follows.


```python
# 
_, err = conn.activate_auto_buffer_mode(max_samples_to_request, flush_buffer=True)
last_sample_cnt = 0
max_samples_to_request = 75000
while last_sample_cnt < max_samples_to_request:
    data = conn.get_auto_buffer_new_samples(last_sample_cnt)
    if data is None:
        time.sleep(0.1)
        continue
    last_sample_cnt += data.sample_cnt
#
```

# Plugins

The plugin functionality available in the C++ client DLL can be used with the Python wrapper. A plugin can be added as shown in the the sample code that follows.

```python
#
# Add a plugin to a connection with plugin ID.
plugin = conn.add_plugin_with_id(Test_Plugin_ID)
# Add a plugin to a connection with plugin name.
plugin = conn.add_plugin(FlyingSpot_Plugin_Name)
#
```

These call written a Plugin object and the operation mode for the command execution depends is the same as the that for the connection to which the plugin was added. The commands for a plugin can be carried out as is possible for a connection. For example in asynchronous operation mode,

```python
#
# resp_cb specifies the callback for this particular execution of the command.
plugin.exec('EXEC', exec_arg, resp_cb=response_callback)
#
```

# Shared connections

These are supported as well. A shared connection can be opened from an existing connection. For example,

```python
#
# Open a shared connection in asynchronous operation mode from 
# an existing connection. # The operation mode of a shared 
# connection can be different from the connection with 
# which it is shared.
shared_conn = conn.open_shared_connection(conn_mode=OperationMode.ASYNC,    
                                          resp_callback=shared_resp_callback,
                                          data_callback=shared_data_callback)
#
```

# Performance Tips

It is recommended to avoid making unnecessary copies of the data and as far as possible operate on numpy arrays instead of converting these to, for example, Python lists. It is also advisable, wherever possible, to operate on large chunks of data or multiple samples simultaneously.

# Good To Know

By default, the chrpy package loads the CHRocodile .dll/.so library from the installation directory of the package and the ini file (CHRocodile.ini) from the current working directory.  

One can override this behavior by specifying these paths while creating the connection object. Please note that the name of the file is part of the path. For example,

```python
#
# The names of the DLL and ini file are part of the specification.
path_to_lib = "C:/lib/Chrocodile.dll"
path_to_ini = "C:/config/CHRocodile.ini"

with connection_from_params(addr=addr, device_type=DeviceType.CHR_2, dll_path=path_to_lib, ini_file=path_to_ini) as conn:
#
```

It is also possible to specify these paths explicitly. **NORMALLY IT IS NOT REQUIRED TO MAKE AN EXPLICIT CALL TO *"load_client_dll"*. HOWEVER, WHEN THIS IS DONE, PLEASE PASS THE LIBRARY HANDLE OBTAINED BY THE *"load_client_dll"* CALL TO THE CONNECTION CONSTRUCTOR OR CREATOR.**  


For example,

```python
#
# The names of the DLL and ini file are part of the specification.
path_to_lib = "C:/lib/Chrocodile.dll"
path_to_ini = "C:/config/CHRocodile.ini"

path_to_lib, dll_handle = load_client_dll(path_to_lib)

error_code = set_ini_file(dll_handle, path_to_ini)

with connection_from_params(addr=addr, device_type=DeviceType.CHR_2, dll_path=path_to_lib, dll_h=dll_handle) as conn:
#
```

***Please note that for performance reasons the Python wrapper avoids making copies of received data as far as possible and delivers numpy arrays which use the supplied memory without copying, or in case of slicing to obtain signal values, views of the underlying data. Therefore, if one wants to keep data for later processing or the data processing step after a synchronous data fetch or in an asynchronous data callback is time consuming relative to the data rates, one will have to make a copy of the data. It is recommended that before making a copy, one may want to check if the obtained samples or signals ndarray is already a copy or not. There are other strategies one can follow for processing data depending on the use case. For example, if the sensor measurements are made with triggering and the start of the next triggering scan is flexible, one could use a larger library buffer so that a complete scan fits in the library buffer. Then the next scan can be triggered after the current scan buffered data has been processed.***

# Troubleshooting
* Please read the section **Good To Know**.
* If when running the supplied examples after unzipping the archive, a module not found error occurs, one can add the chrpy and examples folders to the PYTHONPATH.
* If the client application has problems adding plugins, one may have to specify the correct working directory and edit the PluginDir entry in the Chrocodile.ini.

# Support
Please contact service@precitec-optronik.de.

