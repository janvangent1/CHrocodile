namespace FSSAreaScan
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
            this.ListLog = new System.Windows.Forms.ListBox();
            this.GrpScanner = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CmbPlotSignal = new System.Windows.Forms.ComboBox();
            this.BtStop = new System.Windows.Forms.Button();
            this.BtRun = new System.Windows.Forms.Button();
            this.GrpScanProgram = new System.Windows.Forms.GroupBox();
            this.BtReset = new System.Windows.Forms.Button();
            this.CbInterpolate = new System.Windows.Forms.CheckBox();
            this.RtbProgramCode = new System.Windows.Forms.RichTextBox();
            this.ImgAreaScan = new System.Windows.Forms.PictureBox();
            this.GrpConnection = new System.Windows.Forms.GroupBox();
            this.BtConnection = new System.Windows.Forms.Button();
            this.CbRawDataMode = new System.Windows.Forms.CheckBox();
            this.TbIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GrpScanner.SuspendLayout();
            this.GrpScanProgram.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgAreaScan)).BeginInit();
            this.GrpConnection.SuspendLayout();
            this.SuspendLayout();
            // 
            // ListLog
            // 
            this.ListLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListLog.FormattingEnabled = true;
            this.ListLog.ItemHeight = 16;
            this.ListLog.Location = new System.Drawing.Point(9, 636);
            this.ListLog.Margin = new System.Windows.Forms.Padding(4);
            this.ListLog.Name = "ListLog";
            this.ListLog.Size = new System.Drawing.Size(997, 162);
            this.ListLog.TabIndex = 9;
            // 
            // GrpScanner
            // 
            this.GrpScanner.Controls.Add(this.label2);
            this.GrpScanner.Controls.Add(this.CmbPlotSignal);
            this.GrpScanner.Controls.Add(this.BtStop);
            this.GrpScanner.Controls.Add(this.BtRun);
            this.GrpScanner.Location = new System.Drawing.Point(9, 128);
            this.GrpScanner.Margin = new System.Windows.Forms.Padding(4);
            this.GrpScanner.Name = "GrpScanner";
            this.GrpScanner.Padding = new System.Windows.Forms.Padding(4);
            this.GrpScanner.Size = new System.Drawing.Size(455, 69);
            this.GrpScanner.TabIndex = 8;
            this.GrpScanner.TabStop = false;
            this.GrpScanner.Text = "Scanner";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(243, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Plot signal:";
            // 
            // CmbPlotSignal
            // 
            this.CmbPlotSignal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CmbPlotSignal.FormattingEnabled = true;
            this.CmbPlotSignal.Location = new System.Drawing.Point(328, 28);
            this.CmbPlotSignal.Margin = new System.Windows.Forms.Padding(4);
            this.CmbPlotSignal.Name = "CmbPlotSignal";
            this.CmbPlotSignal.Size = new System.Drawing.Size(113, 24);
            this.CmbPlotSignal.TabIndex = 4;
            this.CmbPlotSignal.SelectedIndexChanged += new System.EventHandler(this.CmbPlotSignal_SelectedIndexChanged);
            // 
            // BtStop
            // 
            this.BtStop.Enabled = false;
            this.BtStop.Location = new System.Drawing.Point(127, 28);
            this.BtStop.Margin = new System.Windows.Forms.Padding(4);
            this.BtStop.Name = "BtStop";
            this.BtStop.Size = new System.Drawing.Size(108, 27);
            this.BtStop.TabIndex = 3;
            this.BtStop.Text = "Stop";
            this.BtStop.UseVisualStyleBackColor = true;
            this.BtStop.Click += new System.EventHandler(this.BtStop_Click);
            // 
            // BtRun
            // 
            this.BtRun.Enabled = false;
            this.BtRun.Location = new System.Drawing.Point(16, 28);
            this.BtRun.Margin = new System.Windows.Forms.Padding(4);
            this.BtRun.Name = "BtRun";
            this.BtRun.Size = new System.Drawing.Size(97, 27);
            this.BtRun.TabIndex = 2;
            this.BtRun.Text = "Run";
            this.BtRun.UseVisualStyleBackColor = true;
            this.BtRun.Click += new System.EventHandler(this.BtRun_Click);
            // 
            // GrpScanProgram
            // 
            this.GrpScanProgram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.GrpScanProgram.Controls.Add(this.BtReset);
            this.GrpScanProgram.Controls.Add(this.CbInterpolate);
            this.GrpScanProgram.Controls.Add(this.RtbProgramCode);
            this.GrpScanProgram.Location = new System.Drawing.Point(9, 205);
            this.GrpScanProgram.Margin = new System.Windows.Forms.Padding(4);
            this.GrpScanProgram.Name = "GrpScanProgram";
            this.GrpScanProgram.Padding = new System.Windows.Forms.Padding(4);
            this.GrpScanProgram.Size = new System.Drawing.Size(455, 423);
            this.GrpScanProgram.TabIndex = 7;
            this.GrpScanProgram.TabStop = false;
            this.GrpScanProgram.Text = "Scan Program";
            // 
            // BtReset
            // 
            this.BtReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtReset.Location = new System.Drawing.Point(349, 18);
            this.BtReset.Margin = new System.Windows.Forms.Padding(4);
            this.BtReset.Name = "BtReset";
            this.BtReset.Size = new System.Drawing.Size(96, 27);
            this.BtReset.TabIndex = 4;
            this.BtReset.Text = "Reset";
            this.BtReset.UseVisualStyleBackColor = true;
            this.BtReset.Click += new System.EventHandler(this.BtReset_Click);
            // 
            // CbInterpolate
            // 
            this.CbInterpolate.AutoSize = true;
            this.CbInterpolate.Location = new System.Drawing.Point(16, 23);
            this.CbInterpolate.Margin = new System.Windows.Forms.Padding(4);
            this.CbInterpolate.Name = "CbInterpolate";
            this.CbInterpolate.Size = new System.Drawing.Size(97, 21);
            this.CbInterpolate.TabIndex = 1;
            this.CbInterpolate.Text = "Interpolate";
            this.CbInterpolate.UseVisualStyleBackColor = true;
            this.CbInterpolate.CheckedChanged += new System.EventHandler(this.CbInterpolate_CheckedChanged);
            // 
            // RtbProgramCode
            // 
            this.RtbProgramCode.AcceptsTab = true;
            this.RtbProgramCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.RtbProgramCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RtbProgramCode.Location = new System.Drawing.Point(16, 50);
            this.RtbProgramCode.Margin = new System.Windows.Forms.Padding(4);
            this.RtbProgramCode.Name = "RtbProgramCode";
            this.RtbProgramCode.Size = new System.Drawing.Size(429, 365);
            this.RtbProgramCode.TabIndex = 0;
            this.RtbProgramCode.Text = "";
            // 
            // ImgAreaScan
            // 
            this.ImgAreaScan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ImgAreaScan.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ImgAreaScan.Location = new System.Drawing.Point(473, 11);
            this.ImgAreaScan.Margin = new System.Windows.Forms.Padding(4);
            this.ImgAreaScan.Name = "ImgAreaScan";
            this.ImgAreaScan.Size = new System.Drawing.Size(533, 617);
            this.ImgAreaScan.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgAreaScan.TabIndex = 6;
            this.ImgAreaScan.TabStop = false;
            // 
            // GrpConnection
            // 
            this.GrpConnection.AutoSize = true;
            this.GrpConnection.Controls.Add(this.BtConnection);
            this.GrpConnection.Controls.Add(this.CbRawDataMode);
            this.GrpConnection.Controls.Add(this.TbIPAddress);
            this.GrpConnection.Controls.Add(this.label1);
            this.GrpConnection.Location = new System.Drawing.Point(9, 7);
            this.GrpConnection.Margin = new System.Windows.Forms.Padding(0);
            this.GrpConnection.Name = "GrpConnection";
            this.GrpConnection.Padding = new System.Windows.Forms.Padding(4);
            this.GrpConnection.Size = new System.Drawing.Size(455, 108);
            this.GrpConnection.TabIndex = 5;
            this.GrpConnection.TabStop = false;
            this.GrpConnection.Text = "Connection";
            // 
            // BtConnection
            // 
            this.BtConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtConnection.Location = new System.Drawing.Point(309, 39);
            this.BtConnection.Margin = new System.Windows.Forms.Padding(4);
            this.BtConnection.Name = "BtConnection";
            this.BtConnection.Size = new System.Drawing.Size(133, 27);
            this.BtConnection.TabIndex = 1;
            this.BtConnection.Text = "Connect";
            this.BtConnection.UseVisualStyleBackColor = true;
            this.BtConnection.Click += new System.EventHandler(this.BtConnection_Click);
            // 
            // CbRawDataMode
            // 
            this.CbRawDataMode.AutoSize = true;
            this.CbRawDataMode.Location = new System.Drawing.Point(16, 68);
            this.CbRawDataMode.Margin = new System.Windows.Forms.Padding(0);
            this.CbRawDataMode.Name = "CbRawDataMode";
            this.CbRawDataMode.Size = new System.Drawing.Size(152, 21);
            this.CbRawDataMode.TabIndex = 1;
            this.CbRawDataMode.Text = "Use raw data mode";
            this.CbRawDataMode.UseVisualStyleBackColor = true;
            this.CbRawDataMode.CheckedChanged += new System.EventHandler(this.CbInterpolate_CheckedChanged);
            // 
            // TbIPAddress
            // 
            this.TbIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TbIPAddress.Location = new System.Drawing.Point(12, 41);
            this.TbIPAddress.Margin = new System.Windows.Forms.Padding(4);
            this.TbIPAddress.Name = "TbIPAddress";
            this.TbIPAddress.Size = new System.Drawing.Size(288, 22);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1016, 806);
            this.Controls.Add(this.ListLog);
            this.Controls.Add(this.GrpScanner);
            this.Controls.Add(this.GrpScanProgram);
            this.Controls.Add(this.ImgAreaScan);
            this.Controls.Add(this.GrpConnection);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1031, 718);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flying Spot Scanner Examples - Area Scan";
            this.GrpScanner.ResumeLayout(false);
            this.GrpScanner.PerformLayout();
            this.GrpScanProgram.ResumeLayout(false);
            this.GrpScanProgram.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgAreaScan)).EndInit();
            this.GrpConnection.ResumeLayout(false);
            this.GrpConnection.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox ListLog;
        private System.Windows.Forms.GroupBox GrpScanner;
        private System.Windows.Forms.Button BtStop;
        private System.Windows.Forms.Button BtRun;
        private System.Windows.Forms.GroupBox GrpScanProgram;
        private System.Windows.Forms.Button BtReset;
        private System.Windows.Forms.CheckBox CbInterpolate;
        private System.Windows.Forms.RichTextBox RtbProgramCode;
        private System.Windows.Forms.PictureBox ImgAreaScan;
        private System.Windows.Forms.GroupBox GrpConnection;
        private System.Windows.Forms.Button BtConnection;
        private System.Windows.Forms.TextBox TbIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CmbPlotSignal;
        private System.Windows.Forms.CheckBox CbRawDataMode;
    }
}

