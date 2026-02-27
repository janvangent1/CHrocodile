using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CHRocodileLib;
using System.Threading;
using System.Globalization;
using FSSCommon;

namespace FSSDataAcquisition
{
    using SignalDict = Dictionary<int, double[]>;

    public partial class MainForm : Form
    {
        /// <summary>
        /// File name of the scanner global configuration.
        /// </summary>
        public const string CONFIG_FILE_NAME = "ScannerGlobalConfig.cfg";

        /// <summary>
        /// Gets the scanner object.
        /// </summary>
        public FlyingSpotScanner Scanner { get; }

        private List<CHRLibPlugin.FSS_PluginShape> _shapes = new List<CHRLibPlugin.FSS_PluginShape>();
        private DataProcessor _dataProc;
        private PlotModel _plot;

        public MainForm()
        {
            InitializeComponent();

            _dataProc = new DataProcessor();
            _plot = new PlotModel();
            _plot.Series.Add(new LineSeries()
            {
                MarkerFill = OxyColors.SteelBlue,
                MarkerType = MarkerType.Circle
            });
            _plot.Axes.Add(new LinearAxis() { Title = "X coordinates / mm", Position = AxisPosition.Bottom, StringFormat = "0.00" });
            _plot.Axes.Add(new LinearAxis() { Title = "Y coordinates / mm", Position = AxisPosition.Left, StringFormat = "0.00" });

            PlotView.Model = _plot;

            // Create a scanner instance and register to events
            Scanner = new FlyingSpotScanner();

            // Default values
            TbIPAddress.Text = "192.168.170.2";
            BtConnection.Text = "Connect";
            TbScanProgramFileName.Text = Application.StartupPath + @"\Scripts\CrossedHousePuzzle.rs";
        }

        private void UpdateChartOptions()
        {
            CmbScanObjects.Items.Clear();

            foreach(var item in _shapes)
                CmbScanObjects.Items.Add(item.Label);
        }

        /// <summary>
        /// Plots the scan path of the desired scan object.
        /// </summary>
        private void UpdateChart(CHRLibPlugin.FSS_PluginShape shape, bool overwrite = true)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                if (overwrite) {
                    _plot.Series[0] = new LineSeries()
                    {
                        MarkerFill = OxyColors.SteelBlue,
                        MarkerType = MarkerType.Circle
                    };
                }

