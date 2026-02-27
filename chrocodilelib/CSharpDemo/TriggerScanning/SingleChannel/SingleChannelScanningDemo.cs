/*
This demo demonstrates how to collect scanning data under trigger each mode with single channel device.
The simulated scan is like normal rectangular scan, which is defined by number of lines to be scanned and number of points in each line.
User can select between sync-in signal trigger and encoder trigger. 
The trigger settings are sent to the device synchronously.
After all the trigger configuration has been properly set, this demo uses the recording mode of the connection to collect data.
The collected data is show as a heatmap is the dialog.
This demo aims to show how to set up device for run with trigger each mode, particularly for encoder trigger.
*/


using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCHRLibSingleChannelScanning
{
    public partial class SingleChannelScanningDemo : Form
    {
        CHRocodileLib.SynchronousConnection Conn;
        
        private int[] OutputSignals;
        private int ScanLineNo, LineSampleCount;
        private int AllSampleCount;
        private int ScanLineIdx, SampleIdx;

        private CHRocodileLib.Data ScanData = null;
        
        private bool InProcess;
        private Bitmap bm;
        private int FirstXPixel;
        private int CurrentPixelX, PixelXStep;
        private int CurrentPixelY, PixelYStep;
        private int DrawLineIdx, DrawSampleIdx;
        private double SigMin, SigMax;

        public SingleChannelScanningDemo()
        {
            InitializeComponent();
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
                    var DeviceType = CHRocodileLib.DeviceType.Chr1;
                    if (RBCHR2.Checked)
                        DeviceType = CHRocodileLib.DeviceType.Chr2;
                    else if (RBCHRC.Checked)
                        DeviceType = CHRocodileLib.DeviceType.ChrCMini;
                    string strConInfo = TbConInfo.Text;
                    Conn = new CHRocodileLib.SynchronousConnection(strConInfo, DeviceType);
                    SetupDevice(DeviceType);
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
                CloseConnection();
            }
            EnableGui(bConnect, false);
        }


        private void CloseConnection()
        {
            StopScan();
            while (InProcess)
                Task.Delay(20);
            Conn.Close();
            Conn = null;
        }


        private void SetupDevice(CHRocodileLib.DeviceType _nDeviceType)
        {
            //according to device type, define available axis
            int nTemp = CBAxis.SelectedIndex;       
            CBAxis.Items.Clear();
            CBAxis.Items.Add("X-Axis");
            CBAxis.Items.Add("Y-Axis");
            CBAxis.Items.Add("Z-Axis");
            if (_nDeviceType != CHRocodileLib.DeviceType.Chr1)
            {
                CBAxis.Items.Add("U-Axis");
                CBAxis.Items.Add("V-Axis");
            }
            if ((nTemp >= 0) && (nTemp < CBAxis.Items.Count))
                CBAxis.SelectedIndex = nTemp;
            else
                CBAxis.SelectedIndex = 0;
            SetDeviceOutput();
        }


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


        //Set output signals
        private void SetDeviceOutput()
        {
            try
            {
                if (TBSignal.Text == "")
                    throw new Exception("No signals are selected for CHR device!");
                char[] delimiters = new char[] { ' ', ',', ';' };
                string[] aTemp = TBSignal.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                int[] aTempSig = Array.ConvertAll(aTemp, int.Parse);
                Array.Sort(aTempSig);
                var oRsp = Conn.Exec(CHRocodileLib.CmdID.OutputSignals, aTempSig);
                OutputSignals = oRsp.GetParam<int[]>(0);
                //update display signal combo box
                int nSelectIdx = -1;
                if (CBDisplaySig.SelectedIndex >= 0)
                {
                    int nLastSig = int.Parse(CBDisplaySig.SelectedItem.ToString());
                    nSelectIdx = Array.IndexOf(OutputSignals, nLastSig);
                }
                CBDisplaySig.Items.Clear();
                foreach (var nID in OutputSignals)
                    CBDisplaySig.Items.Add(nID.ToString());
                if (nSelectIdx > -1)
                    CBDisplaySig.SelectedIndex = nSelectIdx;
                else
                    CBDisplaySig.SelectedIndex = 0;
            }
            catch
            {
                Debug.Fail("Cannot set output signals.");
            }           
        }



        private void EnableGui(bool _bConnect, bool _bInScan)
        {
            BtConnect.Enabled = !_bConnect;
            BtDisCon.Enabled = _bConnect;
            BtScan.Enabled = _bConnect;            
            TBSignal.Enabled = _bConnect && (!_bInScan);
            BtEncoderPos.Enabled = _bConnect && RBEncTrigger.Checked && (!_bInScan);
        } 


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

        private void TBSignal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                SetDeviceOutput();
            }
        }

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

        //during scan to process/display data
        private void timerProcess_Tick(object sender, EventArgs e)
        {
            //to make sure the last process has been finished
            if (InProcess)
                return;

            InProcess = true;

            //Read Data
            ScanData = Conn.GetNextSamples();

            //get total number of recorded data, TotalNumSamples returns the total recorded sample count
            var nTotalSampleCount = ScanData.TotalNumSamples;

            ScanLineIdx = (int)(Math.Floor((double)nTotalSampleCount / LineSampleCount));
            SampleIdx = (int)nTotalSampleCount - ScanLineIdx * LineSampleCount;

            //if there is new data comes in, update display
            //NumSamples returns the new sample count from the last call of GetNextSampels
            if (ScanData.NumSamples>0)
                UpdateDataDisplay(false);

            //enough samples have been saved, stop scan
            if (nTotalSampleCount == AllSampleCount)
                StopScan();

            InProcess = false;
        }

        private void BtScan_Click(object sender, EventArgs e)
        {
            if (timerProcess.Enabled)
            {
                StopScan();
            }
            else
            {
                try
                {
                    ScanLineNo = int.Parse(TBLineNo.Text);
                    LineSampleCount = int.Parse(TBSampleNo.Text);
                    if (RBEncTrigger.Checked && CBTriggerOnReturn.Checked)
                        ScanLineNo *= 2;

                    AllSampleCount = ScanLineNo * LineSampleCount;

                    if (AllSampleCount == 0)
                        return;


                    //Set trigger settings
                    SendTriggerSetting();
                    //use trigger each mode
                    Conn.Exec(CHRocodileLib.CmdID.DeviceTriggerMode, (int)CHRocodileLib.TriggerMode.TriggerEach);
                    
                    //start recording modes
                    Conn.StartRecording(AllSampleCount);
                    //reset record data
                    ScanData = null;
                    
                    SigMin = double.Parse(TBSigMin.Text);
                    SigMax = double.Parse(TBSigMax.Text);
                   
                    ScanLineIdx = 0;
                    SampleIdx = 0;
                    
                    InProcess = false;
                    ResetDrawing();

                    //use timer to display data
                    timerProcess.Enabled = true;
                    EnableGui(true, true);
                    BtScan.Text = "Cancel Scan";
                    PPaint.Invalidate();
                }
                catch
                {
                    Debug.Fail("Cannot set scan related parameters.");
                }
                
            }
        }

        private void StopScan()
        {
            if (!timerProcess.Enabled)
                return;

            timerProcess.Enabled = false;
            
            //quit recording modes,
            //StopRecording also returns recorded data object,
            //which is the same as the data object from GetNextSamples in the timer routine,
            Conn.StopRecording();

            try
            {
                // Set back to free run mode
                Conn.Exec(CHRocodileLib.CmdID.DeviceTriggerMode, CHRocodileLib.TriggerMode.FreeRun);
            }
            catch
            {

            }
            EnableGui(true, false);
            BtScan.Text = "Start Scan";
        }

        private void ResetDrawing()
        {
            CleanDataBitmap();
            if (ScanLineNo == 0)
                return;
            int nTemp = PPaint.Height % ScanLineNo;
            CurrentPixelY = nTemp / 2;
            PixelYStep = PPaint.Height / ScanLineNo;
            if (LineSampleCount > PPaint.Width)
            {
                FirstXPixel = 0;
                PixelXStep = 1;
            }
            else
            {
                nTemp = PPaint.Width % LineSampleCount;
                FirstXPixel = nTemp / 2;
                PixelXStep = PPaint.Width / LineSampleCount;
            }
            CurrentPixelX = FirstXPixel;
            DrawLineIdx = 0;
            DrawSampleIdx = 0;
        }

        //set display min and max value for heat map
        private void TBSigMax_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SigMin = double.Parse(TBSigMin.Text);
                SigMax = double.Parse(TBSigMax.Text);
                UpdateDataDisplay(true);
            }
        }

        //set encoder trigger property
        private void TBEncoderTriggerProperty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                int nStartPos = int.Parse(TBStartPos.Text);
                int nStopPos = int.Parse(TBStopPos.Text);
                float nInterval = float.Parse(TBInterval.Text, CultureInfo.InvariantCulture);
                TBSampleNo.Text = ((int)((nStopPos - nStartPos) / nInterval + 1)).ToString();
            }
        }

        //select signal to display
        private void CBDisplaySig_SelectedIndexChanged(object sender, EventArgs e)
        {
            while (InProcess)
                Task.Delay(20);

            UpdateDataDisplay(true);
        }

        private void PPaint_Paint(object sender, PaintEventArgs e)
        {
           PPaint.CreateGraphics().DrawImage(bm, 0, 0);
        }

        //display data
        private void UpdateDataDisplay(bool _bRepaint)
        {            
            if (_bRepaint)
                ResetDrawing();

            if (ScanData == null)
                return;

            if (AllSampleCount == 0)
                return;


            float nSamplePerPixel = 1;
            if (LineSampleCount > bm.Width)
                nSamplePerPixel = (float)(LineSampleCount) / bm.Width;
            int nSigIdx = CBDisplaySig.SelectedIndex;

            using (Graphics g = Graphics.FromImage(bm))
            using (SolidBrush oBr = new SolidBrush(Color.Black))
            {
                int nInitLineIdx = DrawLineIdx;
                while (DrawLineIdx <= ScanLineIdx)
                {
                    int nSampleNo = LineSampleCount;
                    int nSampleStartIdx = 0;
                    if (DrawLineIdx == ScanLineIdx)
                        nSampleNo = SampleIdx;
                    if (DrawLineIdx == nInitLineIdx)
                        nSampleStartIdx = DrawSampleIdx;
                    int nCurPixelStartIdx = nSampleStartIdx;
                    int nCurPixelStopIdx = (int)(((float)(CurrentPixelX - FirstXPixel) / PixelXStep + 1) * nSamplePerPixel-1);
                    double nData = 0;
                    for (int j = nSampleStartIdx; j < nSampleNo; j++)
                    {
                        //read data from scan data object
                        var nTemp = ScanData.Get(DrawLineIdx * LineSampleCount + j, nSigIdx, 0);
                        if (!double.IsNaN(nTemp))
                            nData += nTemp;
                        if ((j== nCurPixelStopIdx)|| (j== LineSampleCount-1))
                        {
                            nData /= nCurPixelStopIdx - nCurPixelStartIdx + 1;
                            var oColor = getHeatMapColor((float)((nData - SigMin) / (SigMax - SigMin)));
                            oBr.Color = oColor;
                            g.FillRectangle(oBr, new Rectangle(CurrentPixelX, CurrentPixelY, PixelXStep, PixelYStep));
                            nData = 0;
                            CurrentPixelX += PixelXStep;
                            nCurPixelStartIdx = nCurPixelStopIdx + 1;
                            nCurPixelStopIdx = (int)(((CurrentPixelX - FirstXPixel) / PixelXStep + 1) * nSamplePerPixel - 1);
                        }
                    }
                    if (DrawLineIdx != ScanLineIdx)
                    {
                        CurrentPixelY += PixelYStep;
                        CurrentPixelX = FirstXPixel;
                        DrawLineIdx++;
                    }
                    else
                    {
                        DrawSampleIdx = nCurPixelStartIdx;
                        if (DrawSampleIdx >= LineSampleCount)
                        {
                            DrawSampleIdx = 0;
                            DrawLineIdx++;
                        }
                        break;
                    }
                    
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
    }
}
