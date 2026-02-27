namespace TCHRLibCLS2CalibPlugin
{
    partial class MainForm
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
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BtConnect = new System.Windows.Forms.Button();
            this.RBAsyncConn = new System.Windows.Forms.RadioButton();
            this.RBSyncConn = new System.Windows.Forms.RadioButton();
            this.TBConnInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.BtPluginEnable = new System.Windows.Forms.Button();
            this.RTResponse = new System.Windows.Forms.RichTextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.TBCommand = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.chartLower = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartUpper = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel6 = new System.Windows.Forms.Panel();
            this.TBSHZ = new System.Windows.Forms.TextBox();
            this.TBSODX = new System.Windows.Forms.TextBox();
            this.LSampleNo = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.TimerProcess = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartUpper)).BeginInit();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BtConnect);
            this.panel1.Controls.Add(this.RBAsyncConn);
            this.panel1.Controls.Add(this.RBSyncConn);
            this.panel1.Controls.Add(this.TBConnInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(548, 46);
            this.panel1.TabIndex = 0;
            // 
            // BtConnect
            // 
            this.BtConnect.Location = new System.Drawing.Point(423, 4);
            this.BtConnect.Margin = new System.Windows.Forms.Padding(2);
            this.BtConnect.Name = "BtConnect";
            this.BtConnect.Size = new System.Drawing.Size(116, 37);
            this.BtConnect.TabIndex = 5;
            this.BtConnect.Text = "Connect";
            this.BtConnect.UseVisualStyleBackColor = true;
            this.BtConnect.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // RBAsyncConn
            // 
            this.RBAsyncConn.AutoSize = true;
            this.RBAsyncConn.Location = new System.Drawing.Point(248, 24);
            this.RBAsyncConn.Margin = new System.Windows.Forms.Padding(2);
            this.RBAsyncConn.Name = "RBAsyncConn";
            this.RBAsyncConn.Size = new System.Drawing.Size(149, 17);
            this.RBAsyncConn.TabIndex = 4;
            this.RBAsyncConn.Text = "Asynchronous Connection";
            this.RBAsyncConn.UseVisualStyleBackColor = true;
            // 
            // RBSyncConn
            // 
            this.RBSyncConn.AutoSize = true;
            this.RBSyncConn.Checked = true;
            this.RBSyncConn.Location = new System.Drawing.Point(248, 4);
            this.RBSyncConn.Margin = new System.Windows.Forms.Padding(2);
            this.RBSyncConn.Name = "RBSyncConn";
            this.RBSyncConn.Size = new System.Drawing.Size(144, 17);
            this.RBSyncConn.TabIndex = 3;
            this.RBSyncConn.TabStop = true;
            this.RBSyncConn.Text = "Synchronous Connection";
            this.RBSyncConn.UseVisualStyleBackColor = true;
            // 
            // TBConnInfo
            // 
            this.TBConnInfo.Location = new System.Drawing.Point(91, 13);
            this.TBConnInfo.Margin = new System.Windows.Forms.Padding(2);
            this.TBConnInfo.Name = "TBConnInfo";
            this.TBConnInfo.Size = new System.Drawing.Size(137, 20);
            this.TBConnInfo.TabIndex = 1;
            this.TBConnInfo.Text = "192.168.170.3";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection Info:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.BtPluginEnable);
            this.panel2.Controls.Add(this.RTResponse);
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(417, 46);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(131, 380);
            this.panel2.TabIndex = 1;
            // 
            // BtPluginEnable
            // 
            this.BtPluginEnable.Enabled = false;
            this.BtPluginEnable.Location = new System.Drawing.Point(7, 332);
            this.BtPluginEnable.Name = "BtPluginEnable";
            this.BtPluginEnable.Size = new System.Drawing.Size(115, 40);
            this.BtPluginEnable.TabIndex = 3;
            this.BtPluginEnable.Text = "Disable Correction";
            this.BtPluginEnable.UseVisualStyleBackColor = true;
            this.BtPluginEnable.Click += new System.EventHandler(this.BtPluginEnable_Click);
            // 
            // RTResponse
            // 
            this.RTResponse.Location = new System.Drawing.Point(-1, 63);
            this.RTResponse.Margin = new System.Windows.Forms.Padding(2);
            this.RTResponse.Name = "RTResponse";
            this.RTResponse.Size = new System.Drawing.Size(132, 264);
            this.RTResponse.TabIndex = 2;
            this.RTResponse.Text = "";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label3);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 46);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(131, 18);
            this.panel5.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 2);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Response:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.TBCommand);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(131, 46);
            this.panel4.TabIndex = 0;
            // 
            // TBCommand
            // 
            this.TBCommand.Location = new System.Drawing.Point(2, 19);
            this.TBCommand.Margin = new System.Windows.Forms.Padding(2);
            this.TBCommand.Name = "TBCommand";
            this.TBCommand.Size = new System.Drawing.Size(128, 20);
            this.TBCommand.TabIndex = 1;
            this.TBCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBCommand_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 2);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Device Command:";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.splitter1);
            this.panel3.Controls.Add(this.chartLower);
            this.panel3.Controls.Add(this.chartUpper);
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 46);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(417, 380);
            this.panel3.TabIndex = 2;
            // 
            // splitter1
            // 
            this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 204);
            this.splitter1.Margin = new System.Windows.Forms.Padding(2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(417, 2);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // chartLower
            // 
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.Title = "Channel";
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.Name = "ChartArea1";
            this.chartLower.ChartAreas.Add(chartArea1);
            this.chartLower.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chartLower.Legends.Add(legend1);
            this.chartLower.Location = new System.Drawing.Point(0, 204);
            this.chartLower.Margin = new System.Windows.Forms.Padding(1);
            this.chartLower.Name = "chartLower";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartLower.Series.Add(series1);
            this.chartLower.Size = new System.Drawing.Size(417, 176);
            this.chartLower.TabIndex = 2;
            this.chartLower.Text = "chart1";
            title1.Name = "Title1";
            title1.Text = "First Peak Signal";
            this.chartLower.Titles.Add(title1);
            // 
            // chartUpper
            // 
            chartArea2.AxisX.IsLabelAutoFit = false;
            chartArea2.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisX.Title = "Sample Index";
            chartArea2.AxisY.IsLabelAutoFit = false;
            chartArea2.AxisY.IsStartedFromZero = false;
            chartArea2.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.Name = "ChartArea1";
            this.chartUpper.ChartAreas.Add(chartArea2);
            this.chartUpper.Dock = System.Windows.Forms.DockStyle.Top;
            legend2.Enabled = false;
            legend2.Name = "Legend1";
            this.chartUpper.Legends.Add(legend2);
            this.chartUpper.Location = new System.Drawing.Point(0, 42);
            this.chartUpper.Margin = new System.Windows.Forms.Padding(1);
            this.chartUpper.Name = "chartUpper";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartUpper.Series.Add(series2);
            this.chartUpper.Size = new System.Drawing.Size(417, 162);
            this.chartUpper.TabIndex = 1;
            this.chartUpper.Text = "chart2";
            title2.Name = "Title1";
            title2.Text = "First Global Signal";
            this.chartUpper.Titles.Add(title2);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.TBSHZ);
            this.panel6.Controls.Add(this.TBSODX);
            this.panel6.Controls.Add(this.LSampleNo);
            this.panel6.Controls.Add(this.label4);
            this.panel6.Controls.Add(this.label7);
            this.panel6.Controls.Add(this.label5);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(417, 42);
            this.panel6.TabIndex = 0;
            // 
            // TBSHZ
            // 
            this.TBSHZ.Location = new System.Drawing.Point(305, 2);
            this.TBSHZ.Margin = new System.Windows.Forms.Padding(2);
            this.TBSHZ.Name = "TBSHZ";
            this.TBSHZ.Size = new System.Drawing.Size(101, 20);
            this.TBSHZ.TabIndex = 7;
            this.TBSHZ.Text = "35";
            this.TBSHZ.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSHZ_KeyPress);
            // 
            // TBSODX
            // 
            this.TBSODX.Location = new System.Drawing.Point(76, 2);
            this.TBSODX.Margin = new System.Windows.Forms.Padding(2);
            this.TBSODX.Name = "TBSODX";
            this.TBSODX.Size = new System.Drawing.Size(149, 20);
            this.TBSODX.TabIndex = 6;
            this.TBSODX.Text = "83, 16640, 16641";
            this.TBSODX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSODX_KeyPress);
            // 
            // LSampleNo
            // 
            this.LSampleNo.AutoSize = true;
            this.LSampleNo.Location = new System.Drawing.Point(149, 26);
            this.LSampleNo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LSampleNo.Name = "LSampleNo";
            this.LSampleNo.Size = new System.Drawing.Size(0, 13);
            this.LSampleNo.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(228, 2);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Sample Rate: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(2, 26);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(152, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Received Sample per Second:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 2);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Output Signal: ";
            // 
            // TimerProcess
            // 
            this.TimerProcess.Interval = 50;
            this.TimerProcess.Tick += new System.EventHandler(this.TimerProcess_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 426);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartLower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartUpper)).EndInit();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BtConnect;
        private System.Windows.Forms.RadioButton RBAsyncConn;
        private System.Windows.Forms.RadioButton RBSyncConn;
        private System.Windows.Forms.TextBox TBConnInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox RTResponse;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox TBCommand;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartLower;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartUpper;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.TextBox TBSODX;
        private System.Windows.Forms.Label LSampleNo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer TimerProcess;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Button BtPluginEnable;
    }
}

