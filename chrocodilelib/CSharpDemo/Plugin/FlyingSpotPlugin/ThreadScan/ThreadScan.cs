using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CHRocodileLib;
using FSSCommon;

namespace ThreadScan
{
    public partial class ThreadScan : Form
    {
        Task connectAndScanTest = null;
        CHRLibPlugin.FSS_PluginShape _shape;
        AutoResetEvent resultReadyEvent = new AutoResetEvent(false);
        volatile bool _disconnectionFlag = false;


        public const string SCAN_PROGRAM = @"
init
{
   $SHZ 50000;
   $SODX 256 82 65 66 69;
}

fn main(scanFreq=50000)
{
    rect(x0=-20.0, y0=-20.0, x1=20.0, y1=20.0, nCols=200,
            nRows=200, interp=0, label=""AreaScan"", waitAtEnd=20000)
}";

        public ThreadScan()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (connectAndScanTest == null)
            {
                connectAndScanTest = new Task(delegate { FSS_task(); });
                connectAndScanTest.Start();
                button1.Text = "STOP";
            }
            else
            {
                _disconnectionFlag = true;
                connectAndScanTest.Wait();
                connectAndScanTest.Dispose();

                connectAndScanTest = null;
            }
        }

        private void FSS_task()
        {
            try
            {
                FlyingSpotScanner scanner = new FlyingSpotScanner();
                DataProcessor _dataProc = new DataProcessor();

                Debug.Print("OPEN");
                scanner.Open("192.168.170.2", true);

                scanner.GeneralCommandCallback = OnGeneralCommandResponse;
                scanner.ScanProgramCallback = OnScanProgramCallback;

                Debug.Print("CONFIG");

                scanner.ConfigSync("ScannerGlobalConfig.cfg");

                while (true)
                {
                    //startScanEvent.WaitOne();

                    Debug.Print("COMPILE");
                    // retrieve a compiled program handle
                    var progHandle = scanner.CompileSync(SCAN_PROGRAM);

                    if (_disconnectionFlag)
                        break;

                    Debug.Print("RUN");
                    scanner.Run(progHandle);

                    resultReadyEvent.WaitOne();

                    // scan process is finished
                    if (_shape != null) {
                        _dataProc.Shape = _shape;
                        if (_shape.Type == CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                        {
                            for (int i = 0; i < _shape.SignalInfos.Length; i++)
                            {
                                string fname = _dataProc.SaveAsBCRF(i);
                                Console.WriteLine($"Signal #{i} saved to {fname}");
                            }
                        } else if (_shape.Type == CHRLibPlugin.FSS_PluginDataType.RawData)
                        {
                            var fname = _dataProc.SaveAsCSV();
                            Console.WriteLine($"Raw data written to {fname}");
                        }
                    }

                    Debug.Print("STOP");
                    scanner.Stop();

                    if (_disconnectionFlag) break;
                }

                scanner.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"got exception: {ex.Message}");
            }
        }


        /// <summary>
        /// general callback function to be called once any user command response comes
        /// </summary>
        /// <param name="rsp">command response object</param>
        private void OnGeneralCommandResponse(Response rsp)
        {
            Debug.Print(rsp.ToString());

            if (rsp.IsError())
                Debug.Print($"Error occurred: {rsp.ParamCount}");
        }

        /// <summary>
        /// callback function called after each new shape has been scanned (and at the end of the scanning process)
        /// </summary>
        /// <param name="rsp">response object holding shape data</param>
        private void OnScanProgramCallback(Response rsp)
        {
            try
            {
                Debug.Print($"Scan program callback called {rsp}");

                // Shape data are stored in a blob argument of the response
                if (!(rsp.ParamCount > 0 && rsp.TryGetParam(0, out byte[] blob)))
                    return;

                // extract shape data from the binary blob
                CHRLibPlugin.FSS_PluginShape shape = new CHRLibPlugin.FSS_PluginShape(blob);

                // check whether scan is completed: if not, just store last shape for later processing
                if (shape.Type != CHRLibPlugin.FSS_PluginDataType.RecipeTerminate)
                {
                    _shape = shape; // we expect to scan only one large shape to be scanned
                    return;
                }

                resultReadyEvent.Set();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
