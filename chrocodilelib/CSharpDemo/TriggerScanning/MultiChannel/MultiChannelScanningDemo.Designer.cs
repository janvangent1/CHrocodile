namespace TCHRLibMultiChannelScanning
{
    partial class MultiChannelScanningDemo
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.timerProcess = new System.Windows.Forms.Timer(this.components);
            this.panel7 = new System.Windows.Forms.Panel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel4 = new System.Windows.Forms.Panel();
            this.CBGlobalSig = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.BtCTN = new System.Windows.Forms.Button();
            this.BtStopScan = new System.Windows.Forms.Button();
            this.BtScan = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.TBSampleNo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.TBEncoderPos = new System.Windows.Forms.TextBox();
            this.BtEncoderPos = new System.Windows.Forms.Button();
            this.TBInterval = new System.Windows.Forms.TextBox();
            this.TBStopPos = new System.Windows.Forms.TextBox();
            this.TBStartPos = new System.Windows.Forms.TextBox();
            this.CBAxis = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CBTriggerOnReturn = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RBEncTrigger = new System.Windows.Forms.RadioButton();
            this.RBSyncSig = new System.Windows.Forms.RadioButton();
            this.panel5 = new System.Windows.Forms.Panel();
            this.TBSHZ = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TBSignal = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BtDisCon = new System.Windows.Forms.Button();
            this.BtConnect = new System.Windows.Forms.Button();
            this.TbConInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel8 = new System.Windows.Forms.Panel();
            this.PPaint = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.TBSigMax = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TBSigMin = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.CBPeakSig = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerProcess
            // 
            this.timerProcess.Tick += new System.EventHandler(this.timerProcess_Tick);
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.chart1);
            this.panel7.Controls.Add(this.panel4);
            this.panel7.Controls.Add(this.panel3);
            this.panel7.Controls.Add(this.panel2);
            this.panel7.Controls.Add(this.panel5);
            this.panel7.Controls.Add(this.panel1);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel7.Location = new System.Drawing.Point(0, 0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(542, 466);
            this.panel7.TabIndex = 31;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart1.Location = new System.Drawing.Point(0, 257);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(542, 209);
            this.chart1.TabIndex = 35;
            this.chart1.Text = "chart1";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.CBGlobalSig);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 230);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(542, 27);
            this.panel4.TabIndex = 34;
            // 
            // CBGlobalSig
            // 
            this.CBGlobalSig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBGlobalSig.FormattingEnabled = true;
            this.CBGlobalSig.Location = new System.Drawing.Point(137, 3);
            this.CBGlobalSig.Name = "CBGlobalSig";
            this.CBGlobalSig.Size = new System.Drawing.Size(125, 21);
            this.CBGlobalSig.TabIndex = 27;
            this.CBGlobalSig.SelectedIndexChanged += new System.EventHandler(this.CBGlobalSig_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 6);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(109, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "Display Global Signal:";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.BtCTN);
            this.panel3.Controls.Add(this.BtStopScan);
            this.panel3.Controls.Add(this.BtScan);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 195);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(542, 35);
            this.panel3.TabIndex = 33;
            // 
            // BtCTN
            // 
            this.BtCTN.Location = new System.Drawing.Point(440, 5);
            this.BtCTN.Name = "BtCTN";
            this.BtCTN.Size = new System.Drawing.Size(86, 23);
            this.BtCTN.TabIndex = 33;
            this.BtCTN.Text = "Reset to CTN ";
            this.BtCTN.UseVisualStyleBackColor = true;
            this.BtCTN.Click += new System.EventHandler(this.BtCTN_Click);
            // 
            // BtStopScan
            // 
            this.BtStopScan.Enabled = false;
            this.BtStopScan.Location = new System.Drawing.Point(226, 3);
            this.BtStopScan.Name = "BtStopScan";
            this.BtStopScan.Size = new System.Drawing.Size(171, 26);
            this.BtStopScan.TabIndex = 32;
            this.BtStopScan.Text = "Stop Scan";
            this.BtStopScan.UseVisualStyleBackColor = true;
            this.BtStopScan.Click += new System.EventHandler(this.BtStopScan_Click);
            // 
            // BtScan
            // 
            this.BtScan.Enabled = false;
            this.BtScan.Location = new System.Drawing.Point(29, 3);
            this.BtScan.Name = "BtScan";
            this.BtScan.Size = new System.Drawing.Size(171, 26);
            this.BtScan.TabIndex = 31;
            this.BtScan.Text = "Start Scan";
            this.BtScan.UseVisualStyleBackColor = true;
            this.BtScan.Click += new System.EventHandler(this.BtScan_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TBSampleNo);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.TBEncoderPos);
            this.panel2.Controls.Add(this.BtEncoderPos);
            this.panel2.Controls.Add(this.TBInterval);
            this.panel2.Controls.Add(this.TBStopPos);
            this.panel2.Controls.Add(this.TBStartPos);
            this.panel2.Controls.Add(this.CBAxis);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.CBTriggerOnReturn);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.RBEncTrigger);
            this.panel2.Controls.Add(this.RBSyncSig);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 63);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(542, 132);
            this.panel2.TabIndex = 32;
            // 
            // TBSampleNo
            // 
            this.TBSampleNo.Enabled = false;
            this.TBSampleNo.Location = new System.Drawing.Point(302, 5);
            this.TBSampleNo.Name = "TBSampleNo";
            this.TBSampleNo.Size = new System.Drawing.Size(119, 20);
            this.TBSampleNo.TabIndex = 35;
            this.TBSampleNo.Text = "1000";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(208, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Sample Counter :";
            // 
            // TBEncoderPos
            // 
            this.TBEncoderPos.Location = new System.Drawing.Point(211, 105);
            this.TBEncoderPos.Name = "TBEncoderPos";
            this.TBEncoderPos.Size = new System.Drawing.Size(83, 20);
            this.TBEncoderPos.TabIndex = 32;
            this.TBEncoderPos.Text = "0";
            // 
            // BtEncoderPos
            // 
            this.BtEncoderPos.Enabled = false;
            this.BtEncoderPos.Location = new System.Drawing.Point(22, 102);
            this.BtEncoderPos.Name = "BtEncoderPos";
            this.BtEncoderPos.Size = new System.Drawing.Size(178, 23);
            this.BtEncoderPos.TabIndex = 31;
            this.BtEncoderPos.Text = "Set Trigger Axis Encoder Position:";
            this.BtEncoderPos.UseVisualStyleBackColor = true;
            this.BtEncoderPos.Click += new System.EventHandler(this.BtEncoderPos_Click);
            // 
            // TBInterval
            // 
            this.TBInterval.Location = new System.Drawing.Point(440, 76);
            this.TBInterval.Name = "TBInterval";
            this.TBInterval.Size = new System.Drawing.Size(86, 20);
            this.TBInterval.TabIndex = 29;
            this.TBInterval.Text = "100";
            this.TBInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerPP_KeyPress);
            // 
            // TBStopPos
            // 
            this.TBStopPos.Location = new System.Drawing.Point(257, 76);
            this.TBStopPos.Name = "TBStopPos";
            this.TBStopPos.Size = new System.Drawing.Size(120, 20);
            this.TBStopPos.TabIndex = 28;
            this.TBStopPos.Text = "10000";
            this.TBStopPos.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerPP_KeyPress);
            // 
            // TBStartPos
            // 
            this.TBStartPos.Location = new System.Drawing.Point(72, 76);
            this.TBStartPos.Name = "TBStartPos";
            this.TBStartPos.Size = new System.Drawing.Size(119, 20);
            this.TBStartPos.TabIndex = 27;
            this.TBStartPos.Text = "0";
            this.TBStartPos.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerPP_KeyPress);
            // 
            // CBAxis
            // 
            this.CBAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBAxis.FormattingEnabled = true;
            this.CBAxis.Items.AddRange(new object[] {
            "X-Axis",
            "Y-Axis",
            "Z-Axis",
            "U-Axis",
            "V-Axis"});
            this.CBAxis.Location = new System.Drawing.Point(70, 53);
            this.CBAxis.Name = "CBAxis";
            this.CBAxis.Size = new System.Drawing.Size(121, 21);
            this.CBAxis.TabIndex = 26;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Axis:";
            // 
            // CBTriggerOnReturn
            // 
            this.CBTriggerOnReturn.AutoSize = true;
            this.CBTriggerOnReturn.Location = new System.Drawing.Point(211, 54);
            this.CBTriggerOnReturn.Name = "CBTriggerOnReturn";
            this.CBTriggerOnReturn.Size = new System.Drawing.Size(133, 17);
            this.CBTriggerOnReturn.TabIndex = 24;
            this.CBTriggerOnReturn.Text = "Trigger on return move";
            this.CBTriggerOnReturn.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(390, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Interval:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Stop Pos:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Start Pos:";
            // 
            // RBEncTrigger
            // 
            this.RBEncTrigger.AutoSize = true;
            this.RBEncTrigger.Checked = true;
            this.RBEncTrigger.Location = new System.Drawing.Point(14, 29);
            this.RBEncTrigger.Name = "RBEncTrigger";
            this.RBEncTrigger.Size = new System.Drawing.Size(155, 17);
            this.RBEncTrigger.TabIndex = 20;
            this.RBEncTrigger.TabStop = true;
            this.RBEncTrigger.Text = "Trigger by Encoder Counter";
            this.RBEncTrigger.UseVisualStyleBackColor = true;
            this.RBEncTrigger.Click += new System.EventHandler(this.RBSyncSig_Click);
            // 
            // RBSyncSig
            // 
            this.RBSyncSig.AutoSize = true;
            this.RBSyncSig.Location = new System.Drawing.Point(14, 6);
            this.RBSyncSig.Name = "RBSyncSig";
            this.RBSyncSig.Size = new System.Drawing.Size(134, 17);
            this.RBSyncSig.TabIndex = 19;
            this.RBSyncSig.Text = "Trigger by Sync. Signal";
            this.RBSyncSig.UseVisualStyleBackColor = true;
            this.RBSyncSig.Click += new System.EventHandler(this.RBSyncSig_Click);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.TBSHZ);
            this.panel5.Controls.Add(this.label10);
            this.panel5.Controls.Add(this.TBSignal);
            this.panel5.Controls.Add(this.label12);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 36);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(542, 27);
            this.panel5.TabIndex = 31;
            // 
            // TBSHZ
            // 
            this.TBSHZ.Enabled = false;
            this.TBSHZ.Location = new System.Drawing.Point(432, 3);
            this.TBSHZ.Name = "TBSHZ";
            this.TBSHZ.Size = new System.Drawing.Size(94, 20);
            this.TBSHZ.TabIndex = 40;
            this.TBSHZ.Text = "2000";
            this.TBSHZ.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSHZ_KeyPress);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(368, 6);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 13);
            this.label10.TabIndex = 39;
            this.label10.Text = "Scanrate:";
            // 
            // TBSignal
            // 
            this.TBSignal.Enabled = false;
            this.TBSignal.Location = new System.Drawing.Point(137, 3);
            this.TBSignal.Name = "TBSignal";
            this.TBSignal.Size = new System.Drawing.Size(181, 20);
            this.TBSignal.TabIndex = 38;
            this.TBSignal.Text = "65 83 16640 ";
            this.TBSignal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSignal_KeyPress);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 6);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(116, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Device Output Signals:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BtDisCon);
            this.panel1.Controls.Add(this.BtConnect);
            this.panel1.Controls.Add(this.TbConInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(542, 36);
            this.panel1.TabIndex = 30;
            // 
            // BtDisCon
            // 
            this.BtDisCon.Enabled = false;
            this.BtDisCon.Location = new System.Drawing.Point(432, 3);
            this.BtDisCon.Margin = new System.Windows.Forms.Padding(2);
            this.BtDisCon.Name = "BtDisCon";
            this.BtDisCon.Size = new System.Drawing.Size(94, 28);
            this.BtDisCon.TabIndex = 23;
            this.BtDisCon.Text = "Disconnect";
            this.BtDisCon.UseVisualStyleBackColor = true;
            this.BtDisCon.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // BtConnect
            // 
            this.BtConnect.Location = new System.Drawing.Point(327, 4);
            this.BtConnect.Margin = new System.Windows.Forms.Padding(2);
            this.BtConnect.Name = "BtConnect";
            this.BtConnect.Size = new System.Drawing.Size(94, 27);
            this.BtConnect.TabIndex = 22;
            this.BtConnect.Text = "Connect";
            this.BtConnect.UseVisualStyleBackColor = true;
            this.BtConnect.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // TbConInfo
            // 
            this.TbConInfo.Location = new System.Drawing.Point(125, 8);
            this.TbConInfo.Margin = new System.Windows.Forms.Padding(2);
            this.TbConInfo.Name = "TbConInfo";
            this.TbConInfo.Size = new System.Drawing.Size(169, 20);
            this.TbConInfo.TabIndex = 18;
            this.TbConInfo.Text = "192.168.170.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Connection Info";
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.PPaint);
            this.panel8.Controls.Add(this.panel6);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel8.Location = new System.Drawing.Point(542, 0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(548, 466);
            this.panel8.TabIndex = 32;
            // 
            // PPaint
            // 
            this.PPaint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PPaint.Location = new System.Drawing.Point(0, 27);
            this.PPaint.Name = "PPaint";
            this.PPaint.Size = new System.Drawing.Size(548, 439);
            this.PPaint.TabIndex = 32;
            this.PPaint.Paint += new System.Windows.Forms.PaintEventHandler(this.PPaint_Paint);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.TBSigMax);
            this.panel6.Controls.Add(this.label6);
            this.panel6.Controls.Add(this.TBSigMin);
            this.panel6.Controls.Add(this.label8);
            this.panel6.Controls.Add(this.CBPeakSig);
            this.panel6.Controls.Add(this.label15);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(548, 27);
            this.panel6.TabIndex = 31;
            // 
            // TBSigMax
            // 
            this.TBSigMax.Location = new System.Drawing.Point(432, 3);
            this.TBSigMax.Name = "TBSigMax";
            this.TBSigMax.Size = new System.Drawing.Size(100, 20);
            this.TBSigMax.TabIndex = 35;
            this.TBSigMax.Text = "1000";
            this.TBSigMax.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSigMax_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(396, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 34;
            this.label6.Text = "Max:";
            // 
            // TBSigMin
            // 
            this.TBSigMin.Location = new System.Drawing.Point(296, 3);
            this.TBSigMin.Name = "TBSigMin";
            this.TBSigMin.Size = new System.Drawing.Size(94, 20);
            this.TBSigMin.TabIndex = 33;
            this.TBSigMin.Text = "0";
            this.TBSigMin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSigMax_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Min:";
            // 
            // CBPeakSig
            // 
            this.CBPeakSig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBPeakSig.FormattingEnabled = true;
            this.CBPeakSig.Location = new System.Drawing.Point(118, 3);
            this.CBPeakSig.Name = "CBPeakSig";
            this.CBPeakSig.Size = new System.Drawing.Size(131, 21);
            this.CBPeakSig.TabIndex = 27;
            this.CBPeakSig.SelectedIndexChanged += new System.EventHandler(this.CBDisplaySig_SelectedIndexChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 6);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(104, 13);
            this.label15.TabIndex = 22;
            this.label15.Text = "Display Peak Signal:";
            // 
            // MultiChannelScanningDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 466);
            this.Controls.Add(this.panel8);
            this.Controls.Add(this.panel7);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MultiChannelScanningDemo";
            this.Text = "Form1";
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerProcess;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ComboBox CBGlobalSig;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button BtStopScan;
        private System.Windows.Forms.Button BtScan;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox TBSampleNo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TBEncoderPos;
        private System.Windows.Forms.Button BtEncoderPos;
        private System.Windows.Forms.TextBox TBInterval;
        private System.Windows.Forms.TextBox TBStopPos;
        private System.Windows.Forms.TextBox TBStartPos;
        private System.Windows.Forms.ComboBox CBAxis;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox CBTriggerOnReturn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton RBEncTrigger;
        private System.Windows.Forms.RadioButton RBSyncSig;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TBSignal;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BtDisCon;
        private System.Windows.Forms.Button BtConnect;
        private System.Windows.Forms.TextBox TbConInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel PPaint;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TextBox TBSigMax;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TBSigMin;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox CBPeakSig;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button BtCTN;
    }
}

