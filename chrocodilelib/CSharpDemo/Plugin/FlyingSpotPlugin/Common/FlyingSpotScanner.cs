using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CHRocodileLib;

namespace FSSCommon
{
    using CHRCallback = AsynchronousConnection.ResponseAndUpdateCallback;

    public class FlyingSpotScanner
    {
        /// <summary>
        /// Gets the IP address.
        /// Is only set if a connection was successfully established.
        /// </summary>
        public string IPAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating if the device is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets a value indicating if a scan program is running.
        /// </summary>
        public bool IsScanning { get; private set; }

        /// <summary>
        /// CHRocodile asynchronous connection object
        /// </summary>
        public AsynchronousConnection Conn { get; private set; } = null;  
        /// <summary>
        /// FSS Plugin object
        /// </summary>
        public AsynchronousConnection.Plugin Plugin { get; private set; } = null;      

        // blocking wait on command response
        public FlyingSpotScanner()
        {  }

        /// <summary>
        /// connects to the CHRocodile device and registers FSS Plugin
        /// </summary>
        /// <param name="ipAddress">device IP address</param>
        /// <param name="useRawDataMode">whether to operate in double- (default) or raw-data output mode to save traffic</param>
        public void Open(string ipAddress, bool useRawDataMode = false)
        {
            if (IsConnected)
                return;

            Lib.SetLibLogLevel(4);
            // generate at most 8 files of size 4Mb
            Lib.SetLibLogFileDirectory(".", 4096, 8);
            // Connect to device
            Conn = new AsynchronousConnection(ipAddress,        // Desired IP address
                                              DeviceType.Chr2);   // device CHRocodile 2
            Conn.AutomaticMode = true; // automatic mode is required for async/await commands to work correctly

            // sets data output format mode: 
            // 1. Output_Data_Format_Raw: signal data is returned with native data types: i.e., 256 is 32-bit float, 65 is 32-bit integer, etc.
            // 2. Output_Data_Format_Double: all signal data is returned as 64-bit floating-point data (which might not be a good choice for large scans)
            Conn.SetOutputDataFormatMode(useRawDataMode ? OutputDataFormat.Raw : OutputDataFormat.Double);

            // attach FSS plugin to the CHR DLL: the file FlyingSpotPlugin.dll must lie in Plugin/ subfolder of the binary dir
            Plugin = Conn.InsertPlugin("FlyingSpotPlugin");
            IsConnected = true;
        }

        /// <summary>
        /// Disconnects an active connection.
        /// </summary>
        public void Close()
        {
            Conn.Close();
            IsConnected = false;
        }

        /// <summary>
        /// this callback function is to be called every time a command response comes
        /// </summary>
        public CHRCallback GeneralCommandCallback 
        {
            get => Conn?.GeneralResponseAndUpdateCallback;
            set
            {
                if (Conn?.IsOpen ?? false)
                    Conn.SetGeneralResponseCallback(value);
                else
                    throw new Exception("Unable to assign the callback when the connection is not open!");
            }
        }

        /// <summary>
        /// sets or gets a program callback function to be called after each new geometric figure is scanned
        /// </summary>
        public CHRCallback ScanProgramCallback { get; set; } = null;

        /// <summary>
        /// (asynchronously) configures the FSS Plugin (this shall be done once upon device connection)
        /// </summary>
        /// <param name="cfgFileName">path to the config file</param>
        /// <param name="bufferSize">internal FSS global ring-buffer size (optional)</param>
        /// <returns>awaitable Task object</returns>
        public async Task Config(string cfgFileName, int bufferSize = 0)
        {
            checkScanRunning();
            if (bufferSize > 0)
                await Plugin.ExecAsync((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Cfg, cfgFileName, bufferSize);
            else
                await Plugin.ExecAsync((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Cfg, cfgFileName);
        }

        /// <summary>
        /// (synchronously) configures the FSS Plugin (this shall be done once upon device connection)
        /// </summary>
        /// <param name="cfgFileName">path to the config file</param>
        /// <param name="bufferSize">internal FSS global ring-buffer size (optional)</param>
        /// <returns>Response object</returns>
        public Response ConfigSync(string cfgFileName, int bufferSize = 0)
        {
            checkScanRunning();
            var waiter = new SynchronousCommandGroup(Plugin);
            if (bufferSize > 0)
                waiter.Add(Cmd.Command((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Cfg, cfgFileName, bufferSize));
            else
                waiter.Add(Cmd.Command((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Cfg, cfgFileName));
            waiter.WaitAll().TryDequeue(out var response); // TryDequeue will succeed

            return response;
        }

        /// <summary>
        /// (asyncrhonously) compiles the user script and stored the program's handle if it was successful
        /// </summary>
        /// <param name="programCode">script source code to be compiled as a raw string</param>
        /// <returns>awaitable Task object with program handle</returns>
        public async Task< Int32 > Compile(string programCode)
        {
            checkScanRunning();

            // Start compiling
            var rsp = await Plugin.ExecAsync((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Compile,
                CHRLibPlugin.FSS_PROG_InputString, programCode);

            // retrieve a compiled program handle: note that FSS plugin can store up to 32 precompiled programs
            // which might be accessed via unique handles
            return rsp.GetParam<Int32>(0);
        }

        /// <summary>
        /// compiles the user script and stored the program's handle if it was successful (blocking)
        /// </summary>
        /// <param name="programCode">script source code to be compiled as a raw string</param>
        /// <returns>Compiled program handle</returns>
        public Int32 CompileSync(string programCode)
        {
            checkScanRunning();

            var exec = new SynchronousCommandGroup(Plugin);
            // Start compiling
            exec.Add(Cmd.Command((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Compile,
                CHRLibPlugin.FSS_PROG_InputString, programCode));

            // retrieve a compiled program handle: note that FSS plugin can store up to 32 precompiled programs
            // which might be accessed via unique handlesn
            exec.WaitAll().TryDequeue(out var response);
            return response.GetParam<Int32>(0);
        }

        /// <summary>
        /// (asynchronously) starts (previously compiled) script execution
        /// </summary>
        /// <param name="programHandle">32-bit handle of the program to be executed</param>
        public void Run(Int32 programHandle)
        {
            checkScanRunning();

            Conn?.StartDataStream();

            Plugin.ExecWithUserResponseDelegate(
                Cmd.Command((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Exec, programHandle), ScanProgramCallback);
            IsScanning = true;
        }

        /// <summary>
        /// stops the current script execution (blocking)
        /// </summary>
        /// <returns>Response object</returns>
        public Response Stop()
        {
            // Execute stop command
            var exec = new SynchronousCommandGroup(Plugin);
            exec.Add(Cmd.Command((CmdID)CHRLibPlugin.CmdID_Flying_Spot_Stop));
            exec.WaitAll().TryDequeue(out var response);
            // Stop data acquistion
            Conn.StopDataStream();
            IsScanning = false;
            return response;
        }

        private void checkScanRunning()
        {
            if (IsScanning)
                throw new Error("This command is not permitted while scan is being running!");
        }
    }
}
