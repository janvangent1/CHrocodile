/*
This demo shows how to use CLS2 calibration plugin under both synchronous and asynchronous modes.
After opening up the connection, plugin is directly added into the connection based on plugin name. 
Then the calibration file which is used in the plugin is set.
User can also enable and disable the calibration correction through sending command to the plugin.
*/


using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using CHRocodileLib;
using System.Threading;

namespace TCHRLibCLS2CalibPlugin
{
    public partial class MainForm : Form
    {
        SynchronousConnection ConnSync = null;
        SynchronousConnection.Plugin CalibPluginSync = null;

        AsynchronousConnection ConnAsync = null;
        AsynchronousConnection.Plugin CalibPluginAsync = null;


        bool Connected = false;
        //Flag: whether connection is synchronous or asynchronous
        bool isSynConn;
        //Flag: whether calibration correction is active
        bool UseCorrectionEnable;

        const Int32 Data_Length = 1024;
        double[] GlobalData;
        double[] PeakData;
        Int32 CurrentDataPos;

        Stopwatch m_oWatch;
        Int64 m_nSampleCount;

        public MainForm()
        {
            InitializeComponent();

            m_oWatch = new Stopwatch();

            //save one global signal to display in upper chart, x-axis sample index
            GlobalData = new double[Data_Length];
            //first peak data of all the channels to display in the lower chart, x-axis channel index
            PeakData = new double[0];
           

            for (Int32 i = 0; i < Data_Length; i++)
                chartUpper.Series[0].Points.AddY(i);

            //set logging property
            Lib.SetLibLogFileDirectory(".", 500, 20);
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            if (Connected)
                CloseConnection();
            else
            {
                try
                {
                    isSynConn = RBSyncConn.Checked;
                    if (isSynConn)
                    {
                        //Open connection in synchronous mode
                        ConnSync = new SynchronousConnection(TBConnInfo.Text, DeviceType.MultiChannel, 256 * 1024 * 1024);
                        //Add calibration plugin
                        CalibPluginSync = ConnSync.InsertPlugin(CHRLibPlugin.CLS2_Calib_Plugin_Name);
                        // use original data format instead of double to save data size
                        Lib.SetOutputDataFormatMode(ConnSync.Handle, OutputDataFormat.Raw);
                    }
                    else
                    {
                        //Open connection in asynchronous mode
                        ConnAsync = new AsynchronousConnection(TBConnInfo.Text, DeviceType.MultiChannel, 256 * 1024 * 1024);
                        //Add calibration plugin
                        CalibPluginAsync = ConnAsync.InsertPlugin(CHRLibPlugin.CLS2_Calib_Plugin_Name);
                        // use original data format instead of double to save data size
                        Lib.SetOutputDataFormatMode(ConnAsync.Handle, OutputDataFormat.Raw);
                        SetupAsyncConn();
                    }
                       
                    //set up device
                    SetupDevice();
                    //set up plugin
                    SetupPlugin();
                    CurrentDataPos = 0;
                    //timer call GetNextSamples to read data
                    TimerProcess.Enabled = isSynConn;                 
                    Connected = true;
                    m_oWatch.Restart();
                    m_nSampleCount = 0;
                    UpdateGUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message} - closing connection.");
                    CloseConnection();
                }
            }
        }

        private void CloseConnection()
        {
            TimerProcess.Enabled = false;
            if (isSynConn)
            {
                ConnSync?.Close();
                CalibPluginSync = null;
                ConnSync= null;
            }
            else
            {

                ConnAsync?.Close();
                CalibPluginAsync = null;
                ConnAsync = null;
            }
                
                
            Connected = false;
            UpdateGUI();
        }



        private void SetupDevice()
        {
            if (isSynConn)
            {
                ConnSync.Exec(CmdID.OutputSignals, new Int32[] { 83, 16640, 16641 });
                ConnSync.Exec(CmdID.ScanRate, 2000);
            }
            else
            {
                ConnAsync.Exec(CmdID.OutputSignals, null, new Int32[] { 83, 16640, 16641 });
                ConnAsync.Exec(CmdID.ScanRate, null, 2000);
            }
        }


