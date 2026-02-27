using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CHRocodileLib
{
    /// <summary>
    /// The core of C# async / await support. Implements INotifyCompletion interface,
    /// translates callback-based CHRococileLib support for asynchronous mode to task-based
    /// async / await.
    /// </summary>
    /// <remarks>
    /// Saves current SynchronizationContext (thread) at the point when ExecAsync is called
    /// </remarks>
    public class CHRocoAwaiter : System.Runtime.CompilerServices.INotifyCompletion
    {
        private Action _continuation;
        private Response _response;
        private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();
        private readonly SynchronizationContext _context;
        public Lib.ResponseAndUpdateCallback _libCallback;

        public CHRocoAwaiter(SynchronizationContext context)
        {
            _continuation = null;
            _response = null;
            _context = context;
            //_libCallback = null; // because struct must be fully initialized before use of "LibCallback"
            _libCallback = LibCallback;
        }

        public Response GetResult()
        {
            Debug.Assert(_response != null);
            if ((_response.Info.Flags & RspFlag.Error) != 0)
                throw new Error($"command failed: {_response.ToString()}");
            return _response;
        }

        /// <summary>
        /// Called from CHRocodileLib in case of asynchronous events. The task is to translate
        /// this call to a C# continuation invocation so that "await execCmd()" will be continued.
        /// </summary>
        /// <param name="sInfo"></param>
        /// <param name="hRsp"></param>
        public void LibCallback(TRspCallbackInfo sInfo, Rsp_h hRsp)
        {
            _response = new Response(hRsp, sInfo.Source, false, false);
            // this function might get called even before OnCompleted() - in this case, wait for the continuation:
            while (_continuation == null) // TODO: Find something more elegant
                Thread.Sleep(1);

            // make sure the continuation action is called in the original context / thread:
            _context.Post(_postCallback, _continuation);
        }

        /// <summary>
        /// Get a C pointer to LibCallback() above to be passed to CHRocodileLib
        /// </summary>
        public IntPtr LibCallbackPtr
        {
            get
            {
                var ptr = Marshal.GetFunctionPointerForDelegate(_libCallback);
                return ptr;
            }
        }

        /// <summary>
        /// Called from C# runtime to check if suspending is even needed. We assume that any
        /// asynchronous call to the library will take "long" so that we do not support this short-cut.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return false;
            }
        }

        public void OnCompleted(Action continuation) => _continuation = continuation;
    };

    /// <summary>
    /// To be implemented / finished. DO NOT USE!
    /// </summary>
    public class CHRocoMultiResponseAwaiter : System.Runtime.CompilerServices.INotifyCompletion
    {
        private Action _onCompletion = null;
        private List<Response> _responses = new List<Response>();

        private int _responseCount = 0;
        private int _responseIdx = 0;

        private readonly object _lock = new object();

        public CHRocoMultiResponseAwaiter(int responseCount)
        {
            _responseCount = responseCount;
        }
        public void SetResponseCount(int responseCount)
        {
            lock(_lock)
            {
                _responseCount = responseCount;
                if (_responseIdx >= _responseCount && _onCompletion != null)
                    ((Action)_onCompletion).Invoke();
            }
        }

        public bool IsCompleted
        {
            get { return _responseIdx >= _responseCount; }
        }
        public List<Response> GetResult()
        {
            return _responses;
        }

        public void LibCallback(TRspCallbackInfo sInfo, Rsp_h hRsp)
        {
            lock (_lock)
            {
                _responses.Add(new Response(hRsp, sInfo.Source, false));
                _responseIdx++;
                if (_responseIdx >= _responseCount && _onCompletion != null)
                    ((Action)_onCompletion).Invoke();
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _onCompletion = continuation;
        }
    };

    public enum CTState { idle, ctor, awaitCtor, exec, awaitCB, reply, awaitDTor, dtor };

    /// <summary>
    /// Used for async / await support. Implements GetAwaiter() which in turn
    /// returns a CHRocoAwaiter object - see above.
    /// </summary>
    public class CHRocoTask
    {
        private CHRocoAwaiter _awaiter;
        public CHRocoAwaiter GetAwaiter()
        {
            return _awaiter;
        }
        public CHRocoTask()
        {
            _awaiter = new CHRocoAwaiter(SynchronizationContext.Current);
        }
        ~CHRocoTask()
        {
        }
    };
    //[System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(TaskLikeMethodBuilder))]
    public class CHRocoMultiTask
    {
        public static CTState _state = CTState.idle;
        public static int Counter { get; }
        private readonly CHRocoMultiResponseAwaiter _awaiter;
        private readonly Lib.ResponseAndUpdateCallback _libCallback;
        public IntPtr LibCallbackPtr
        {
            get
            {
                var ptr = Marshal.GetFunctionPointerForDelegate(_libCallback);
                return ptr;
            }
        }
        //public Lib.ResponseAndUpdateCallback LibCallback
        //{
        //    get => _libCallback;
        //}
        public CHRocoMultiTask(int responseCount)
        {
            _state = CTState.ctor;
            //Debug.WriteLine($"state: {_state.ToString()}");
            _awaiter = new CHRocoMultiResponseAwaiter(responseCount);
            _libCallback = _awaiter.LibCallback;
        }
        public CHRocoMultiResponseAwaiter GetAwaiter()
        {
            return _awaiter;
        }
        public void SetResponseCount(int responseCount)
        {
            _awaiter.SetResponseCount(responseCount);
        }
        ~CHRocoMultiTask()
        {
        }
    };
}
