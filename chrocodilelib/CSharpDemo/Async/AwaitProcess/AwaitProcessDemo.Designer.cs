namespace TCHRLibAwaitProcess
{
    partial class AwaitProcessDemo
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
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
            this.TBMMD = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chart3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
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
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(558, 54);
            this.panel1.TabIndex = 3;
            // 
            // BConnect
            // 
            this.BConnect.Location = new System.Drawing.Point(440, 9);
            this.BConnect.Margin = new System.Windows.Forms.Padding(2);
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
            this.RBCHRC.Margin = new System.Windows.Forms.Padding(2);
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
            this.RBCLS.Margin = new System.Windows.Forms.Padding(2);
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
            this.RBCHR2.Margin = new System.Windows.Forms.Padding(2);
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
            this.RBFirst.Margin = new System.Windows.Forms.Padding(2);
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
            this.TBConInfo.Margin = new System.Windows.Forms.Padding(2);
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
            this.panel2.Controls.Add(this.TBMMD);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 54);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(558, 30);
            this.panel2.TabIndex = 4;
            // 
            // TBSODX
            // 
            this.TBSODX.Location = new System.Drawing.Point(442, 6);
            this.TBSODX.Margin = new System.Windows.Forms.Padding(2);
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
            this.TBAVD.Margin = new System.Windows.Forms.Padding(2);
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
            this.TBSHZ.Margin = new System.Windows.Forms.Padding(2);
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
            // TBMMD
            // 
            this.TBMMD.Location = new System.Drawing.Point(98, 7);
            this.TBMMD.Margin = new System.Windows.Forms.Padding(2);
            this.TBMMD.Name = "TBMMD";
            this.TBMMD.ReadOnly = true;
            this.TBMMD.Size = new System.Drawing.Size(38, 20);
            this.TBMMD.TabIndex = 1;
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
            // panel4
            // 
            this.panel4.Controls.Add(this.chart3);
            this.panel4.Controls.Add(this.chart2);
            this.panel4.Controls.Add(this.chart1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 84);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(558, 412);
            this.panel4.TabIndex = 7;
            // 
            // chart3
            // 
            chartArea4.AxisX.IsLabelAutoFit = false;
            chartArea4.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea4.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisY.IsLabelAutoFit = false;
            chartArea4.AxisY.IsStartedFromZero = false;
            chartArea4.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea4.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea4.Name = "ChartArea1";
            this.chart3.ChartAreas.Add(chartArea4);
            this.chart3.Dock = System.Windows.Forms.DockStyle.Fill;
            legend4.Enabled = false;
            legend4.Name = "Legend1";
            this.chart3.Legends.Add(legend4);
            this.chart3.Location = new System.Drawing.Point(0, 276);
            this.chart3.Margin = new System.Windows.Forms.Padding(1);
            this.chart3.Name = "chart3";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series4.IsVisibleInLegend = false;
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chart3.Series.Add(series4);
            this.chart3.Size = new System.Drawing.Size(558, 136);
            this.chart3.TabIndex = 14;
            this.chart3.Text = "chart3";
            // 
            // chart2
            // 
            chartArea5.AxisX.IsLabelAutoFit = false;
            chartArea5.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea5.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisY.IsLabelAutoFit = false;
            chartArea5.AxisY.IsStartedFromZero = false;
            chartArea5.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea5.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea5.Name = "ChartArea1";
            this.chart2.ChartAreas.Add(chartArea5);
            this.chart2.Dock = System.Windows.Forms.DockStyle.Top;
            legend5.Enabled = false;
            legend5.Name = "Legend1";
            this.chart2.Legends.Add(legend5);
            this.chart2.Location = new System.Drawing.Point(0, 141);
            this.chart2.Margin = new System.Windows.Forms.Padding(1);
            this.chart2.Name = "chart2";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series5.IsVisibleInLegend = false;
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            this.chart2.Series.Add(series5);
            this.chart2.Size = new System.Drawing.Size(558, 135);
            this.chart2.TabIndex = 13;
            this.chart2.Text = "chart2";
            // 
            // chart1
            // 
            chartArea6.AxisX.IsLabelAutoFit = false;
            chartArea6.AxisX.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea6.AxisX.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisX.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisX.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisY.IsLabelAutoFit = false;
            chartArea6.AxisY.IsStartedFromZero = false;
            chartArea6.AxisY.LabelStyle.Font = new System.Drawing.Font("Modern No. 20", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea6.AxisY.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisY.MajorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.AxisY.MinorTickMark.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea6.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea6);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Top;
            legend6.Enabled = false;
            legend6.Name = "Legend1";
            this.chart1.Legends.Add(legend6);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Margin = new System.Windows.Forms.Padding(1);
            this.chart1.Name = "chart1";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series6.IsVisibleInLegend = false;
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            this.chart1.Series.Add(series6);
            this.chart1.Size = new System.Drawing.Size(558, 141);
            this.chart1.TabIndex = 12;
            this.chart1.Text = "chart1";
            // 
            // AwaitProcessDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 496);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AwaitProcessDemo";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AwaitProcess_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox TBMMD;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TBSODX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TBAVD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TBSHZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        //private CustomeControlTest1.RoundedPanel roundedPanel1;
    }
}