                int sigX = shape.SignalIndex(65),
                    sigY = shape.SignalIndex(66);
                foreach(var S in shape.Samples())
                {
                    if (_plot.Series[0] is LineSeries lineSeries)
                    {
                        lineSeries.Points.Add(new DataPoint(S.Get(sigX), S.Get(sigY)));
                    }
                }
                _plot.InvalidatePlot(true);
            }));
        }

        public static void GlobalException(object sender, ThreadExceptionEventArgs t)
        {
            //var dlg = (Dlg)Application.OpenForms[0];
            //dlg.CloseConnection();
            string message = $"Top-level exception: {t.Exception.Message}";
            message += $"\nInner: {t.Exception.InnerException?.Message}";
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void Log(string message)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                ListLog.Items.Add(message);
                ListLog.SelectedIndex = ListLog.Items.Count - 1;
            }));
        }

        private void EnableControls(bool enable)
        {
            //TbIPAddress.Enabled = !enable;
            BtRun.Enabled = enable;
            BtStop.Enabled = enable;
            GrpScanObject.Enabled = enable;
            TbScanProgramFileName.Enabled = enable;
            BtConnection.Enabled = enable;
            CbPlotData.Enabled = enable;
            CbSaveToFile.Enabled = enable;
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
                // Shape data are stored in a blob argument of the response
                if (!(rsp.ParamCount > 0 && rsp.TryGetParam(0, out byte[] blob)))
                {
                    return;
                }

                var shape = new CHRLibPlugin.FSS_PluginShape(blob);
                Console.WriteLine($"shape type {shape.Type}");

                if (shape.Type == CHRLibPlugin.FSS_PluginDataType.RecipeTerminate)
                {
                    Console.WriteLine("ScanCallback: scan finished..");
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        BtStop_Click(null, null);
                    }));
                    return;
                }

                _shapes.Add(shape);
                _dataProc.Shape = shape;

                if (shape.Type == CHRLibPlugin.FSS_PluginDataType.RawData)
                {
                    if (CbPlotData.Checked)
                        UpdateChart(shape, false);

                    if (CbSaveToFile.Checked)
                    {
                        var fname = _dataProc.SaveAsCSV();
                        Log($"The raw scan data has been written to {fname}");
                    }
                }
                else // CHRLibPlugin.FSS_PluginDataType.Interpolated2D
                {
                    for(int i = 0; i < shape.SignalInfos.Length; i++)
                    {
                        // convert each individual signal into an RGB bitmap
                        var bmp = _dataProc.GridDataToBitmapRGB(i);

                        var sigID = shape.SignalInfos[i].SignalID;
                        var fname = $"shape_{shape.Label}_{shape.ShapeIndex}_sig{sigID}.png";
                        bmp.Save(fname);
                        Log($"Bitmap data has been saved to {fname}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private async void BtConnection_Click(object sender, EventArgs e)
        {
            if (!Scanner.IsConnected)
            {
                EnableControls(false);
                Scanner.Open(TbIPAddress.Text);
                Scanner.GeneralCommandCallback = OnGeneralCommandResponse;
                Scanner.ScanProgramCallback = OnScanProgramCallback;
                BtConnection.Text = "Disconnect";
                await Scanner.Config(CONFIG_FILE_NAME);
                EnableControls(true);
            }
            else
            {
                Scanner.Close();
                BtConnection.Text = "Connect";
                EnableControls(false);
                BtConnection.Enabled = true;
            }
        }

        private void BtOpenFileDialog_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OfdScanProgram.FileName))
                OfdScanProgram.InitialDirectory = Application.StartupPath + @"\Scripts";

            var res = OfdScanProgram.ShowDialog();
            if (res == DialogResult.OK)
            {
                TbScanProgramFileName.Text = OfdScanProgram.FileName;
            }
        }

        private async void BtRun_Click(object sender, EventArgs e)
        {
            _shapes.Clear();
                
            _plot.Series[0] = new LineSeries()
            {
                MarkerFill = OxyColors.SteelBlue,
                MarkerType = MarkerType.Circle
            };

            _plot.InvalidatePlot(true);

            var programCode = File.ReadAllText(TbScanProgramFileName.Text);
            var progHandle = await Scanner.Compile(programCode);
            CmbScanObjects.SelectedItem = null;
            CmbScanObjects.Items.Clear();
            EnableControls(false);
            BtStop.Enabled = true;
            Scanner.Run(progHandle);
        }

        private void BtStop_Click(object sender, EventArgs e)
        {
            Scanner.Stop();
            EnableControls(true);
            UpdateChartOptions();
        }

        private void CbPlotData_CheckedChanged(object sender, EventArgs e)
        {
            PlotView.Enabled = CbPlotData.Checked;
            GrpScanObject.Visible = CbPlotData.Checked;
            BtRun.Enabled = CbPlotData.Checked || CbSaveToFile.Checked;
        }

        private void CbSaveToFile_CheckedChanged(object sender, EventArgs e)
        {
            BtRun.Enabled = CbPlotData.Checked || CbSaveToFile.Checked;
        }

        private void CmbScanObjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbScanObjects.SelectedItem is string label)
            {
                var item = _shapes.Find(x => x.Label == label);
                if(item != null)
                    UpdateChart(item);
            }
        }

        private void BtResetChart_Click(object sender, EventArgs e)
        {
            _plot.Series[0] = new LineSeries()
            {
                MarkerFill = OxyColors.SteelBlue,
                MarkerType = MarkerType.Circle
            };

            _plot.InvalidatePlot(true);

            foreach(var item in _shapes)
            {
                UpdateChart(item, false);
            }
        }
    }
}
