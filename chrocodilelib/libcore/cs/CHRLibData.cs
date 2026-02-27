using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHRocodileLib
{
    using SignalList = List<(SignalInfo, Int32)>;

    public class DataInfo
    {
        public Int64 SizePerSample { get; private set; }
        public readonly Int32 GlobalSignalSize = 0;
        public readonly Int32 PeakSignalSize = 0;
        public readonly SignalGeneralInfo SignalGenInfo;
        /// <summary>
        ///  signal infortmation + signal's byte offset within a sample
        /// </summary>
        public readonly SignalList SignalInfos = new SignalList();

        public DataInfo(Int64 sizePerSample, SignalGeneralInfo genInfo, IntPtr sigInfo)
        {
            SignalGenInfo = genInfo;

            if (SignalGenInfo.ChannelCount <= 0)
                throw new Error($"DataInfo: Invalid channel count {SignalGenInfo.ChannelCount}");

            Int32 nOffset = 0;
            int sigInfoSize = Marshal.SizeOf<SignalInfo>();
            for (int i = 0; i < SignalGenInfo.PeakSignalCount + SignalGenInfo.GlobalSignalCount; i++)
            {
                var info = (SignalInfo)Marshal.PtrToStructure(sigInfo, typeof(SignalInfo));
                SignalInfos.Add((info, nOffset));
                sigInfo = IntPtr.Add(sigInfo, sigInfoSize);
                var nSize = SignalSize(info.DataType);
                nOffset += nSize;
                if (SignalGenInfo.ChannelCount == 1 || IsGlobalSignal(info.SignalID))
                    GlobalSignalSize += nSize;
                else
                    PeakSignalSize += nSize;
            }
            SizePerSample = sizePerSample;
        }

        public DataInfo(Conn_h connectionHandle)
        {
            Int64 nBufSize = 0;
            Lib.GetConnectionOutputSignalInfos(connectionHandle, out SignalGenInfo,
                new SignalInfo[0], ref nBufSize);
            var aSigInfo = new SignalInfo[nBufSize / Marshal.SizeOf(typeof(SignalInfo))];
            if (nBufSize > 0)
            {
                Lib.Check(Lib.GetConnectionOutputSignalInfos(connectionHandle, out SignalGenInfo,
                      aSigInfo, ref nBufSize), "output signal info query failed.");
            }

            Int32 nOffset = 0;
            int sigInfoSize = Marshal.SizeOf<SignalInfo>();
            for (int i = 0; i < SignalGenInfo.PeakSignalCount + SignalGenInfo.GlobalSignalCount; i++)
            {
                var info = aSigInfo[i];
                SignalInfos.Add((info, nOffset));
                var nSize = SignalSize(info.DataType);
                nOffset += nSize;
                if (SignalGenInfo.ChannelCount == 1 || IsGlobalSignal(info.SignalID))
                {
                    GlobalSignalSize += nSize;
                }
                else
                    PeakSignalSize += nSize;
            }
            SizePerSample = GlobalSignalSize + SignalGenInfo.ChannelCount * PeakSignalSize;
        }


        public static Int32 SignalSize(DataType _nDataType)
        {
            switch (_nDataType)
            {
                case DataType.UChar:
                case DataType.Char:
                    return 1;
                case DataType.UShort:
                case DataType.Short:
                    return 2;
                case DataType.UInt32:
                case DataType.Int32:
                case DataType.Float:
                    return 4;
                default:
                    return 8;
            }
        }
        public static bool IsGlobalSignal(UInt16 _nSigID)
        {
            return (_nSigID & 0x100) == 0;
        }
    }

    public enum DataStatus : Int32
    {
        Error = -1,
        Detached = 0,
        Saving = 1,
        Stopped = 2
    }


    /// <summary>
    /// Represents a buffer containing CHRocodile sensor data (zero, one or more samles)
    /// Is implemented by means of a CHRocodileLib auto / user buffer that gets filled asynchronously.
    /// This approach allows for efficient data transport and storage.
    /// Size can be configured. Buffer can be "detached" in order to keep data permanently.
    /// Exposes functions for accessing the samples.
    /// </summary>
    public class Data
    {
        /// <summary>
        /// describes data sample structure for this buffer
        /// </summary>
        public DataInfo Info;
        /// <summary>
        /// raw buffer size in bytes this object holds
        /// </summary>
        public readonly Int64 BufferSize;

        public Connection ParentConnection;     // connection handle needed to get sample count
        public byte[] _sampleData;
        private GCHandle _pinnedPtr;
        private Int64 _end = 0;
        private Int64 _begin = 0;
        private Int64 _initialSampleCount = 0; // samples copied during StartNewAutoBuffer() call

        
        /// <summary>
        /// Gants access to raw buffer content as filled by CHRocodileLib
        /// </summary>
        public byte[] Raw { get => _sampleData; }

        /// <summary>
        /// Updates local end marker to current state of auto buffer to lib's saved sample count.<br>
        /// 
        /// Within an async callback or after a GetNextSamples() count, the _end marker remains stable
        /// even though the auto buffer keeps being filled with new sample data. This avoids problems
        /// if users use NumSamples expecting that this number does not suddenly change.<br>
        /// 
        /// Calling SynchronizeEndPos() is a means to intentionally gain access to potentially more samples.
        /// <return>true if there are in fact more samples available now</return>
        /// </summary>
        public bool SynchronizeEndPos(Int64 maxSamples = 0)
        {
            if (ParentConnection == null)
                return false;
            Int64 count = Lib.GetSavedSampleCount(ParentConnection.Handle) + _initialSampleCount;
            bool moreSamplesAvailable = count > _end;
            // if a maximum sample count is given, limit the number of samples:
            _end = (maxSamples <= 0)? count : Math.Min(count, _begin + maxSamples);
            return moreSamplesAvailable;
        }

        /// <summary>
        /// the number of data samples currently available for reading
        /// </summary>
        public Int64 NumSamples {
            get {
                return _end - _begin;
            }
        }

        /// <summary>
        /// Total sample count as filled in by the DLL (can be more than indicated by _end)
        /// </summary>
        public Int64 TotalNumSamples
        { get => ParentConnection == null? _end : Lib.GetSavedSampleCount(ParentConnection.Handle) + _initialSampleCount; }

        /// <summary>
        /// Constructor which is invoked by a connection object
        /// </summary>
        /// <param name="info">Data meta-info such as signal descriptions</param>
        /// <param name="handle">Connection handle necessary for CHRocodileLib calls</param>
        /// <param name="bufSize">Size of the associated AutoBuffer</param>
        public Data(Connection parent, Int64 bufSize)
        {
            BufferSize = bufSize;
            ParentConnection = parent;
            _sampleData = new byte[bufSize];
            _pinnedPtr = GCHandle.Alloc(_sampleData, GCHandleType.Pinned);
        }

        public DataStatus Status { get; set; }

        /// <summary>
        /// Re-initialize data object
        /// </summary>
        /// <param name="info">Data meta-info such as signal descriptions</param>
        /// <param name="begin">Data reading begin position</param>
        /// <param name="end">Data reading end position</param>
        public void Reset(DataInfo info, Int64 initialSampleCount, Int64 begin = 0, Int64 end = 0)
        {
            Info = info;
            _begin = begin;
            _end = end;
            _initialSampleCount = initialSampleCount;
            Debug.WriteLine($"**********Reset {_begin} {_end} {_initialSampleCount}");
        }

        /// <summary>
        /// Resets begin position to access all samples in the buffer from foreach loop
        /// </summary>
        public void Rewind()
        {
            _begin = 0;
        }

        /// <summary>
        /// Skip any left samples to read
        /// </summary>
        public void MoveReadPosToEnd()
        {
            _begin = _end;
        }

        /// <summary>
        /// detaches the buffer from library's autobuffer and "seal" it, i.e., the buffer won't be updated anymore
        /// </summary>
        public Data DetachRenew(int newBufferSampleCount = 0)
        {
            if (ParentConnection == null)
                return this;
            ParentConnection.DetachRenewDataBuffer(newBufferSampleCount);
            return this;
        }

        /// <summary>
        /// return whether the buffer is not active anymore (detached from library's autobuffer)
        /// </summary>
        public bool IsDetached { get => ParentConnection == null; }

        ~Data()
        {
            if (_pinnedPtr.IsAllocated)
                _pinnedPtr.Free();
        }

        // raw buffer pointer
        public IntPtr BufferPtr {
            get => _pinnedPtr.AddrOfPinnedObject();
        }

        #region Get sample data functions / Enumerators

        /// <summary>
        /// returns a specific signal value from a given data sample (using channel and signal indices)
        /// </summary>
        /// <param name="sampleIdx">data sample index</param>
        /// <param name="chnIdx">channel index</param>
        /// <param name="sigIdx">signal index (this is not a signal ID!)</param>
        /// <returns>signal value</returns>
        public double Get(int sampleIdx, int sigIdx, int chnIdx)
        {
            // TODO: Check sigIdx
            if (chnIdx >= Info.SignalGenInfo.ChannelCount)
                throw new Error("Wrong channel index!");

            if (sampleIdx >= _end)
                throw new Error("Wrong sample index!");

            var (sinfo, sofs) = Info.SignalInfos[(int)sigIdx];
            long nOffset = sampleIdx * Info.SizePerSample + sofs;
            if (sigIdx >= Info.SignalGenInfo.GlobalSignalCount)
                nOffset += chnIdx * Info.PeakSignalSize;

            return ReadRaw(nOffset, sinfo.DataType);
        }

        public (SignalInfo, Int32) SignalInfo(UInt16 signalID)
        {
            int k = 0;
            foreach (var (sinfo, sofs) in Info.SignalInfos)
            {
                if (sinfo.SignalID == signalID)
                {
                    // all signals are treated as global for single-channel devices
                    // distinguish global signals by negative sign
                    var xofs = (k < Info.SignalGenInfo.GlobalSignalCount ? -sofs - 1 : sofs);
                    return (sinfo, xofs);
                }
                k++;
            }
            throw new Error($"Signal {signalID} not available / included in data stream.");
        }

        /// <summary>
        /// returns a signal index from signal ID for fast iterator access by GlobalByIndex or PeakByIndex
        /// </summary>
        /// <param name="signalID">Signal ID to be iterated on</param>
        /// <returns>index of the signal in the internal array to be iterate on</returns>
        public Int32 SignalIndex(UInt16 signalID)
        {
            for(int i = 0; i < Info.SignalInfos.Count; i++)  
            {
                if (Info.SignalInfos[i].Item1.SignalID == signalID)
                    return i;
            }
            return -1;
        }

        private (Int64, Int64) CalcSamplesRange(Int64 start, Int64 end)
        {
            if (start == Int64.MaxValue)
                start = _begin;
            if (end == Int64.MaxValue)
                end = _end;

            var S = start < 0 ? start + _end : start;
            var E = end < 0 ? end + _end + 1 : end;

            if ((UInt64)S > (UInt64)_end || (UInt64)E > (UInt64)_end)
                throw new Error($"Samples range [{S}; {E}] out of bounds [0; {_end}]!");

            return (S, E);
        }

        /// <summary>
        /// returns an iteratator over a given samples range. Both 'start' and 'end' might be negative:
        /// this means that the offset is calculated from the end of the array (pythonic notation).
        /// E.g.: Samples(-3): iterator over the last three samples
        /// Samples(10, -2): iterator starting from the 10th sample until the 2nd last sample      
        /// If start / stop is omitted, reading starts at current _begin position
        /// </summary>
        /// <param name="start">buffer position (in samples) to start reading from</param>
        /// <param name="end">buffer position (in samples) to stop reading at (will not be read itself)</param>
        /// <returns>an iterator object</returns>
        public IEnumerable<Sample> Samples(Int64 start = Int64.MaxValue, Int64 end = -1)
        {
            var samp = new Sample(this);

            var (S, E) = CalcSamplesRange(start, end);

            long sofs = S * Info.SizePerSample;

            if (S == E)
                yield break;

            for (var i = S; i < E; i++, sofs += Info.SizePerSample)
            {
                samp.SampleIndex = i;
                samp.SampleOfs = sofs;
                yield return samp;
            }
        }


        // TODO: new array of signal x values for all channels

        
        public class Sample
        {
            private readonly Data _data;
            public Int64 SampleIndex = 0;
            public Int64 SampleOfs = 0;  // SampleIndex * info.SizePerSample but precomputed for efficiency

            public Sample(Data data)
            {
                _data = data;
            }

            public double Get(int signalIndex)
            {
                if (signalIndex >= (uint)_data.Info.SignalGenInfo.GlobalSignalCount)
                    throw new Error("invalid signal index.");

                var (sinfo, sofs) = _data.Info.SignalInfos[signalIndex];
                return _data.ReadRaw(SampleOfs + sofs, sinfo.DataType);
            }

            public double Get(int signalIndex, int channelIndex)
            {
                DataInfo info = _data.Info;
                if (signalIndex < info.SignalGenInfo.GlobalSignalCount)
                    throw new Error($"signal index {signalIndex} is invalid. Use this get() overload for multi-channel peak signals only");

                if (channelIndex >= info.SignalGenInfo.ChannelCount)
                    throw new Error($"channel index {channelIndex} is invalid (must be < {info.SignalGenInfo.ChannelCount}).");

                var (sinfo, sofs) = info.SignalInfos[signalIndex];
                sofs += channelIndex * info.PeakSignalSize;
                return _data.ReadRaw(SampleOfs + sofs, sinfo.DataType);
            }


        } // class XSample
        #endregion

        #region private functions
        //based on signal data type read data from buffer, return double as common data type
        unsafe private double ReadRaw(long ofs, DataType type)
        {
            fixed (byte* src = _sampleData)
            {
                switch (type)
                {
                    case DataType.UChar:
                        return *(byte*)(src + ofs);

                    case DataType.Char:
                        return *(sbyte*)(src + ofs);

                    case DataType.UShort:
                        return *(ushort*)(src + ofs);

                    case DataType.Short:
                        return *(short*)(src + ofs);

                    case DataType.UInt32:
                        return *(UInt32*)(src + ofs);

                    case DataType.Int32:
                        return *(Int32*)(src + ofs);

                    case DataType.Float:
                        return *(float*)(src + ofs);

                    default:
                        return *(double*)(src + ofs);

                } // switch
            }
        }
#endregion
    } // class XData
}
