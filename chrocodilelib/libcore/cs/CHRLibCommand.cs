/*
TCHRLibCmdWrapper provides classes/functions related to CHR command/response.
Command/Response classes fall into two different categories: general and specific.
General classes can be used to construct any arbitary command (TCommand class) 
and read out response from the response handle passed over from CHRocodileLib (TResponse class).
Specific classes are only for one special command, e.g. TOutputSignalsCmd, TOutputSignalsRsp are only for setting/getting output signals. 
General and specific classes all inherit from the same interface.
"ExecCommand"/"ExecCommandAsync" function, which are provided here, uses above defined command/response c# interface.
These classes/functions aim to simplify the procedure of command sending and response processing.
*/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace CHRocodileLib
{
    public partial class Cmd
    {
        private static void addInt(Cmd_h h, int v)
        {
            Lib.AddCommandIntArg(h, v);
        }

        private static void addIntArray(Cmd_h h, int[] v)
        {
            Lib.AddCommandIntArrayArg(h, v, v.Length);
        }

        private static void addFloat(Cmd_h h, float v)
        {
            Lib.AddCommandFloatArg(h, v);
        }

        private static void addFloatArray(Cmd_h h, float[] v)
        {
            Lib.AddCommandFloatArrayArg(h, v, v.Length);
        }

        private static void addString(Cmd_h h, string v)
        {
            Lib.AddCommandStringArg(h, v, v.Length);
        }

        private static void addBytes(Cmd_h h, byte[] v)
        {
            Lib.AddCommandBlobArg(h, v, v.Length);
        }

        /// <summary>
        /// Used internally to create command objects. Such objects are created and held inside the
        /// library and not here.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="v"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void Add(Cmd_h h, object v)
        {
            switch (v)
            {
                case uint i: addInt(h, (int)i); break;
                case int i: addInt(h, i); break;
                case int[] i: addIntArray(h, i); break;
                case float f: addFloat(h, f); break;
                case float[] i: addFloatArray(h, i); break;
                case double f: addFloat(h, (float)f); break; // only float supported
                case string s: addString(h, s); break;
                case byte[] bytes: addBytes(h, bytes); break;
                case null: throw new ArgumentNullException(nameof(v));
                default:
                {
                        var t = v.GetType();
                        if (t.BaseType == typeof(Enum))
                        {
                            var et = Enum.GetUnderlyingType(t);
                            if (et == typeof(Int32))
                                addInt(h, (int)v);
                            else if (et == typeof(UInt32))
                                addInt(h, (int)((uint)v));
                            break;
                        }
                    throw new ArgumentException(message: "Invalid argument type", paramName: v.GetType().ToString());
                }
            };
        }

        /// <summary>
        /// Used internally to create a command or - more specifically - let the library create a command or query.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isQuery"></param>
        /// <param name="args">arguments such as ints, floats, strings, byte arrays (blobs)</param>
        /// <returns></returns>
        private static Cmd_h Make(CmdID cmd, bool isQuery, params object[] args)
        {
            Cmd_h hCmd = Cmd_h.InvalidHandle;
            Lib.NewCommand(cmd, isQuery? 1 : 0, out hCmd);
            foreach (object a in args)
                Add(hCmd, a);
            return hCmd;
        }

        /// <summary>
        /// Creates a new command based upon the ID and the parameters given.
        /// </summary>
        /// <remarks>
        /// NOTE: In most cases you can call Connection.Exec() directly.
        /// You should use this command only when calling Exec with multiple commands that are
        /// supposed to be pipelined.
        /// NOTE: A command generated this way can be used only once.
        /// The returned handle is invalid after first use.
        /// </remarks>
        /// <param name="cmd">The command ID</param>
        /// <param name="args">Arguments of any supported type (int, float, string, byte array) - must match the command's requirements.</param>
        /// <returns>The command's handle that can be passed into Connection.Exec() or ExecAsync()</returns>
        /// <example>
        /// _con.Exec(
        ///     Cmd.Command(CmdID.MeasuringMethod, false, 1),
        ///     Cmd.Command(CmdID.ScanRate, false, 10000.0)
        /// );
        /// </example>
        /// 
        public static Cmd_h Command(CmdID cmd, params object[] args)
        {
            return Make(cmd, false, args);
        }

        /// <summary>
        /// Creates a new query based upon the ID and the parameters given.
        /// </summary>
        /// <remarks>
        /// NOTE: In most cases you can call Connection.Exec() directly.
        /// You should use this command only when calling Exec with multiple commands that are
        /// supposed to be pipelined.
        /// NOTE: A command generated this way can be used only once.
        /// The returned handle is invalid after first use.
        /// </remarks>
        /// <param name="cmd">The command ID</param>
        /// <param name="args">Arguments of any supported type (int, float, string, byte array) - must match the query's requirements.</param>
        /// <returns>The command's handle that can be passed into Connection.Exec() or ExecAsync()</returns>
        /// <example>
        /// _con.Exec(
        ///     Cmd.Query(CmdID.MeasuringMethod),
        ///     Cmd.Query(CmdID.ScanRate)
        /// );
        /// </example>
        /// 
        public static Cmd_h Query(CmdID cmd, params object[] args)
        {
            return Make(cmd, true, args);
        }

        /// <summary>
        /// Creates a new command based upon the string passed in.
        /// </summary>
        /// <remarks>
        /// NOTE: In most cases you can call Connection.Exec() directly
        /// NOTE: A command generated this way can be used only once.
        /// The returned handle is invalid after first use.
        /// </remarks>
        /// <param name="cmd">A complete command string such as 'DWD 12.0 14.0 200.3 240.5' or 'SHZ?'</param>
        /// <returns>The command's handle that can be passed into Connection.Exec() or ExecAsync()</returns>
        public static Cmd_h FromStr(string cmd)
        {
            Lib.Check(Lib.NewCommandFromString(cmd, out var hCmd), "New from string");
            return hCmd;
        }
    };

    /// <summary>
    /// Represents a command response so that users can conveniently access all response information.
    /// </summary>
    public class Response
    {
        private ResponseInfo _info;
        public ResponseInfo Info { get => _info; }

        private Rsp_h _handle = Cmd_h.InvalidHandle;
        public readonly Conn_h Source;
        private readonly bool _isUpdate;

        public Rsp_h Handle { get => _handle; }

        private object[] _params = Array.Empty<object>();
        public object[] Params { get => _params; }

        private string _signature = ""; // types of all response parameters as string
        public string Signature { get => _signature; }
        public int ParamCount { get => _signature.Length; }
        /// <summary>
        /// Return type char of n_th parameter
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <returns></returns>
        /// <exception cref="Error"></exception>
        public ParamType ParamTypes(int paramIndex)
        {
            switch (_signature[paramIndex])
            {
                case 'i': return ParamType.Int;
                case 'f': return ParamType.Float;
                case 's': return ParamType.String;
                case 'B': return ParamType.Bytes;
                case 'I': return ParamType.Ints;
                case 'F': return ParamType.Floats;
                default: throw new Error($"Unknown response argument type ({_signature[paramIndex]})");
            }
        }

        /// <summary>
        /// translate parameter type to character for easy command signature checks
        /// via ParamTypes property
        /// </summary>
        /// <param name="type">parameter type code</param>
        /// <returns>char</returns>
        /// <exception cref="Error"></exception>
        private char TypeToChar(ParamType type)
        {
            switch (type)
            {
                case ParamType.Int: return 'i';
                case ParamType.Float: return 'f';
                case ParamType.String: return 's';
                case ParamType.Bytes: return 'B';
                case ParamType.Ints: return 'I';
                case ParamType.Floats: return 'F';
                default: throw new Error($"Unknown response argument type ({type})");
            }
        }

        /// <summary>
        /// return parameter no. "paramIndex". Cast to type T.
        /// T must match the original response parameter type.
        /// </summary>
        /// <typeparam name="T">must match the parameter's type</typeparam>
        /// <param name="paramIndex">index of parameter to extract</param>
        /// <returns>parameter</returns>
        public T GetParam<T>(int paramIndex)
        {
            return (T)_params[paramIndex];
        }

        /// <summary>
        /// As GetParam<T> but returns success as boolean instead of throwing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramIndex">index of parameter to extract</param>
        /// <param name="value">output variable to receive the parameter value</param>
        /// <returns>success</returns>
        public bool TryGetParam<T>(int paramIndex, out T value)
        {
            value = default;
            if (_params[paramIndex] is T val)
            {
                value = val;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Used internally. Creates response from CHRocodileLib response handle.
        /// </summary>
        /// <param name="hRsp">CHRocodileLib response handle</param>
        /// <param name="hSource">Connection that has initiated the command</param>
        /// <param name="throwOnSetErrorFlag">if true, throws if error flag has been set by library</param>
        public Response(Rsp_h hRsp, Conn_h hSource, bool isUpdate, bool throwOnSetErrorFlag = true)
        {
            _handle = hRsp;
            Source = hSource;
            _isUpdate = isUpdate;
            ReadFromLib(hRsp, throwOnSetErrorFlag);
        }

        public bool IsError()
        {
            return (_info.Flags & RspFlag.Error) != 0;
        }

        public void SetError()
        {
            _info.Flags |= RspFlag.Error;
        }
        public bool IsWarning()
        {
            return (_info.Flags & RspFlag.Warning) != 0;
        }
        public bool IsQuety()
        {
            return (_info.Flags & RspFlag.Query) != 0;
        }
        /// <summary>
        /// Indicates whether this response is an update or a regular response.<br/>
        /// <b>NOTE:</b> An update is a response for which the command has not been sent by<br/>
        /// the connection emitting the response object.<br/>
        /// An update can originate either from a sensor update or a command from a shared connection.
        /// </summary>
        /// <returns></returns>
        public bool IsUpdate()
        {
            return _isUpdate || ((_info.Flags & RspFlag.Update) != 0);
        }
        
        /// <summary>
        /// Translates response to string.
        /// </summary>
        /// <returns>response in string representation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public override string ToString()
        {
            string str = Lib.CmdIDToStr(_info.CmdID);
            if (_info.Flags != 0)
            {
                str += " (";
                if ((_info.Flags & RspFlag.Query) != 0)
                    str += "?";
                if ((_info.Flags & RspFlag.Update) != 0)
                    str += "U";
                if ((_info.Flags & RspFlag.Warning) != 0)
                    str += "W";
                if ((_info.Flags & RspFlag.Error) != 0)
                    str += "E";
                str += ")";
            }
            

            foreach (object p in _params)
            {
                switch (p)
                {
                    case int i: str += $" {i}"; break;
                    case float f: str += $" {f}"; break;
                    case string s: str += $" {s}"; break;
                    case byte[] bytes: str += " <blob>"; break;
                    case int[] ints:   
                        {
                            str += " [";
                            foreach (int i in ints)
                                str += $" {i}";
                            str += " ]";
                            break;
                        }
                    case float[] floats:
                        {
                            str += " [";
                            foreach (int f in floats)
                                str += $" {f}";
                            str += " ]";
                            break;
                        }
                    case null: throw new ArgumentNullException(nameof(p));
                    default: throw new ArgumentException(message: "Invalid argument type", paramName: nameof(p));
                }
            }
            return str;
        }

        /// <summary>
        /// Used internally. Fills in all response information from library.
        /// </summary>
        /// <param name="hRsp">library response handle</param>
        /// <param name="throwOnSetErrorFlag">if true, throws if error flag has been set by library</param>
        /// <exception cref="Error"></exception>
        private void ReadFromLib(Rsp_h hRsp, bool throwOnSetErrorFlag)
        {
            Lib.Check(Lib.GetResponseInfo(hRsp, out _info), "response from lib");

            _params = new object[_info.ParamCount];

            for (UInt32 i = 0; i < _info.ParamCount; i++)
            {
                Lib.Check(Lib.GetResponseArgType(hRsp, i, out ParamType type));
                _signature += TypeToChar(type); // throws on unknown type code
                switch (type)
                {
                    case ParamType.Int:
                        {
                            Int32 nIntParam;
                            Lib.Check(Lib.GetResponseIntArg(_handle, i, out nIntParam), $"get arg {i} type {type}");
                            _params[i] = nIntParam;
                            break;
                        }
                    case ParamType.Float:
                        {
                            float nFloatParam;
                            Lib.Check(Lib.GetResponseFloatArg(_handle, i, out nFloatParam), $"get arg {i} type {type}");
                            _params[i] = nFloatParam;
                            break;
                        }
                    case ParamType.Ints:
                        {
                            Lib.Check(Lib.GetResponseIntArrayArg(_handle, i, out IntPtr pData, out int len), $"get arg {i} type {type}");
                            Int32[] aIntArray = new Int32[len];
                            if (len > 0)
                                Marshal.Copy(pData, aIntArray, 0, len);
                            _params[i] = aIntArray;
                            break;
                        }
                    case ParamType.Floats:
                        {
                            Lib.Check(Lib.GetResponseFloatArrayArg(_handle, i, out IntPtr pData, out int len), $"get arg {i} type {type}");
                            float[] aFloatArray = new float[len];
                            if (len > 0)
                                Marshal.Copy(pData, aFloatArray, 0, len);
                            _params[i] = aFloatArray;
                            break;
                        }
                    case ParamType.String:
                        {
                            Lib.Check(Lib.GetResponseStringArg(_handle, i, out IntPtr pData, out int len), $"get arg {i} type {type}");
                            string sParam = "";
                            if (len > 0)
                                sParam = Marshal.PtrToStringAnsi(pData, len-1);
                            _params[i] = sParam;
                            break;
                        }
                    case ParamType.Bytes:
                        {
                            Lib.Check(Lib.GetResponseBlobArg(_handle, i, out IntPtr pData, out int len), $"get arg {i} type {type}");
                            byte[] bytes = new Byte[len];
                            if (len > 0)
                                Marshal.Copy(pData, bytes, 0, len);
                            _params[i] = bytes;
                            break;
                        }
                }
            }

            if ((_info.Flags & RspFlag.Error) != 0 && throwOnSetErrorFlag)
                throw new Error($"command failed: {ToString()}");
        }

    }
}
