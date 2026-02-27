## c++ Demo

For each c++ demo, a CMakelist file is provided.

- Basic_Single_Channel contains demos for single channel devices. All the demos use synchronous communication to send commands and collect data.

  - PullSample - after setting up device, uses "GetNextSamples" to constantly pulling and displaying data.
  - DataBuffer - after setting up device, uses "Autobuffer" mode to let CHRocodileLib fill up a user buffer and display.
  - SpecTable - download spectrum and device tables

- Basic_Multi_Channel contains demos for multi channel devices. All the demos use synchronous communication to send commands and collect data.

  - PullSample - after setting up device, uses "GetNextSamples" to constantly pulling and displaying data.
  - DataBuffer - after setting up device, uses "Autobuffer" mode to let CHRocodileLib fill up a user buffer and display.
  - SpecTable - download spectrum and device tables

- Async contains demos which uses asynchronous communication.

  - Demo1 - uses two threads to communicate with the device. One thread sends commands to the CHR device without waiting for the responses, another thread constantly calls "ProcessDeviceOutput" to process CHR device output.
  - Demo2 - sets CHRocodileLib to process CHR device output automatically. Data and responses are delievered through callback functions. After setting up the device, a user buffer, instead of using the internal data buffer of CHRocodileLib, has been passed to be filled with data samples.

- SharedConnection
  opens up two virtual shared connections to single CHR device. One connection works under asynchronous mode, the other under synchronous mode. They function totally independent of each other.

- EncoderTrigger
  sets up CHR device for encoder trigger under trigger each mode and then collects data.

- Plugin
  CLS2CalibPlugin - add CLS2CalibPlugin into the opened connection, set up calibration file and then read data
  FSSSimpleDemo - demo for flyspot plugin
