using System;
using System.Collections.Concurrent;
using System.Threading;
using CHRocodileLib;

namespace TCHRLibAsyncScanConsole
{
    internal class Axis : IDisposable
    {
        public CHRocodileLib.AsynchronousConnection Con { get; set; }
        public void Dispose() { }

        public void MoveToStart() { Console.WriteLine("Asis moving to start..."); }
        public void MoveToEnd()
        {
            Thread.Sleep(100); // simulate axis acceleration
            Console.WriteLine("Asis moving to end...");
            // simulate encoder / index move -> triggers sensor
            // NOTE: This does not work with FSS connected!
            Con.Exec("STR", null);
        }
    }
    internal class Program
    {
        static volatile State state = State.WaitForStart;
        enum State { WaitForStart, Running, Error, FinishedMove };
        static void Main(string[] args)
        {
            Console.WriteLine("Simulating axis move with data acquisition.");
            Console.WriteLine("THIS DEMO ONLY WORKS WITHOUT ACTIVE FSS.");
            using (var con = new AsynchronousConnection("192.168.170.2", DeviceType.Chr2))
            using (var axis = new Axis() { Con = con })
            {
                try
                {
                    const int scanSampleCount = 1000;
                    //Initialize scanning status
                    //reset scan data object
                    ConcurrentQueue<Data> myData = new ConcurrentQueue<Data>();
                    con.SetDataCallback((status, data) =>
                    {
                        Console.WriteLine("Acquiring data...");
                        switch (data.Status)
                        {
                            case DataStatus.Error: state = State.Error; break;
                            case DataStatus.Stopped:
                                {
                                    //if scan is running, buffer is full, means enough samples have been collected
                                    if (state == State.Running)
                                    {
                                        con.Exec(CmdID.StopDataStream, null);
                                        //keep the data object
                                        myData.Enqueue(data.DetachRenew());
                                        state = State.FinishedMove;
                                    }
                                    break;
                                }
                        }
                    }, 100, 100);
                    //automatic process CHR device output
                    con.AutomaticMode = true;                              
                    //move axis
                    axis.MoveToStart();
                    {
                        //set device in waiting for trigger mode
                        //used for waiting response under asynchronous mode
                        var exec = new CHRocodileLib.SynchronousCommandGroup(con);
                        //set scan rate:
                        exec.Add(Cmd.Command(CmdID.ScanRate, 1000));
                        //set data average:
                        exec.Add(Cmd.Command(CmdID.DataAverage, 5));
                        //set output signals:
                        exec.Add(Cmd.Command(CmdID.OutputSignals, 256));
                        // let device wait for trigger signal (which we will simulate up in the mock axis):
                        exec.Add(Cmd.Command(CmdID.DeviceTriggerMode, TriggerMode.WaitTrigger));
                        var responses = exec.WaitAll(); // wait for all commands to complete

                        while (responses.TryDequeue(out var response))
                            if (response.IsError())
                                throw new Error($"Command {response} has failed.");
                    }
                    // remove old samples, begin new buffer for 1000 samples:
                    con.DetachRenewDataBuffer(scanSampleCount);
                    state = State.Running;
                    axis.MoveToEnd();

                    while (state == State.Running)
                        Thread.Sleep(10);

                    if (!myData.TryDequeue(out Data outData))
                        throw new Exception("no data");
                    //finish scanning
                    double d = 0.0;
                    //read data from beginning
                    outData.Rewind();
                    foreach (var s in outData.Samples())
                        d += s.Get(0);
                    Console.WriteLine($"Average distance = {d / outData.TotalNumSamples} - press return.");
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred: {e.Message}");
                }
            }

        }
    }
}

