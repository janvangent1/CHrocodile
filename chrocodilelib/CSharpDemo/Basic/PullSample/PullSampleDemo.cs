/*
This demo shows how to perform synchronous communication with different types of the devices.
It includes sending commands, reading response, use "GetNextSamples" to constantly read data and downloading spectrum.
*/


using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using CHRocodileLib;


namespace TCHRLibBasicPullSample
{
    public partial class TPullSample : Form
    {
        //used for display data
        const int Data_Length = 1000;
        double[,] DataSamples;
        int CurrentDataPos;

        //connection object
        SynchronousConnection Conn;

        //device parameter setting
        MeasurementMode MeasuringMethod = MeasurementMode.Confocal;
        int[] SignalIDs;
        float ScanRate;


        public TPullSample()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {

            //intializing data display
            DataSamples = new double[Data_Length, 3]; // only shows the first 3 signals of the last 1024 samples
            for (int i = 0; i < Data_Length; i++)
            {
                chart1.Series[0].Points.AddY(i);
                chart2.Series[0].Points.AddY(i);
                chart3.Series[0].Points.AddY(i);
                chart4.Series[0].Points.AddY(i);
            }
        }


        private void BtConnect_Click(object sender, EventArgs e)
        {
            bool bConnect = false;
            //connect to device
            if (sender == BtConnect)
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
                    string strConInfo = TbConInfo.Text;
                    Conn = new SynchronousConnection(strConInfo, devType);
                    //set up device
                    SetupDevice();
                    bConnect = true;
                    CurrentDataPos = 0;
                    TTimerUpdate.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            //close connection to device
            else
            {
                TTimerUpdate.Enabled = false;
                Conn.Close();
                //after close, connection object cannot be used
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
                var oRsp = Conn.Exec(CmdID.MeasuringMethod, nMMD);
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
                var oRsp = Conn.Exec(CmdID.OutputSignals, signals);
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
                var oRsp = Conn.Exec(CmdID.ScanRate, nSHZ);
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
            BtSend.Enabled = _bEnabled;
            TBCMD.Enabled = _bEnabled;
            RBConfocal.Enabled = _bEnabled && (RBCHRNomal.Checked || RBCHR2.Checked);
            RBInterfero.Enabled = _bEnabled && (RBCHRNomal.Checked || RBCHR2.Checked);
            TBSHZ.Enabled = _bEnabled;
            TBSODX.Enabled = _bEnabled;
        }

        private void TTimerUpdate_Tick(object sender, EventArgs e)
        {
            //read in CHRocodile data with GetNextSamples
            bool bNewSample = false;
            while (true)
            {
                Int64 nMaxCount = 1000;
                var data = Conn.GetNextSamples(nMaxCount);
                if (data.NumSamples>0)
                {
                    foreach (var s in data.Samples())
                    {
                        //only read in and show the first 3 signals
                        for (int i=0;i<3; i++)
                        {
                            DataSamples[CurrentDataPos, i] = 0;
                            //first show global signal
                            if (data.Info.SignalGenInfo.GlobalSignalCount > i)
                                DataSamples[CurrentDataPos, i] = s.Get(i);
                            //if not enough global signal, show peak signal. for peak signal, only shows the value for the first channel
                            else if (data.Info.SignalGenInfo.GlobalSignalCount+data.Info.SignalGenInfo.PeakSignalCount>i)
                                DataSamples[CurrentDataPos, i] = s.Get(i, 0);
                        }
                        CurrentDataPos++;
                        if (CurrentDataPos >= Data_Length)
                            CurrentDataPos = 0;
                    }
                    bNewSample = true;
                }
                if (data.NumSamples<nMaxCount)
                    break;
            }
            //refresh data display
            if ((TCData.SelectedTab == TPData) && bNewSample)
            {
                for (int i = 0; i < Data_Length; i++)
                {
                    chart2.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 0])? 0 : DataSamples[i, 0];
                    chart3.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 1]) ? 0 : DataSamples[i, 1];
                    chart4.Series[0].Points[i].YValues[0] = Double.IsNaN(DataSamples[i, 2]) ? 0 : DataSamples[i, 2];
                }

                chart2.ChartAreas[0].RecalculateAxesScale();
                chart2.Invalidate();
                chart3.ChartAreas[0].RecalculateAxesScale();
                chart3.Invalidate();
                chart4.ChartAreas[0].RecalculateAxesScale();
                chart4.Invalidate();
            }

            //download spectrum
            if (TCData.SelectedTab == TPSpec)
            {
                try
                {
                    var specType = SpecType.Raw;
                    if (RBConfocalSpec.Checked)
                        specType = SpecType.Confocal;
                    else if (RBFFTSpec.Checked)
                        specType = SpecType.FT;

                    Response oRsp;
                    //for downloading spectra of several channles from multi-channel device,  needs to add start channel index and channel count
                    //here only download the specturm of the first channel
                    if (RBCLS.Checked)
                        oRsp = Conn.Exec(CmdID.DownloadSpectrum, specType, 0, 1);
                    else
                        oRsp = Conn.Exec(CmdID.DownloadSpectrum, specType);
                    //the last parameter of the response is the spectrum data
                    var aBytes = oRsp.GetParam<byte[]>(oRsp.ParamCount - 1);
                    //convert to 16bit data
                    Int16[] SpecData =  new Int16[aBytes.Length/2];
                    Buffer.BlockCopy(aBytes, 0, SpecData, 0, aBytes.Length);
                    int len = Math.Min(Data_Length, SpecData.Length);
                    for (int i = 0; i < len; i++)
                        chart1.Series[0].Points[i].YValues[0] = SpecData[i];
                    chart1.ChartAreas[0].RecalculateAxesScale();
                    chart1.Invalidate();
                }
                catch
                {
                    Debug.Fail("Cannot set download spectrum");
                }
                
          
            }
        }

        //Execute string command
        private void BtSend_Click(object sender, EventArgs e)
        {
            try
            {
                var oRsp = Conn.ExecString(TBCMD.Text);
                if (RTResponse.Text != "")
                    RTResponse.AppendText(Environment.NewLine);
                RTResponse.AppendText(oRsp.ToString());
                switch (oRsp.Info.CmdID)
                {
                    //In case of measuring method response
                    case (CmdID.MeasuringMethod):
                        MeasuringMethod = (MeasurementMode)oRsp.GetParam<int>(0);
                        if (MeasuringMethod == MeasurementMode.Confocal)
                            RBConfocal.Checked = true;
                        else
                            RBInterfero.Checked = true;
                        break;
                    //In case of scan rate response
                    case (CmdID.ScanRate):
                        ScanRate = oRsp.GetParam<float>(0);
                        TBSHZ.Text = ScanRate.ToString();
                        break;
                    //In case of output signal response
                    case (CmdID.OutputSignals):
                        SignalIDs = oRsp.GetParam<int[]>(0);
                        TBSODX.Text = String.Join(",", SignalIDs.Select(p => p.ToString()).ToArray());
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void TBCMD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                BtSend_Click(TBCMD, e);
            }
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

        private void TPullSample_FormClosing(object sender, FormClosingEventArgs e)
        {
            //close connection
            TTimerUpdate.Enabled = false;
            Conn = null;
        }
    }
}
