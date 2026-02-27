/*
This demo shows how to use the recording mode of the synchronous connection to collect data
When the connection is in the recording modes, all the samples will be saved in a buffer. This buffer can be retrieved upon the calling of "StopRecording".
During recording, no commands can be executed. "GetNextSamples" will return newly recorded data since the last call of "GetNextSamples".
*/


using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using CHRocodileLib;

namespace TCHRLibBasicRecordSample
{
    public partial class TRecordSample : Form
    {
        CHRocodileLib.SynchronousConnection Conn;

        //record 1000 samples
        int SampleCount;

        MeasurementMode MeasuringMethod = MeasurementMode.Confocal;
        int[] SignalIDs;
        float ScanRate;

        int CurrentDataPos;
        CHRocodileLib.Data RecordData = null;


        public TRecordSample()
        {
            InitializeComponent();
        }



        private void BtConnect_Click(object sender, EventArgs e)
        {
            bool bConnect = false;
            //connect to device
            if (sender == BtConnect)
            {
                try
                {
                    var DeviceType = CHRocodileLib.DeviceType.Chr1;
                    if (RBCHR2.Checked)
                        DeviceType = CHRocodileLib.DeviceType.Chr2;
                    else if (RBCLS.Checked)
                        DeviceType = CHRocodileLib.DeviceType.MultiChannel;
                    else if (RBCHRC.Checked)
                        DeviceType = CHRocodileLib.DeviceType.ChrCMini;
                    string strConInfo = TbConInfo.Text;
                    Conn = new CHRocodileLib.SynchronousConnection(strConInfo, DeviceType);
                    //set up device
                    SetupDevice();
                    bConnect = true;
                    labelRecordingHint.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            //close connection to device
            else
            {
                StopRecording();
                Conn.Close();
                Conn = null;
            }
            EnableGui(bConnect);

        }


        private void SetupDevice()
        {
            //default signals are: Sample counter, peak 1 value, peak 1 quality/intensity
            //signal definition for CLS device, only 16bit integer signal for peak signal
            if (RBCLS.Checked)
                SignalIDs = new int[] { 83, 16640, 16641 };
            //other devices, float values are ordered
            else
                SignalIDs = new int[] { 83, 256, 257 };
            //Update TextBox
            TBSODX.Text = String.Join(",", SignalIDs.Select(p => p.ToString()).ToArray());
            ScanRate = 4000;
            //CLS device, normally maximum scan rate ist 2000
            //ScanRate = 2000;
            TBSHZ.Text = ScanRate.ToString();
            SetUpMeasuringMethod();
            SetUpScanrate();
            SetUpOutputSignals();
        }

        private void SetUpMeasuringMethod()
        {
            try
            {
                MeasurementMode nMMD = MeasurementMode.Confocal;
                if (RBInterfero.Checked)
                    nMMD = MeasurementMode.Interferometric;
                var oRsp = Conn.Exec(CHRocodileLib.CmdID.MeasuringMethod, nMMD);
                MeasuringMethod = (MeasurementMode)oRsp.GetParam<int>(0);
            }
            catch
            {
                Debug.Fail("Cannot set measuring method");
            }
            if (MeasuringMethod == MeasurementMode.Confocal)
                RBConfocal.Checked = true;
            else
                RBInterfero.Checked = true;
        }

        private void SetUpOutputSignals()
        {
            try
            {
                //Set device output signals
                char[] delimiters = new char[] { ' ', ',', ';' };
                int[] signals = TBSODX.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).
                    Select(int.Parse).ToArray();
                var oRsp = Conn.Exec(CHRocodileLib.CmdID.OutputSignals, signals);
                SignalIDs = oRsp.GetParam<int[]>(0);
            }
            catch
            {
                Debug.Fail("Cannot set output signals");
            }
            TBSODX.Text = String.Join(",", SignalIDs.Select(p => p.ToString()).ToArray());
        }

        private void SetUpScanrate()
        {
            try
            {
                float nSHZ = float.Parse(TBSHZ.Text);
                var oRsp = Conn.Exec(CHRocodileLib.CmdID.ScanRate, nSHZ);
                ScanRate = oRsp.GetParam<float>(0);
            }
            catch
            {
                Debug.Fail("Cannot set scan rate");
            }
            TBSHZ.Text = ScanRate.ToString();
        }


        private void EnableGui(bool _bEnabled)
        {
            BtConnect.Enabled = !_bEnabled;
            BtDisCon.Enabled = _bEnabled;
            BtRecord.Enabled = _bEnabled;
            EnableSetting(_bEnabled);
        }

        private void EnableSetting(bool _bEnabled)
        {
            RBConfocal.Enabled = _bEnabled && (RBCHR1.Checked || RBCHR2.Checked);
            RBInterfero.Enabled = _bEnabled && (RBCHR1.Checked || RBCHR2.Checked);
            TBSHZ.Enabled = _bEnabled;
            TBSODX.Enabled = _bEnabled;
        }

        private void RBConfocal_Click(object sender, EventArgs e)
        {
            SetUpMeasuringMethod();
        }

        private void TBSHZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
                SetUpScanrate();
        }

