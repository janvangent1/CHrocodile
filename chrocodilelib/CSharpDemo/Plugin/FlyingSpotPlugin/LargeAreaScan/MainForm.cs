using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CHRocodileLib;
using FSSCommon;

namespace FSSLargeAreaScan
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// File name of the scanner global configuration.
        /// </summary>
        public const string CONFIG_FILE_NAME = "ScannerGlobalConfig310.cfg";

        /// <summary>
        /// Gets the scanner object.
        /// </summary>
        public FlyingSpotScanner Scanner;

        // the last scanned shape
        private CHRLibPlugin.FSS_PluginShape _shape;

        private DataProcessor _dataProc;

        private Int32 _plotSignal = 82;

        private OpenFileDialog _fileDialog = new OpenFileDialog();

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
            TbIPAddress.Text = "192.168.170.2";
            BtConnection.Text = "Connect";
            CbShowBitmap.Checked = true;
            CbSaveAsBcrf.Checked = false;
            LoadScriptFile(Application.StartupPath + @"/Scripts/LargeAreaScanInline.rs");
            EnableControls(State.Disconnected);
        }

        /// <summary>
        /// connects to the CHRocodile device and configures the FSS plugin
        /// </summary>
        private async void BtConnection_Click(object sender, EventArgs e)
        {
            if (!Scanner.IsConnected)
            {
                // connect to the scanner with raw data mode to save on traffic
                bool UseRawDataMode = true;
                Scanner.Open(TbIPAddress.Text, UseRawDataMode);
                Scanner.GeneralCommandCallback = OnGeneralCommandResponse;
                Scanner.ScanProgramCallback = OnScanProgramCallback;

                // IMPORTANT: the internal FSS buffer size must be large enough for area scan, e.g.
                // for a scan of size 10000x10000, the buffer size must be at least 10^8 samples !!
                // Hence, the value of TblBufferSize, set by the user, must be at least 10^5 (as it's multiplied by 1000)
                await Scanner.Config(CONFIG_FILE_NAME, (int)TblBufferSize.Value * 1000); 
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

        /// <summary>
        /// compiles and executes the script program
        /// </summary>
        private async void BtRun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RtbProgramCode.Text))
                throw new Exception("Please select a script to execute !");

            _shape = null;
            EnableControls(State.ScanRunning);
            var progHandle = await Scanner.Compile(RtbProgramCode.Text);
            Scanner.Run(progHandle);
        }

        /// <summary>
        /// prematurely stops script execution (if any)
        /// </summary>
        private void BtStop_Click(object sender, EventArgs e)
        {
            Scanner.Stop();
            EnableControls(State.Connected);
            UpdateSignals();
        }

        private void btLoadScript_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_fileDialog.FileName))
                _fileDialog.InitialDirectory = Application.StartupPath + @"\Scripts";

            _fileDialog.Title = "Open FSS script";
            _fileDialog.Filter = "FSS Scripts|*.rs";
            _fileDialog.CheckFileExists = true;
            if(_fileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadScriptFile(_fileDialog.FileName);
            }
        }

        private void LoadScriptFile(string fname)
        {
            try
            {
                var txt = System.IO.File.ReadAllText(fname);
                RtbProgramCode.Text = txt.Replace("\t", "   ");
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"Exception opening {fname}: {ex.Message}");
            }
        }

        private void CmbPlotSignal_SelectedIndexChanged(object sender, EventArgs e)
        {
            _plotSignal = (Int32)CmbPlotSignal.SelectedItem;

            if (CbShowBitmap.Checked && _shape != null)
            {
                if (_shape.Type == CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                {
                    // get signal index from signal ID
                    var sigIdx = _shape.SignalIndex(_plotSignal);
                    // convert 2D interpolated floating-point data to RGB image (rainbow)
                    ImgAreaScan.Image = _dataProc.GridDataToBitmapRGB(sigIdx);
                }
                else
                    ListLog.Items.Add("Only 2D Interpolated data can be currently visualized!");
            }
        }

        private void EnableControls(State s)
        {
            TbIPAddress.Enabled = s == State.Disconnected;
            TblBufferSize.Enabled = s == State.Disconnected;
            CmbPlotSignal.Enabled = s == State.Connected;
            BtConnection.Enabled = s == State.Disconnected || s == State.Connected;
            CbSaveAsBcrf.Enabled = s == State.Connected;
            CbShowBitmap.Enabled = s == State.Connected;
            BtRun.Enabled = s == State.Connected;
            BtStop.Enabled = s == State.Connected || s == State.ScanRunning;
            btLoadScript.Enabled = s == State.Connected;
        }

        /// <summary>
        /// shows an error message in case top-level exception occurred
        /// </summary>
        public static void GlobalException(object sender, ThreadExceptionEventArgs t)
        {
            string message = $"Top-level exception:\n{t.Exception.Message}";
            message += $"\nInner: {t.Exception.InnerException?.Message}";
            MessageBox.Show(message, "FlyingSpot Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// updates the drop-down list of signals to be plotted
        /// </summary>
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
          
#region Flying Spot Scanner Events
          
        /// <summary>
        /// general callback function to be called once any user command response comes
        /// </summary>
        /// <param name="rsp">command response object</param>
        private void OnGeneralCommandResponse(Response rsp)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                ListLog.Items.Add(rsp.ToString());
                if (rsp.IsError())
                    ListLog.Items.Add($"Error occurred: {rsp.ParamCount}");
                ListLog.SelectedIndex = ListLog.Items.Count - 1;
            }));
        }

        /// <summary>
        /// callback function called after each new shape has been scanned (and at the end of the scanning process)
        /// </summary>
        /// <param name="rsp">response object holding shape data</param>
        private void OnScanProgramCallback(Response rsp)
        {
            try
            {
                Console.WriteLine($"Scan program callback called {rsp}");
                // Shape data are stored in a blob argument of the response
                if (!(rsp.ParamCount > 0 && rsp.TryGetParam(0, out byte[] blob)))
                    return;

                // extract shape data from the binary blob
                var shape = new CHRLibPlugin.FSS_PluginShape(blob);

                // check whether scan is completed: if not, just store last shape for later processing
                if (shape.Type != CHRLibPlugin.FSS_PluginDataType.RecipeTerminate)
                {
                    _shape = shape; // we expect to scan only one large shape to be scanned
                    _dataProc.Shape = _shape;
                    return;
                }

                // scan process is finished
                BeginInvoke(new MethodInvoker(() =>
                {
                    ListLog.Items.Add("ScanCallback: scan finished.."); 
                    if (CbSaveAsBcrf.Checked && _shape != null &&
                                _shape.Type == CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                    {
                        for (int i = 0; i < _shape.SignalInfos.Length; i++) {
                            var fname = _dataProc.SaveAsBCRF(i);
                            ListLog.Items.Add($"The file {fname} has been written..");
                        }
                    }
                    BtStop_Click(null, null);
                }));
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

#endregion
    }
}
