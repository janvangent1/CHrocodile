/*
TCHRLibConnectionWrapper contains information/wrapper functions for single connection.
TCHRLibConnData is the wrapper class for CHR data.
Two classes aim to simplify function calls to basic Lib functions
*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CHRocodileLib
{
    using ResCb = AsynchronousCommandExecution.ResponseAndUpdateCallback;

    /// <summary>
    /// Base class of all (shared) connections.
    /// </summary>
    public class Connection : IDisposable
    {
        /// <summary>
        /// CHRocodileLib connection handle<br>
        /// Can be used for direct library invocations if needed.
        /// </summary>
        public Conn_h Handle { get; protected set; }

        /// <summary>
        /// returns true if connection is active
        /// </summary>
        public bool IsOpen
        {
            get => Handle != Cmd_h.InvalidHandle;
        }

        /// <summary>
        /// Type of device being connected to
        /// </summary>
        public DeviceType DeviceType { get; protected set; } = DeviceType.Unspecified;

        /// <summary>
        /// The string that is used to identify the device during connection (e. g. IP address)
        /// </summary>
        public string ConnectionInfo { get; protected set; }

        protected DataInfo _curDataInfo;

        public long BufferSize { get; protected set; } = 0; // DLL selects default value

        protected readonly object _currentDataBufferLock = new object();

        protected Data CurrentDataBuffer { get; set; }

        public Data DetachRenewDataBuffer(int newBufferSampleCount = 0)
        {
            if (newBufferSampleCount != 0)
            {
                if (_curDataInfo == null)
                    _curDataInfo = new DataInfo(Handle);
            }

            if (_curDataInfo.SizePerSample == 0)
                throw new Error("DetachRenewDataBuffer: No valid data format info available (Have OutputSignals been set?)");
            long bufferSize = newBufferSampleCount == 0 ?
                BufferDefaultSize : newBufferSampleCount * _curDataInfo.SizePerSample;
            lock (_currentDataBufferLock)
            {
                DeactivateAutoBufferMode();
                Data res = CurrentDataBuffer;
                if (res != null)
                {
                    res.SynchronizeEndPos();
                    res.ParentConnection = null; // now this buffer is detached
                }
                CurrentDataBuffer = new Data(this, bufferSize);
                InitializeNewBuffer(CurrentDataBuffer, 0, IntPtr.Zero);
                return res;
            }
        }

        void IDisposable.Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by derived classes' constructors.<br>
        /// Use SynchronousConnection or AsynchronousConnection constructors.
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <param name="deviceType"></param>
        /// <param name="devBufSize"></param>
        protected Connection(string connectionInfo, DeviceType deviceType, Int64 devBufSize)
        {
            DeviceType = deviceType;
            ConnectionInfo = connectionInfo;
            BufferSize = devBufSize;
        }

        ~Connection()
        {
            Close();
        }

        /// <summary>
        /// Spawn a new synchronous shared connection from this.
        /// </summary>
        /// <returns>new shared synchronous connection</returns>
        public SynchronousConnection ShareSynchronous()
        {
            return new SynchronousConnection(this);
        }

        /// <summary>
        /// Spawn a new asynchronous shared connection from this.
        /// </summary>
        /// <returns>new shared asynchronous connection</returns>
        public AsynchronousConnection ShareAsynchronous()
        {
            return new AsynchronousConnection(this);
        }

        /// <summary>
        /// Close this connection. Any shared connections remain open. If no (shared) connection
        /// is left, the real network or serial device connection will be closed by the library.
        /// NOTE: when used in a "using" environment, explicit call to close is not necessary.
        /// </summary>
        public virtual void Close()
        {
            if (Handle != Cmd_h.InvalidHandle)
            {
                Lib.CloseConnection(Handle);
                Handle = Cmd_h.InvalidHandle;
            }
        }

        /// <summary>
        /// Starts the data stream of this connection. Does NOT affect measurement process itself.
        /// </summary>
        public void StartDataStream()
        {
            Lib.Check(Lib.StartConnectionDataStream(Handle), "Unable to start data stream");
        }

        /// <summary>
        /// Stops the data stream of this connection. Does NOT affect measurement process itself.
        /// </summary>
        public void StopDataStream()
        {
            Lib.Check(Lib.StartConnectionDataStream(Handle), "Unable to stop data stream");
        }

        /// <summary>
        /// specifies output data mode for a CHRocodile device
        /// </summary>
        /// <param name="mode">Lib.Output_Data_Format_Double or Lib.Output_Data_Format_Raw</param>
        public void SetOutputDataFormatMode(OutputDataFormat mode)
        {
            Lib.Check(Lib.SetOutputDataFormatMode(Handle, mode), "Unable to set output data format mode!");
        }

        /// <summary>
        /// Do not call this function directly. Buffer management is done internally.
        /// </summary>
        public void DeactivateAutoBufferMode()
        {
            if (Lib.ConnectionIsAsynchronous(Handle))
                Lib.Check(Lib.DeactivateAsyncDataUserBuffer(Handle));
            else
                Lib.Check(Lib.DeactivateAutoBufferMode(Handle));
        }

        /// <summary>
        /// the number of data samples to be saved in the auto-buffer when it is activated
        /// </summary>
        public Int64 BufferDefaultSize { get; set; } = 50000000; // TODO: make configurable

        /// <summary>
        /// used internally.
        /// </summary>
        /// <param name="allocateNew">specifies if a new memory buffer shall be allocated</param>
        /// <param name="bufferSize">custom size of buffer. Default size used if zero</param>
        /// <returns></returns>
        protected Data AllocateBuffer(bool allocateNew = false, Int64 bufferSize = 0)
        {
            if (allocateNew || CurrentDataBuffer == null)
                CurrentDataBuffer = new Data(this, bufferSize <= 0 ? BufferDefaultSize : bufferSize);
            return CurrentDataBuffer;
        }

        protected void InitializeNewBuffer(Data xd, int preloadSampleCount, IntPtr preloadSamplesSrc,
            Int64 endPos = 0, bool activateAutoBufferMode = true)
        {
            if (_curDataInfo == null)
                _curDataInfo = new DataInfo(Handle);
            if (_curDataInfo.SizePerSample == 0)
                throw new Error("DetachRenewDataBuffer: No valid data format info available (Have OutputSignals been set?)");

            Int64 bufferSize = xd.BufferSize;
            int sizeOfExistingData = (int)(preloadSampleCount * _curDataInfo.SizePerSample);
            IntPtr autoSaveTargetPtr = IntPtr.Add(xd.BufferPtr, sizeOfExistingData);
            // the number of samples which remain to be filled via autoBufferSave:
            Int64 leftBufferSize = bufferSize - sizeOfExistingData;
            Int64 leftSampleCount = (leftBufferSize / _curDataInfo.SizePerSample);

            Int64 end = endPos <= 0 ? (long)preloadSampleCount : Math.Min(endPos, (long)preloadSampleCount);
            xd.Reset(_curDataInfo, preloadSampleCount, 0, end);

            // copy existing data (from prior GetNextSamples() call) into buffer:
            if (sizeOfExistingData > 0)
                Marshal.Copy(preloadSamplesSrc, xd.Raw, 0, sizeOfExistingData);

            // activate auto buffer save function only if there is any space left to fill:
            if (activateAutoBufferMode && leftSampleCount > 0)
            {
                Int64 requiredSize = leftBufferSize;
                if (Lib.ConnectionIsAsynchronous(Handle))
                    Lib.Check(Lib.ActivateAsyncDataUserBuffer(Handle, autoSaveTargetPtr, leftSampleCount, ref requiredSize));
                else
                {
                    Lib.Check(Lib.ActivateAutoBufferMode(Handle, autoSaveTargetPtr, leftSampleCount, ref requiredSize));
                    Debug.WriteLine("**********BufferPtr=" + xd.BufferPtr.ToString("X")
                        + "    TP=" + autoSaveTargetPtr.ToString("X") + $" SCnt={leftSampleCount} "
                        + " "
                        + "    End=" + IntPtr.Add(xd.BufferPtr, (int)BufferDefaultSize).ToString("X")
                        + "    LeftSizeEnd=" + IntPtr.Add(autoSaveTargetPtr, (int)leftBufferSize).ToString("X")
                        + "    requiredEnd=" + IntPtr.Add(autoSaveTargetPtr, (int)requiredSize).ToString("X")
                        );
                }
            }
        }

        #region Plugin
        protected Conn_h AddPlugin(string _strPluginName)
        {
            Lib.Check(Lib.AddConnectionPlugIn(Handle, _strPluginName, out Conn_h H),
                $"Error in adding plugin: {_strPluginName}");
            return H;
        }

        protected Conn_h AddPlugin(UInt32 _pluginID)
        {
            Lib.Check(Lib.AddConnectionPlugInByID(Handle, _pluginID, out Conn_h H),
                $"Error in adding plugin by ID {_pluginID}");
            return H;
        }
        #endregion

        private Exception _lastException = null;

        /// <summary>
        /// Exception caught in one of the callback routines
        /// </summary>
        public Exception LastException
        {
            get => Volatile.Read(ref _lastException);
            set => Volatile.Write(ref _lastException, value);
        }
    };

    public abstract class SynchronousCommandExecution : Connection
    {
        protected SynchronousCommandExecution(string connectionInfo, DeviceType deviceType, long devBufSize)
            : base(connectionInfo, deviceType, devBufSize)
        {
        }

        public abstract Response Exec(Cmd_h cmdHandle);

        /// <summary>
        /// Executes a command that is given as CmdID plus comma-separated arguments
        /// 
        /// Example:
        ///     var res = Exec(CmdID.ScanRate, 4000);
        /// </summary>
        /// <param name="cmd">Command ID enumeration type\b
        /// commands not included in the enumeration can be converted from strings, see Lib.StrToCmdID()</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>Response</returns>
        public Response Exec(CmdID cmd, params object[] args)
        {
            return Exec(Cmd.Command(cmd, args));
        }

        /// <summary>
        /// Executes a command that is given as a string.
        /// 
        /// Example:
        ///     var res = ExecString("SHZ 4000");
        /// </summary>
        /// <param name="cmd">complete command string including command name and all arguments\b
        /// Queries end with a question mark at the end. Example: ENC 0 ?</param>
        /// <returns>Response</returns>
        public Response ExecString(string cmd)
        {
            Lib.Check(Lib.NewCommandFromString(cmd, out var hCmd));
            return Exec(hCmd);
        }

        /// <summary>
        /// Executes a command query that is given as CmdID plus comma-separated arguments
        /// 
        /// Example:
        ///     var res = Query(CmdID.ScanRate);
        /// </summary>
        /// <param name="cmd">Command ID enumeration type</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>Response</returns>
        public Response Query(CmdID cmd, params object[] args)
        {
            return Exec(Cmd.Query(cmd, args));
        }
    }



    public class SynchronousConnection : SynchronousCommandExecution
    {
        /// <summary>
        /// used to pin the data buffer so that CHRocodileLib can maintain access to it
        /// </summary>
        private GCHandle _pinnedArray;

        /// <summary>
        /// buffer to store "LastSample" (see GetLastSample())
        /// </summary>
        private Data _getLastSampleData;

        /// <summary>
        /// Create and open a synchronous connection given the connection info (IP address or COM port),
        /// the device type, and optionally the size of the device stream input buffer.<br>
        /// Note that the connection is opened immediately. If the connection cannot be established,
        /// the object will not be created.
        /// </summary>
        /// <param name="connectionInfo">connection information as described in the library manual</param>
        /// <param name="deviceType"></param>
        /// <param name="devBufSize"></param>
        public SynchronousConnection(string connectionInfo, DeviceType deviceType = DeviceType.Unspecified,
            Int64 devBufSize = 0) : base(connectionInfo, deviceType, devBufSize)
        {
            Open();
            //CurrentDataBuffer = new Data(this, BufferDefaultSize); // accessed via GetNextSample()
            _getLastSampleData = new Data(this, 4096); // initial sample size, can be enlarged if needed
        }

        /// <summary>
        /// Used to share an existing connection
        /// </summary>
        /// <param name="connectionToShare">
        /// The existing connection from which this shared connection shall be spawned
        /// </param>
        public SynchronousConnection(Connection connectionToShare)
            : base(connectionToShare.ConnectionInfo, connectionToShare.DeviceType, connectionToShare.BufferSize)
        {
            Lib.Check(Lib.OpenSharedConnection(connectionToShare.Handle, (Int32)ConnectionMode.Synchronous, out var handle),
                "Opening shared connection failed");
            Handle = handle;
            CurrentDataBuffer = new Data(this, BufferDefaultSize); // accessed via GetNextSample()
            _getLastSampleData = new Data(this, 4096); // initial sample size, can be enlarged if needed
        }

        #region Open/Close connection functions

        /// <summary>
        /// Open connection to sensor.
        /// Called internally by the constructor.
        /// </summary>
        private void Open()
        {
            Lib.Check(Lib.OpenConnection(ConnectionInfo, (Int32)DeviceType,
                (Int32)ConnectionMode.Synchronous, BufferSize, out var handle),
                $"Openning connection {ConnectionInfo} failed");
            Lib.SetLibConfigFlags(ConfigFlags.None);
            Handle = handle;
        }

        /// <summary>
        /// Close this (shared) connection and free it's buffer.
        /// Once closed, the connection cannot be reused. Create a new one to reconnect.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (_pinnedArray.IsAllocated)
                _pinnedArray.Free();
        }
        #endregion

        /// <summary>
        /// Used internally to "prime" a new AutoBuffer (i. e. fill in existing data before entering the auto mode)
        /// </summary>
        /// <exception cref="Error"></exception>
        private void GetNextSamples(Int64 bufferSize, ref DataInfo info, out IntPtr dataRawPointer, out Int64 sampleCount)
        {
            Lib.Check(Lib.GetSingleOutputSampleSize(Handle, out Int64 sizePerSample));
            sampleCount = bufferSize / sizePerSample;
            Int32 res = Lib.GetNextSamples(Handle, ref sampleCount, out dataRawPointer, out Int64 sampleSize,
                out SignalGeneralInfo genInfo, out IntPtr sigInfo);
            if (sampleSize == 0)
                throw new Error("No active signals. Request signals via SODX first.");
            if (res < 0)
                Lib.Check(res);

            // If needed, update local signal info and set up a new buffer:
            if (info == null || genInfo.InfoIndex != info.SignalGenInfo.InfoIndex)
            {
                info = new DataInfo(sizePerSample, genInfo, sigInfo);
            }
        }

        protected object _recordingLock = new object();
        public bool IsInRecordingMode { get; protected set; }

        /// <summary>
        /// Starts a new auto buffer save process. Returns error code 
        /// </summary>
        /// <param name="createNew"></param>
        /// <returns></returns>
        protected void StartNewAutoBuffer(Int64 maxSampleCount)
        {
            // before starting the asynchronous autobuffer save mode, "prime" it, i.e. read as many samples
            //  as available / possible into the buffer. The function below makes sure there will be only
            // as many samples retrieved as fit in the buffer:
            GetNextSamples(BufferDefaultSize, ref _curDataInfo, out IntPtr dataSrc, out Int64 sampleCount);

            Data xd = AllocateBuffer();

            InitializeNewBuffer(xd, (int)sampleCount, dataSrc, maxSampleCount, true);
            CurrentDataBuffer = xd;
        }

        #region exec/query functions
        /// <summary>
        /// Executes a command that has been created by means of the base functions given in the Lib class.
        /// 
        /// Examples:
        ///     Lib.NewCommandFromString("SHZ 4000", out var handle);
        ///     var res = Exec(handle);
        ///     Lib.NewCommand("SHZ", false, out var handle);
        ///     Lib.AddCommandFloatArg(handle, 4000.0);
        ///     var res = Exec(handle);
        /// </summary>
        /// <param name="cmdHandle">Command handle as returned by Lib.NewCommand... functions</param>
        /// <returns>Response</returns>
        public override Response Exec(Cmd_h cmdHandle)
        {
            lock (_recordingLock)
            {
                if (IsInRecordingMode)
                    throw new Error("Cannot execute command or query - connection currently in recording mode.");
                DeactivateAutoBufferMode();
                Lib.Check(Lib.ExecCommand(Handle, cmdHandle, out var rsp));
                return new Response(rsp, Handle, false);
            }
        }

        #endregion

        #region GetNext(Last)Samples 
        public Data.Sample GetLastSample()
        {
            if (IsInRecordingMode)
                throw new Error("GetLastSample() calls not allowed in recording mode");

            DeactivateAutoBufferMode();

            // read next samples from lib:
            Res_t res = Lib.GetLastSample(Handle, out IntPtr pTemp,
                out long sampleSize, out SignalGeneralInfo genInfo, out IntPtr pSigInfo);

            Lib.Check(res, "GetLastSample:");

            if ((Int32)res == 0)
                return null; // no sample available

            if (sampleSize > _getLastSampleData.BufferSize)
                _getLastSampleData = new Data(this, sampleSize);

            // generate new data info object if necessary:
            if (_curDataInfo == null || genInfo.InfoIndex != _curDataInfo.SignalGenInfo.InfoIndex)
                _curDataInfo = new DataInfo(sampleSize, genInfo, pSigInfo);

            InitializeNewBuffer(_getLastSampleData, 1, pTemp, 0, false);
            // create and fill data container:
            return _getLastSampleData.Samples().First();
        }
        #endregion

        // DOES NOT WORK -> FIX in libary.
        //private uint GetSignalInfoIndex()
        //{
        //    Int64 bufSize = 0;
        //    Res_t res = Lib.GetConnectionOutputSignalInfos(Handle, out var genInfo, new SignalInfo[0], ref bufSize);
        //    return genInfo.InfoIndex;
        //}

        #region Recording mode

        public Int32[] GetDeviceOutputSignalIDs()
        {
            Lib.GetDeviceOutputSignals(Handle, new Int32[0], out int nSignalNr);
            Int32[] aSignals = new int[nSignalNr];
            if (nSignalNr >= 0)
            {
                var nRes = Lib.GetDeviceOutputSignals(Handle, aSignals, out nSignalNr);
                if (!Lib.ResultSuccess(nRes))
                    throw new Error("Error in getting device output signal IDs.");
            }
            return (aSignals);
        }
        #endregion

        #region Auto buffer functions
        public void FlushConnectionBuffer()
        {
            DeactivateAutoBufferMode();
            var nRes = Lib.FlushConnectionBuffer(Handle);
            if (!Lib.ResultSuccess(nRes))
                throw new Error("Error in flushing connection buffer!");
        }

        /// <summary>
        /// Enter recording mode. Start recording a defined number of samples.\b
        /// During recording mode, commands for forbidden.
        /// </summary>
        /// <param name="recordingSampleCount">number of samples to be saved</param>
        public void StartRecording(int recordingSampleCount)
        {
            DetachRenewDataBuffer(recordingSampleCount);

            //_curDataInfo = new DataInfo(Handle);
            //int size = (int)(recordingSampleCount * _curDataInfo.SizePerSample);
            //CurrentDataBuffer = new Data(this, size);
            //InitializeNewBuffer(CurrentDataBuffer, 0, IntPtr.Zero);
            IsInRecordingMode = true;
        }

        /// <summary>
        /// Leaves recording mode, detaches collected data from recording.
        /// </summary>
        /// <returns>data recorded (detached, can be freely used)</returns>
        public Data StopRecording()
        {
            IsInRecordingMode = false;
            return DetachRenewDataBuffer();
        }


        /// <summary>
        /// returns if in recording mode and still actively recording samples
        /// </summary>
        public bool IsRecording
        {
            get => IsInRecordingMode
                && Lib.GetAutoBufferStatusWithCheck(Handle) == AutoBufferStatus.Saving;
        }

        public Data GetNextSamples(Int64 maxSampleCount = 0)
        {
            AutoBufferStatus autoBufferStatus = Lib.GetAutoBufferStatusWithCheck(Handle);
            DataStatus dataStatus;
            if (autoBufferStatus < 0)
                dataStatus = DataStatus.Error;
            else if (autoBufferStatus == AutoBufferStatus.Saving)
                dataStatus = DataStatus.Saving;
            else
                dataStatus = DataStatus.Stopped;

            if (CurrentDataBuffer != null)
            {
                var data = CurrentDataBuffer;
                data.Status = dataStatus;
                data.MoveReadPosToEnd(); // make sure the client will not read the same samples twice

                /*
                 * in synchronous mode, any samples moved into the buffer after the last
                 * GetNExtSamples() call - but before a consecutive Exec() call, must be ignored.
                 * So only call SynchronizeEndPos() in case of neither Response nor Deactivated.
                 */
                if (autoBufferStatus != AutoBufferStatus.Response
                                    && autoBufferStatus != AutoBufferStatus.Deactivated)
                {
                    if (data.SynchronizeEndPos(maxSampleCount))
                        return data;
                }

                // there are no more samples available, just return the current (depleted) buffer:
                if (autoBufferStatus == AutoBufferStatus.Saving)
                    return CurrentDataBuffer;
            }

            // buffer needs to be renewed
            StartNewAutoBuffer(maxSampleCount);
            return CurrentDataBuffer;
        }

        #endregion

        #region Plugins handling

        public class Plugin : SynchronousCommandExecution
        {
            public SynchronousConnection Parent { get; }
            private object _parentRecordingLock;

            public Plugin(SynchronousConnection parent, string name, Conn_h pluginHandle)
                : base(parent.ConnectionInfo + $"/{name}", parent.DeviceType, 0)
            {
                base.Handle = pluginHandle;
                Parent = parent;
                _parentRecordingLock = parent._recordingLock;
            }

            public override Response Exec(Cmd_h cmdHandle)
            {
                lock (_parentRecordingLock)
                {
                    if (Parent.IsInRecordingMode)
                        throw new Error("Cannot execute command or query - connection currently in recording mode.");
                    Parent.DeactivateAutoBufferMode();
                    Lib.Check(Lib.ExecCommand(Handle, cmdHandle, out var rsp));
                    return new Response(rsp, Handle, false);
                }
            }

        }

        /// <summary>
        /// Loads and activates a plugin
        /// </summary>
        /// <param name="pluginName">Name of the plugin as described in API description / AddConnectionPlugIn()</param>
        /// <returns>Plugin object used to communicate with the plugin</returns>
        public Plugin InsertPlugin(string pluginName)
        {
            return new Plugin(this, pluginName, AddPlugin(pluginName));
        }

        /// <summary>
        /// Loads and activates a plugin
        /// </summary>
        /// <param name="pluginID">ID of the plugin as described in API description / AddConnectionPlugInByID()</param>
        /// <returns>Plugin object used to communicate with the plugin</returns>
        public Plugin InsertPlugin(UInt32 pluginID)
        {
            return new Plugin(this, $"ID{pluginID}", AddPlugin(pluginID));
        }

        #endregion
    }


    public abstract class AsynchronousCommandExecution : Connection
    {
        /// <summary>
        /// Signature of functions registered as callback functions for command responses or updates
        /// </summary>
        /// <param name="rsp"></param>
        public delegate void ResponseAndUpdateCallback(Response rsp);
        protected Lib.ResponseAndUpdateCallback _libSpecCmdCb;    // special user callback
        protected Lib.ResponseAndUpdateCallback _libSpecCmdCbExternalDelegateReference;    // special user callback

        protected AsynchronousCommandExecution(string connectionInfo, DeviceType deviceType,
            long devBufSize) : base(connectionInfo, deviceType, devBufSize)
        {
            _libSpecCmdCb = CmdSpecialCbFct;
            _libSpecCmdCbExternalDelegateReference = CmdSpecialCbFctExternalDelegateReference;
        }


        private void CmdSpecialCbFct(TRspCallbackInfo _sInfo, Rsp_h _hRsp)
        {
            if (_sInfo.User == IntPtr.Zero)
                return;

            GCHandle handle = new GCHandle();
            try
            {
                /*
                 * Call user-defined general response callback, but not throw in case of set error flag.
                 * Since this is typically executing in a separate thread, throwing is not straight-forward.
                 */
                handle = GCHandle.FromIntPtr(_sInfo.User);
                var cb = handle.Target as ResponseAndUpdateCallback;
                if (cb == null)
                    throw new Error("CmdSpecialCbFct: Invalid delegate.");
                bool isUpdate = _sInfo.Source != Handle;
                var rsp = new Response(_hRsp, _sInfo.Source, isUpdate, false);
                cb.Invoke(rsp);
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        private void CmdSpecialCbFctExternalDelegateReference(TRspCallbackInfo _sInfo, Rsp_h _hRsp)
        {
            if (_sInfo.User == IntPtr.Zero)
                return;
            try
            {
                /*
                 * Call user-defined general response callback, but not throw in case of set error flag.
                 * Since this is typically executing in a separate thread, throwing is not straight-forward.
                 */
                var cb = Marshal.GetDelegateForFunctionPointer<ResponseAndUpdateCallback>(_sInfo.User);
                bool isUpdate = _sInfo.Source != Handle;
                var rsp = new Response(_hRsp, _sInfo.Source, isUpdate, false);
                cb.Invoke(rsp);
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }

        /// <summary>
        /// Executes (asynchronously) a command that has been created by means of the base functions
        /// given in the Lib class.<br/>
        /// It is also used as a backend for other variants such as "Exec(CmdID.ScanRate, ...)"<br/>
        /// <b>IMPORTANT</b>: No updates will be forwarded to userCb, only the response. If your command
        /// will generate updates (such as FSS/run), call ExecWithUserResponseDelegate() with an own delegate.
        /// </summary>
        /// Example:
        /// <code>
        ///     Cmd_h cmd = Cmd.FromStr("SHZ 4000");
        ///     var ticket = Exec(cmd, rsp => Console.WriteLine($"Response: {rsp}"));
        /// </code>
        /// <param name="cmdHandle">Command handle as returned by Cmd.Command, Cmd.Query ...</param>
        /// <param name="userCb">User callback function to be called once the response is ready</param>
        /// <returns>ticket to later identify the response, especially when using </returns>
        public int Exec(Cmd_h cmdHandle, ResponseAndUpdateCallback userCb)
        {
            IntPtr puser = IntPtr.Zero;
            if (userCb != null)
            {
                GCHandle gcHandleUserCb = GCHandle.Alloc(userCb);
                puser = GCHandle.ToIntPtr(gcHandleUserCb);
            }
            Lib.Check(Lib.ExecCommandAsync(Handle, cmdHandle, puser, _libSpecCmdCb, out var ticket));
            return ticket;
        }

        /// <summary>
        /// Executes (asynchronously) a command that has been created by means of the base functions
        /// given in the Lib class.<br/>
        /// It is also used as a backend for other variants such as "Exec(CmdID.ScanRate, ...)"<br/>
        /// <b>IMPORTANT</b>: This variant forwards the response as well as any subsequent command-related
        /// updates to userCb - and is therefore suited for e. g. FSS/run.<br/>
        /// However, the delegate ResponseAndUpdateCallback passed in <b>must</b> be managed
        /// by the user (e. g. kept in a member variable).
        /// </summary>
        /// Example:
        /// <code>
        ///     private Lib.ResponseAndUpdateCallback myCallback = ....; // object field
        ///     [...]
        ///     Cmd_h cmd = Cmd.FromStr("SHZ 4000");
        ///     var ticket = Exec(cmd, myCallback);
        /// </code>
        /// <param name="cmdHandle">Command handle as returned by Cmd.Command, Cmd.Query ...</param>
        /// <param name="userCb">User callback function to be called once the response is ready</param>
        /// <returns>ticket to later identify the response, especially when using </returns>
        public int ExecWithUserResponseDelegate(Cmd_h cmdHandle, ResponseAndUpdateCallback userCb)
        {
            IntPtr puser = (userCb == null) ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(userCb);
            Lib.Check(Lib.ExecCommandAsync(Handle, cmdHandle, puser, _libSpecCmdCbExternalDelegateReference,
                out var ticket));
            return ticket;
        }

        /// <summary>
        /// Executes (asynchronously) a command that has been created by means of the base functions given in the Lib class.X
        /// </summary>
        /// <code>
        ///     Lib.NewCommandFromString("SHZ 4000", out var handle);
        ///     var res = Exec(handle);
        ///     
        ///     Lib.NewCommand("SHZ", false, out var handle);
        ///     Lib.AddCommandFloatArg(handle, 4000.0);
        ///     var resp = await ExecAsync(handle);
        /// </code>
        /// <param name="cmdHandle">Command handle as returned by Lib.NewCommand... functions</param>
        /// <returns>CHRocoTask object returing Response when async operation is finished</returns>
        /// <summary>
        public CHRocoTask ExecAsync(Cmd_h commandHandle)
        {
            var task = new CHRocoTask();
            Lib.Check(Lib.ExecCommandAsync(Handle, commandHandle, IntPtr.Zero,
                    task.GetAwaiter()._libCallback, out var ticket));
            return task;
        }

        /// <summary>
        /// Executes (asynchronously) a command that is given as a string.
        /// 
        /// Example:
        ///     var resp = await ExecStringAsync("SHZ 4000");
        /// </summary>
        /// <param name="cmd">complete command string including command name and all arguments</param>
        /// <returns>CHRocoTask object returing Response when async operation is finished</returns>
        public CHRocoTask ExecStringAsync(string cmd)
        {
            Lib.Check(Lib.NewCommandFromString(cmd, out var hCmd));
            return ExecAsync(hCmd);
        }

        /// <summary>
        /// Executes (asynchronously) a command that is given as CmdID plus comma-separated arguments
        /// 
        /// Example:
        ///     var resp = await ExecAsync(CmdID.ScanRate, 4000);
        /// </summary>
        /// <param name="cmd">Command ID enumeration type</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>CHRocoTask object returing Response when async operation is finished</returns>
        public CHRocoTask ExecAsync(CmdID cmd, params object[] args)
        {
            Cmd_h hCmd = Cmd.Command(cmd, args);
            return ExecAsync(hCmd);
        }

        /// <summary>
        /// Executes (asynchronously) a command that is given by string identifier plus comma-separated arguments
        /// 
        /// Example:
        ///     var resp = await ExecAsync("SHZ", 4000);
        /// </summary>
        /// <param name="cmdStr">command string identifier consisting of 3 or 4 capital letters</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>Response</returns>
        public CHRocoTask ExecAsync(string cmdStr, params object[] args)
        {
            return ExecAsync(Lib.StrToCmdID(cmdStr), args);
        }

        /// <summary>
        /// Executes (asynchronously) a command query that is given as CmdID plus comma-separated arguments
        /// 
        /// Example:
        ///     var resp = QueryAsync(CmdID.ScanRate);
        /// </summary>
        /// <param name="cmd">Command ID enumeration type</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>Response</returns>
        public CHRocoTask QueryAsync(CmdID cmd, params object[] args)
        {
            Cmd_h hCmd = Cmd.Query(cmd, args);
            return ExecAsync(hCmd);
        }

        /// <summary>
        /// Executes (asynchronously) a command query that is given by string identifier plus comma-separated arguments
        /// 
        /// Example:
        ///     var resp = QueryAsync("SCAN", 13);
        /// </summary>
        /// <param name="cmdStr">command string identifier consisting of 3 or 4 capital letters</param>
        /// <param name="args">Comma-separated arguments of various types</param>
        /// <returns>Response</returns>
        public CHRocoTask QueryAsync(string cmdStr, params object[] args)
        {
            return QueryAsync(Lib.StrToCmdID(cmdStr), args);
        }

        //public abstract int Exec(Cmd_h cmdHandle, ResponseAndUpdateCallback userCb);

        public int Exec(CmdID cmd, ResponseAndUpdateCallback userCb, params object[] args)
        {
            var hCmd = Cmd.Command(cmd, args);
            return Exec(hCmd, userCb);
        }

        public int Exec(string cmdStr, ResponseAndUpdateCallback userCb, params object[] args)
        {
            var hCmd = Cmd.Command(Lib.StrToCmdID(cmdStr), args);
            return Exec(hCmd, userCb);
        }

        /// <summary>
        /// Executes (asynchronously) a command that has been created by means of the base functions given in the Lib class.
        /// </summary>
        /// <param name="cmdStr">Command plus arguments. Example: "AAL 1 70"</param>
        /// <param name="userCb"></param>
        /// <returns></returns>
        public int ExecString(string cmdStr, ResponseAndUpdateCallback userCb = null)
        {
            var hCmd = Cmd.FromStr(cmdStr);
            return Exec(hCmd, userCb);
        }

        public int Query(CmdID cmd, ResponseAndUpdateCallback userCb, params object[] args)
        {
            var hCmd = Cmd.Query(cmd, args);
            return Exec(hCmd, userCb);
        }

        public int Query(string cmdStr, ResponseAndUpdateCallback userCb, params object[] args)
        {
            var hCmd = Cmd.Query(Lib.StrToCmdID(cmdStr), args);
            return Exec(hCmd, userCb);
        }
    }

    /// <summary>
    /// Executes one or more asynchronous commands and collects all responses.<br/>
    /// WaitAll() blocks until all responses have arrived.<br/>
    /// This can also increase the command throughput as compared to pure synchronous command execution.<br/>
    /// <b>Use as:</b><br/>
    /// var commands = new CHRocodileLib.SynchronousCommandGroup(con);<br/>
    /// commands.Add(Cmd.Command(CmdID.ScanRate, 1000));<br/>
    /// // more commands / queries if needed...<br/>
    /// while (commands.WaitAll().TryDequeue(out var response))<br/>
    ///     // process responses<br/>
    /// </summary>
    public class SynchronousCommandGroup
    {
        /// <summary>
        /// The connection / plugin for the commands to send to
        /// </summary>
        public AsynchronousCommandExecution Connection { get; private set; }
        /// <summary>
        /// The number of commands sent
        /// </summary>
        public int Count { get; private set; } = 0;
        /// <summary>
        /// Comstructs a command group for the given connection.<br/>
        /// </summary>
        /// <param name="con"></param>
        public SynchronousCommandGroup(AsynchronousCommandExecution con)
        {
            Connection = con;
        }
        /// <summary>
        /// Adds a command to execute from within this synchronized group.<br/>
        /// <b>NOTE:</b> Add() and WaitAll() must be called from the same thread.<br/>
        /// <b>Usage (example):</b><br/>
        /// commands.Add(Cmd.Command(CmdID.ScanRate, 1000));<br/>
        /// </summary>
        /// <param name="command">The Cmd_h handle (as returned by Cmd.Command() and Query())</param>
        /// <returns></returns>
        public int Add(Cmd_h command)
        {
            if (_pickedUp)
            {
                Responses = new ConcurrentQueue<Response>();
                Count = 0;
                _pickedUp = false;
            }
            int res = Connection.Exec(command, SetResponse);
            Count++;
            return res;
        }
        /// <summary>
        /// Used internally. This is the response callback invoked from CHRocodileLib.
        /// </summary>
        /// <param name="res">The response.</param>
        private void SetResponse(Response res)
        {
            lock (_conditionVar)
            {
                Responses.Enqueue(res);
                Monitor.Pulse(_conditionVar);
            }
        }
        /// <summary>
        /// Waits for all responses to arrive.<br/>
        /// Optional timeout is given per command response.<br/>
        /// <b>NOTE:</b> Add() and WaitAll() must be called from the same thread.
        /// </summary>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns>Collection of all command responses</returns>
        /// <exception cref="Error">On timeout</exception>
        public ConcurrentQueue<Response> WaitAll(int timeoutMilliseconds = Timeout.Infinite)
        {
            lock (_conditionVar)
            {
                while (true)
                {
                    if (!Monitor.Wait(_conditionVar, timeoutMilliseconds))
                        throw new Error($"SynchronousCommandGroup: timeout waiting for response #{Responses.Count}");

                    if (Responses.Count >= Count)
                        break;
                }
            };
            _pickedUp = true;
            return Responses;
        }

        /// <summary>
        /// The collection of responses in the order of arrival.<br/>
        /// After WaitAll(), the next call to Add() will first clear the list.
        /// </summary>
        public ConcurrentQueue<Response> Responses { get; private set; } = new ConcurrentQueue<Response>();

        /// <summary>
        /// Used internally. The synchronization object.
        /// </summary>
        private object _conditionVar = new object();
        private bool _pickedUp = false;
    }




    public partial class AsynchronousConnection : AsynchronousCommandExecution
    {
        // library callback delegates
        private Lib.ResponseAndUpdateCallback _libGenCmdCb;     // general callback
        // callback delegates of this class / interface
        private ResponseAndUpdateCallback _genCmdCb;

        /// <summary>
        /// sets general response and update callback function. 
        /// </summary>
        public ResponseAndUpdateCallback GeneralResponseAndUpdateCallback
        {
            get => _genCmdCb;
            private set
            {
                _genCmdCb = value;
                Lib.Check(Lib.RegisterGeneralResponseAndUpdateCallback(Handle, IntPtr.Zero,
                        _genCmdCb != null ? _libGenCmdCb : null), "Unable to register general command callback");
            }
        }
        private Lib.SampleDataCallback _libDataCb; // CHRocoLib side

        public delegate void DataCallback(AsyncDataStatus status, Data data); // client side

        /// <summary>
        /// Your callback routine to receive measurement data. Use SetDataCallback() to register it.
        /// </summary>
        public DataCallback DataCb { get; private set; }

        /// <summary>
        /// Used to register the application function that is called whenever new data arrives.
        /// </summary>
        /// <param name="dataCb">pointer to user data callback</param>
        /// <param name="maxSampleCount">maximum number of samples to be read during single device output processing</param>
        /// <param name="readTimeOut">timeout in msec waiting for 'maxSampleCount' samples. If set to 0, only available samples are read without waiting</param>
        public void SetDataCallback(DataCallback dataCb, long maxSampleCount = 100, int readTimeOut = 100)
        {
            lock (this)
            {
                DataCb = dataCb;
                Lib.Check(Lib.RegisterSampleDataCallback(Handle, maxSampleCount, readTimeOut, IntPtr.Zero,
                    DataCb != null ? _libDataCb : null), "Unable to register data callback");
            }
        }
        /// <summary>
        /// Used to register the application function that is called whenever a command response
        /// or update arrives.<br/>
        /// This function is called after any command-specific callback function (if defined).<br/>
        /// Implementing this handler is a straight-forward way of keeping the application up-to-date
        /// with the sensor state.
        /// </summary>
        /// <param name="cb">the callback function to invoke</param>
        public void SetGeneralResponseCallback(ResponseAndUpdateCallback cb)
        {
            GeneralResponseAndUpdateCallback = cb;
        }

        private bool? _automaticMode;

        /// <summary>
        /// If true, activates automatic mode which means theat an internal library thread<br/>
        /// pushes data and command responses to the user application, i. e. any data / response<br/>
        /// callback will be called from within that internal thread.<br/>
        /// <b>Alternatively</b> on can set this property to "false" and instead call<br/>
        /// Lib.ProcessDeviceOutput(handle) repeatedly from a user thread.
        /// </summary>
        public bool AutomaticMode
        {
            get => _automaticMode ?? false;
            set
            {
                if (IsOpen && (_automaticMode ?? false) != value)
                {
                    var res = value ? Lib.StartAutomaticDeviceOutputProcessing(Handle) :
                                      Lib.StopAutomaticDeviceOutputProcessing(Handle);
                    Lib.Check(res, "Unable to start/stop automatic device output processing");
                }
                _automaticMode = value;
            }
        }

        /// <summary>
        /// Close connection. This will be called automatically when the connection object
        /// gets destroyed or "Disposed".
        /// </summary>
        public override void Close()
        {
            base.Close();
        }
        /// <summary>
        /// Create an asynchronous connection, based on ip address, /dev/ttySx file or COM port.
        /// </summary>
        /// <param name="connectionInfo">ip address, /dev/ttySx file, or COMx port</param>
        /// <param name="deviceType">device type (CHR1, CHR2, multi-channel, ...)</param>
        /// <param name="devBufSize">size of the buffer that holds incoming socket data.
        ///  Leave unassigned for default size.</param>
        public AsynchronousConnection(string connectionInfo, DeviceType deviceType, Int64 devBufSize = 0)
                    : base(connectionInfo, deviceType, devBufSize)
        {
            _libGenCmdCb = CmdGenCbFct;
            _libDataCb = SampleDataCbFct;
            Open();
            //CurrentDataBuffer = new Data(this, BufferDefaultSize);
        }

        public AsynchronousConnection(Connection connectionToShare)
            : base(connectionToShare.ConnectionInfo, connectionToShare.DeviceType, connectionToShare.BufferSize)
        {
            Lib.Check(Lib.OpenSharedConnection(connectionToShare.Handle,
                (Int32)ConnectionMode.Asynchronous, out Conn_h hShared), "Opening shared connection failed");
            Handle = hShared;
            _libGenCmdCb = CmdGenCbFct;
            _libDataCb = SampleDataCbFct;
            CurrentDataBuffer = new Data(this, BufferDefaultSize);
        }
        /// <summary>
        /// Used internally. Calls library function.
        /// </summary>
        private void Open()
        {
            Lib.Check(Lib.OpenConnection(ConnectionInfo, (Int32)DeviceType, (Int32)ConnectionMode.Asynchronous,
                BufferSize, out var handle), $"Opening connection {ConnectionInfo} failed");
            Handle = handle;
        }

        /// <summary>
        /// Prepare (new) AutoBuffer for usage in sample callback routine
        /// </summary>
        /// <param name="info">data format info</param>
        /// <param name="existingSampleCount">"priming" the buffer: in case of existing samples, copy them here</param>
        /// <param name="dataSrc">source of data to copy (see above)</param>
        /// <returns></returns>
        private Data StartNewAutoBuffer(DataInfo info, int existingSampleCount, IntPtr dataSrc)
        {
            Data xd = AllocateBuffer();
            InitializeNewBuffer(xd, existingSampleCount, dataSrc);
            return xd;
        }
        /// <summary>
        /// Used internally. Will be called by library. Details can be found in library documentation.
        /// </summary>
        
        private void HandleDataFormatChange()
        {
            // Inform user, give opportunity to detach:
            if (DataCb != null)
                DataCb(AsyncDataStatus.DataFormatChange, CurrentDataBuffer);
            DeactivateAutoBufferMode();
            // Get new data format info via "manual" call because SampleDataCbFct does not carry this info (yet)
            _curDataInfo = new DataInfo(Handle);
            if (_curDataInfo.SizePerSample > 0)
            {
                Data xd = StartNewAutoBuffer(_curDataInfo, 0, IntPtr.Zero);
                xd.Status = DataStatus.Saving;
            }
        }

        private void SampleDataCbFct(IntPtr pUser, AsyncDataStatus status, Int64 sampleCount,
            IntPtr sampleBuffer, Int64 sizePerSample, SignalGeneralInfo genInfo, IntPtr signalInfo)
        {
            if (DataCb == null)
                return;

            if (status == AsyncDataStatus.DataFormatChange)
            {
                HandleDataFormatChange();
                return;
            }

            // TODO: Handle  sizePerSample == 0

            DataStatus dataStatus;
            if (status < 0)
                dataStatus = DataStatus.Error;
            else if (status == AsyncDataStatus.UserBufferFull || status == AsyncDataStatus.DataFormatChange)
                dataStatus = DataStatus.Stopped;
            else
                dataStatus = DataStatus.Saving;


            lock (this) // TODO: other lock object
            {
                try
                {
                    bool mustStartNewBuffer = false;
                    if (_curDataInfo == null || genInfo.InfoIndex != _curDataInfo.SignalGenInfo.InfoIndex)
                    {
                        _curDataInfo = new DataInfo(sizePerSample, genInfo, signalInfo);
                        mustStartNewBuffer = true;
                    }

                    if (CurrentDataBuffer == null)
                    {
                        // this is the first auto buffer allocated - so we need "prime" the buffer with current samples:
                        Data xd = StartNewAutoBuffer(_curDataInfo, (int)sampleCount, sampleBuffer);
                        xd.Status = DataStatus.Saving;
                        if (sampleCount > 0)
                            DataCb(status, xd);
                        return;
                    }

                    Data data = CurrentDataBuffer;
                    data.MoveReadPosToEnd(); // make sure the client will not read the same samples twice
                    data.Status = dataStatus;

                    if (data.SynchronizeEndPos(0)) // true if there are more samples available
                        DataCb(status, data);
                    if (DataCb == null) // data flow can be reset inside DataCb!
                        return;

                    mustStartNewBuffer |= (status == AsyncDataStatus.UserBufferFull)
                        || (status == AsyncDataStatus.DataFormatChange);

                    if (mustStartNewBuffer)
                    {
                        Data xd = StartNewAutoBuffer(_curDataInfo, 0, IntPtr.Zero); // no priming necessary
                        xd.Status |= DataStatus.Saving;
                        if (sampleCount > 0)
                            DataCb(status, xd);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // communicate error to application but do not propagate any exceptions from that callback call:
                    try
                    {
                        DataCb(AsyncDataStatus.Error, CurrentDataBuffer);
                    }
                    catch (Exception)
                    {
                        LastException = ex;
                    }
                }
            }
        }

        /// <summary>
        /// Used internally. called from CHRocodileLib, creates response and calls
        /// registered client's C# general response and update callback.
        /// Details can be found in the library documentation.
        /// </summary>
        /// <param name="_sInfo">The info struct</param>
        /// <param name="_hRsp">The response handle used to extract the actual response</param>
        private void CmdGenCbFct(TRspCallbackInfo _sInfo, Rsp_h _hRsp)
        {
            lock (this)
            {
                try
                {
                    /*
                     * Call user-defined general response callback, but not throw in case of set error flag.
                     * Since this is typically executing in a separate thread, throwing is not straight-forward.
                     */
                    bool isUpdate = _sInfo.Source != Handle;
                    _genCmdCb?.Invoke(new Response(_hRsp, _sInfo.Source, isUpdate, false));
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }
            }
        }

        #region Exec/Query methods


        #endregion

        #region Plugins handling

        /// <summary>
        /// Wraps CHRocodile plugin exec/query functions
        /// </summary>
        public class Plugin : AsynchronousCommandExecution
        {
            public AsynchronousConnection Parent { get; }

            public Plugin(AsynchronousConnection parent, string name, Conn_h pluginHandle)
                : base(parent.ConnectionInfo + $"/{name}", parent.DeviceType, 0)
            {
                base.Handle = pluginHandle;
                Parent = parent;
                _libSpecCmdCb = CmdSpecialCbFct;
            }

            private void CmdSpecialCbFct(TRspCallbackInfo _sInfo, Rsp_h _hRsp)
            {
                GCHandle handle = new GCHandle();
                try
                {
                    /*
                     * Call user-defined general response callback, but not throw in case of set error flag.
                     * Since this is typically executing in a separate thread, throwing is not straight-forward.
                     */
                    bool isUpdate = _sInfo.Source != Handle;
                    handle = GCHandle.FromIntPtr(_sInfo.User);
                    var cb = handle.Target as ResponseAndUpdateCallback;
                    var rsp = new Response(_hRsp, _sInfo.Source, isUpdate, false);
                    cb.Invoke(rsp);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }
                finally
                {
                 //   if (handle.IsAllocated)
                    //    handle.Free();
                }
            }
        }
        #endregion

        public Plugin InsertPlugin(string pluginName)
        {
            return new Plugin(this, pluginName, AddPlugin(pluginName));
        }

        public Plugin InsertPlugin(UInt32 pluginID)
        {
            return new Plugin(this, $"ID{pluginID}", AddPlugin(pluginID));
        }
    }


    public class DeviceSearch
    {
        public class Descr
        {
            private string _deviceSearchOutput;
            private Dictionary<string, string> _keyValues;
            public Descr(string deviceSearchOutput)
            {
                _deviceSearchOutput = deviceSearchOutput;
            }

            private void SearchOutputToList()
            {
                if (_keyValues != null)
                    return;

                _keyValues = new Dictionary<string, string>();
                foreach (string s in _deviceSearchOutput.Split(','))
                {
                    var t = s.Split(':');
                    _keyValues.Add(t[0].Trim(), t[1].Trim());
                }
            }

            public override string ToString()
            {
                return _deviceSearchOutput;
            }

            public string Value(string key)
            {
                SearchOutputToList();
                return _keyValues.TryGetValue(key, out string value) ? value : "";
            }

            public string IPAddr { get => Value("IP"); }
            public DeviceType DevType { get => (DeviceType)int.Parse(Value("Device Type")); }
            public string SerialNo { get => Value("SNR"); }

        }

        public static void Start(Search type = Search.OnlyTCPIPConnection)
        {
            Lib.Check(Lib.StartCHRDeviceAutoSearch(type, 0));
        }
        public static List<Descr> SearchAll(Search type = Search.OnlyTCPIPConnection)
        {
            Lib.Check(Lib.StartCHRDeviceAutoSearch(type, 1));
            return DiscoveredDevices();
        }


        public static void Cancel()
        {
            Lib.CancelCHRDeviceAutoSearch();
        }

        public static bool IsFinished()
        {
            return Lib.IsCHRDeviceAutoSearchFinished() == 1;
        }

        private static List<Descr> ParseDeviceSearchResultString(string sr)
        {
            List<Descr> res = new List<Descr>();

            foreach (string s in sr.Split(';'))
            {
                var v = s.Trim();
                if (v.Length > 0)
                    res.Add(new Descr(v));
            }

            return res;
        }

        public static List<Descr> DiscoveredDevices()
        {
            StringBuilder sb = new StringBuilder();
            Int64 size = 0;
            Lib.DetectedCHRDeviceInfo(null, ref size);
            if (size == 0)
                throw new Error("DeviceSearch: cannot retrieve discovered devices.");
            sb.Length = (int)size;
            Lib.Check(Lib.DetectedCHRDeviceInfo(sb, ref size));
            return ParseDeviceSearchResultString(sb.ToString());
        }
    }
}