        private void TBSODX_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
                SetUpOutputSignals();
        }

        private void StartRecording()
        {
            labelRecordingHint.Visible = false;
            //throw away the old data
            if (CBFlush.Checked)
                Conn.FlushConnectionBuffer();
            //recording sample count
            SampleCount = int.Parse(TBSampleCount.Text);
            //start recording, enter recording modes
            Conn.StartRecording(SampleCount);
            initDataChart();
            CurrentDataPos = 0;
            timerData.Enabled= true;
            EnableSetting(false);
            BtRecord.Text = "Stop Recording";
            BtRecord.Tag = 1;
            BtSave.Enabled = false;
        }


        private void initDataChart()
        {
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();
            for (int i = 0; i < SampleCount; i++)
            {
                chart1.Series[0].Points.AddY(0);
                chart2.Series[0].Points.AddY(0);
                chart3.Series[0].Points.AddY(0);
            }
        }


        private void StopRecording() 
        {
            timerData.Enabled = false;
            //stop recording, get recorded data buffer/object
            RecordData = Conn.StopRecording();

            EnableSetting(true);
            BtRecord.Text = "Start Recording";
            BtRecord.Tag = 0;
            BtSave.Enabled = true;
        }

        private void timerData_Tick(object sender, EventArgs e)
        {
            //read in newly recorded samples and display
            var oData = Conn.GetNextSamples();
            if (oData.NumSamples > 0)
            {
                double[] aData = new double[3];
                foreach (var s in oData.Samples())
                {

                    if (CurrentDataPos >= SampleCount)
                        break;
                    //only read in and show the first 3 signals
                    for (int i = 0; i < 3; i++)
                    {
                        aData[i] = 0;
                        if (oData.Info.SignalGenInfo.GlobalSignalCount > i)
                            aData[i] = s.Get(i);
                        //if not enough global signal, show peak signal. for peak signal, only shows the value for the first channel
                        else if (oData.Info.SignalGenInfo.GlobalSignalCount + oData.Info.SignalGenInfo.PeakSignalCount > i)
                            aData[i] = s.Get(i, 0);                                        
                    }
                    chart1.Series[0].Points[CurrentDataPos].YValues[0] = Double.IsNaN(aData[0]) ? 0 : aData[0];
                    chart2.Series[0].Points[CurrentDataPos].YValues[0] = Double.IsNaN(aData[1]) ? 0 : aData[1];
                    chart3.Series[0].Points[CurrentDataPos].YValues[0] = Double.IsNaN(aData[2]) ? 0 : aData[2];
                    CurrentDataPos++;                   
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.Invalidate();
                chart2.ChartAreas[0].RecalculateAxesScale();
                chart2.Invalidate();
                chart3.ChartAreas[0].RecalculateAxesScale();
                chart3.Invalidate();
            }
            //enough samples have been acquired, stop recording
            if (CurrentDataPos >= SampleCount)
                StopRecording();

        }

        private void BtRecord_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32((sender as Button).Tag) == 0)
                StartRecording();
            else
                StopRecording();
        }

        //here save the recorded data into a file 
        private void BtSave_Click(object sender, EventArgs e)
        {
            if (SaveDlg.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(SaveDlg.OpenFile());
                var nSigCount = RecordData.Info.SignalGenInfo.GlobalSignalCount 
                    + RecordData.Info.SignalGenInfo.PeakSignalCount;

                //reread all the samples, save...
                RecordData.Rewind();
                foreach (var s in RecordData.Samples())
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j=0; j<nSigCount; j++)
                    {
                        if (j < RecordData.Info.SignalGenInfo.GlobalSignalCount)
                            sb.Append(s.Get(j)+", ");
                        else
                        {
                            for (int k=0; k< RecordData.Info.SignalGenInfo.ChannelCount; k++)
                                sb.Append(s.Get(j, k) + ", ");
                        }
                    }
                    writer.WriteLine(sb.ToString());
                }
                writer.Dispose();
            }
        }
    }
}
