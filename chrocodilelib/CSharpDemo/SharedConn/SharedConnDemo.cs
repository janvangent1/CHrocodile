/*
In this demo, two shared vitual connections are opened to the same CHR device.
First connection (ConnAsync) is reponsible for sending commands, processing reponses and data. 
This connection is working under asynchronous mode, i.e data and command reponses are delivered through callback functions.
Second connection (ConnSync) synchronously downloads and display spectrum.
Both connections work independent of each other.
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CHRocodileLib;

namespace TCHRLibSharedConn
{
    public partial class SharedConn : Form
    {
        int SampleCount;

        int MeasuringMethod;
        int DataAverage;
        float ScanRate;
        int[] SignalIDs;
        const int Spec_Length = 1024;

        AsynchronousConnection ConnAsync;
        SynchronousConnection ConnSync;

        private void Init()
        {
            //initialize specturm display
            for (int i = 0; i < Spec_Length; i++)
                chart1.Series[0].Points.AddY(0);
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
                // TODO
            }

            if (_Rsp.Info.CmdID != CmdID.DownloadSpectrum)
            {
                //get response in string form and update response text box
                string strRsp = (_Rsp.IsUpdate()? "(U) " : "    ") + _Rsp.ToString();
                this.BeginInvoke((Action<string>)delegate (string _strRsp)
                {
                    RTRsp.AppendText(_strRsp);
                    RTRsp.AppendText(Environment.NewLine);
                }, strRsp);
            }
        }


        public SharedConn()
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
                ConnAsync = new AsynchronousConnection(strConInfo, devType);
                //register callback function
                //data callback function: maximum read in 1000 samples every time and maximum wait for 10ms  
                ConnAsync.SetDataCallback(OnData, 1000, 10);
                //general command callback function, which will be called for all the responses/updates from the device
                ConnAsync.SetGeneralResponseCallback(GenCmdCbFct);
                //set connection to automatically process device output, 
                //i.e. let CHRocodileLib to create an internal thread for output processing
                //all the reponses and data are delivered through callback function withing CHRocodileLib internal thread 
                ConnAsync.AutomaticMode = true;
                //Set output signals 
                SetOutputSignals();

                //Open up the shared connection in synchronous mode based on the first connection
                //The shared connection is responsible for synchronous spectrum downloading
                ConnSync = new SynchronousConnection(ConnAsync);

                //Start to download spectrum.
                timer1.Enabled = true;

                RTSample.Clear();
                SampleCount = 0;
                
                TBCMD.Enabled = true;
                BtnSendCmd.Enabled = true;
                BConnect.Tag = 1;
                BConnect.Text = "Disconnect";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseConnection()
        {
            timer1.Enabled = false;
            ConnAsync.Close();
            ConnAsync = null;
            ConnSync.Close();
            ConnSync = null;
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
                ConnAsync.Exec(CmdID.OutputSignals, null, SignalIDs);
            }
            catch
            {
                Debug.Fail("Cannot set output signals");
            }
        }

        //Data callback function
        private void OnData(AsyncDataStatus status, Data _oData)
        {

            if (_oData.NumSamples > 0)
            {
                List<string> dataList = new List<string>();
                //display every 100th sample 
                foreach (var s in _oData.Samples())
                {
                    if (SampleCount % 100 == 0)
                    {
                        string strTemp = "";
                        for (int j = 0; j < _oData.Info.SignalGenInfo.GlobalSignalCount; j++)
                        {
                            double nTemp = s.Get(j);
                            if (double.IsNaN(nTemp))
                                strTemp += "Nan ";
                            else
                                strTemp += nTemp.ToString() + " ";
                        }
                        for (int k = 0; k < _oData.Info.SignalGenInfo.ChannelCount; k++)
                            for (int j = 0; j < _oData.Info.SignalGenInfo.PeakSignalCount; j++)
                            {
                                double nTemp = s.Get(j+ _oData.Info.SignalGenInfo.GlobalSignalCount, k);
                                if (double.IsNaN(nTemp))
                                    strTemp += "Nan ";
                                else
                                    strTemp += nTemp.ToString() + " ";
                            }
                        dataList.Add(strTemp);
                    }
                    SampleCount++;
                }

                this.BeginInvoke((Action<List<string>, Int64>)delegate (List<string> _DataList, Int64 _nSampleCount)
                {
                    foreach (var str in _DataList)
                    {
                        RTSample.AppendText(str);
                        RTSample.AppendText(Environment.NewLine);
                    }
                    TBSampleNumber.Text = _nSampleCount.ToString();
                }, dataList, _oData.NumSamples);
            }

            if (_oData.Status == DataStatus.Error)
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
            //Send string command without waiting for response, the response is delivered in the callback function upon arrival
            ConnAsync.ExecString(TBCMD.Text, null);
        }

        private void SharedConn_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close connection
            if (Convert.ToInt32(BConnect.Tag) != 0)
                CloseConnection();
        }

        private void BtnSendCmd_Click(object sender, EventArgs e)
        {
            SendAsyncCmd();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Synchronously download spectrum.
            try
            {
                var specType = SpecType.Raw;
                if (RBConfocal.Checked)
                    specType = SpecType.Confocal;
                else if (RBFFT.Checked)
                    specType = SpecType.FT;

                Response oRsp;
                //for downloading spectra of several channles from multi-channel device,  needs to add start channel index and channel count
                //here only download the specturm of the first channel
                if (RBCLS.Checked)
                    oRsp = ConnSync.Exec(CmdID.DownloadSpectrum, specType, 0, 1);
                else
                    oRsp = ConnSync.Exec(CmdID.DownloadSpectrum, specType);
                //the last parameter of the response is the spectrum data
                var aBytes = oRsp.GetParam<byte[]>(oRsp.ParamCount - 1);
                //convert to 16bit data
                Int16[] SpecData = new Int16[aBytes.Length / 2];
                Buffer.BlockCopy(aBytes, 0, SpecData, 0, aBytes.Length);

                for (int i = 0; i < SpecData.Length; i++)
                    chart1.Series[0].Points[i].YValues[0] = SpecData[i];
                // TODO: check why!?
                //for (int i = SpecData.Length / 2; i < 1024; i++)
                //    chart1.Series[0].Points[i].YValues[0] = 0;
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.Invalidate();
            }
            catch
            {
                Debug.Fail("Cannot set download spectrum");
                timer1.Enabled = false;
            }
        }

        private void BClear_Click(object sender, EventArgs e)
        {
            RTSample.Clear();
        }
    }

    
}