        private void SetupPlugin()
        {
            //Set calibration file
            //Dummy calibration file, should be real calibration file here
            string strFileName = "pixel-calibration-sn-15.pc-csv";//"CalibDummy.csv";
            if (isSynConn)
                CalibPluginSync.Exec((CmdID)CHRLibPlugin.CmdID_CalibFile_Name, strFileName);
            else
                CalibPluginAsync.Exec((CmdID)CHRLibPlugin.CmdID_CalibFile_Name, null, strFileName);
            //By default, calibration correction is always active
            UseCorrectionEnable = true;
            BtPluginEnable.Text = "Disable Correction";
        }

        //according to the response to update GUI interface
        private void UpdateResponse(Response _oRsp)
        {
            if (_oRsp == null)
                return;

            //check whether the response is from calibration plugin
            if (_oRsp.Source == CalibPluginAsync.Handle)
            {
                //plugin response
                //if it is correction active command response, update corresponding GUI element.
                if ((uint)_oRsp.Info.CmdID == CHRLibPlugin.CmdID_Calib_Active)
                {
                    Int32 bActive = _oRsp.GetParam<int>(0);
                    UseCorrectionEnable = (bActive != 0);
                    if (UseCorrectionEnable)
                        BtPluginEnable.Text = "Disable Correction";
                    else
                        BtPluginEnable.Text = "Enable Correction";
                }
            }
            else
            {
                //device response
                if (_oRsp.Info.CmdID == CmdID.OutputSignals)
                {
                    int[] aSigIDs;
                    if (_oRsp.ParamCount > 0)
                        aSigIDs = _oRsp.GetParam<int[]>(0);
                    else
                        aSigIDs = new int[0];
                    TBSODX.Text = String.Join(",", aSigIDs.Select(p => p.ToString()).ToArray());

                }
                else if (_oRsp.Info.CmdID == CmdID.ScanRate)
                {
                    TBSHZ.Text = _oRsp.GetParam<float>(0).ToString();
                }
            }

            var strRsp = _oRsp.ToString();
            if (RTResponse.Lines.Length>0)
                RTResponse.AppendText(Environment.NewLine);
            RTResponse.AppendText(strRsp);
        }

        //command response callback
        private void GenCmdCbFct(Response _Rsp)
        {
            this.BeginInvoke((Action <Response>)delegate (Response _oRsp)
            {
                UpdateResponse(_oRsp);
            }, _Rsp);
        }

        //Data callback
        private void ReceiveSample(AsyncDataStatus _status, Data _oData)
        {
            if (_oData.NumSamples > 0)
            {
                ProcessData(_oData);
                m_nSampleCount += _oData.NumSamples;
                if (_oData.NumSamples > 0)
                {
                    this.BeginInvoke((Action)delegate ()
                    {
                        DisplayData();
                    });
                }
                this.BeginInvoke((Action)delegate ()
                {
                    UpdateDataInfo();
                });
            }

            if (_oData.Status == DataStatus.Error)
            {
                Console.WriteLine("Error in processing device output!");
            }
        }



        //Setup asynchronous callback functins and start device output processing
        private void SetupAsyncConn()
        {
            //register callback function 
            ConnAsync.SetDataCallback(ReceiveSample);
            ConnAsync.SetGeneralResponseCallback(GenCmdCbFct);
            //set CHRocodileLib to automatically process device output, 
            //i.e. let CHRocodileLib to create an internal thread for output processing
            //all the reponses and data are delivered through callback function withing CHRocodileLib internal thread 
            ConnAsync.AutomaticMode= true;
        }

        //needed in synchronous mode to constantly call GetNextSamples to read data
        private void TimerProcess_Tick(object sender, EventArgs e)
        {
            try
            {

                var oData = ConnSync.GetNextSamples();
                ProcessData(oData);
                m_nSampleCount += oData.NumSamples;

                if (oData.NumSamples > 0)
                    DisplayData();
                UpdateDataInfo();

            }
            catch
            {

            }

        }


