namespace TCHRLibAsyncProcess
{
    partial class AsyncProcessDemo
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BConnect = new System.Windows.Forms.Button();
            this.RBCHRC = new System.Windows.Forms.RadioButton();
            this.RBCLS = new System.Windows.Forms.RadioButton();
            this.RBCHR2 = new System.Windows.Forms.RadioButton();
            this.RBFirst = new System.Windows.Forms.RadioButton();
            this.TBConInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.RTRsp = new System.Windows.Forms.RichTextBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.BtnSendCmd = new System.Windows.Forms.Button();
            this.TBCMD = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.TBSODX = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TBAVD = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TBSHZ = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TBMOD = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.TBSODX);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.TBAVD);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.TBSHZ);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.TBMOD);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.BConnect);
            this.panel1.Controls.Add(this.RBCHRC);
            this.panel1.Controls.Add(this.RBCLS);
            this.panel1.Controls.Add(this.RBCHR2);
            this.panel1.Controls.Add(this.RBFirst);
            this.panel1.Controls.Add(this.TBConInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(693, 54);
            this.panel1.TabIndex = 3;
            // 
            // BConnect
            // 
            this.BConnect.Location = new System.Drawing.Point(585, 9);
            this.BConnect.Margin = new System.Windows.Forms.Padding(2);
            this.BConnect.Name = "BConnect";
            this.BConnect.Size = new System.Drawing.Size(100, 40);
            this.BConnect.TabIndex = 8;
            this.BConnect.Tag = "0";
            this.BConnect.Text = "Connect";
            this.BConnect.UseVisualStyleBackColor = true;
            this.BConnect.Click += new System.EventHandler(this.BConnect_Click);
            // 
            // RBCHRC
            // 
            this.RBCHRC.AutoSize = true;
            this.RBCHRC.Location = new System.Drawing.Point(509, 9);
            this.RBCHRC.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHRC.Name = "RBCHRC";
            this.RBCHRC.Size = new System.Drawing.Size(58, 17);
            this.RBCHRC.TabIndex = 6;
            this.RBCHRC.Tag = "3";
            this.RBCHRC.Text = "CHR C";
            this.RBCHRC.UseVisualStyleBackColor = true;
            // 
            // RBCLS
            // 
            this.RBCLS.AutoSize = true;
            this.RBCLS.Location = new System.Drawing.Point(390, 8);
            this.RBCLS.Margin = new System.Windows.Forms.Padding(2);
            this.RBCLS.Name = "RBCLS";
            this.RBCLS.Size = new System.Drawing.Size(115, 17);
            this.RBCLS.TabIndex = 5;
            this.RBCLS.Tag = "2";
            this.RBCLS.Text = "Multi-Channel CHR";
            this.RBCLS.UseVisualStyleBackColor = true;
            // 
            // RBCHR2
            // 
            this.RBCHR2.AutoSize = true;
            this.RBCHR2.Checked = true;
            this.RBCHR2.Location = new System.Drawing.Point(335, 8);
            this.RBCHR2.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHR2.Name = "RBCHR2";
            this.RBCHR2.Size = new System.Drawing.Size(51, 17);
            this.RBCHR2.TabIndex = 4;
            this.RBCHR2.TabStop = true;
            this.RBCHR2.Tag = "1";
            this.RBCHR2.Text = "CHR²";
            this.RBCHR2.UseVisualStyleBackColor = true;
            // 
            // RBFirst
            // 
            this.RBFirst.AutoSize = true;
            this.RBFirst.Location = new System.Drawing.Point(206, 8);
            this.RBFirst.Margin = new System.Windows.Forms.Padding(2);
            this.RBFirst.Name = "RBFirst";
            this.RBFirst.Size = new System.Drawing.Size(125, 17);
            this.RBFirst.TabIndex = 3;
            this.RBFirst.Tag = "0";
            this.RBFirst.Text = "First Generation CHR";
            this.RBFirst.UseVisualStyleBackColor = true;
            // 
            // TBConInfo
            // 
            this.TBConInfo.Location = new System.Drawing.Point(86, 7);
            this.TBConInfo.Margin = new System.Windows.Forms.Padding(2);
            this.TBConInfo.Name = "TBConInfo";
            this.TBConInfo.Size = new System.Drawing.Size(116, 20);
            this.TBConInfo.TabIndex = 2;
            this.TBConInfo.Text = "192.168.170.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection Info:";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.RTRsp);
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(522, 54);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(171, 445);
            this.panel3.TabIndex = 5;
            // 
            // RTRsp
            // 
            this.RTRsp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RTRsp.Location = new System.Drawing.Point(0, 90);
            this.RTRsp.Margin = new System.Windows.Forms.Padding(2);
            this.RTRsp.Name = "RTRsp";
            this.RTRsp.ReadOnly = true;
            this.RTRsp.Size = new System.Drawing.Size(171, 355);
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
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(171, 90);
            this.panel6.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 72);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Received Response";
            // 
            // BtnSendCmd
            // 
            this.BtnSendCmd.Enabled = false;
            this.BtnSendCmd.Location = new System.Drawing.Point(112, 46);
            this.BtnSendCmd.Margin = new System.Windows.Forms.Padding(2);
            this.BtnSendCmd.Name = "BtnSendCmd";
            this.BtnSendCmd.Size = new System.Drawing.Size(56, 19);
            this.BtnSendCmd.TabIndex = 6;
            this.BtnSendCmd.Text = "Send";
            this.BtnSendCmd.UseVisualStyleBackColor = true;
            this.BtnSendCmd.Click += new System.EventHandler(this.BtnSendCmd_Click);
            // 
            // TBCMD
            // 
            this.TBCMD.Enabled = false;
            this.TBCMD.Location = new System.Drawing.Point(8, 24);
            this.TBCMD.Margin = new System.Windows.Forms.Padding(2);
            this.TBCMD.Name = "TBCMD";
            this.TBCMD.Size = new System.Drawing.Size(155, 20);
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
            this.panel4.Controls.Add(this.chart3);
            this.panel4.Controls.Add(this.chart2);
            this.panel4.Controls.Add(this.chart1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 54);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(522, 445);
            this.panel4.TabIndex = 7;
            // 
            // chart3
            // 
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.Name = "ChartArea1";
            this.chart3.ChartAreas.Add(chartArea1);
            this.chart3.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chart3.Legends.Add(legend1);
            this.chart3.Location = new System.Drawing.Point(0, 276);
            this.chart3.Margin = new System.Windows.Forms.Padding(1);
            this.chart3.Name = "chart3";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart3.Series.Add(series1);
            this.chart3.Size = new System.Drawing.Size(522, 169);
            this.chart3.TabIndex = 11;
            this.chart3.Text = "chart3";
            // 
            // chart2
            // 
            chartArea2.AxisX.IsLabelAutoFit = false;
            chartArea2.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.IsLabelAutoFit = false;
            chartArea2.AxisY.IsStartedFromZero = false;
            chartArea2.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.Name = "ChartArea1";
            this.chart2.ChartAreas.Add(chartArea2);
            this.chart2.Dock = System.Windows.Forms.DockStyle.Top;
            legend2.Enabled = false;
            legend2.Name = "Legend1";
            this.chart2.Legends.Add(legend2);
            this.chart2.Location = new System.Drawing.Point(0, 141);
            this.chart2.Margin = new System.Windows.Forms.Padding(1);
            this.chart2.Name = "chart2";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chart2.Series.Add(series2);
            this.chart2.Size = new System.Drawing.Size(522, 135);
            this.chart2.TabIndex = 10;
            this.chart2.Text = "chart2";
            // 
            // chart1
            // 
            chartArea3.AxisX.IsLabelAutoFit = false;
            chartArea3.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea3.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisY.IsLabelAutoFit = false;
            chartArea3.AxisY.IsStartedFromZero = false;
            chartArea3.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea3.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea3.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea3);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Top;
            legend3.Enabled = false;
            legend3.Name = "Legend1";
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Margin = new System.Windows.Forms.Padding(1);
            this.chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series3.IsVisibleInLegend = false;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(522, 141);
            this.chart1.TabIndex = 9;
            this.chart1.Text = "chart1";
            // 
            // TBSODX
            // 
            this.TBSODX.Location = new System.Drawing.Point(438, 29);
            this.TBSODX.Margin = new System.Windows.Forms.Padding(2);
            this.TBSODX.Name = "TBSODX";
            this.TBSODX.ReadOnly = true;
            this.TBSODX.Size = new System.Drawing.Size(100, 20);
            this.TBSODX.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(358, 31);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Output Signals:";
            // 
            // TBAVD
            // 
            this.TBAVD.Location = new System.Drawing.Point(316, 29);
            this.TBAVD.Margin = new System.Windows.Forms.Padding(2);
            this.TBAVD.Name = "TBAVD";
            this.TBAVD.ReadOnly = true;
            this.TBAVD.Size = new System.Drawing.Size(38, 20);
            this.TBAVD.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 31);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Data Average:";
            // 
            // TBSHZ
            // 
            this.TBSHZ.Location = new System.Drawing.Point(196, 29);
            this.TBSHZ.Margin = new System.Windows.Forms.Padding(2);
            this.TBSHZ.Name = "TBSHZ";
            this.TBSHZ.ReadOnly = true;
            this.TBSHZ.Size = new System.Drawing.Size(38, 20);
            this.TBSHZ.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Scan Rate:";
            // 
            // TBMOD
            // 
            this.TBMOD.Location = new System.Drawing.Point(94, 30);
            this.TBMOD.Margin = new System.Windows.Forms.Padding(2);
            this.TBMOD.Name = "TBMOD";
            this.TBMOD.ReadOnly = true;
            this.TBMOD.Size = new System.Drawing.Size(38, 20);
            this.TBMOD.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Measuring Mode:";
            // 
            // AsyncDemo2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 499);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AsyncDemo2";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AsyncProcessFormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
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
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RichTextBox RTRsp;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button BtnSendCmd;
        private System.Windows.Forms.TextBox TBCMD;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private System.Windows.Forms.TextBox TBSODX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TBAVD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBMOD;
        private System.Windows.Forms.Label label2;
        //private CustomeControlTest1.RoundedPanel roundedPanel1;
    }
}

