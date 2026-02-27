/**
 *  \file
 *
 *  \copyright    Copyright (C) @CHRLIB_GIT_TIME_YEAR@ by Precitec Optronik GmbH
 *  \brief Error definitions for the CHRocodileLib
 *
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CHRocodileLib;

public static class CHRLibPlugin
{
    #region CLS 2 Calibration Plugin
    public const string CLS2_Calib_Plugin_Name = "CLS2CalibrationPlugin";

    public const UInt32 CmdID_CalibFile_Name = 0x464C4143;
    public const UInt32 CmdID_CalibFile_List = 0x4C464143;
    public const UInt32 CmdID_Calib_Active = 0x414C4143;
    public const UInt32 CmdID_Calib_Mode = 0x444D4143;
    public const UInt32 CmdID_Calib_Prepare_Status = 0x53504143;
    public const UInt32 CmdID_Calib_Resample_Count = 0x43524143;

    //user try to upload more than 1 calibration file
    public const Int32 ERR_CALIB_MULTIPLE_FILE = unchecked((Int32)0xE4200000L);
    public const Int32 ERR_CALIB_INVALID_FILE = unchecked((Int32)0xE4200001L);

    public enum Calib_Prepare_StatusType
    {
        Idle = 0,
        Download,
        SEN
    }

    #endregion // CLS 2 Calibration Plugin

    #region Flying Spot Plugin

    public const string FSS_Plugin_Name = "FlyingSpotPlugin";

    public const UInt32 CmdID_Flying_Spot_Cfg = 0x474643;       ///< sets up FSS plugin using a configuration file
    public const UInt32 CmdID_Flying_Spot_Compile = 0x474f5250; ///< compiles a recipe script and returns the script's handle
    public const UInt32 CmdID_Flying_Spot_Exec = 0x43455845;    ///< executes previously compiled recipe
    public const UInt32 CmdID_Flying_Spot_Stop = 0x504f5453;    ///< terminates a recipe execution
    public const UInt32 CmdID_Flying_Spot_Blob = 0x424f4c42;     ///< retrieves a binary code of a previosly compiled script
    public const UInt32 CmdID_Flying_Spot_SDCard = 0x44524143;   ///< uploads a binary code of a previosly compiled script to the SD card

    public const UInt32 FSS_PROG_InputString = 0;
    public const UInt32 FSS_PROG_InputFile = 1;

    public enum FSS_PluginDataType
    {
        RawData = 0,
        Interpolated2D,
        RecipeTerminate
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct TFSS_PluginShapeData
    {
        public IntPtr label;
        public IntPtr signalInfo;
        public IntPtr metaBuf;

        public FSS_PluginDataType type;
        public UInt32 shapeIndex;
        public UInt32 numSignals;
        public UInt32 numSamples;

        public double x0;
        public double y0;
        public double x1;
        public double y1;

        public UInt32 imageW;
        public UInt32 imageH;
    }

#nullable enable
    /// <summary>
    /// Structure describing a scanned geometric shape 
    /// </summary>
    public class FSS_PluginShape
    {
        private TFSS_PluginShapeData _data;

        private IntPtr[] _rawBufs = new IntPtr[0];

        /// <summary>
        /// null-terminated string holding a user-defined label of this shape
        /// </summary>
        public string Label { get; private set; } = "";

        /// <summary>
        /// an array of Lib.SignalInfo structs describing the output data signals 
        /// </summary>
        public SignalInfo[] SignalInfos { get; private set; } = new SignalInfo[0];

        /// <summary>
        /// this shape type
        /// </summary>
        public FSS_PluginDataType Type { get => _data.type; }

        /// <summary>
        /// index defining how many times this shape appeared in the data stream: for instance,
        /// if a program contains a while/for loop, same shapes might appear several times
        /// </summary>
        public UInt32 ShapeIndex { get => _data.shapeIndex; }

        /// <summary>
        /// Gets the count of samples.
        /// </summary>
        public UInt32 NumSamples { get => _data.numSamples; }

        /// <summary>
        /// returns True if this shape is "detached" from the internal FSS ring-buffer 
        /// by calling Detach() function
        /// </summary>
        public bool IsDetached { get; private set; } = false;

        /// <summary>
        /// bitmap type provided to access 2D interpolated data
        /// </summary>
        public partial class BitmapType
        {
            private FSS_PluginShape _shape;

            public BitmapType(FSS_PluginShape shape)
            {
                if (shape.Type != FSS_PluginDataType.Interpolated2D)
                    throw new Error("Bitmap can only be created for 2D interpolated shapes!");
                _shape = shape;
            }
        }

        /// <summary>
        /// 2D interpolated bitmap associated with this shape 
        /// Only available for shape type = <see cref="FSS_PluginDataType.FSS_PluginInterpolated2D"/>.
        /// </summary>
        public BitmapType? Bitmap { get; private set; } = null;
       
        /// <summary>
        /// constructs object from the binary FSS Response 
        /// </summary>
        /// <param name="blob"></param>
        public FSS_PluginShape(byte[] blob)
        {
            if (Marshal.SizeOf<TFSS_PluginShapeData>() != blob.Length)
                throw new Error("Binary data cannot be mapped to PluginShapeData structure!");

            var handle = GCHandle.Alloc(blob, GCHandleType.Pinned);
            try
            {
                _data = Marshal.PtrToStructure<TFSS_PluginShapeData>(
                            handle.AddrOfPinnedObject());

                if (_data.type == FSS_PluginDataType.RecipeTerminate)
                    return;

                // Get label from shape data
                Label = Marshal.PtrToStringAnsi(_data.label) ?? "NULL";

                _rawBufs = new IntPtr[_data.numSignals];
                // get individual signal buffer pointers
                Marshal.Copy(_data.metaBuf, _rawBufs, 0, _rawBufs.Length);

                // Once per scan store the signal ids
                SignalInfos = new SignalInfo[_data.numSignals];
                var size = Marshal.SizeOf<SignalInfo>();

                for (int i = 0; i < SignalInfos.Length; i++)
                {
                    var ptr = IntPtr.Add(_data.signalInfo, i * size);
                    SignalInfos[i] = Marshal.PtrToStructure<SignalInfo>(ptr);
                }

                if(_data.type == FSS_PluginDataType.Interpolated2D)
                    Bitmap = new BitmapType(this);
            }
            finally
            {
                handle.Free();
            }
        }

        ~FSS_PluginShape()
        {
            if(IsDetached) 
            foreach(var H in _rawBufs)
            {
                Marshal.FreeHGlobal(H);
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, UIntPtr count);

        /// <summary>
        /// "detaches" this shape from the internal FSS ring buffer by copying all signal data into the local memory
        /// </summary>
        public void Detach()
        {
            if (IsDetached)
                return;

            for(int i = 0; i < SignalInfos.Length; i++)
            {
                var len = NumSamples * DataInfo.SignalSize(SignalInfos[i].DataType);
                var H = Marshal.AllocHGlobal((int)len);
                CopyMemory(H, _rawBufs[i], (UIntPtr)len);
                _rawBufs[i] = H;
            }
            IsDetached = true;
        }

        /// <summary>
        /// returns a signal index from signal ID for fast iterator access
        /// </summary>
        /// <param name="signalID">Signal ID to be iterated on</param>
        /// <returns>this signal index</returns>
        public Int32 SignalIndex(Int32 signalID)
        {
            int i = 0;
            foreach(var s in SignalInfos) { 
                if ((Int32)s.SignalID == signalID)
                    return i;
                i++;
            }
            throw new Error($"No signal ID #{signalID} is found for this shape!");
        }

        /// <summary>
        /// Fly-weight class defining encapsulating one data sample
        /// </summary>
        public class XSample
        {
            public long SampleIndex = 0;
            private FSS_PluginShape _shape;

            public XSample(FSS_PluginShape shape) => _shape = shape;

            /// <summary>
            /// returns a value for a given signal index for this data sample
            /// </summary>
            /// <param name="sigIdx">signal index obtained with SignalIndex() function</param>
            /// <returns>data value</returns>
            public double Get(Int32 sigIdx)
            {
                return _shape.ReadAsDouble(SampleIndex, sigIdx);
            }

        } // class XSample

        /// <summary>
        /// returns an iterator over a given samples range. Both 'start' and 'end' might be negative:
        /// this means that the offset is calculated from the end of the array (pythonic notation).
        /// E.g.: Samples(-3): iterator over the last three samples
        /// Samples(10, -2): iterator starting from the 10th sample until the 2nd last sample      
        /// By default, all available samples will be iterated upon.
        /// </summary>
        /// <param name="start">buffer position (in samples) to start reading from</param>
        /// <param name="end">buffer position (in samples) to stop reading at
        /// <returns>an iterator object</returns>
        public IEnumerable<XSample> Samples(Int64 start = 0, Int64 end = -1)
        {
            var samp = new XSample(this);

            var S = (uint)(start < 0 ? start + NumSamples : start);
            var E = (uint)(end < 0 ? end + NumSamples : end);

            if (S >= (uint)NumSamples || E >= (uint)NumSamples)
                throw new Error($"Wrong samples range [{S}; {E}] specified!");

            for (var i = S; i < E; i++)
            {
                samp.SampleIndex = i;
                yield return samp;
            }
        }

        /// <summary>
        /// returns a value for a given signal index from a specific data sample
        /// </summary>
        /// <param name="sampleIndex">data sample index, must lie in [0; NumSamples-1]</param>
        /// <param name="sigIdx">signal index obtained with SignalIndex() function</param>
        /// <returns></returns>
        public double Get(Int64 sampleIndex, Int32 sigIdx)
        {
            if ((UInt64)sampleIndex >= NumSamples)
                throw new Error("Invalid sample index!");

            return ReadAsDouble(sampleIndex, sigIdx);
        }
        public partial class BitmapType
        {
            /// <summary>
            /// Bitmap width in pixels
            /// </summary>
            public UInt32 Width { get => _shape._data.imageW; }

            /// <summary>
            /// Bitmap height in pixels
            /// </summary>
            public UInt32 Height { get => _shape._data.imageH; }

            /// <summary>
            /// Top left x coodrinate of the interpolated bitmap in mm.
            /// </summary>
            public double X0 { get => _shape._data.x0; }

            /// <summary>
            /// Top left y coordinate of the interpolated bitmap in mm.
            /// </summary>
            public double Y0 { get => _shape._data.y0; }

            /// <summary>
            /// Bottom right x coordinate of the interpolated bitmap in mm.
            /// </summary>
            public double X1 { get => _shape._data.x1; }

            /// <summary>
            /// Bottom right y coordinate of the interpolated bitmap in mm.
            /// </summary>
            public double Y1 { get => _shape._data.y1; }

            /// <summary>
            /// returns a typed raw pointer to the bitmap's scan-line 'y' and a given signal 'sigIdx'
            /// </summary>
            /// <typeparam name="T">requested data type (must match the internal data type)</typeparam>
            /// <param name="sigIdx">signal index obtained with the funcion SignalIndex()</param>
            /// <param name="y">scan-line index, must lie within [0; Height-1]</param>
            /// <returns>raw data pointer</returns>
            public unsafe T* GetScanLineAs<T>(int sigIdx, uint y = 0) where T : unmanaged
            {
                if (y >= Height)
                    throw new Error("Wrong scan line index !");

                bool ok = false;
                var type = typeof(T);

                switch (_shape.SignalInfos[sigIdx].DataType)
                {
                    case DataType.Double:
                        ok = (type == typeof(double));
                        break;
                    case DataType.Float:
                        ok = (type == typeof(float));
                        break;
                    case DataType.UInt32:
                        ok = (type == typeof(UInt32));
                        break;
                    case DataType.Int32:
                        ok = (type == typeof(Int32));
                        break;
                    case DataType.UShort:
                        ok = (type == typeof(UInt16));
                        break;
                    case DataType.Short:
                        ok = (type == typeof(Int16));
                        break;
                    case DataType.UChar:
                        ok = (type == typeof(byte));
                        break;
                    case DataType.Char:
                        ok = (type == typeof(sbyte));
                        break;
                }
                if (!ok)
                {
                    throw new Error($"Requeted data type: {type} must agree with the internal buffer type!");
                }
                return (T*)_shape._rawBufs[sigIdx] + y * Width;
            }
        };

        unsafe private T ReadT<T>(long sampleIdx, int sigIdx) where T : unmanaged
        {
            T* src = (T *)_rawBufs[sigIdx];
            return src[sampleIdx];
        }

        //based on signal data type read data from buffer, return double as common data type
        unsafe private double ReadAsDouble(long sampleIdx, int sigIdx)
        {
            var type = SignalInfos[sigIdx].DataType;
            byte* src = (byte *)_rawBufs[sigIdx];

            switch (type)
            {
                case DataType.Double:
                    return ((double*)src)[sampleIdx];

                case DataType.Float:
                    return ((float*)src)[sampleIdx];

                case DataType.UInt32:
                    return ((UInt32*)src)[sampleIdx];

                case DataType.Int32:
                    return ((Int32*)src)[sampleIdx];

                case DataType.UShort:
                    return ((UInt16*)src)[sampleIdx];

                case DataType.Short:
                    return ((Int16*)src)[sampleIdx];

                case DataType.UChar:
                    return ((byte*)src)[sampleIdx];

                case DataType.Char:
                    return ((sbyte*)src)[sampleIdx];
            } // switch

            throw new Error($"Unexpected signal type: {type}");
        }
    } // class FSS_PluginShape
#nullable disable

    #endregion // Flying Spot plugin

    #region Downsample Plugin
    public const string DownSample_Plugin_Name = "DownSamplePlugin";

    public const UInt32 CmdID_DownSample_Rate = 0x54525344; //DSRT
    public const UInt32 CmdID_DownSample_Output_Rate = 0x5452544F; //OTRT
    public const UInt32 CmdID_DownSample_Output_Max_Min = 0x4D4D544F; //OTMM
    #endregion // Downsample Plugin
}
