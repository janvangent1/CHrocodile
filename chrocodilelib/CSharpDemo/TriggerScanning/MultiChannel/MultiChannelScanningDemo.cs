/*
This demo demonstrates how to collect scanning data under trigger each mode with multi-channel device like CLS.
User can select between sync-in signal trigger and encoder trigger. 
The trigger settings are sent to the device synchronously.
After all the trigger configuration has been properly set, this demo uses the recording mode of the connection to collect data.
The data of the selected global signal is shown in the chart.
The data of the selected peak signal is show as a heatmap (horizontal direction is different channels, vertical direction is collected samples).
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CHRocodileLib;


namespace TCHRLibMultiChannelScanning
{
    public partial class MultiChannelScanningDemo : Form
    {
        private CHRocodileLib.SynchronousConnection Conn;

        private int ChannelCount;
        private int[] OutputSignals;

        private CHRocodileLib.Data ScanData = null;

        private int SampleNo;
        private int SampleIdx;

        private bool InProcess;
        private Bitmap bm;
        private int FirstXPixel;
        private int CurrentPixelX, PixelXStep;
        private int CurrentPixelY, PixelYStep;
        private int PeakDrawSampleIdx, GlobalDrawSampleIdx;
        private double SigMin, SigMax;

        public MultiChannelScanningDemo()
        {
            InitializeComponent();
            //default encoder trigger axis
            CBAxis.SelectedIndex = 0;
            //bit map to display peak signals
            bm = new Bitmap(PPaint.Width, PPaint.Height);
            CleanDataBitmap();
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            bool bConnect = false;
            //connect to device
            if (sender == BtConnect)
            {
                try
                {
                    //Open connection in synchronous mode
                    Conn = new CHRocodileLib.SynchronousConnection(TbConInfo.Text, CHRocodileLib.DeviceType.MultiChannel);
                    //multi-channel device use raw data to save space
                    Conn.SetOutputDataFormatMode(OutputDataFormat.Raw);
                    //Get device channel count
                    ChannelCount = CHRocodileLib.Lib.GetDeviceChannelCount(Conn.Handle);
                    SetupDevice();
                    ScanData = null;
                    bConnect = true;
                }
                catch (Exception _e)
                {
                    MessageBox.Show("Error in connecting to the CHR device: " + _e.Message);
                }
            }
            //close connection to device
            else
            {
                //first stop scan
                StopScan();
                while (InProcess)
                    Task.Delay(20);
                //close connection
                Conn.Close();
                Conn = null;
            }
            EnableGui(bConnect, false);
        }


        private void SetupDevice()
        {
            SetDeviceOutput();
            GetScanRate();
        }


        //Set output signals
        private void SetDeviceOutput()
        {
            if (TBSignal.Text == "")
                throw new Exception("No signals are selected for CHR device!");
            char[] delimiters = new char[] { ' ', ',', ';' };

            string[] aTemp = TBSignal.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            int[] aTempSig = Array.ConvertAll(aTemp, int.Parse);
            Array.Sort(aTempSig);

            //there should be peak signals, for CLS device, only 16bit integer signals are supported
            bool bPeakSig = false;
            foreach (var nID in aTempSig)
            {
                if (!CHRocodileLib.DataInfo.IsGlobalSignal((UInt16)nID))
                {
                    bPeakSig = true;
                    break;
                }
            }
            if (!bPeakSig)
                throw new Exception("No peak signals are selected for CHR device!");

            //set device output signals
            var oRsp = Conn.Exec(CHRocodileLib.CmdID.OutputSignals, aTempSig);
            OutputSignals = oRsp.GetParam<int[]>(0);

            //update display signal combobox
            List<int> aGlobalSig = new List<int>();
            List<int> aPeakSig = new List<int>();

            foreach (var nSigID in OutputSignals)
            {
                if (CHRocodileLib.DataInfo.IsGlobalSignal((UInt16)nSigID))
                    aGlobalSig.Add(nSigID);
                else
                    aPeakSig.Add(nSigID);
            }

            int nGlobalSelectIdx = -1;
            if (CBGlobalSig.SelectedIndex >= 0)
            {
                int nLastSig = int.Parse(CBGlobalSig.SelectedItem.ToString());
                nGlobalSelectIdx = aGlobalSig.IndexOf(nLastSig);
            }
            int nPeakSelectIdx = -1;
            if (CBPeakSig.SelectedIndex >= 0)
            {
                int nLastSig = int.Parse(CBPeakSig.SelectedItem.ToString());
                nPeakSelectIdx = aPeakSig.IndexOf(nLastSig);
            }

            CBGlobalSig.Items.Clear();
            foreach (var nID in aGlobalSig)
                CBGlobalSig.Items.Add(nID.ToString());

            CBPeakSig.Items.Clear();
            foreach (var nID in aPeakSig)
                CBPeakSig.Items.Add(nID.ToString());

            if (nGlobalSelectIdx > -1)
                CBGlobalSig.SelectedIndex = nGlobalSelectIdx;
            else
                CBGlobalSig.SelectedIndex = 0;

            if (nPeakSelectIdx > -1)
                CBPeakSig.SelectedIndex = nPeakSelectIdx;
            else
                CBPeakSig.SelectedIndex = 0;
        }

        //Query device scan rate
        private void GetScanRate()
        {
            try
            {
                var oSHZRsp = Conn.Query(CHRocodileLib.CmdID.ScanRate);
                TBSHZ.Text = oSHZRsp.GetParam<float>(0).ToString();
            }
            catch
            {
                Debug.Fail("Cannot get scan rate.");
            }
            
        }

        //Set up scan triggering
        private void SendTriggerSetting()
        {
            if (RBSyncSig.Checked)
            {
                //Use sync-in signal to trigger
                //disabel encoder trigger
                Conn.Exec(CHRocodileLib.CmdID.EncoderTriggerEnabled, 0);
            }
            else
            {
                //use encoder to trigger
                Conn.Exec(CHRocodileLib.CmdID.EncoderTriggerEnabled, 1);
                //set encoder trigger property
                int nAxis = CBAxis.SelectedIndex;
                int nStartPos = int.Parse(TBStartPos.Text);
                int nStopPos = int.Parse(TBStopPos.Text);
                float nInterval = float.Parse(TBInterval.Text, CultureInfo.InvariantCulture);
                int bTriggerOnReturn = CBTriggerOnReturn.Checked ? 1 : 0;
                Conn.Exec(CHRocodileLib.CmdID.EncoderTriggerProperty, nAxis, nStartPos, nStopPos, nInterval, bTriggerOnReturn);
                TBSampleNo.Text = ((int)((nStopPos - nStartPos) / nInterval + 1)).ToString();
            }
        }


        


        private void EnableGui(bool _bConnect, bool _bInScan)
        {
            BtConnect.Enabled = !_bConnect;
            BtDisCon.Enabled = _bConnect;
            BtScan.Enabled = _bConnect && (!_bInScan);
            BtStopScan.Enabled = _bConnect && _bInScan;
            TBSignal.Enabled = _bConnect && (!_bInScan);
            TBSHZ.Enabled = _bConnect && (!_bInScan);
            BtCTN.Enabled = _bConnect && (!_bInScan);
            BtEncoderPos.Enabled = _bConnect && RBEncTrigger.Checked && (!_bInScan);
        } 

        //manually set current encoder counter position
        private void BtEncoderPos_Click(object sender, EventArgs e)
        {
            try
            {
                //Set encoder current position
                int nPos = int.Parse(TBEncoderPos.Text);
                Conn.Exec(CHRocodileLib.CmdID.EncoderCounter, CBAxis.SelectedIndex, nPos);
            }
            catch
            {
                Debug.Fail("Cannot set encoder pos.");
            }
        }

        //change device output 
        private void TBSignal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                SetDeviceOutput();
            }
        }

        //select encoder or signal triggering
        private void RBSyncSig_Click(object sender, EventArgs e)
        {
            bool bEncTrigger = (sender == RBEncTrigger);
            CBAxis.Enabled = bEncTrigger;
            TBStartPos.Enabled = bEncTrigger;
            TBStopPos.Enabled = bEncTrigger;
            TBInterval.Enabled = bEncTrigger;
            CBTriggerOnReturn.Enabled = bEncTrigger;
            BtEncoderPos.Enabled = bEncTrigger;
            TBEncoderPos.Enabled = bEncTrigger;
            TBSampleNo.Enabled = !bEncTrigger;
        }


        //Start scam
        private void BtScan_Click(object sender, EventArgs e)
        {
            try
            {
                SampleNo = int.Parse(TBSampleNo.Text);
                if (SampleNo == 0)
                    return;

                //Set trigger settings
                SendTriggerSetting();
                //use trigger each mode
                Conn.Exec(CHRocodileLib.CmdID.DeviceTriggerMode, (int)CHRocodileLib.TriggerMode.TriggerEach);

                //start recording modes
                Conn.StartRecording(SampleNo);
                //reset record data
                ScanData = null;

                SampleIdx = 0;

                SigMin = double.Parse(TBSigMin.Text);
                SigMax = double.Parse(TBSigMax.Text);

                InProcess = false;
                ResetDrawing();

                //Start to process data
                timerProcess.Enabled = true;
                EnableGui(true, true);
                PPaint.Invalidate();
            }
            catch
            {
                Debug.Fail("Cannot set scan related parameters.");
            }
        }


        //Process data saved into m_oScanData
        private void timerProcess_Tick(object sender, EventArgs e)
        {
            if (InProcess)
                return;

            InProcess = true;

            //Read Data
            ScanData = Conn.GetNextSamples();

            //get total number of recorded data, TotalNumSamples returns the total recorded sample count
            SampleIdx = (int)ScanData.TotalNumSamples;

            //if there is new data comes in, update display
            //NumSamples returns the new sample count from the last call of GetNextSampels
            if (ScanData.NumSamples > 0)
            {
                UpdateGlobalDataDisplay(false);
                UpdatePeakDataDisplay(false);
            }

            //enough samples have been saved, stop scan
            if (SampleIdx == SampleNo)
                StopScan();

            InProcess = false;
        }  

        private void StopScan()
        {
            if (!timerProcess.Enabled)
                return;

            timerProcess.Enabled = false;

            //quit recording modes
            Conn.StopRecording();

            EnableGui(true, false);
            BtScan.Text = "Start Scan";
            UpdateGlobalDataDisplay(false);
            UpdatePeakDataDisplay(false);
        }


        private void ResetDrawing()
        {
            //reset peak heatmap
            ResetBitMapDrawing();

            //reset global signal chart
            ResetChartSeries();
        }

        private void ResetBitMapDrawing()
        {
            CleanDataBitmap();
            int nTemp = PPaint.Height % SampleNo;
            CurrentPixelY = nTemp / 2;
            PixelYStep = PPaint.Height / SampleNo;
            if (ChannelCount > PPaint.Width)
            {
                FirstXPixel = 0;
                PixelXStep = 1;
            }
            else
            {
                nTemp = PPaint.Width % ChannelCount;
                FirstXPixel = nTemp / 2;
                PixelXStep = PPaint.Width / ChannelCount;
            }
            CurrentPixelX = FirstXPixel;
            PeakDrawSampleIdx = 0;
        }


        private void ResetChartSeries()
        {
            int nDataCount = chart1.Series[0].Points.Count;
            for (int i = nDataCount; i < SampleNo; i++)
                chart1.Series[0].Points.AddY(i);
            for (int i = nDataCount - 1; i >= SampleNo; i--)
                chart1.Series[0].Points.RemoveAt(i);
            for (int i = 0; i < chart1.Series[0].Points.Count; i++)
                chart1.Series[0].Points[i].YValues[0] = 0;
            GlobalDrawSampleIdx = 0;
            chart1.Invalidate();
        }

        //update encoder trigger related perperty
        private void TBEncoderTriggerPP_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                int nStartPos = int.Parse(TBStartPos.Text);
                int nStopPos = int.Parse(TBStopPos.Text);
                float nInterval = float.Parse(TBInterval.Text, CultureInfo.InvariantCulture);
                TBSampleNo.Text = ((int)((nStopPos - nStartPos) / nInterval + 1)).ToString();
            }
        }

        //stop scanning
        private void BtStopScan_Click(object sender, EventArgs e)
        {
            StopScan();
        }

        //Set Device back to free-run mode
        private void BtCTN_Click(object sender, EventArgs e)
        {
            try
            {
                // Set back to free run mode
                Conn.Exec(CHRocodileLib.CmdID.DeviceTriggerMode, CHRocodileLib.TriggerMode.FreeRun);
            }
            catch
            {
                Debug.Fail("Cannot set device to free run mode");
            }
        }


        //change peak signal to be displayed
        private void CBDisplaySig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SampleNo == 0)
                return;

            while (InProcess)
                Task.Delay(20);

            UpdatePeakDataDisplay(true);
        }
        
        //change global signal to be displayed
        private void CBGlobalSig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ScanData==null)
                return;

            if ((SampleNo == 0) || (ScanData.Info.SignalGenInfo.GlobalSignalCount == 0))
                return;

            while (InProcess)
                Task.Delay(20);

            UpdateGlobalDataDisplay(true);
        }

        //display peak data in heatmap
        private void UpdatePeakDataDisplay(bool _bRepaint)
        {
            if (_bRepaint)
                ResetBitMapDrawing();

            if (ScanData == null)
                return;

            float nChannelDataPerPixel = 1;
            if (ChannelCount > bm.Width)
                nChannelDataPerPixel = (float)(ChannelCount) / bm.Width;
            int nSigIdx = ScanData.Info.SignalGenInfo.GlobalSignalCount + CBPeakSig.SelectedIndex;

            using (Graphics g = Graphics.FromImage(bm))
            using (SolidBrush oBr = new SolidBrush(Color.Black))
            {
                while (PeakDrawSampleIdx < SampleIdx)
                {
                    int nCurPixelStartIdx = 0;
                    int nCurPixelStopIdx = (int)(((float)(CurrentPixelX - FirstXPixel) / PixelXStep + 1) * nChannelDataPerPixel-1);
                    double nData = 0;
                    for (int j = 0; j < ChannelCount; j++)
                    {
                        var nTempData = ScanData.Get(PeakDrawSampleIdx, nSigIdx, j);
                        if (!double.IsNaN(nTempData))
                            nData += nTempData;
                        if (j== nCurPixelStopIdx)
                        {
                            nData /= nCurPixelStopIdx - nCurPixelStartIdx + 1;
                            var oColor = getHeatMapColor((float)((nData - SigMin) / (SigMax - SigMin)));
                            oBr.Color = oColor;
                            g.FillRectangle(oBr, new Rectangle(CurrentPixelX, CurrentPixelY, PixelXStep, PixelYStep));
                            nData = 0;
                            CurrentPixelX += PixelXStep;
                            nCurPixelStartIdx = nCurPixelStopIdx + 1;
                            nCurPixelStopIdx = (int)(((CurrentPixelX - FirstXPixel) / PixelXStep + 1) * nChannelDataPerPixel - 1);
                        }
                    }
                    CurrentPixelY += PixelYStep;
                    CurrentPixelX = FirstXPixel;
                    PeakDrawSampleIdx++;
                }
            }
            PPaint.Invalidate();
        }

        private void CleanDataBitmap()
        {
            using (Graphics g = Graphics.FromImage(bm))
            using (SolidBrush oBr = new SolidBrush(Color.Black))
            {
                g.FillRectangle(oBr, new Rectangle(0, 0, bm.Width, bm.Height));
            }
        }


        private Color getHeatMapColor(float value)
        {
            const int NUM_COLORS = 4;
            // A static array of 4 colors:  (blue,   green,  yellow,  red) using {r,g,b} for each.
            float[,] color = new float[,] { { 0, 0, 255 }, { 0, 255, 0 }, { 255, 255, 0 }, { 255, 0, 0 } };


            int idx1;        
            int idx2;        
            float fractBetween = 0;  

            if (value <= 0) { idx1 = idx2 = 0; }   
            else if (value >= 1) { idx1 = idx2 = NUM_COLORS - 1; }    
            else
            {
                value = value * (NUM_COLORS - 1);        
                idx1 = (int)(Math.Floor(value));       
                idx2 = idx1 + 1;                      
                fractBetween = value - idx1;   
            }
            int red = (int)((color[idx2, 0] - color[idx1, 0]) * fractBetween + color[idx1, 0]);
            int green = (int)((color[idx2, 1] - color[idx1, 1]) * fractBetween + color[idx1, 1]);
            int blue = (int)((color[idx2, 2] - color[idx1, 2]) * fractBetween + color[idx1, 2]);
            return Color.FromArgb(red, green, blue);
        }

        

        //display global signal in chart series
        private void UpdateGlobalDataDisplay(bool _bRepaint)
        {
            if (ScanData == null)
                return;

            if (ScanData.Info.SignalGenInfo.GlobalSignalCount == 0)
                return;

            if (_bRepaint)
                GlobalDrawSampleIdx = 0;

            int nSigIdx = CBGlobalSig.SelectedIndex;
            while (GlobalDrawSampleIdx < SampleIdx)
            {
                var nTempData = ScanData.Get(GlobalDrawSampleIdx, nSigIdx, 0);
                if (!double.IsNaN(nTempData))
                    chart1.Series[0].Points[GlobalDrawSampleIdx].YValues[0] = nTempData;
                else
                    chart1.Series[0].Points[GlobalDrawSampleIdx].YValues[0] = 0;
                GlobalDrawSampleIdx++;
            }

            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.Invalidate();
        }

        private void PPaint_Paint(object sender, PaintEventArgs e)
        {
            PPaint.CreateGraphics().DrawImage(bm, 0, 0);
        }

        //setup heatmap min and max
        private void TBSigMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SigMin = double.Parse(TBSigMin.Text);
                SigMax = double.Parse(TBSigMax.Text);
                UpdatePeakDataDisplay(true);
            }
        }


        //Setup device scan rate
        private void TBSHZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                try
                {
                    float nScanRate = float.Parse(TBSHZ.Text);
                    Conn.Exec(CHRocodileLib.CmdID.ScanRate, nScanRate);
                }
                catch
                {
                }
            }
        }
    }
}
