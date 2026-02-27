namespace FSSDataAcquisition
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GrpScanObject = new System.Windows.Forms.GroupBox();
            this.BtResetChart = new System.Windows.Forms.Button();
            this.CmbScanObjects = new System.Windows.Forms.ComboBox();
            this.OfdScanProgram = new System.Windows.Forms.OpenFileDialog();
            this.ListLog = new System.Windows.Forms.ListBox();
            this.CbSaveToFile = new System.Windows.Forms.CheckBox();
            this.CbPlotData = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BtStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.BtRun = new System.Windows.Forms.Button();
            this.BtOpenFileDialog = new System.Windows.Forms.Button();
            this.TbScanProgramFileName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtConnection = new System.Windows.Forms.Button();
            this.TbIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GrpScanner = new System.Windows.Forms.GroupBox();
            this.GrpConnection = new System.Windows.Forms.GroupBox();
            this.PlotView = new OxyPlot.WindowsForms.PlotView();
            this.GrpScanObject.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.GrpScanner.SuspendLayout();
            this.GrpConnection.SuspendLayout();
            this.SuspendLayout();
            // 
            // GrpScanObject
            // 
            this.GrpScanObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GrpScanObject.BackColor = System.Drawing.Color.White;
            this.GrpScanObject.Controls.Add(this.BtResetChart);
            this.GrpScanObject.Controls.Add(this.CmbScanObjects);
            this.GrpScanObject.Location = new System.Drawing.Point(948, 86);
            this.GrpScanObject.Margin = new System.Windows.Forms.Padding(4);
            this.GrpScanObject.Name = "GrpScanObject";
            this.GrpScanObject.Padding = new System.Windows.Forms.Padding(4);
            this.GrpScanObject.Size = new System.Drawing.Size(233, 94);
            this.GrpScanObject.TabIndex = 15;
            this.GrpScanObject.TabStop = false;
            this.GrpScanObject.Text = "Scan object";
            // 
            // BtResetChart
            // 
            this.BtResetChart.Location = new System.Drawing.Point(20, 54);
            this.BtResetChart.Margin = new System.Windows.Forms.Padding(4);
            this.BtResetChart.Name = "BtResetChart";
            this.BtResetChart.Size = new System.Drawing.Size(196, 27);
            this.BtResetChart.TabIndex = 9;
            this.BtResetChart.Text = "Reset";
            this.BtResetChart.UseVisualStyleBackColor = true;
            this.BtResetChart.Click += new System.EventHandler(this.BtResetChart_Click);
            // 
            // CmbScanObjects
            // 
            this.CmbScanObjects.FormattingEnabled = true;
            this.CmbScanObjects.Location = new System.Drawing.Point(20, 25);
            this.CmbScanObjects.Margin = new System.Windows.Forms.Padding(4);
            this.CmbScanObjects.Name = "CmbScanObjects";
            this.CmbScanObjects.Size = new System.Drawing.Size(195, 24);
            this.CmbScanObjects.TabIndex = 8;
            this.CmbScanObjects.SelectedIndexChanged += new System.EventHandler(this.CmbScanObjects_SelectedIndexChanged);
            // 
            // OfdScanProgram
            // 
            this.OfdScanProgram.Filter = "FSS Script | *.rs";
            // 
            // ListLog
            // 
            this.ListLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ListLog.FormattingEnabled = true;
            this.ListLog.ItemHeight = 16;
            this.ListLog.Location = new System.Drawing.Point(0, 478);
            this.ListLog.Margin = new System.Windows.Forms.Padding(4);
            this.ListLog.Name = "ListLog";
            this.ListLog.Size = new System.Drawing.Size(1191, 180);
            this.ListLog.TabIndex = 16;
            // 
            // CbSaveToFile
            // 
            this.CbSaveToFile.AutoSize = true;
            this.CbSaveToFile.Location = new System.Drawing.Point(24, 47);
            this.CbSaveToFile.Margin = new System.Windows.Forms.Padding(4);
            this.CbSaveToFile.Name = "CbSaveToFile";
            this.CbSaveToFile.Size = new System.Drawing.Size(104, 21);
            this.CbSaveToFile.TabIndex = 1;
            this.CbSaveToFile.Text = "Save to File";
            this.CbSaveToFile.UseVisualStyleBackColor = true;
            this.CbSaveToFile.CheckedChanged += new System.EventHandler(this.CbSaveToFile_CheckedChanged);
            // 
            // CbPlotData
            // 
            this.CbPlotData.AutoSize = true;
            this.CbPlotData.Checked = true;
            this.CbPlotData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CbPlotData.Location = new System.Drawing.Point(24, 23);
            this.CbPlotData.Margin = new System.Windows.Forms.Padding(4);
            this.CbPlotData.Name = "CbPlotData";
            this.CbPlotData.Size = new System.Drawing.Size(88, 21);
            this.CbPlotData.TabIndex = 0;
            this.CbPlotData.Text = "Plot Data";
            this.CbPlotData.UseVisualStyleBackColor = true;
            this.CbPlotData.CheckedChanged += new System.EventHandler(this.CbPlotData_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.CbSaveToFile);
            this.groupBox3.Controls.Add(this.CbPlotData);
            this.groupBox3.Location = new System.Drawing.Point(1027, 2);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(155, 76);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Data Acquistion";
            // 
            // BtStop
            // 
            this.BtStop.Enabled = false;
            this.BtStop.Location = new System.Drawing.Point(158, 32);
            this.BtStop.Margin = new System.Windows.Forms.Padding(4);
            this.BtStop.Name = "BtStop";
            this.BtStop.Size = new System.Drawing.Size(111, 27);
            this.BtStop.TabIndex = 5;
            this.BtStop.Text = "Stop";
            this.BtStop.UseVisualStyleBackColor = true;
            this.BtStop.Click += new System.EventHandler(this.BtStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 21);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "File name";
            // 
            // BtRun
            // 
            this.BtRun.Enabled = false;
            this.BtRun.Location = new System.Drawing.Point(23, 32);
            this.BtRun.Margin = new System.Windows.Forms.Padding(4);
            this.BtRun.Name = "BtRun";
            this.BtRun.Size = new System.Drawing.Size(110, 27);
            this.BtRun.TabIndex = 4;
            this.BtRun.Text = "Run";
            this.BtRun.UseVisualStyleBackColor = true;
            this.BtRun.Click += new System.EventHandler(this.BtRun_Click);
            // 
            // BtOpenFileDialog
            // 
            this.BtOpenFileDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtOpenFileDialog.Location = new System.Drawing.Point(350, 39);
            this.BtOpenFileDialog.Margin = new System.Windows.Forms.Padding(4);
            this.BtOpenFileDialog.Name = "BtOpenFileDialog";
            this.BtOpenFileDialog.Size = new System.Drawing.Size(48, 27);
            this.BtOpenFileDialog.TabIndex = 1;
            this.BtOpenFileDialog.Text = "...";
            this.BtOpenFileDialog.UseVisualStyleBackColor = true;
            this.BtOpenFileDialog.Click += new System.EventHandler(this.BtOpenFileDialog_Click);
            // 
            // TbScanProgramFileName
            // 
            this.TbScanProgramFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TbScanProgramFileName.Location = new System.Drawing.Point(12, 41);
            this.TbScanProgramFileName.Margin = new System.Windows.Forms.Padding(4);
            this.TbScanProgramFileName.Name = "TbScanProgramFileName";
            this.TbScanProgramFileName.Size = new System.Drawing.Size(328, 22);
            this.TbScanProgramFileName.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.BtOpenFileDialog);
            this.groupBox1.Controls.Add(this.TbScanProgramFileName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(311, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(406, 76);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scan program";
            // 
            // BtConnection
            // 
            this.BtConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtConnection.Location = new System.Drawing.Point(176, 39);
            this.BtConnection.Margin = new System.Windows.Forms.Padding(4);
            this.BtConnection.Name = "BtConnection";
            this.BtConnection.Size = new System.Drawing.Size(112, 27);
            this.BtConnection.TabIndex = 1;
            this.BtConnection.Text = "Connect";
            this.BtConnection.UseVisualStyleBackColor = true;
            this.BtConnection.Click += new System.EventHandler(this.BtConnection_Click);
            // 
            // TbIPAddress
            // 
            this.TbIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TbIPAddress.Location = new System.Drawing.Point(12, 41);
            this.TbIPAddress.Margin = new System.Windows.Forms.Padding(4);
            this.TbIPAddress.Name = "TbIPAddress";
            this.TbIPAddress.Size = new System.Drawing.Size(155, 22);
            this.TbIPAddress.TabIndex = 1;
            this.TbIPAddress.Text = "192.168.170.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP Address";
            // 
            // GrpScanner
            // 
            this.GrpScanner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GrpScanner.Controls.Add(this.BtStop);
            this.GrpScanner.Controls.Add(this.BtRun);
            this.GrpScanner.Location = new System.Drawing.Point(725, 2);
            this.GrpScanner.Margin = new System.Windows.Forms.Padding(4);
            this.GrpScanner.Name = "GrpScanner";
            this.GrpScanner.Padding = new System.Windows.Forms.Padding(4);
            this.GrpScanner.Size = new System.Drawing.Size(293, 76);
            this.GrpScanner.TabIndex = 13;
            this.GrpScanner.TabStop = false;
            this.GrpScanner.Text = "Scanner";
            // 
            // GrpConnection
            // 
            this.GrpConnection.Controls.Add(this.BtConnection);
            this.GrpConnection.Controls.Add(this.TbIPAddress);
            this.GrpConnection.Controls.Add(this.label1);
            this.GrpConnection.Location = new System.Drawing.Point(8, 2);
            this.GrpConnection.Margin = new System.Windows.Forms.Padding(4);
            this.GrpConnection.Name = "GrpConnection";
            this.GrpConnection.Padding = new System.Windows.Forms.Padding(4);
            this.GrpConnection.Size = new System.Drawing.Size(295, 76);
            this.GrpConnection.TabIndex = 11;
            this.GrpConnection.TabStop = false;
            this.GrpConnection.Text = "Connection";
            // 
            // PlotView
            // 
            this.PlotView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlotView.Location = new System.Drawing.Point(8, 86);
            this.PlotView.Margin = new System.Windows.Forms.Padding(4);
            this.PlotView.Name = "PlotView";
            this.PlotView.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.PlotView.Size = new System.Drawing.Size(1167, 384);
            this.PlotView.TabIndex = 18;
            this.PlotView.Text = "plotView1";
            this.PlotView.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.PlotView.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.PlotView.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1191, 658);
            this.Controls.Add(this.GrpScanObject);
            this.Controls.Add(this.ListLog);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.GrpScanner);
            this.Controls.Add(this.GrpConnection);
            this.Controls.Add(this.PlotView);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fyling Spot Examples - Asynchronous - Data Acquistition";
            this.GrpScanObject.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.GrpScanner.ResumeLayout(false);
            this.GrpConnection.ResumeLayout(false);
            this.GrpConnection.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GrpScanObject;
        private System.Windows.Forms.ComboBox CmbScanObjects;
        private System.Windows.Forms.OpenFileDialog OfdScanProgram;
        private System.Windows.Forms.ListBox ListLog;
        private System.Windows.Forms.CheckBox CbSaveToFile;
        private System.Windows.Forms.CheckBox CbPlotData;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button BtStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button BtRun;
        private System.Windows.Forms.Button BtOpenFileDialog;
        private System.Windows.Forms.TextBox TbScanProgramFileName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtConnection;
        private System.Windows.Forms.TextBox TbIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox GrpScanner;
        private System.Windows.Forms.GroupBox GrpConnection;
        private System.Windows.Forms.Button BtResetChart;
        private OxyPlot.WindowsForms.PlotView PlotView;
    }
}

