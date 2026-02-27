/*
This demo is a simple console demo to demonstrate, how to create a synchronous connection with CHRocodile² device
and then send commands (either using command ID or pure command string) and collecting data.
 */

using System;
using System.Threading;
using CHRocodileLib;


namespace TCHRLibBasicConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //create sychronous connection, for other type of device, please use corresponding device type
                using (var con = new SynchronousConnection("192.168.170.2", DeviceType.Chr2))
                {
                    //set scan rate
                    con.Exec(CmdID.ScanRate, 4000.0);
                    //query scan rate
                    var response = con.Query(CmdID.ScanRate); // throws on error
                    // response carries one parameter which is the queried value:
                    Console.WriteLine($"The current scan rate is {response.Params[0]}Hz.");

                    //execute a pure string command to set device output signal
                    // signals 83 (sample counter) and 256 (1st distance)
                    // NOTE: In this example signal 83 is not being used.
                    // access to signal values (see below): signal 83: Get(0), signal 256: Get(1)
                    con.ExecString("SODX 83 256");

                    //read in 1000 sample and then calculate the averaged distance
                    const int total = 1000;
                    int count = total;
                    double d = 0.0;
                    while (count > 0)
                    {
                        var data = con.GetNextSamples(); // get available samples
                        if (data.NumSamples == 0)
                            Thread.Sleep(10); // wait if none have been available yet
                        else
                        {
                            // read the data
                            foreach (var s in data.Samples())
                            {
                                d += s.Get(1); // gets 2nd signal (index 1) in the order of SODX request above, which is the distance
                                if (--count <= 0)
                                    break;
                            }
                        }                      
                    }
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