        //function to process data
        private void ProcessData(Data _oData)
        {
            if (_oData.NumSamples == 0)
                return;
            var itSamples = _oData.Samples();
            foreach (var s in itSamples)
            {
                //Upper chart always displays the first global signal, time based
                if (_oData.Info.SignalGenInfo.GlobalSignalCount > 0)
                {
                    GlobalData[CurrentDataPos] = s.Get(0);
                    CurrentDataPos++;
                    if (CurrentDataPos >= Data_Length)
                        CurrentDataPos = 0;
                }
            }

            
            //lower chart always displays the first peak signal of all the channels of the last sample
            if (_oData.Info.SignalGenInfo.PeakSignalCount > 0)
            {
                if (PeakData.Length != _oData.Info.SignalGenInfo.ChannelCount)
                    PeakData = new double[_oData.Info.SignalGenInfo.ChannelCount];

                var sample = itSamples.Last();
                for (int i = 0; i < _oData.Info.SignalGenInfo.ChannelCount; i++)
                    PeakData[i] = sample.Get(_oData.Info.SignalGenInfo.GlobalSignalCount, i);
            }
        }


        void DisplayData()
        {
            for (Int32 i = 0; i < Data_Length; i++)
                chartUpper.Series[0].Points[i].YValues[0] = GlobalData[i];
            Int32 nOldCount = chartLower.Series[0].Points.Count;
            for (Int32 i = nOldCount; i < PeakData.Length; i++)
                chartLower.Series[0].Points.AddY(0);
            for (Int32 i = nOldCount - 1; i >= PeakData.Length; i--)
                chartLower.Series[0].Points.RemoveAt(i);
            for (Int32 i = 0; i < PeakData.Length; i++)
                chartLower.Series[0].Points[i].YValues[0] = PeakData[i];
            chartUpper.ChartAreas[0].RecalculateAxesScale();
            chartUpper.Invalidate();
            chartLower.ChartAreas[0].RecalculateAxesScale();
            chartLower.Invalidate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseConnection();
        }

        //execute string command to device
        private void TBCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;

            string strCmd = TBCommand.Text;
            try
            {

                if (isSynConn)
                {
                    var oRsp = ConnSync.ExecString(strCmd);
                    UpdateResponse(oRsp);
                }
                else
                {
                    ConnAsync.ExecString(strCmd);
                }
            }
            catch
            {

            }
        }

        private void TBSODX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;

            try
            {
                char[] delimiters = new char[] { ' ', ',', ';'};
                var parts = TBSODX.Text.Split(delimiters).
                  Select(part => part.Trim()).
                  Where(part => !string.IsNullOrEmpty(part));
                int[] aSigs = Array.ConvertAll<string, int>(parts.ToArray<string>(), int.Parse);
                if (isSynConn)
                    ConnSync.Exec(CmdID.OutputSignals, aSigs);
                else
                    ConnAsync.Exec(CmdID.OutputSignals, null, aSigs);
            }
            catch
            {

            }
        }

        private void TBSHZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return)
                return;

            try
            {
                float nSHZ = float.Parse(TBSHZ.Text);
                if (isSynConn)
                    ConnSync.Exec(CmdID.ScanRate, nSHZ);
                else
                    ConnAsync.Exec(CmdID.ScanRate, null, nSHZ);
            }
            catch
            {
            }
        }


        private void UpdateGUI()
        {
            if (Connected)
            {
                BtConnect.Text = "Disconnect";
                BtConnect.BackColor = Color.LightGreen;
            }
            else
            {
                BtConnect.Text = "Connect";
                BtConnect.BackColor = SystemColors.Control;
            }
            TBConnInfo.Enabled = !Connected;
            RBSyncConn.Enabled = !Connected;
            RBAsyncConn.Enabled = !Connected;
            BtPluginEnable.Enabled = Connected;

        }


        private void UpdateDataInfo()
        {
            if (m_oWatch.ElapsedMilliseconds>1000)
            {
                LSampleNo.Text = m_nSampleCount.ToString();
                m_nSampleCount = 0;
                m_oWatch.Restart();
            }
        }

        //enable or disable calibration correction, send CmdID_Calib_Active command to calibration plugin
        private void BtPluginEnable_Click(object sender, EventArgs e)
        {
            try
            {
                //Set calibration file
                Int32 bActive = UseCorrectionEnable ? 0 : 1;
                if (isSynConn)
                    CalibPluginSync.Exec((CmdID)CHRLibPlugin.CmdID_Calib_Active, bActive);
                else
                    CalibPluginAsync.Exec((CmdID)CHRLibPlugin.CmdID_Calib_Active, null, bActive);
            }
            catch 
            { 
            }

        }
    }
}
