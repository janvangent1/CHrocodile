namespace TCHRLibSharedConn
{
    partial class SharedConn
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BConnect = new System.Windows.Forms.Button();
            this.RBCHRC = new System.Windows.Forms.RadioButton();
            this.RBCLS = new System.Windows.Forms.RadioButton();
            this.RBCHR2 = new System.Windows.Forms.RadioButton();
            this.RBFirst = new System.Windows.Forms.RadioButton();
            this.TBConInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.TBSODX = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TBAVD = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TBSHZ = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TBMOD = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.RTRsp = new System.Windows.Forms.RichTextBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.BtnSendCmd = new System.Windows.Forms.Button();
            this.TBCMD = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.BClear = new System.Windows.Forms.Button();
            this.TBSampleNumber = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RTSample = new System.Windows.Forms.RichTextBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel8 = new System.Windows.Forms.Panel();
            this.RBFFT = new System.Windows.Forms.RadioButton();
            this.RBConfocal = new System.Windows.Forms.RadioButton();
            this.RBRaw = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BConnect);
            this.panel1.Controls.Add(this.RBCHRC);
            this.panel1.Controls.Add(this.RBCLS);
            this.panel1.Controls.Add(this.RBCHR2);
            this.panel1.Controls.Add(this.RBFirst);
            this.panel1.Controls.Add(this.TBConInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(633, 54);
            this.panel1.TabIndex = 3;
            // 
            // BConnect
            // 
            this.BConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BConnect.Location = new System.Drawing.Point(522, 9);
            this.BConnect.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BConnect.Name = "BConnect";
            this.BConnect.Size = new System.Drawing.Size(100, 38);
            this.BConnect.TabIndex = 8;
            this.BConnect.Tag = "0";
            this.BConnect.Text = "Connect";
            this.BConnect.UseVisualStyleBackColor = true;
            this.BConnect.Click += new System.EventHandler(this.BConnect_Click);
            // 
            // RBCHRC
            // 
            this.RBCHRC.AutoSize = true;
            this.RBCHRC.Location = new System.Drawing.Point(342, 31);
            this.RBCHRC.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBCHRC.Name = "RBCHRC";
            this.RBCHRC.Size = new System.Drawing.Size(92, 17);
            this.RBCHRC.TabIndex = 6;
            this.RBCHRC.Tag = "3";
            this.RBCHRC.Text = "CHRC Device";
            this.RBCHRC.UseVisualStyleBackColor = true;
            // 
            // RBCLS
            // 
            this.RBCLS.AutoSize = true;
            this.RBCLS.Location = new System.Drawing.Point(236, 31);
            this.RBCLS.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBCLS.Name = "RBCLS";
            this.RBCLS.Size = new System.Drawing.Size(110, 17);
            this.RBCLS.TabIndex = 5;
            this.RBCLS.Tag = "2";
            this.RBCLS.Text = "CLS/MPS Device";
            this.RBCLS.UseVisualStyleBackColor = true;
            // 
            // RBCHR2
            // 
            this.RBCHR2.AutoSize = true;
            this.RBCHR2.Checked = true;
            this.RBCHR2.Location = new System.Drawing.Point(147, 31);
            this.RBCHR2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBCHR2.Name = "RBCHR2";
            this.RBCHR2.Size = new System.Drawing.Size(91, 17);
            this.RBCHR2.TabIndex = 4;
            this.RBCHR2.TabStop = true;
            this.RBCHR2.Tag = "1";
            this.RBCHR2.Text = "CHR2 Device";
            this.RBCHR2.UseVisualStyleBackColor = true;
            // 
            // RBFirst
            // 
            this.RBFirst.AutoSize = true;
            this.RBFirst.Location = new System.Drawing.Point(9, 31);
            this.RBFirst.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBFirst.Name = "RBFirst";
            this.RBFirst.Size = new System.Drawing.Size(136, 17);
            this.RBFirst.TabIndex = 3;
            this.RBFirst.Tag = "0";
            this.RBFirst.Text = "First Generation Device";
            this.RBFirst.UseVisualStyleBackColor = true;
            // 
            // TBConInfo
            // 
            this.TBConInfo.Location = new System.Drawing.Point(101, 9);
            this.TBConInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBConInfo.Name = "TBConInfo";
            this.TBConInfo.Size = new System.Drawing.Size(116, 20);
            this.TBConInfo.TabIndex = 2;
            this.TBConInfo.Text = "192.168.170.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection Info:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TBSODX);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.TBAVD);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.TBSHZ);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.TBMOD);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 54);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(633, 30);
            this.panel2.TabIndex = 4;
            // 
            // TBSODX
            // 
            this.TBSODX.Location = new System.Drawing.Point(442, 6);
            this.TBSODX.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBSODX.Name = "TBSODX";
            this.TBSODX.ReadOnly = true;
            this.TBSODX.Size = new System.Drawing.Size(100, 20);
            this.TBSODX.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(362, 8);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Output Signals:";
            // 
            // TBAVD
            // 
            this.TBAVD.Location = new System.Drawing.Point(320, 6);
            this.TBAVD.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBAVD.Name = "TBAVD";
            this.TBAVD.ReadOnly = true;
            this.TBAVD.Size = new System.Drawing.Size(38, 20);
            this.TBAVD.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(245, 8);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Data Average:";
            // 
            // TBSHZ
            // 
            this.TBSHZ.Location = new System.Drawing.Point(200, 6);
            this.TBSHZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBSHZ.Name = "TBSHZ";
            this.TBSHZ.ReadOnly = true;
            this.TBSHZ.Size = new System.Drawing.Size(38, 20);
            this.TBSHZ.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(141, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Scan Rate:";
            // 
            // TBMOD
            // 
            this.TBMOD.Location = new System.Drawing.Point(98, 7);
            this.TBMOD.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBMOD.Name = "TBMOD";
            this.TBMOD.ReadOnly = true;
            this.TBMOD.Size = new System.Drawing.Size(38, 20);
            this.TBMOD.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Measuring Mode:";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.RTRsp);
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Location = new System.Drawing.Point(396, 84);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(237, 510);
            this.panel3.TabIndex = 5;
            // 
            // RTRsp
            // 
            this.RTRsp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RTRsp.Location = new System.Drawing.Point(0, 87);
            this.RTRsp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RTRsp.Name = "RTRsp";
            this.RTRsp.ReadOnly = true;
            this.RTRsp.Size = new System.Drawing.Size(237, 423);
            this.RTRsp.TabIndex = 5;
            this.RTRsp.Text = "";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label8);
            this.panel6.Controls.Add(this.BtnSendCmd);
            this.panel6.Controls.Add(this.TBCMD);
            this.panel6.Controls.Add(this.label7);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(237, 87);
            this.panel6.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 67);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Received Response";
            // 
            // BtnSendCmd
            // 
            this.BtnSendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSendCmd.Enabled = false;
            this.BtnSendCmd.Location = new System.Drawing.Point(170, 25);
            this.BtnSendCmd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BtnSendCmd.Name = "BtnSendCmd";
            this.BtnSendCmd.Size = new System.Drawing.Size(56, 19);
            this.BtnSendCmd.TabIndex = 6;
            this.BtnSendCmd.Text = "Send";
            this.BtnSendCmd.UseVisualStyleBackColor = true;
            this.BtnSendCmd.Click += new System.EventHandler(this.BtnSendCmd_Click);
            // 
            // TBCMD
            // 
            this.TBCMD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TBCMD.Enabled = false;
            this.TBCMD.Location = new System.Drawing.Point(8, 24);
            this.TBCMD.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBCMD.Name = "TBCMD";
            this.TBCMD.Size = new System.Drawing.Size(158, 20);
            this.TBCMD.TabIndex = 5;
            this.TBCMD.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TBCMD_KeyDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 7);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Send Command";
            // 
            // panel4
            // 
            this.panel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.RTSample);
            this.panel4.Location = new System.Drawing.Point(0, 84);
            this.panel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(396, 201);
            this.panel4.TabIndex = 7;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.BClear);
            this.panel5.Controls.Add(this.TBSampleNumber);
            this.panel5.Controls.Add(this.label6);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(396, 28);
            this.panel5.TabIndex = 8;
            // 
            // BClear
            // 
            this.BClear.Location = new System.Drawing.Point(333, 2);
            this.BClear.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.BClear.Name = "BClear";
            this.BClear.Size = new System.Drawing.Size(56, 19);
            this.BClear.TabIndex = 2;
            this.BClear.Text = "Clear";
            this.BClear.UseVisualStyleBackColor = true;
            this.BClear.Click += new System.EventHandler(this.BClear_Click);
            // 
            // TBSampleNumber
            // 
            this.TBSampleNumber.Location = new System.Drawing.Point(114, 5);
            this.TBSampleNumber.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TBSampleNumber.Name = "TBSampleNumber";
            this.TBSampleNumber.ReadOnly = true;
            this.TBSampleNumber.Size = new System.Drawing.Size(104, 20);
            this.TBSampleNumber.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 6);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Number of Samples:";
            // 
            // RTSample
            // 
            this.RTSample.Location = new System.Drawing.Point(0, 0);
            this.RTSample.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RTSample.Name = "RTSample";
            this.RTSample.Size = new System.Drawing.Size(395, 242);
            this.RTSample.TabIndex = 7;
            this.RTSample.Text = "";
            // 
            // panel7
            // 
            this.panel7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel7.Controls.Add(this.chart1);
            this.panel7.Controls.Add(this.panel8);
            this.panel7.Location = new System.Drawing.Point(0, 326);
            this.panel7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(396, 242);
            this.panel7.TabIndex = 8;
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.Black;
            this.chart1.BorderlineColor = System.Drawing.Color.Yellow;
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisX.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.ScaleBreakStyle.LineColor = System.Drawing.Color.Yellow;
            chartArea1.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorGrid.Interval = 0D;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 26);
            this.chart1.Margin = new System.Windows.Forms.Padding(0);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.Yellow;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.MarkerColor = System.Drawing.Color.Red;
            series1.MarkerSize = 2;
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(396, 216);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart1";
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.RBFFT);
            this.panel8.Controls.Add(this.RBConfocal);
            this.panel8.Controls.Add(this.RBRaw);
            this.panel8.Controls.Add(this.label9);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(0, 0);
            this.panel8.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(396, 26);
            this.panel8.TabIndex = 0;
            // 
            // RBFFT
            // 
            this.RBFFT.AutoSize = true;
            this.RBFFT.Location = new System.Drawing.Point(256, 5);
            this.RBFFT.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBFFT.Name = "RBFFT";
            this.RBFFT.Size = new System.Drawing.Size(44, 17);
            this.RBFFT.TabIndex = 3;
            this.RBFFT.Text = "FFT";
            this.RBFFT.UseVisualStyleBackColor = true;
            // 
            // RBConfocal
            // 
            this.RBConfocal.AutoSize = true;
            this.RBConfocal.Location = new System.Drawing.Point(169, 5);
            this.RBConfocal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBConfocal.Name = "RBConfocal";
            this.RBConfocal.Size = new System.Drawing.Size(67, 17);
            this.RBConfocal.TabIndex = 2;
            this.RBConfocal.Text = "Confocal";
            this.RBConfocal.UseVisualStyleBackColor = true;
            // 
            // RBRaw
            // 
            this.RBRaw.AutoSize = true;
            this.RBRaw.Checked = true;
            this.RBRaw.Location = new System.Drawing.Point(105, 5);
            this.RBRaw.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RBRaw.Name = "RBRaw";
            this.RBRaw.Size = new System.Drawing.Size(47, 17);
            this.RBRaw.TabIndex = 1;
            this.RBRaw.TabStop = true;
            this.RBRaw.Text = "Raw";
            this.RBRaw.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 6);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(82, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Spectrum Type:";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // SharedConn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 568);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(570, 520);
            this.Name = "SharedConn";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SharedConn_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BConnect;
        private System.Windows.Forms.RadioButton RBCHRC;
        private System.Windows.Forms.RadioButton RBCLS;
        private System.Windows.Forms.RadioButton RBCHR2;
        private System.Windows.Forms.RadioButton RBFirst;
        private System.Windows.Forms.TextBox TBConInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox TBMOD;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TBSODX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TBAVD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RichTextBox RTSample;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.TextBox TBSampleNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox RTRsp;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button BtnSendCmd;
        private System.Windows.Forms.TextBox TBCMD;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button BClear;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.RadioButton RBFFT;
        private System.Windows.Forms.RadioButton RBConfocal;
        private System.Windows.Forms.RadioButton RBRaw;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Timer timer1;
        //private CustomeControlTest1.RoundedPanel roundedPanel1;
    }
}

