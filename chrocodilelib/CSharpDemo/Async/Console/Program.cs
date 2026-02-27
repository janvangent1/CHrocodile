/*
This console demo is to demonstrate, how to create a asynchronous connection with CHRocodile² device
and then send commands (either using command ID or pure command string) and collecting data.
 */


using System;
using System.Threading;
using CHRocodileLib;

namespace TCHRLibAsyncConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //create asychronous connection, for other type of device, please use corresponding device type
                using (var con = new AsynchronousConnection("192.168.170.2", DeviceType.Chr2))
                {
                    //set connection to automatically process device output, 
                    //i.e. let CHRocodileLib to create an internal thread for output processing
                    //all the reponses and data are delivered through callback function withing CHRocodileLib internal thread 
                    con.AutomaticMode = true;

                    //set scan rate asynchronously, i.e. without waiting for the response. For this command, no callback function is set
                    con.Exec(CmdID.ScanRate, null, 4000.0);

                    // query scan rate, lambda routine is called on command completion:
                    con.Query("CmdID.ScanRate", rsp => Console.WriteLine($"Response: SHZ={rsp.GetParam<float>(0)}"));

                    Console.WriteLine("...now setting up measurement...");

                    // Set up device for measurement (with a few example commands):
                    string[] cmds = { "MMD 0", "SODX 83 256", "LAI 2", "SHZ 10000", "AVD 1", "AVS 1" };
                    // Execute the above commands blocking, i. e. wait for all responses and check for errors:
                    var grp = new SynchronousCommandGroup(con);
                    foreach (string cmdString in cmds)
                        grp.Add(Cmd.FromStr(cmdString));
                    var responses = grp.WaitAll();
                    foreach (Response res in responses)
                    {
                        Console.WriteLine($"{res.ToString()} done.");
                        if (res.IsError())
                            throw new Exception($"command {res.ToString()} failed.");
                    }

                    Console.WriteLine("Setup done. Now measuring (10sec)...");

                    //read in 100000 sample and then calculate the averaged distance
                    const int total = 100000;
                    int count = total;
                    double d = 0.0;

                    // request some signals: sample counter (ID 83) and distance 1 (ID 256),
                    // then register data callback function:
                    AsynchronousConnection.DataCallback dcb = (status, data) =>
                    {
                        foreach (var s in data.Samples())
                        {
                            if (count <= 0)
                                break;
                            // gets 2nd signal of SODX request above (distance):
                            double v = s.Get(1);
                            if (v > 0.0) // average only if distance is valid
                            {
                                d += v;
                                --count;
                            }
                        }
                    };
                    con.SetDataCallback(dcb, 1000, 100);

                    while (count > 0) // wait for data collection to finish
                        Thread.Sleep(10);

                    con.Exec(CmdID.StopDataStream, null);

                    Console.WriteLine($"The average distance is {d / total} - press return.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.ToString()}");
            }
        }
    }
}
