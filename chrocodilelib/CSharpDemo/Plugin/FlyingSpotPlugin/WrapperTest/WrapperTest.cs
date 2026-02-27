using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CHRocodileLib;


namespace FSSExamples.WrapperTest
{
    class AsyncTest
    {
        private AsynchronousConnection _conn = null;          
        private AsynchronousConnection.Plugin _plugin = null;
        public AsyncTest(string ipAddress)
        {
            _conn = new AsynchronousConnection(ipAddress, DeviceType.Chr2)
            {
                GeneralResponseCallback = GenResponseCallback
            };
            _conn.SetDataCallback(XDataCallback, 1024, 0);

            // _plugin = _conn.CreatePlugin("FlyingSpotPlugin");

            //Lib.SetLibLogLevel(4);
            //Lib.SetLibLogFileDirectory(".", 100000, 1000);
        }

        ~AsyncTest()
        {
            _conn?.StoptDataStream();
        }

        public void Run()
        {
            _conn.Exec(CmdID.OutputSignals, null, 65, 66, 67, 68, 69, 256, 82, 83);
            _conn.BufferMode = AsynchronousConnection.BufferModeE.StartNew;
            //_conn.NumRequiredAutoBufferSamples = 1000;
            _conn.StartDataStream();

            Thread.Sleep(2000);
            
            var buf = _conn.DataList.Last();
            buf.Detach(); // stop automatic updates on this buffer
            Console.WriteLine($"Last buffer #samples: {buf.NumSamples}");
            
            //var offsets = buf.Prepare(new ushort[] { 83, 82, 256 });
            foreach (var smp in buf.Samples(-10,-1)) // enumerate over the last 10 samples only
            {
                string ss = "";
                //foreach (var s in smp.Signals(offsets))
                //{
                //    ss += $"{s}; ";
                //}
                Console.WriteLine($"sample {smp.SampleIndex} = {ss}");
            }
            //return;

            //offsets = buf.Prepare(x => x < 90);
            {
                //int i = 0, numSigs = offsets.Count; ;
                //string ss = "";
                //// iterate over the last 3 samples in the buffer
                //foreach (var s in buf.Signals(offsets, -3,-1))
                //{
                //    ss += $"{s}; ";
                //    i++;
                //    if (i % numSigs == 0)
                //    {
                //        Console.WriteLine($"{i / numSigs}: = {ss}");
                //        ss = "";
                //    }
                //}
            }
        }

        private void GenResponseCallback(Response rsp)
        {
            Console.WriteLine($"Got response: {rsp}");
        }

        private void XDataCallback(DataState state, XData data)
        {
            int num = 0;
            foreach (var samp in data.Samples())
            {
                num++;
            }
            //Console.WriteLine($"Got total samples: {num}");
        }


    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                var test1 = new AsyncTest("192.168.170.2");
                test1.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }
    }
}
