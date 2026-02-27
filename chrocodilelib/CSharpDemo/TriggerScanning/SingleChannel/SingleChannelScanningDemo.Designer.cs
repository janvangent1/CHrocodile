namespace TCHRLibSingleChannelScanning
{
    partial class SingleChannelScanningDemo
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.RBCHRC = new System.Windows.Forms.RadioButton();
            this.BtDisCon = new System.Windows.Forms.Button();
            this.BtConnect = new System.Windows.Forms.Button();
            this.RBCHR2 = new System.Windows.Forms.RadioButton();
            this.RBCHRNomal = new System.Windows.Forms.RadioButton();
            this.TbConInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.TBSignal = new System.Windows.Forms.TextBox();
            this.TBSampleNo = new System.Windows.Forms.TextBox();
            this.TBLineNo = new System.Windows.Forms.TextBox();
            this.BtScan = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.TBSigMax = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.TBSigMin = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.CBDisplaySig = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.PPaint = new System.Windows.Forms.Panel();
            this.timerProcess = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.RBCHRC);
            this.panel1.Controls.Add(this.BtDisCon);
            this.panel1.Controls.Add(this.BtConnect);
            this.panel1.Controls.Add(this.RBCHR2);
            this.panel1.Controls.Add(this.RBCHRNomal);
            this.panel1.Controls.Add(this.TbConInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(535, 67);
            this.panel1.TabIndex = 17;
            // 
            // RBCHRC
            // 
            this.RBCHRC.AutoSize = true;
            this.RBCHRC.Location = new System.Drawing.Point(243, 38);
            this.RBCHRC.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHRC.Name = "RBCHRC";
            this.RBCHRC.Size = new System.Drawing.Size(58, 17);
            this.RBCHRC.TabIndex = 24;
            this.RBCHRC.Text = "CHR C";
            this.RBCHRC.UseVisualStyleBackColor = true;
            // 
            // BtDisCon
            // 
            this.BtDisCon.Enabled = false;
            this.BtDisCon.Location = new System.Drawing.Point(427, 34);
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
            this.BtConnect.Location = new System.Drawing.Point(427, 4);
            this.BtConnect.Margin = new System.Windows.Forms.Padding(2);
            this.BtConnect.Name = "BtConnect";
            this.BtConnect.Size = new System.Drawing.Size(94, 27);
            this.BtConnect.TabIndex = 22;
            this.BtConnect.Text = "Connect";
            this.BtConnect.UseVisualStyleBackColor = true;
            this.BtConnect.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // RBCHR2
            // 
            this.RBCHR2.AutoSize = true;
            this.RBCHR2.Checked = true;
            this.RBCHR2.Location = new System.Drawing.Point(151, 38);
            this.RBCHR2.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHR2.Name = "RBCHR2";
            this.RBCHR2.Size = new System.Drawing.Size(88, 17);
            this.RBCHR2.TabIndex = 20;
            this.RBCHR2.TabStop = true;
            this.RBCHR2.Text = "CHR² Device";
            this.RBCHR2.UseVisualStyleBackColor = true;
            // 
            // RBCHRNomal
            // 
            this.RBCHRNomal.AutoSize = true;
            this.RBCHRNomal.Location = new System.Drawing.Point(11, 38);
            this.RBCHRNomal.Margin = new System.Windows.Forms.Padding(2);
            this.RBCHRNomal.Name = "RBCHRNomal";
            this.RBCHRNomal.Size = new System.Drawing.Size(136, 17);
            this.RBCHRNomal.TabIndex = 19;
            this.RBCHRNomal.Text = "First Generation Device";
            this.RBCHRNomal.UseVisualStyleBackColor = true;
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
            // panel2
            // 
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
            this.panel2.Location = new System.Drawing.Point(0, 67);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(535, 132);
            this.panel2.TabIndex = 19;
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
            this.TBInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerProperty_KeyPress);
            // 
            // TBStopPos
            // 
            this.TBStopPos.Location = new System.Drawing.Point(257, 76);
            this.TBStopPos.Name = "TBStopPos";
            this.TBStopPos.Size = new System.Drawing.Size(120, 20);
            this.TBStopPos.TabIndex = 28;
            this.TBStopPos.Text = "10000";
            this.TBStopPos.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerProperty_KeyPress);
            // 
            // TBStartPos
            // 
            this.TBStartPos.Location = new System.Drawing.Point(72, 76);
            this.TBStartPos.Name = "TBStartPos";
            this.TBStartPos.Size = new System.Drawing.Size(119, 20);
            this.TBStartPos.TabIndex = 27;
            this.TBStartPos.Text = "0";
            this.TBStartPos.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBEncoderTriggerProperty_KeyPress);
            // 
            // CBAxis
            // 
            this.CBAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBAxis.FormattingEnabled = true;
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
            // panel3
            // 
            this.panel3.Controls.Add(this.TBSignal);
            this.panel3.Controls.Add(this.TBSampleNo);
            this.panel3.Controls.Add(this.TBLineNo);
            this.panel3.Controls.Add(this.BtScan);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 199);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(535, 61);
            this.panel3.TabIndex = 21;
            // 
            // TBSignal
            // 
            this.TBSignal.Enabled = false;
            this.TBSignal.Location = new System.Drawing.Point(126, 32);
            this.TBSignal.Name = "TBSignal";
            this.TBSignal.Size = new System.Drawing.Size(177, 20);
            this.TBSignal.TabIndex = 34;
            this.TBSignal.Text = "256, 264, 83, 65";
            this.TBSignal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSignal_KeyPress);
            // 
            // TBSampleNo
            // 
            this.TBSampleNo.Enabled = false;
            this.TBSampleNo.Location = new System.Drawing.Point(327, 9);
            this.TBSampleNo.Name = "TBSampleNo";
            this.TBSampleNo.Size = new System.Drawing.Size(71, 20);
            this.TBSampleNo.TabIndex = 33;
            this.TBSampleNo.Text = "101";
            // 
            // TBLineNo
            // 
            this.TBLineNo.Location = new System.Drawing.Point(125, 9);
            this.TBLineNo.Name = "TBLineNo";
            this.TBLineNo.Size = new System.Drawing.Size(64, 20);
            this.TBLineNo.TabIndex = 32;
            this.TBLineNo.Text = "5";
            // 
            // BtScan
            // 
            this.BtScan.Enabled = false;
            this.BtScan.Location = new System.Drawing.Point(404, 6);
            this.BtScan.Name = "BtScan";
            this.BtScan.Size = new System.Drawing.Size(117, 52);
            this.BtScan.TabIndex = 31;
            this.BtScan.Text = "Start Scan";
            this.BtScan.UseVisualStyleBackColor = true;
            this.BtScan.Click += new System.EventHandler(this.BtScan_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Device Output Signals:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(195, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(126, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Sample Counter per Line:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Number of Scan Linear:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.TBSigMax);
            this.panel4.Controls.Add(this.label11);
            this.panel4.Controls.Add(this.TBSigMin);
            this.panel4.Controls.Add(this.label10);
            this.panel4.Controls.Add(this.CBDisplaySig);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 260);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(535, 27);
            this.panel4.TabIndex = 22;
            // 
            // TBSigMax
            // 
            this.TBSigMax.Location = new System.Drawing.Point(416, 3);
            this.TBSigMax.Name = "TBSigMax";
            this.TBSigMax.Size = new System.Drawing.Size(107, 20);
            this.TBSigMax.TabIndex = 35;
            this.TBSigMax.Text = "1000";
            this.TBSigMax.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSigMax_KeyPress);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(380, 6);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(30, 13);
            this.label11.TabIndex = 34;
            this.label11.Text = "Max:";
            // 
            // TBSigMin
            // 
            this.TBSigMin.Location = new System.Drawing.Point(257, 3);
            this.TBSigMin.Name = "TBSigMin";
            this.TBSigMin.Size = new System.Drawing.Size(107, 20);
            this.TBSigMin.TabIndex = 33;
            this.TBSigMin.Text = "0";
            this.TBSigMin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TBSigMax_KeyPress);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(220, 6);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(27, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "Min:";
            // 
            // CBDisplaySig
            // 
            this.CBDisplaySig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBDisplaySig.FormattingEnabled = true;
            this.CBDisplaySig.Location = new System.Drawing.Point(90, 3);
            this.CBDisplaySig.Name = "CBDisplaySig";
            this.CBDisplaySig.Size = new System.Drawing.Size(112, 21);
            this.CBDisplaySig.TabIndex = 27;
            this.CBDisplaySig.SelectedIndexChanged += new System.EventHandler(this.CBDisplaySig_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 6);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "Display Signal:";
            // 
            // PPaint
            // 
            this.PPaint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PPaint.Location = new System.Drawing.Point(0, 287);
            this.PPaint.Name = "PPaint";
            this.PPaint.Size = new System.Drawing.Size(535, 342);
            this.PPaint.TabIndex = 23;
            this.PPaint.Paint += new System.Windows.Forms.PaintEventHandler(this.PPaint_Paint);
            // 
            // timerProcess
            // 
            this.timerProcess.Tick += new System.EventHandler(this.timerProcess_Tick);
            // 
            // SingleChannelScanningDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 629);
            this.Controls.Add(this.PPaint);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SingleChannelScanningDemo";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton RBCHRC;
        private System.Windows.Forms.Button BtDisCon;
        private System.Windows.Forms.Button BtConnect;
        private System.Windows.Forms.RadioButton RBCHR2;
        private System.Windows.Forms.RadioButton RBCHRNomal;
        private System.Windows.Forms.TextBox TbConInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton RBEncTrigger;
        private System.Windows.Forms.RadioButton RBSyncSig;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox CBTriggerOnReturn;
        private System.Windows.Forms.TextBox TBInterval;
        private System.Windows.Forms.TextBox TBStopPos;
        private System.Windows.Forms.TextBox TBStartPos;
        private System.Windows.Forms.ComboBox CBAxis;
        private System.Windows.Forms.TextBox TBEncoderPos;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox TBSignal;
        private System.Windows.Forms.TextBox TBSampleNo;
        private System.Windows.Forms.TextBox TBLineNo;
        private System.Windows.Forms.Button BtScan;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ComboBox CBDisplaySig;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel PPaint;
        private System.Windows.Forms.Timer timerProcess;
        private System.Windows.Forms.TextBox TBSigMax;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox TBSigMin;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button BtEncoderPos;
    }
}

