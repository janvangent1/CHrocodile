# CHRocodileLib Changelog

## CHANGES CHRocodileLib version 5.1

* C# wrapper: Fix several bugs
* C# wrapper: add ExecWithUserResponseDelegate function: receive response plus updates
* C# wrapper: Potentially BREAKING CHANGE: replace some integer arguments by enumerations:
               OutputDataFormat, MeasurementMode, EncoderPreloadConfig, TableType, Search
* C# wrapper: Add SynchronousCommandGroup to support blocking execution of multiple commands even
              with asynchronous connections
* C# wrapper: Asynchronous command response does not throw by default in case of command failure
* several bug fixes in FSS plugin


## CHANGES CHRocodileLib version 5.0

* Add asynchronous user buffer function in asynchronous mode, so that samples will be saved to the user buffer,
  instead of CHRocodileLib internal buffer from any thread
* will be downloaded automatically by the CHRocodileLib so the client software does not need download every chunk itself
* Add plugin functions such as: "AddConnectionPlugInByID", "GetConnectionPlugInInfo" and "PluginTypeIDToName"
* Add option for not invalidating response when the next response is received
* Add option for using keepalive to detect device TCP/IP disconnection
* Add option for setting device to free-run mode upon connection
* Automatically de-select all the signals from CHR device upon connection, the user needs to actively order signal with "SODX" command
* Add flying spot plugin
* Add python wrapper
* BREAKING CHANGE: Rewrite c# wrapper completely
* Remove convenient C functions, i.e. remove CHRocodileLibSpecialFunc.h file
