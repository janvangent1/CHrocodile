using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CHRocodileLib;
using FSSCommon;

namespace FSSAreaScan
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Gets the default scan program that performs an area scan which is defined through the points
        /// top left = (-20, -20) and bottom right = (20, 20).
        /// The device frquency is set in the init function to 50000Hz and the
        /// scanner frequency is the as a parameter of the main function also to 50000Hz.
        /// </summary>
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

        /// <summary>
        /// Same as the default scan program but with the interpolated flag set.
        /// </summary>
        public const string SCAN_PROGRAM_INTER = @"
init
{
   $SHZ 50000;
   $SODX 256 82 65 66 69;
}

fn main(scanFreq=50000)
{
    rect(x0=-20.0, y0=-20.0, x1=20.0, y1=20.0, nCols=200,
            nRows=200, interp=1, label=""AreaScan"", waitAtEnd=20000)
}";

        /// <summary>
        /// File name of the scanner global configuration.
        /// </summary>
        public const string CONFIG_FILE_NAME = "ScannerGlobalConfig.cfg";

        /// <summary>
        /// Gets the scanner object.
        /// </summary>
        public FlyingSpotScanner Scanner { get; }

        // the last scanned shape...
        private CHRLibPlugin.FSS_PluginShape _shape;

        private DataProcessor _dataProc;

        private Int32 _plotSignal = 82;

        private enum State : Int32
        {
            Disconnected,
            Connected,
            ScanRunning,
        }

        public MainForm()
        {
            InitializeComponent();

            _dataProc = new DataProcessor();
            // Create a scanner instance and register to events
            Scanner = new FlyingSpotScanner();
            // force interpolated program selection
            CbInterpolate.Checked = true;
            TbIPAddress.Text = "192.168.170.2";
            BtConnection.Text = "Connect";
        }

        private async void BtConnection_Click(object sender, EventArgs e)
        {
            if (!Scanner.IsConnected)
            {
                Scanner.Open(TbIPAddress.Text, CbRawDataMode.Checked);
                Scanner.GeneralCommandCallback = OnGeneralCommandResponse;
                Scanner.ScanProgramCallback = OnScanProgramCallback;
                await Scanner.Config(CONFIG_FILE_NAME); // this is a waiting command
                BtConnection.Text = "Disconnect";
                EnableControls(State.Connected);
            }
            else
            {
                Scanner.Close();
                BtConnection.Text = "Connect";
                EnableControls(State.Disconnected);
            }
        }

        private async void BtRun_Click(object sender, EventArgs e)
        {
            _shape = null;
            EnableControls(State.ScanRunning);
            var progHandle = await Scanner.Compile(RtbProgramCode.Text);
            Scanner.Run(progHandle);
        }

        private void BtStop_Click(object sender, EventArgs e)
        {
            Scanner.Stop();
            EnableControls(State.Connected);
            UpdateSignals();
        }

        private void BtReset_Click(object sender, EventArgs e)
        {
            CbInterpolate.Checked = false;
        }

        private void CbInterpolate_CheckedChanged(object sender, EventArgs e)
        {
            RtbProgramCode.Text = CbInterpolate.Checked ? SCAN_PROGRAM_INTER : SCAN_PROGRAM;
        }

        private void CmbPlotSignal_SelectedIndexChanged(object sender, EventArgs e)
        {
            _plotSignal = (Int32)CmbPlotSignal.SelectedItem;

            if (_shape != null)
            {
                // obtain the signal index from signal ID
                var sigIdx = _shape.SignalIndex(_plotSignal);
                if (_shape.Type == CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                {
                    ImgAreaScan.Image = _dataProc.GridDataToBitmapRGB(sigIdx);
                }
                else
                {
                    int Xidx = _shape.SignalIndex(65),  // signal indices for X- and Y-encoder coordinates
                        Yidx = _shape.SignalIndex(66);
                    // plot raw 2D data onto RGB bitmap with a given size
                    ImgAreaScan.Image = _dataProc.RawDataToBitmapRGB(sigIdx, Xidx, Yidx, 512, 512);
                }
            }
        }

        private void EnableControls(State s)
        {
            TbIPAddress.Enabled = s == State.Disconnected;
            CbRawDataMode.Enabled = s == State.Disconnected;
            CmbPlotSignal.Enabled = s == State.Connected;
            BtConnection.Enabled = s == State.Disconnected || s == State.Connected;
            BtRun.Enabled = s == State.Connected;
            BtStop.Enabled = s == State.Connected || s == State.ScanRunning;
        }

        public static void GlobalException(object sender, ThreadExceptionEventArgs t)
        {
            string message = $"Top-level exception: {t.Exception.Message}";
            message += $"\nInner: {t.Exception.InnerException?.Message}";
            MessageBox.Show(message, "FlyingSpot Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateSignals()
        {
            CmbPlotSignal.Items.Clear();
            if (_shape?.SignalInfos != null)
            {
                foreach (var sig in _shape.SignalInfos)
                    CmbPlotSignal.Items.Add((int)sig.SignalID);

                CmbPlotSignal.SelectedIndex = 0;
            }
            CmbPlotSignal.Enabled = CmbPlotSignal.Items.Count > 0;
        }

        private void OnGeneralCommandResponse(Response rsp)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                ListLog.Items.Add(rsp.ToString());
                ListLog.SelectedIndex = ListLog.Items.Count - 1;
            }));
        }
        
        private void OnScanProgramCallback(Response rsp)
        {
            try
            {
                Console.WriteLine($"Scan program callback called {rsp}");
                // Shape data are stored in a blob argument of the response
                if (!(rsp.ParamCount > 0 && rsp.TryGetParam(0, out byte[] blob))) {
                    return;
                }

                var shape = new CHRLibPlugin.FSS_PluginShape(blob);
                if (shape.Type == CHRLibPlugin.FSS_PluginDataType.RecipeTerminate)
                {
                    Console.WriteLine("ScanCallback: scan finished..");
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        BtStop_Click(null, null);
                    }));
                    return;
                }
                shape.Detach(); // "detaches" the shape from the FSS internal buffer by copying
                                // all signal data to the local storage: this is not absolutely necessary
                                // but is a safe way if the data is not supposed to be processed immediately
                _shape = shape; // otherwise, save this shape for future use
                _dataProc.Shape = _shape;    // point data manipulator to the new shape
            }
            catch (Exception ex)
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    ListLog.Items.Add(ex.Message);
                    ListLog.SelectedIndex = ListLog.Items.Count - 1;
                }));
            }
        }
    }
}
