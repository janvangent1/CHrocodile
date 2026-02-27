/*
This demo uses the asychronous communication.
However during device setup, a special await function is applied to execute command. 
This await function is used to wait for the command response for asynchronous connection, which has the same effect as sychronous command execution
This function provides the possiblity of pipelining command operation.
*/


using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using CHRocodileLib;
using System.Threading.Tasks;

namespace TCHRLibAwaitProcess
{
    public partial class AwaitProcessDemo : Form
    {
        //asychronous connection object
        AsynchronousConnection Conn;

        //for data display
        const int Data_Length = 1024;
        double[,] DataSamples;
        int CurrentDataPos;
        Stopwatch stopwatch;


        //data callback function
        AsynchronousConnection.DataCallback SampleCB;

        private void Init()
        {
            //create callback function
            SampleCB = new AsynchronousConnection.DataCallback(ReceiveSample);

            //initialize data display
            DataSamples = new double[Data_Length, 3]; // only shows the first 3 signals of the last 1024 samples
            for (int i = 0; i < Data_Length; i++)
            {
                chart1.Series[0].Points.AddY(i);
                chart2.Series[0].Points.AddY(i);
                chart3.Series[0].Points.AddY(i);
            }
        }


        public AwaitProcessDemo()
        {
            InitializeComponent();
            Init();
        }


        private async Task OpenConnection()
        {
            try
            {
                var devType = DeviceType.Chr1;
                if (RBCHR2.Checked)
                    devType = DeviceType.Chr2;
                else if (RBCLS.Checked)
                    devType = DeviceType.MultiChannel;
                else if (RBCHRC.Checked)
                    devType = DeviceType.ChrCMini;
                string strConInfo = TBConInfo.Text;
                //Open connection in asynchoronous mode
                Conn = new AsynchronousConnection(strConInfo, devType);
                //set connection to automatically process device output, 
                Conn.AutomaticMode = true;         
                //set up device
                await SetupDevice();               
                BConnect.Tag = 1;
                BConnect.Text = "Disconnect";

            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}");
            }
        }

        private void CloseConnection()
        {
            Conn.Close();
            //after close, connection object cannot be used
            Conn = null;
            BConnect.Tag = 0;
            BConnect.Text = "Connect";
        }

        private async void BConnect_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(BConnect.Tag) == 0)
            {
                await OpenConnection();
            }
            else
            {
                CloseConnection();
            }
        }


        private async Task SetupDevice()
        {
            /*
             * use await to suspend until the response has arrived.
             * here all the settings are strictly sequential, the next setting
             * is started only after the previous one is finished 
             */

            // first stop data stream
            await Conn.ExecAsync(CmdID.StopDataStream);

            // after stop data stream, set measuring method to confocal measurment
            // ExecAsync returns the waited response
            var oRsp = await Conn.ExecAsync(CmdID.MeasuringMethod, MeasurementMode.Confocal);
            TBMMD.Text = oRsp.GetParam<int>(0).ToString();

            // set output signals
            int[] signals;
            // CLS device only outputs 16 bit integer data
            if (RBCLS.Checked)
                signals = new int[] { 83, 16640, 16641 };
            else
                signals = new int[] { 83, 256, 257 };
            oRsp = await Conn.ExecAsync(CmdID.OutputSignals, signals);
            signals = oRsp.GetParam<int[]>(0);
            TBSODX.Text = String.Join(",", signals.Select(p => p.ToString()).ToArray());

            //set scan rate
            float nScanRate = 4000;
            //CLS device, normally maximum scan rate ist 2000
            if (RBCLS.Checked)
                nScanRate = 2000;                
            oRsp = await Conn.ExecAsync(CmdID.ScanRate, nScanRate);
            TBSHZ.Text = oRsp.GetParam<float>(0).ToString();

            //then set data average to 1
            oRsp = await Conn.ExecAsync(CmdID.DataAverage, 1);
            TBAVD.Text = oRsp.GetParam<int>(0).ToString();

            //setting is finished, register data callback function: maximum read in 1000 samples every time and maximum wait for 10ms  
            Conn.SetDataCallback(SampleCB, 1000, 10);
            stopwatch = Stopwatch.StartNew();

            //after everything is set, start data stream
            await Conn.ExecAsync(CmdID.StartDataStream);
            Console.WriteLine("Device setup is finished!");
        }

        //Data sample callback function
        private void ReceiveSample(AsyncDataStatus status, Data data)
        {
            if (data.NumSamples > 0)
            {
                foreach (var s in data.Samples())
                {
                    //only display the first three signals
                    for (int i = 0; i < 3; i++)
                    {
                        DataSamples[CurrentDataPos, i] = 0;
                        //first show global signal
                        if (data.Info.SignalGenInfo.GlobalSignalCount > i)
                            DataSamples[CurrentDataPos, i] = s.Get(i);
                        //if not enough global signal, show peak signal. for peak signal, only shows the value for the first channel
                        else if (data.Info.SignalGenInfo.GlobalSignalCount + data.Info.SignalGenInfo.PeakSignalCount > i)
                            DataSamples[CurrentDataPos, i] = s.Get(i, 0);
                    }
                    CurrentDataPos++;
                    if (CurrentDataPos >= Data_Length)
                        CurrentDataPos = 0;
                }
                //every 20 miliseconds to update data display
                if (stopwatch.ElapsedMilliseconds > 20)
                {
                    this.BeginInvoke((Action)delegate
                    {
                        for (int i = 0; i < Data_Length; i++)
                        {
                            chart1.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 0]) ? 0 : DataSamples[i, 0];
                            chart2.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 1]) ? 0 : DataSamples[i, 1];
                            chart3.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 2]) ? 0 : DataSamples[i, 2];
                        }

                        chart1.ChartAreas[0].RecalculateAxesScale();
                        chart1.Invalidate();
                        chart2.ChartAreas[0].RecalculateAxesScale();
                        chart2.Invalidate();
                        chart3.ChartAreas[0].RecalculateAxesScale();
                        chart3.Invalidate();
                    });
                    stopwatch.Restart();
                }
            }

            if (status == AsyncDataStatus.Error)
            {
                Console.WriteLine("Error in processing device output!");
            }
        }


        private void AwaitProcess_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close connection
            if (Convert.ToInt32(BConnect.Tag) != 0)
                CloseConnection();
        }
    }

    
}
