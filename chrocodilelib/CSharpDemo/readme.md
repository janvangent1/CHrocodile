# c# Demos

## Note
The C# wrapper has been created based on C# version
7.3 which is supported by .NET framework (4.8), .NET core >= 2.2 and all .NET versions
thereafter.**

## List of Demo projects

- Basic: contains demos which use synchronous communication to send commands and collect data.

  - Console - a simple console demo to demonstrate command sending and data collecting.
  - PullSample - a windows form demo which uses "GetNextSamples" to constantly pull and at the same time display pulled data. The demo is also capabile of sending command and downloading spectrum from the device.
  - RecordSample - a windows form demo, which after setting up device, uses the recording mode to collet data

- Async: contains demos which uses asynchronous communication.

  - Console - a simple console demo to demonstrate command sending and data collecting with asynchonous connection.
  - AsyncProcess - a widows form demo, which sets the connection to automatically process device output. Responses and data are delivered through callback function.
  - AwaitProcess - a windows form demo, which uses the await feature of the asynchronous connection to achieve synchronous device setup. Only after setting up device, the data will be collected through callback function.

- SharedConn
  Opens up two virtual shared connections to single CHR device. One connection works under asynchronous mode, sending commands and recieving responses. One connection is synchronously downloanding spectra.

- TriggerScanning
  Sets up device to work under trigger each mode (with trigger signal or encoder trigger) and collects sanned data. For the scanning application, normally synchronous connection is recommended.

  - SingleChannel - a windows form demo for single channel device, which uses the recording mode of synchorous connection to collect scanned data.
  - MultiChannel - a windows form demo for multi-channel device, which uses the recording mode of synchorous connection to collect scanned data.
  - AsyncScanConsole - a console demo to demonstrate how to achieve setting up device and retrieving scanning data with asynchronous connection

- Plugin
  - CLS2CalibPlugin - uses CLS2 calibration plugin to correct CLS2 peak distance data.
  - FlyingSpotPlugin


Note all the provided c# projects are configured to build 32bit executables. If 64bit is wished, you need to change related project settings...