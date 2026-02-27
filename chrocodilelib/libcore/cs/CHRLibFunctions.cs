/*
TCHRLibFunctionWrapper directly translates basic CHRocodileLib API functions into c# functions. 
The detail description of each function please refer to the ducumentation
*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CHRocodileLib
{
    /// <summary>
    /// CHRocodile command IDs
    /// ======================
    /// 
    /// The list below is not complete and will probably never be. It is a set of "well known" commands shared
    /// among most CHRocodile sensors. Gnerally, commands can also be given as strings. Example:
    /// 
    ///     ExecCmd()
    /// 
    /// Command arguments
    /// =================
    /// 
    /// Arguments of each command are listed in the "<b>Args:</b>" section. Format:
    /// (type) argument that must be added in case of "set" commands ("ExecCmd(...)") - these are the values to actually set
    /// or
    /// (type/q) that must be given in BOTH set and query commands (to further specify the query)
    /// 
    /// Example: EncoderCounter
    /// -----------------------
    /// 
    /// <b>Args:</b>
    /// (int/q) index of counter to set or query
    /// (int) new value
    /// 
    /// Query current value of encoder counter 3:
    ///     ExecQuery(CmdID.EncoderCounter, 3) // encoder counter index must always be given
    /// Set encoder counter 3 to value 0:
    ///     ExecCmd(EncoderCounter, 3, 0) // the new value only in case of the set command
    /// 
    /// Command responses
    /// =================
    /// 
    /// Unless otherwise specified (with a "Response:" section), both set and query responses have identical
    /// argument arrangements as set commands.
    /// </summary>
    public enum CmdID : UInt32
    {
        /// <summary>
        /// Sets or queries active output signals (SODX).<br/>
        /// <b>Args:</b><br/>
        /// (int) sigID1, sigID2, ..., sigIDn
        /// </summary>
        OutputSignals = 0x58444f53,

        /// <summary>
        /// Queries firmware version (VER).<br/>
        /// <b>Args:</b> none<br/>
        /// Response:<br/>
        /// (string) firmware version information
        /// </summary>
        FirmwareVersion = 0x00524556,

        /// <summary>
        /// Sets / Queries mode: Confocal or interferometric (MMD).<br/>
        /// <b>Args:</b><br/>
        /// (int) mode (0: confocal, 1: interferometric)<br/>
        /// </summary>
        MeasuringMethod = 0x00444d4d,

        /// <summary>
        /// Queries the full scale's equivalent in micrometers, i. e. the measuring range (SCA).<br/>
        /// <b>Args:</b> none<br/>
        /// Response:<br/>
        /// (int) number of microns corresponding to the full scale value (32767)<br/>
        /// </summary>
        FullScale = 0x00414353,

        /// <summary>
        /// Sets / Queries free run scan rate in samples per seconds (SHZ).<br/>
        /// <b>Args:</b><br/>
        /// (float) scan rate (in Hz)<br/>
        /// </summary>
        ScanRate = 0x005a4853,

        /// <summary>
        /// Sets / Queries data averaging factor (AVD).<br/>
        /// <b>Args:</b><br/>
        /// (int) averaging factor (1..999)<br/>
        /// </summary>
        DataAverage = 0x00445641,

        /// <summary>
        /// Sets / Queries spectra averaging factor (AVS).<br/>
        /// <b>Args:</b><br/>
        /// (int) averaging factor (1..999)<br/>
        /// </summary>
        SpectrumAverage = 0x00535641,

        /// <summary>
        /// Sets / Queries serial data averaging factor (applies to data on RS422 port only) (AVDS).<br/>
        /// <b>Args:</b><br/>
        /// (int) averaging factor (1..999)<br/>
        /// </summary>
        SerialDataAverage = 0x53445641,

        /// <summary>
        /// Sets / Queries index (1.0 .. 4.0) of refraction of layers 1..n; n depends on device (SRI).<br/>
        /// <b>Args:</b><br/>
        /// (float) refractive index layer 1, ..., refractive index layer n<br/>
        /// </summary>
        RefractiveIndices = 0x00495253,

        /// <summary>
        /// Sets / Queries abbe numbers (0.0 .. 500.0) of layers 1..n; n depends on device, 0.0 = constant refractive index (ABE).<br/>
        /// <b>Args:</b><br/>
        /// (float) abbe number layer 1, ... refractive index layer n<br/>
        /// </summary>
        AbbeNumbers = 0x00454241,

        /// <summary>
        /// Sets / Queries refractive index table of layers 1..n; n = 0..15 (SRT)<br/>
        /// <b>Args:</b><br/>
        /// (int) index of table for layer 1, ... index of table for layer n<br/>
        /// </summary>
        RefractiveIndexTables = 0x00545253,

        /// <summary>
        /// Sets / Queries lamp intensity (deviations of response can be due to rounding to nearest vaid value) (LAI).<br/>
        /// <b>Args:</b><br/>
        /// (float) intensity in percent (0.0 .. 100.0)<br/>
        /// </summary>
        LampIntensity = 0x0049414c,

        /// <summary>
        /// Sets / Queries active calibration table index (SEN).<br/>
        /// <b>Args:</b><br/>
        /// (int) calibration table index (0..15)<br/>
        /// </summary>
        OpticalProbe = 0x004e4553,

        /// <summary>
        /// Sets / Queries detection threshold in confocal mode (THR).<br/>
        /// <b>Args:</b><br/>
        /// (float) detection threshold (0..1000)<br/>
        /// </summary>
        ConfocalDetectionThreshold = 0x00524854,

        /// <summary>
        /// Sets / Queries the peak separation minimum (PSM).<br/>
        /// <b>Args:</b><br/>
        /// (float) peak separation minimum in percent (10..90)<br/>
        /// </summary>
        PeakSeparationMin = 0x004D4350,

        /// <summary>
        /// Sets / Queries interferometric minimum quality for interferometric peak qualification (QTH).<br/>
        /// <b>Args:</b><br/>
        /// (float) quality threshold<br/>
        /// </summary>
        InterferometricQualityThreshold = 0x00485451,

        /// <summary>
        /// Sets / Queries double exposure duty cycle (DCY).<br/>
        /// <b>Args:</b><br/>
        /// (float) fraction of shorter interval in percent (1..49, 100 - to turn off)<br/>
        /// </summary>
        DutyCycle = 0x00594344,

        /// <summary>
        /// Sets / Queries detection windows status (LMA).<br/>
        /// <b>Args:</b><br/>
        /// (int) 0 = off, 1 = on<br/>
        /// </summary>
        DetectionWindowActive = 0x00414d4c,

        /// <summary>
        /// Sets / Queries detection windows (DWD).<br/>
        /// <b>Args:</b><br/>
        /// (float...) up to 16 window definitions win1-L, win1-R, win2-L, win2-R, ...<br/>
        /// </summary>
        DetectionWindow = 0x00445744,

        /// <summary>
        /// Sets / Queries number of peaks to be taken into account (NOP).<br/>
        /// <b>Args:</b><br/>
        /// (int) number of peaks (1 .. n where n depends on device)<br/>
        /// </summary>
        NumberOfPeaks = 0x00504f4e,

        /// <summary>
        /// Sets / Queries the order in which interferometric peak data appears in the data stream (POD).<br/>
        /// <b>Args:</b><br/>
        /// (int) oder 0 = quality, descending, 1 = position, ascending, 2 = position, descending<br/>
        /// </summary>
        PeakOrdering = 0x00444f50,

        /// <summary>
        /// Perform dark reference (DRK).<br/>
        /// <b>Args:</b> none<br/>
        /// Response: (float) virtual stray light saturating frequency<br/>
        /// </summary>
        DarkReference = 0x004b5244,

        /// <summary>
        /// Sets / Queries continuous dark referencing (CRDK)<br/>
        /// <b>Args:</b><br/>
        /// (float) refresh factor<br/>
        /// </summary>
        ContinuousDarkReference = 0x4b445243,

        /// <summary>
        /// Starts data stream - independent of sensor activity / triggering (STA).<br/>
        /// <b>Args:</b> none<br/>
        /// </summary>
        StartDataStream = 0x00415453,

        /// <summary>
        /// Stops data stream - independent of sensor activity / triggering (STO).<br/>
        /// <b>Args:</b> none<br/>
        /// </summary>
        StopDataStream = 0x004f5453,

        /// <summary>
        /// Sets / Queries light source auto-adapt function (AAL).<br/>
        /// <b>Args:</b><br/>
        /// (int) 0/1 = on/off, (float) target detector level in percent, 33 recommended<br/>
        /// </summary>
        LightSourceAutoAdapt = 0x004c4141,

        /// <summary>
        /// Sets / Queries active detector range (CRA).<br/>
        /// <b>Args:</b><br/>
        /// (int) start pixel (0..2048), (int) stop pixel (0..2048)<br/>
        /// </summary>
        CCDRange = 0x00415243,

        /// <summary>
        /// Sets / Queries median width (MED).<br/>
        /// <b>Args:</b><br/>
        /// (int) width (number of samples to build the median for) (0..31)<br/>
        /// </summary>
        Median = 0x5844454d,

        /// <summary>
        /// Sets / Queries analog output configuration (ANAX).<br/>
        /// <b>Args:</b><br/>
        /// (int/q) target output port (0..1),<br/>
        /// (int) ID of signal to map to analog output,<br/>
        /// (float) min. signal value to map,<br/>
        /// (float) max. signal value to map,<br/>
        /// (float) voltage corresponsing to min. signal value,<br/>
        /// (float) voltage corresponsing to max. signal value,<br/>
        /// (float) voltage corresponsing to invalid signal value,<br/>
        /// (float) hold time in microseconds<br/>
        /// </summary>
        AnalogOutput = 0x58414e41,

        /// <summary>
        /// Sets / Queries encoder counter.<br/>
        /// <b>Args:</b><br/>
        /// (int/q) index of counter to set or query,<br/>
        /// (int) new value,<br/>
        /// Response: (int) index of counter (int) current encoder counter value<br/>
        /// </summary>
        EncoderCounter = 0x53504525,

        /// <summary>
        /// Sets / Queries encoder counter source<br/>
        /// <b>Args:</b><br/>
        /// (int/q) index of counter, if set: (EncoderSrc) index of source<br/>
        /// </summary>
        EncoderCounterSource = 0x53434525,

        /// <summary>
        /// Sets / Queries encoder preload fúnction.<br/>
        /// <b>Args:</b><br/>
        /// (int/q): axis index,<br/>
        /// (int): preload value,<br/>
        /// (EncoderPreloadConfig): preload config,<br/>
        /// (int): trigger source<br/>
        /// </summary>
        EncoderPreloadFunction = 0x46504525,

        /// <summary>
        /// Sets / Queries encoder trigger status<br/>
        /// <b>Args:</b><br/>
        /// (int) 0 = triggered by sync-in 1 = triggered by encoder counter<br/>
        /// </summary>
        EncoderTriggerEnabled = 0x45544525,

        /// <summary>
        /// Sets / Queries encoder trigger settings.<br/>
        /// <b>Args:</b><br/>
        /// (int/q) encoder counter index,<br/>
        /// (int) start position,<br/>
        /// (int) stop position,<br/>
        /// (float) distance between adjacent triggering points in increments,<br/>
        /// (int) trigger on return move 0 = off, 1 = on<br/>
        /// </summary>
        EncoderTriggerProperty = 0x50544525,

        /// <summary>
        /// Sets / Queries current device trigger mode.<br/>
        /// <b>Args:</b><br/>
        /// (Triggermode) trigger mode<br/>
        /// </summary>
        DeviceTriggerMode = 0x4d525425,

        /// <summary>
        /// Download spectrum (DNLD).<br/>
        /// <b>Args:</b><br/>
        /// (SpecType) spectrum type<br/>
        /// Response:<br/>
        /// (int) Spectrum Type,<br/>
        /// (int) Start Channel Index,<br/>
        /// (int) Channel Count,<br/>
        /// (int) Exposure Number,<br/>
        /// (int) Micrometers per bin (interferometric mode only),<br/>
        /// (int) Exponent<br/>
        /// (byte[]) Blob containing the data - use BitConverter.ToInt16<br/>
        /// </summary>
        DownloadSpectrum = 0x444c4e44,

        /// <summary>
        /// Save device settings permanently on device (SSU).<br/>
        /// <b>Args:</b> none<br/>
        /// </summary>
        SaveSettings = 0x00555353,

        /// <summary>
        /// Sets / Queries table (TABL).<br/>
        /// <b>Args:</b><br/>
        /// (TableType/q) table type,<br/>
        /// (int/q) table index,<br/>
        /// (int/q) byte offset of table chunk,<br/>
        /// (int/q) total size of table,<br/>
        /// (byte[]) blob containing table chunk up- or downloaded<br/>
        /// </summary>
        DownloadUploadTable = 0x4C424154
    };

    public enum DataType : Int16
    {
        UChar = 0,
        Char = 1,
        UShort = 2,
        Short = 3,
        UInt32 = 4,
        Int32 = 5,
        Float = 6,
        Double = 255
    };

    public enum ParamType : Int32
    {
        Int = 0,
        Float = 1,
        String = 2,
        Bytes = 4,
        Ints = 254,
        Floats = 255
    };

    /// <summary>
    /// Response status flags
    /// </summary>
    public enum RspFlag : UInt32
    {
        Query = 0x0001,
        Error = 0x8000,
        Warning = 0x4000,
        Update = 0x2000
    };

    public enum DeviceType : Int32
    {
        Unspecified = -1,
        Chr1 = 0,
        Chr2 = 1,
        MultiChannel = 2,
        ChrCMini = 3
    };

    public enum ConnectionMode: Int32
    {
        Synchronous = 0,
        Asynchronous = 1
    };

    /// <summary>
    /// Live spectrum type used with DownloadSpectrum
    /// </summary>
    public enum SpecType : Int32
    {
        Raw = 0,
        Confocal = 1,
        FT = 2,
        Image2d = 3
    }

    public enum AutoBufferStatus: Int32
    {
        Error = -1,
        Saving = 0,
        Finished = 1,
        Response = 2,
        Deactivated = 3,
        UnInit = 4
    }



    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TErrorInfo
    {
        public Conn_h ConHandle;
        public Int32 ErrorCode;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TRspCallbackInfo
    {
        public IntPtr User;
        public IntPtr State;
        public Int32 Ticket;
        public UInt32 Source;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ResponseInfo
    {
        public CmdID CmdID;
        public Int32 Ticket;
        public RspFlag Flags;
        public UInt32 ParamCount;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct SignalInfo
    {
        public UInt16 SignalID;
        public DataType DataType;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SignalGeneralInfo
    {
        public UInt32 InfoIndex;
        public Int32 PeakSignalCount;
        public Int32 GlobalSignalCount;
        public Int32 ChannelCount;
    }

    public enum AsyncDataStatus : Int32
    {
        Error = -1,
        TimeOut = 0,
        Success = 1,
        ResponseReceived = 2,
        BufferLimitHit = 3,
        UserBufferFull = 4,
        DataFormatChange = 5
    }

    public enum ConfigFlags: UInt32
    {
        None = 0,
        DeactivateAutoBufferOnResponse = 1,
        AutoChangeDataBufferSize = 2
    }

    public enum EncoderSrc : Int32
    {
        A0 = 0,
        B0 = 1,
        A1 = 2,
        B1 = 3,
        A2 = 4,
        B2 = 5,
        A3 = 6,
        B3 = 7,
        A4 = 8,
        B4 = 9,
        SyncIn = 10,
        Quadrature = 15,
        Immediate = 15
    }

    public enum TriggerMode : UInt32
    {
        FreeRun = 0,
        WaitTrigger = 1,
        TriggerEach = 2,
        TriggerWindow = 3
    }

    public enum OutputDataFormat : Int32
    {
        Double = 0,
        Raw = 1
    }

    public enum MeasurementMode : Int32
    {
        Confocal = 0,
        Interferometric = 1
    }

    public enum EncoderPreloadConfig : Int32 // TODO
    {
        Once = 0,
        Eachtime = 1,
        TriggerRisingEdge = 0,
        TriggerFallingEdge = 2,
        TriggerOnEdge = 0,
        TriggerOnLevel = 4,
        Active = 8
    }

    public enum TableType : Int32
    {
        Confocal_Calibration = 1,
        WaveLength = 2,
        RefractiveIndex = 3,
        DarkCorrection = 4,
        ConfocalCalibrationMultiChannel = 5,
        CLSMask = 6,
        MPSMask = 8,
        CLS2ConfocalCalibration = 18
    }

    public enum Search : Int32
    {
        BothSerialTCPIPConnection = 0,
        OnlySerialConnection = 1,
        OnlyTCPIPConnection = 2
    }

#region Type Definition
//Lib interface function return type
public struct Res_t
    {
        private Int32 nRes;
        public static implicit operator Res_t(Int32 i)
        {
            return new Res_t { nRes = i };
        }
        public static implicit operator Int32(Res_t Res)
        {
            return Res.nRes;
        }
        public static readonly Int32 Ok = 0;
    }

    //Connection handle type
    public struct Conn_h
    {
        private UInt32 nHandle;
        public const UInt32 InvalidHandle = 0xFFFFFFFF;
        public static implicit operator Conn_h(UInt32 i)
        {
            return new Conn_h { nHandle = i };
        }
        public static implicit operator UInt32(Conn_h h)
        {
            return h.nHandle;
        }
    }


    //Command handle type
    public struct Cmd_h
    {
        private UInt32 nHandle;
        public const UInt32 InvalidHandle = 0xFFFFFFFF;
        public static implicit operator Cmd_h(UInt32 i)
        {
            return new Cmd_h { nHandle = i };
        }
        public static implicit operator UInt32(Cmd_h h)
        {
            return h.nHandle;
        }
    }

    //Response handle type
    public struct Rsp_h
    {
        private UInt32 nHandle;
        public static implicit operator Rsp_h(UInt32 i)
        {
            return new Rsp_h { nHandle = i };
        }
        public static implicit operator UInt32(Rsp_h h)
        {
            return h.nHandle;
        }
    }

    #endregion


    public class Error : Exception
    {
        public Error()
        { }

        public Error(string message)
            : base(message)
        {  }
    }

    /// <summary>
    /// Low level CHRocodileLib definitions and wrapper functions.
    /// You typically do not call the functions directly.
    /// </summary>
    public static class Lib
    {
        public static string ErrorString(Res_t res)
        {
            var str = "error - no details available.";
            Int64 size = 0;
            Lib.ErrorCodeToString(res, null, ref size);
            if (size == 0)
                return str;

            var builder = new StringBuilder((int)size);
            if (Lib.ResultSuccess(Lib.ErrorCodeToString(res, builder, ref size)))
                str = $"error: {builder.ToString()} ({ res})";
            return str;
        }

        public static void Check(Res_t res, string msg = "")
        {
            if (res >= 0)
                return;

            if (msg != "")
                msg += " - ";

            throw new Error(msg + ErrorString(res));
        }

        public static bool ConnectionIsAsynchronous(Conn_h h)
        {
            return ((UInt32)h & (1 << 28)) != 0; // TODO: replace by lib API function
        }

            public static string CmdIDToStr(CmdID cmdID)
        {
            byte[] aCmdBytes = BitConverter.GetBytes((UInt32)cmdID);
            if (aCmdBytes[3] == 0)
                return Encoding.ASCII.GetString(aCmdBytes, 0, 3);
            else
                return Encoding.ASCII.GetString(aCmdBytes);
        }

        public static CmdID StrToCmdID(string cmd)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(cmd);
            uint v = 0;
            int s = 0;
            foreach(byte b in bytes)
            {
                v |= (uint)b << s;
                s += 8;
            }
            return (CmdID)v;
        }

        private const string DLL_Filename = "CHRocodile.dll";



        #region Constant Definition

        public const Int32 Handle_Unknown_Type = -1;
        public const Int32 Handle_Connection = 0;
        public const Int32 Handle_Command = 1;
        public const Int32 Handle_Response = 2;

        public const UInt32 OPID_Save_Performance = 0x5356504D;

        #endregion


        #region Delegate Definition
        /// <summary>
        /// Raw wrapper of CHRocoLib's data callback function. Do not use. Use connection class instead.
        /// For parameters see CHRocoLib manual
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SampleDataCallback(IntPtr pUser, AsyncDataStatus state, Int64 sampleCount,
            IntPtr pSampleBuffer, Int64 sizePerSample, SignalGeneralInfo signalGeneralInfo, [In] IntPtr psSignalInfo);
        /// <summary>
        /// Raw wrapper of CHRocoLib's response and update callback function. Do not use. Use connection class instead.
        /// For parameters see CHRocoLib manual
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ResponseAndUpdateCallback(TRspCallbackInfo _sInfo, Rsp_h _hRsp);
        #endregion

        #region Handle Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetHandleType(UInt32 _Handle);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyHandle(UInt32 _Handle);
        #endregion

        #region Result Checking Functions
        public static bool ResultSuccess(Res_t _nRes)
        {
            return (_nRes >= 0);
        }
        public static bool ResultInformation(Res_t _nRes)
        {
            return ((_nRes >> 30) == 1);
        }
        public static bool ResultWarning(Res_t _nRes)
        {
            return ((_nRes >> 30) == 2);
        }
        public static bool ResultError(Res_t _nRes)
        {
            return ((_nRes >> 30) == 3);
        }
        #endregion

        #region Error Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t LastErrors(Conn_h _hConnection, [In, Out] TErrorInfo[] _aErrorInfos, ref Int64 _pnBufSizeInBytes);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ClearErrors(Conn_h _hConnection);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ErrorCodeToString(Int32 _nErrorCode, [MarshalAs(UnmanagedType.LPStr)] StringBuilder _strErrorString, ref Int64 _pnSize);
        #endregion

        #region Open/Close Connection Functions
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t OpenConnection([MarshalAs(UnmanagedType.LPStr)] string _strConnectionInfo, Int32 _nDeviceType, Int32 _eConnectionMode, Int64 _nDevBufSize, out Conn_h _pHandle);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t OpenSharedConnection(Conn_h _nExistingConnection, Int32 _eConnectionMode, out Conn_h _pHandle);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t CloseConnection(Conn_h _hConnection);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetDeviceConnectionInfo(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPStr)] StringBuilder _strConnectInfo, ref Int64 _pnSize);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetConnectionMode(Conn_h _hConnection);
        #endregion

        #region Device Info. Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetDeviceType(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetDeviceChannelCount(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetDeviceOutputSignals(Conn_h _hConnection, Int32[] _pSignalIDbuffer, out Int32 _pSignalCount);
        #endregion

        #region Sample Data Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetNextSamples(Conn_h _hConnection, ref Int64 _pSampleCount, out IntPtr _ppData, out Int64 _pnSizePerSample,
            out SignalGeneralInfo _pSignalGeneralInfo, out IntPtr _psSignalInfo);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetLastSample(Conn_h _hConnection, out IntPtr _ppData, out Int64 _pnSizePerSample,
            out SignalGeneralInfo _pSignalGeneralInfo, out IntPtr _psSignalInfo);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t RegisterSampleDataCallback(Conn_h _hConnection, Int64 _nReadSampleCount, Int32 _nReadSampleTimeOut, IntPtr _pUser, SampleDataCallback _pOnReadSample);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetCurrentAvailableSampleCount(Conn_h _hConnection, out Int64 _pnSampleCount);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetSingleOutputSampleSize(Conn_h _hConnection, out Int64 _pnSampleSize);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetConnectionOutputSignalInfos(
            Conn_h _hConnection, out SignalGeneralInfo _pSignalGeneralInfo, [In, Out] SignalInfo[] _pSignalInfoBuf, ref Int64 _pnSignalInfoBufSizeInBytes);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t StopConnectionDataStream(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t StartConnectionDataStream(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t SetOutputDataFormatMode(Conn_h _hConnection, OutputDataFormat _eMode);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern OutputDataFormat GetOutputDataFormatMode(Conn_h _hConnection);
        #endregion

        #region Auto Buffer Mode Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ActivateAutoBufferMode(Conn_h _hConnection, IntPtr _pBuffer, Int64 _nSampleCount, ref Int64 _pBufferSize);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetAutoBufferSavedSampleCount(Conn_h _hConnection, out Int64 _pnSampleCount);

        public static Int64 GetSavedSampleCount(Conn_h _hConnection)
        {
            Check(
                ConnectionIsAsynchronous(_hConnection) ?
                    GetAsyncDataUserBufferSavedSampleCount(_hConnection, out Int64 count)
                    : GetAutoBufferSavedSampleCount(_hConnection, out count));
            return count;
        }

        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t DeactivateAutoBufferMode(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern AutoBufferStatus GetAutoBufferStatus(Conn_h _hConnection);

        public static AutoBufferStatus GetAutoBufferStatusWithCheck(Conn_h _hConnection)
        {
            var res = Lib.GetAutoBufferStatus(_hConnection);
            var r = (int)res;
            if (r < 0)
                Check((Res_t)r);
            return (res);
        }

        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ActivateAsyncDataUserBuffer(Conn_h _hConnection, IntPtr _pBuffer, Int64 _nSampleCount, ref Int64 _pBufferSize);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetAsyncDataUserBufferSavedSampleCount(Conn_h _hConnection, out Int64 _pnSampleCount);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t DeactivateAsyncDataUserBuffer(Conn_h _hConnection);
        #endregion

        #region Command Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ExecCommand(UInt32 _hDest, Cmd_h _hCmd, out Rsp_h _hRsp);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ExecCommandAsync(UInt32 _hDest, Cmd_h _hCmd, IntPtr _pUser, ResponseAndUpdateCallback _pCB, out Int32 _nTicket);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t RegisterGeneralResponseAndUpdateCallback(Conn_h _hConnection, IntPtr _pUser, ResponseAndUpdateCallback _pGenCBFct);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t NewCommand(CmdID _nCmdID, Int32 _bQuery, out Cmd_h _hCmd);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 CmdNameToID([MarshalAs(UnmanagedType.LPStr)] string _strCmdName, Int32 _nLength);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandIntArg(Cmd_h _hCmd, Int32 _nArg);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandFloatArg(Cmd_h _hCmd, float _nArg);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandIntArrayArg(Cmd_h _hCmd, Int32[] _pArg, Int32 _nLength);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandFloatArrayArg(Cmd_h _hCmd, float[] _pArg, Int32 _nLength);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandStringArg(Cmd_h _hCmd, [MarshalAs(UnmanagedType.LPStr)] string _pArg, Int32 _nLength);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddCommandBlobArg(Cmd_h _hCmd, [MarshalAs(UnmanagedType.LPArray)]  byte[] _pArg, Int32 _nLength); // TODO: check!
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t NewCommandFromString([MarshalAs(UnmanagedType.LPStr)] string _strCommand, out Cmd_h _hCmd);
        #endregion

        #region Response Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseInfo(Rsp_h _hRsp, out ResponseInfo sRspInfo);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseArgType(Rsp_h _hRsp, UInt32 _nIndex, out ParamType _pArgType);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseIntArg(Rsp_h _hRsp, UInt32 _nIndex, out Int32 _pArg);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseFloatArg(Rsp_h _hRsp, UInt32 _nIndex, out float _pArg);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseIntArrayArg(Rsp_h _hRsp, UInt32 _nIndex, out IntPtr _pArg, out Int32 _pLength);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseFloatArrayArg(Rsp_h _hRsp, UInt32 _nIndex, out IntPtr _pArg, out Int32 _pLength);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseStringArg(Rsp_h _hRsp, UInt32 _nIndex, out IntPtr _pArg, out Int32 _pLength);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t GetResponseBlobArg(Rsp_h _hRsp, UInt32 _nIndex, out IntPtr _pArg, out Int32 _pLength);
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ResponseToString(Rsp_h _hRsp, [MarshalAs(UnmanagedType.LPStr)] StringBuilder _aResponseStr, ref Int64 _pnSize);
        #endregion

        #region Connection Output Processing Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t ProcessDeviceOutput(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t StartAutomaticDeviceOutputProcessing(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t StopAutomaticDeviceOutputProcessing(Conn_h _hConnection);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t FlushConnectionBuffer(Conn_h _hConnection);
        #endregion

        #region Lib Configuration Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t SetLibLogFileDirectory([MarshalAs(UnmanagedType.LPTStr)] string _pstrDirectory, Int64 _nMaxFileSize, Int32 _nMaxFileNumber);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t SetLibLogLevel(Int32 _nLevel);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLibConfigFlags(ConfigFlags flags);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t SetSampleDataBufferSize(Conn_h _hConnection, Int64 _nBufferSize);
        #endregion

        #region Device Auto. Search Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t StartCHRDeviceAutoSearch(Search _eType, Int32 _bSBlockingSearch);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CancelCHRDeviceAutoSearch();
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        
        ///<summary>
        /// returns 1 if device search is finshed
        ///</summary>
        public static extern Res_t IsCHRDeviceAutoSearchFinished();
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t DetectedCHRDeviceInfo([MarshalAs(UnmanagedType.LPStr)] StringBuilder _strDeviceInfos, ref Int64 _pnSize);
        #endregion

        #region Upload Table Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t UploadConfocalCalibrationTableFromFile(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPTStr)] string _strFullFileName, UInt32 _nProbeSerialNumber, UInt32 _nCHRTableIndex);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t UploadWaveLengthTableFromFile(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPTStr)] string _strFullFileName);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t UploadRefractiveIndexTableFromFile(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPTStr)] string _strFullFileName, [MarshalAs(UnmanagedType.LPStr)] string _strTableName, UInt32 _nCHRTableIndex, float _nRefSRI);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t UploadMultiChannelMaskTable(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPTStr)] string _strFullFileName);
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t UploadFirmware(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPTStr)] string _strFullFileName);
        #endregion

        #region Extra Data Processing Functions
        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t SetMultiChannelProfileInterpolation(Conn_h _hConnection, Int32 _nMaxHoleSize);
        #endregion

        #region Plugin
        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddConnectionPlugIn(Conn_h _hConnection, [MarshalAs(UnmanagedType.LPStr)] string _strPluginName, out Conn_h _hPlugin);

        [DllImport(DLL_Filename, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t AddConnectionPlugInByID(Conn_h _hConnection, UInt32 _nTypeID, out Conn_h _hPlugin);

        #endregion

        #region Perform Lib Operation
        public const Int32 Performance_Snapshot_Mode = 0;
        public const Int32 Performance_Complete_Mode = 1;

        [DllImport(DLL_Filename, CallingConvention = CallingConvention.Cdecl)]
        public static extern Res_t PerformLibOperation(Conn_h _hConnection, Cmd_h _hCmd, out Rsp_h _phRsp);
        #endregion

    }

}

