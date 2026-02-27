namespace TCHRLibBasicRecordSample
{
    partial class TRecordSample
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelRecordingHint = new System.Windows.Forms.Label();
            this.chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.CBFlush = new System.Windows.Forms.CheckBox();
            this.TBSampleCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BtSave = new System.Windows.Forms.Button();
            this.BtRecord = new System.Windows.Forms.Button();
            this.panel9 = new System.Windows.Forms.Panel();
            this.RBInterfero = new System.Windows.Forms.RadioButton();
            this.RBConfocal = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.TBSHZ = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TBSODX = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel10 = new System.Windows.Forms.Panel();
            this.RBCHRC = new System.Windows.Forms.RadioButton();
            this.BtDisCon = new System.Windows.Forms.Button();
            this.BtConnect = new System.Windows.Forms.Button();
            this.RBCLS = new System.Windows.Forms.RadioButton();
            this.RBCHR2 = new System.Windows.Forms.RadioButton();
            this.RBCHR1 = new System.Windows.Forms.RadioButton();
            this.TbConInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timerData = new System.Windows.Forms.Timer(this.components);
            this.SaveDlg = new System.Windows.Forms.SaveFileDialog();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panel10.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.labelRecordingHint);
            this.panel3.Controls.Add(this.chart3);
            this.panel3.Controls.Add(this.chart2);
            this.panel3.Controls.Add(this.chart1);
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(764, 510);
            this.panel3.TabIndex = 5;
            // 
            // labelRecordingHint
            // 
            this.labelRecordingHint.AutoSize = true;
            this.labelRecordingHint.BackColor = System.Drawing.Color.White;
            this.labelRecordingHint.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRecordingHint.Location = new System.Drawing.Point(20, 238);
            this.labelRecordingHint.Name = "labelRecordingHint";
            this.labelRecordingHint.Size = new System.Drawing.Size(329, 37);
            this.labelRecordingHint.TabIndex = 16;
            this.labelRecordingHint.Text = "Press Start Recording";
            this.labelRecordingHint.Visible = false;
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
            this.chart3.Location = new System.Drawing.Point(0, 350);
            this.chart3.Margin = new System.Windows.Forms.Padding(1);
            this.chart3.Name = "chart3";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart3.Series.Add(series1);
            this.chart3.Size = new System.Drawing.Size(764, 160);
            this.chart3.TabIndex = 15;
            this.chart3.Text = "chart4";
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
            this.chart2.Location = new System.Drawing.Point(0, 215);
            this.chart2.Margin = new System.Windows.Forms.Padding(1);
            this.chart2.Name = "chart2";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chart2.Series.Add(series2);
            this.chart2.Size = new System.Drawing.Size(764, 135);
            this.chart2.TabIndex = 14;
            this.chart2.Text = "chart3";
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
            this.chart1.Location = new System.Drawing.Point(0, 74);
            this.chart1.Margin = new System.Windows.Forms.Padding(1);
            this.chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series3.IsVisibleInLegend = false;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(764, 141);
            this.chart1.TabIndex = 13;
            this.chart1.Text = "chart2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.panel9);
            this.panel1.Controls.Add(this.panel10);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(764, 74);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.CBFlush);
            this.panel2.Controls.Add(this.TBSampleCount);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.BtSave);
            this.panel2.Controls.Add(this.BtRecord);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(569, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(195, 74);
            this.panel2.TabIndex = 13;
            // 
            // CBFlush
            // 
            this.CBFlush.AutoSize = true;
            this.CBFlush.Checked = true;
            this.CBFlush.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBFlush.Location = new System.Drawing.Point(3, 46);
            this.CBFlush.Name = "CBFlush";
            this.CBFlush.Size = new System.Drawing.Size(96, 17);
            this.CBFlush.TabIndex = 5;
            this.CBFlush.Text = "Flush Old Data";
            this.CBFlush.UseVisualStyleBackColor = true;
            // 
            // TBSampleCount
            // 
            this.TBSampleCount.Location = new System.Drawing.Point(5, 19);
            this.TBSampleCount.Margin = new System.Windows.Forms.Padding(2);
            this.TBSampleCount.Name = "TBSampleCount";
            this.TBSampleCount.Size = new System.Drawing.Size(94, 20);
            this.TBSampleCount.TabIndex = 4;
            this.TBSampleCount.Text = "10000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sample Count:";
            // 
            // BtSave
            // 
            this.BtSave.Enabled = false;
            this.BtSave.Location = new System.Drawing.Point(110, 45);
            this.BtSave.Name = "BtSave";
            this.BtSave.Size = new System.Drawing.Size(73, 23);
            this.BtSave.TabIndex = 1;
            this.BtSave.Text = "Save Data";
            this.BtSave.UseVisualStyleBackColor = true;
            this.BtSave.Click += new System.EventHandler(this.BtSave_Click);
            // 
            // BtRecord
            // 
            this.BtRecord.Location = new System.Drawing.Point(110, 3);
            this.BtRecord.Name = "BtRecord";
            this.BtRecord.Size = new System.Drawing.Size(73, 38);
            this.BtRecord.TabIndex = 0;
            this.BtRecord.Tag = "0";
            this.BtRecord.Text = "Start Recording";
            this.BtRecord.UseVisualStyleBackColor = true;
            this.BtRecord.Click += new System.EventHandler(this.BtRecord_Click);
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.RBInterfero);
            this.panel9.Controls.Add(this.RBConfocal);
            this.panel9.Controls.Add(this.label6);
            this.panel9.Controls.Add(this.TBSHZ);
            this.panel9.Controls.Add(this.label5);
            this.panel9.Controls.Add(this.TBSODX);
            this.panel9.Controls.Add(this.label4);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel9.Location = new System.Drawing.Point(306, 0);
            this.panel9.Margin = new System.Windows.Forms.Padding(2);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(263, 74);
            this.panel9.TabIndex = 11;
            // 
            // RBInterfero
            // 
            this.RBInterfero.AutoSize = true;
            this.RBInterfero.Location = new System.Drawing.Point(172, 7);
            this.RBInterfero.Margin = new System.Windows.Forms.Padding(2);
            this.RBInterfero.Name = "RBInterfero";
            this.RBInterfero.Size = new System.Drawing.Size(92, 17);
            this.RBInterfero.TabIndex = 6;
            this.RBInterfero.Text = "Interferometric";
            this.RBInterfero.UseVisualStyleBackColor = true;
            this.RBInterfero.Click += new System.EventHandler(this.RBConfocal_Click);
            // 
            // RBConfocal
            // 
            this.RBConfocal.AutoSize = true;
            this.RBConfocal.Checked = true;
            this.RBConfocal.Location = new System.Drawing.Point(105, 7);
            this.RBConfocal.Margin = new System.Windows.Forms.Padding(2);
            this.RBConfocal.Name = "RBConfocal";
            this.RBConfocal.Size = new System.Drawing.Size(67, 17);
            this.RBConfocal.TabIndex = 5;
            this.RBConfocal.TabStop = true;
            this.RBConfocal.Text = "Confocal";
            this.RBConfocal.UseVisualStyleBackColor = true;
            this.RBConfocal.Click += new System.EventHandler(this.RBConfocal_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 9);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Measuring Methos:";
            // 
            // TBSHZ
            // 
            this.TBSHZ.Location = new System.Drawing.Point(92, 25);
            this.TBSHZ.Margin = new System.Windows.Forms.Padding(2);
            this.TBSHZ.Name = "TBSHZ";
            this.TBSHZ.Size = new System.Drawing.Size(85, 20);
            this.TBSHZ.TabIndex = 3;
            this.TBSHZ.Text = "2000";
            this.TBSHZ.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSHZ_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 28);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Scanrate:";
            // 
            // TBSODX
            // 
            this.TBSODX.Location = new System.Drawing.Point(92, 46);
            this.TBSODX.Margin = new System.Windows.Forms.Padding(2);
            this.TBSODX.Name = "TBSODX";
            this.TBSODX.Size = new System.Drawing.Size(109, 20);
            this.TBSODX.TabIndex = 1;
            this.TBSODX.Text = "83, 256, 257";
            this.TBSODX.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSODX_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 47);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Selected signals:";
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.RBCHRC);
            this.panel10.Controls.Add(this.BtDisCon);
            this.panel10.Controls.Add(this.BtConnect);
            this.panel10.Controls.Add(this.RBCLS);
            this.panel10.Controls.Add(this.RBCHR2);
            this.panel10.Controls.Add(this.RBCHR1);
            this.panel10.Controls.Add(this.TbConInfo);
            this.panel10.Controls.Add(this.label1);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel10.Location = new System.Drawing.Point(0, 0);
            this.panel10.Margin = new System.Windows.Forms.Padding(2);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(306, 74);
            this.panel10.TabIndex = 10;
            // 
            // RBCHRC
            // 
            this.RBCHRC.AutoSize = true;
            this.RBCHRC.Location = new System.Drawing.Point(146, 54);
            this.RBCHRC.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHRC.Name = "RBCHRC";
            this.RBCHRC.Size = new System.Drawing.Size(58, 17);
            this.RBCHRC.TabIndex = 16;
            this.RBCHRC.Text = "CHR C";
            this.RBCHRC.UseVisualStyleBackColor = true;
            // 
            // BtDisCon
            // 
            this.BtDisCon.Enabled = false;
            this.BtDisCon.Location = new System.Drawing.Point(225, 40);
            this.BtDisCon.Margin = new System.Windows.Forms.Padding(2);
            this.BtDisCon.Name = "BtDisCon";
            this.BtDisCon.Size = new System.Drawing.Size(76, 28);
            this.BtDisCon.TabIndex = 15;
            this.BtDisCon.Text = "Disconnect";
            this.BtDisCon.UseVisualStyleBackColor = true;
            this.BtDisCon.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // BtConnect
            // 
            this.BtConnect.Location = new System.Drawing.Point(233, 4);
            this.BtConnect.Margin = new System.Windows.Forms.Padding(2);
            this.BtConnect.Name = "BtConnect";
            this.BtConnect.Size = new System.Drawing.Size(68, 32);
            this.BtConnect.TabIndex = 14;
            this.BtConnect.Text = "Connect";
            this.BtConnect.UseVisualStyleBackColor = true;
            this.BtConnect.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // RBCLS
            // 
            this.RBCLS.AutoSize = true;
            this.RBCLS.Location = new System.Drawing.Point(8, 54);
            this.RBCLS.Margin = new System.Windows.Forms.Padding(2);
            this.RBCLS.Name = "RBCLS";
            this.RBCLS.Size = new System.Drawing.Size(115, 17);
            this.RBCLS.TabIndex = 13;
            this.RBCLS.Text = "Multi-Channel CHR";
            this.RBCLS.UseVisualStyleBackColor = true;
            // 
            // RBCHR2
            // 
            this.RBCHR2.AutoSize = true;
            this.RBCHR2.Checked = true;
            this.RBCHR2.Location = new System.Drawing.Point(146, 33);
            this.RBCHR2.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHR2.Name = "RBCHR2";
            this.RBCHR2.Size = new System.Drawing.Size(51, 17);
            this.RBCHR2.TabIndex = 12;
            this.RBCHR2.TabStop = true;
            this.RBCHR2.Text = "CHR²";
            this.RBCHR2.UseVisualStyleBackColor = true;
            // 
            // RBCHR1
            // 
            this.RBCHR1.AutoSize = true;
            this.RBCHR1.Location = new System.Drawing.Point(8, 33);
            this.RBCHR1.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHR1.Name = "RBCHR1";
            this.RBCHR1.Size = new System.Drawing.Size(125, 17);
            this.RBCHR1.TabIndex = 11;
            this.RBCHR1.Text = "First Generation CHR";
            this.RBCHR1.UseVisualStyleBackColor = true;
            // 
            // TbConInfo
            // 
            this.TbConInfo.Location = new System.Drawing.Point(92, 12);
            this.TbConInfo.Margin = new System.Windows.Forms.Padding(2);
            this.TbConInfo.Name = "TbConInfo";
            this.TbConInfo.Size = new System.Drawing.Size(138, 20);
            this.TbConInfo.TabIndex = 10;
            this.TbConInfo.Text = "192.168.170.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Connection Info";
            // 
            // timerData
            // 
            this.timerData.Tick += new System.EventHandler(this.timerData_Tick);
            // 
            // SaveDlg
            // 
            this.SaveDlg.Filter = "Text Files (*.txt)|*.txt";
            // 
            // TRecordSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 510);
            this.Controls.Add(this.panel3);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TRecordSample";
            this.Text = "CHRDLL Demo";
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.RadioButton RBConfocal;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TBSODX;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.RadioButton RBCHRC;
        private System.Windows.Forms.Button BtDisCon;
        private System.Windows.Forms.Button BtConnect;
        private System.Windows.Forms.RadioButton RBCLS;
        private System.Windows.Forms.RadioButton RBCHR2;
        private System.Windows.Forms.RadioButton RBCHR1;
        private System.Windows.Forms.TextBox TbConInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton RBInterfero;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button BtRecord;
        private System.Windows.Forms.Timer timerData;
        private System.Windows.Forms.Button BtSave;
        private System.Windows.Forms.TextBox TBSampleCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CBFlush;
        private System.Windows.Forms.SaveFileDialog SaveDlg;
        private System.Windows.Forms.Label labelRecordingHint;
    }
}

