/*
This demo uses the asychronous communication.
The command is sent without waiting for the response.
All the reponses and data are delivered through pre-registered callback functions.
The connection is set to automatically process the output of the CHR device.
*/


using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using CHRocodileLib;

namespace TCHRLibAsyncProcess
{
    public partial class AsyncProcessDemo : Form
    {
        //asychronous connection object
        AsynchronousConnection Conn;

        //for data display
        const int Data_Length = 1024;
        double[,] DataSamples;
        int CurrentDataPos;

        //device parameter setting
        int MeasuringMethod;
        int DataAverage;
        float ScanRate;
        int[] SignalIDs;

        Stopwatch stopwatch;
        private void Init()
        {
            //initialize data display
            DataSamples = new double[Data_Length, 3]; // only shows the first 3 signals of the last 1024 samples
            for (int i = 0; i < Data_Length; i++)
            {
                chart1.Series[0].Points.AddY(i);
                chart2.Series[0].Points.AddY(i);
                chart3.Series[0].Points.AddY(i);
            }
        }

        //General callback function for all the command response
        private void GenCmdCbFct(Response _Rsp)
        {
            try
            {
                //based on the response ID, interprete 
                switch (_Rsp.Info.CmdID)
                {
                    case CmdID.MeasuringMethod:
                        {
                            MeasuringMethod = _Rsp.GetParam<int>(0);
                            this.BeginInvoke((Action)delegate { TBMOD.Text = MeasuringMethod.ToString(); });
                            break;
                        }
                    case CmdID.ScanRate:
                        {
                            ScanRate = _Rsp.GetParam<float>(0);
                            this.BeginInvoke((Action)delegate { TBSHZ.Text = ScanRate.ToString(); });
                            break;
                        }
                    case CmdID.DataAverage:
                        {
                            DataAverage = _Rsp.GetParam<int>(0);
                            this.BeginInvoke((Action)delegate { TBAVD.Text = DataAverage.ToString(); });
                            break;
                        }
                    case CmdID.OutputSignals:
                        {
                            if (_Rsp.ParamCount > 0)
                                SignalIDs = _Rsp.GetParam<int[]>(0);
                            else
                                SignalIDs = new int[0];
                            this.BeginInvoke((Action)delegate { TBSODX.Text = String.Join(",", SignalIDs.Select(p => p.ToString()).ToArray()); });
                            break;
                        }
                }
            }
            catch
            {

            }


            //get response in string form and update response text box
            string strRsp = _Rsp.ToString();            
            this.BeginInvoke((Action<string>)delegate(string _strRsp)
                {
                    RTRsp.AppendText(_strRsp);
                    RTRsp.AppendText(Environment.NewLine);
                }, strRsp);
        }


        public AsyncProcessDemo()
        {
            InitializeComponent();
            Init();
        }


        private void OpenConnection()
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
                //register callback function
                //data callback function: maximum read in 1000 samples every time and maximum wait for 10ms  
                Conn.SetDataCallback(OnData, 1000, 10);
                //general command callback function, which will be called for all the responses/updates from the device
                Conn.SetGeneralResponseCallback(GenCmdCbFct);
                //set connection to automatically process device output, 
                //i.e. let CHRocodileLib to create an internal thread for output processing
                //all the reponses and data are delivered through callback function withing CHRocodileLib internal thread 
                Conn.AutomaticMode = true;
                //Set output signals 
                SetOutputSignals();
                TBCMD.Enabled = true;
                BtnSendCmd.Enabled = true;
                BConnect.Tag = 1;
                BConnect.Text = "Disconnect";
                CurrentDataPos = 0;
                stopwatch = Stopwatch.StartNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseConnection()
        {
            Conn.Close();
            //after close, connection object cannot be used
            Conn = null;
            TBCMD.Enabled = false;
            BtnSendCmd.Enabled = false;

            BConnect.Tag = 0;
            BConnect.Text = "Connect";
        }

        private void BConnect_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(BConnect.Tag) == 0)
            {
                OpenConnection();
            }
            else
            {
                CloseConnection();
            }
        }


        private void SetOutputSignals()
        {
            try
            {
                //Set device output signals
                //Sample counter, peak 1 value, peak 1 quality/intensity
                if (RBCLS.Checked)
                    SignalIDs = new int[] { 83, 16640, 16641 };
                //other devices, float values are ordered
                else
                    SignalIDs = new int[] { 83, 256, 257 };
                Conn.Exec(CmdID.OutputSignals,null, SignalIDs);
            }
            catch
            {
                Debug.Fail("Cannot set output signals");
            }
        }

        //Data sample callback function
        private void OnData(AsyncDataStatus status, Data data)
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
                if (stopwatch.ElapsedMilliseconds>20)
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

                //if user wants to store _oData object for later use, need to detach the _oData when it is full, otherwise _oData will be reused
                /*for example
                 * 
                 * if(_oData.State == DataStatus.UserBufferFull)
                 * _oData.DetachRenew();
                */
            }

            if (data.Status == DataStatus.Error)
            {
                Console.WriteLine("Error in processing device output!");
            }
        }

        private void TBCMD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SendAsyncCmd();
        }

        private void SendAsyncCmd()
        {
            //Asynchronous string command
            Conn.ExecString(TBCMD.Text, null);
        }

        private void AsyncProcessFormClosing(object sender, FormClosingEventArgs e)
        {
            //close connection
            if (Convert.ToInt32(BConnect.Tag) != 0)
                CloseConnection();
        }

        private void BtnSendCmd_Click(object sender, EventArgs e)
        {
            SendAsyncCmd();
        }
    }

    
}
