namespace WCFTestApp
{
    partial class Main
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
            this.edtConnectString = new System.Windows.Forms.TextBox();
            this.btnReaders = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstReaders = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSetResultRequestType = new System.Windows.Forms.Button();
            this.lstResultRequestType = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkBoxResultOnThread = new System.Windows.Forms.CheckBox();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.btnGetResults = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chkTimeoutMode = new System.Windows.Forms.CheckBox();
            this.edtCycle = new System.Windows.Forms.TextBox();
            this.btnSetCycle = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnRead = new System.Windows.Forms.Button();
            this.edtReadTimeout = new System.Windows.Forms.TextBox();
            this.ExecuteCommandSet = new System.Windows.Forms.GroupBox();
            this.edtCommandSet = new System.Windows.Forms.TextBox();
            this.btnExecuteCommandSet = new System.Windows.Forms.Button();
            this.edtConfigFile = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btnExecuteCommand = new System.Windows.Forms.Button();
            this.edtCommand = new System.Windows.Forms.TextBox();
            this.btnReadAndGetSample = new System.Windows.Forms.Button();
            this.btnReadAndGetSample2 = new System.Windows.Forms.Button();
            this.bReadIF2Inputs = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.ExecuteCommandSet.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // edtConnectString
            // 
            this.edtConnectString.Location = new System.Drawing.Point(12, 12);
            this.edtConnectString.Name = "edtConnectString";
            this.edtConnectString.Size = new System.Drawing.Size(829, 20);
            this.edtConnectString.TabIndex = 0;
            this.edtConnectString.Text = "http://localhost:8733/VRHReaderFrameworkWCFInterface/AppInterface/";
            // 
            // btnReaders
            // 
            this.btnReaders.Location = new System.Drawing.Point(6, 19);
            this.btnReaders.Name = "btnReaders";
            this.btnReaders.Size = new System.Drawing.Size(188, 23);
            this.btnReaders.TabIndex = 1;
            this.btnReaders.Text = "Olvasók lekérdezése";
            this.btnReaders.UseVisualStyleBackColor = true;
            this.btnReaders.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstReaders);
            this.groupBox1.Controls.Add(this.btnReaders);
            this.groupBox1.Location = new System.Drawing.Point(12, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 177);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Olvasó kijelölése";
            // 
            // lstReaders
            // 
            this.lstReaders.FormattingEnabled = true;
            this.lstReaders.Location = new System.Drawing.Point(6, 48);
            this.lstReaders.Name = "lstReaders";
            this.lstReaders.Size = new System.Drawing.Size(188, 121);
            this.lstReaders.TabIndex = 2;
            this.lstReaders.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstReaders_MouseClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSetResultRequestType);
            this.groupBox2.Controls.Add(this.lstResultRequestType);
            this.groupBox2.Location = new System.Drawing.Point(218, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(199, 112);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ControllerResultRequestType";
            // 
            // btnSetResultRequestType
            // 
            this.btnSetResultRequestType.Location = new System.Drawing.Point(6, 81);
            this.btnSetResultRequestType.Name = "btnSetResultRequestType";
            this.btnSetResultRequestType.Size = new System.Drawing.Size(187, 23);
            this.btnSetResultRequestType.TabIndex = 1;
            this.btnSetResultRequestType.Text = "Beállít";
            this.btnSetResultRequestType.UseVisualStyleBackColor = true;
            this.btnSetResultRequestType.Click += new System.EventHandler(this.btnSetResultRequestType_Click);
            // 
            // lstResultRequestType
            // 
            this.lstResultRequestType.FormattingEnabled = true;
            this.lstResultRequestType.Items.AddRange(new object[] {
            "Semmi",
            "Szűrt",
            "Mind"});
            this.lstResultRequestType.Location = new System.Drawing.Point(6, 19);
            this.lstResultRequestType.Name = "lstResultRequestType";
            this.lstResultRequestType.Size = new System.Drawing.Size(187, 56);
            this.lstResultRequestType.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkBoxResultOnThread);
            this.groupBox3.Controls.Add(this.lstResults);
            this.groupBox3.Controls.Add(this.btnGetResults);
            this.groupBox3.Location = new System.Drawing.Point(12, 292);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(829, 340);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Eredmények";
            // 
            // chkBoxResultOnThread
            // 
            this.chkBoxResultOnThread.AutoSize = true;
            this.chkBoxResultOnThread.Location = new System.Drawing.Point(87, 23);
            this.chkBoxResultOnThread.Name = "chkBoxResultOnThread";
            this.chkBoxResultOnThread.Size = new System.Drawing.Size(85, 17);
            this.chkBoxResultOnThread.TabIndex = 2;
            this.chkBoxResultOnThread.Text = "Háttérszálon";
            this.chkBoxResultOnThread.UseVisualStyleBackColor = true;
            this.chkBoxResultOnThread.CheckedChanged += new System.EventHandler(this.chkBoxResultOnThread_CheckedChanged);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(6, 48);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(817, 277);
            this.lstResults.TabIndex = 1;
            // 
            // btnGetResults
            // 
            this.btnGetResults.Location = new System.Drawing.Point(6, 19);
            this.btnGetResults.Name = "btnGetResults";
            this.btnGetResults.Size = new System.Drawing.Size(75, 23);
            this.btnGetResults.TabIndex = 0;
            this.btnGetResults.Text = "Lekérdez";
            this.btnGetResults.UseVisualStyleBackColor = true;
            this.btnGetResults.Click += new System.EventHandler(this.btnGetResults_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.chkTimeoutMode);
            this.groupBox4.Controls.Add(this.edtCycle);
            this.groupBox4.Controls.Add(this.btnSetCycle);
            this.groupBox4.Location = new System.Drawing.Point(423, 38);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(199, 112);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Cycle && TimeoutMode";
            // 
            // chkTimeoutMode
            // 
            this.chkTimeoutMode.AutoSize = true;
            this.chkTimeoutMode.Location = new System.Drawing.Point(6, 19);
            this.chkTimeoutMode.Name = "chkTimeoutMode";
            this.chkTimeoutMode.Size = new System.Drawing.Size(91, 17);
            this.chkTimeoutMode.TabIndex = 3;
            this.chkTimeoutMode.Text = "TimeoutMode";
            this.chkTimeoutMode.UseVisualStyleBackColor = true;
            // 
            // edtCycle
            // 
            this.edtCycle.Location = new System.Drawing.Point(6, 48);
            this.edtCycle.Name = "edtCycle";
            this.edtCycle.Size = new System.Drawing.Size(187, 20);
            this.edtCycle.TabIndex = 2;
            // 
            // btnSetCycle
            // 
            this.btnSetCycle.Location = new System.Drawing.Point(6, 81);
            this.btnSetCycle.Name = "btnSetCycle";
            this.btnSetCycle.Size = new System.Drawing.Size(187, 23);
            this.btnSetCycle.TabIndex = 1;
            this.btnSetCycle.Text = "Beállít";
            this.btnSetCycle.UseVisualStyleBackColor = true;
            this.btnSetCycle.Click += new System.EventHandler(this.btnSetCycle_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnRead);
            this.groupBox5.Controls.Add(this.edtReadTimeout);
            this.groupBox5.Location = new System.Drawing.Point(12, 221);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(200, 65);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Read";
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(119, 19);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(75, 23);
            this.btnRead.TabIndex = 1;
            this.btnRead.Text = "Olvas";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // edtReadTimeout
            // 
            this.edtReadTimeout.Location = new System.Drawing.Point(6, 19);
            this.edtReadTimeout.Name = "edtReadTimeout";
            this.edtReadTimeout.Size = new System.Drawing.Size(100, 20);
            this.edtReadTimeout.TabIndex = 0;
            this.edtReadTimeout.Text = "10";
            // 
            // ExecuteCommandSet
            // 
            this.ExecuteCommandSet.Controls.Add(this.edtCommandSet);
            this.ExecuteCommandSet.Controls.Add(this.btnExecuteCommandSet);
            this.ExecuteCommandSet.Controls.Add(this.edtConfigFile);
            this.ExecuteCommandSet.Location = new System.Drawing.Point(218, 156);
            this.ExecuteCommandSet.Name = "ExecuteCommandSet";
            this.ExecuteCommandSet.Size = new System.Drawing.Size(200, 130);
            this.ExecuteCommandSet.TabIndex = 7;
            this.ExecuteCommandSet.TabStop = false;
            this.ExecuteCommandSet.Text = "Execute commands";
            // 
            // edtCommandSet
            // 
            this.edtCommandSet.Location = new System.Drawing.Point(6, 45);
            this.edtCommandSet.Name = "edtCommandSet";
            this.edtCommandSet.Size = new System.Drawing.Size(187, 20);
            this.edtCommandSet.TabIndex = 2;
            this.edtCommandSet.Text = "successcommand";
            // 
            // btnExecuteCommandSet
            // 
            this.btnExecuteCommandSet.Location = new System.Drawing.Point(6, 101);
            this.btnExecuteCommandSet.Name = "btnExecuteCommandSet";
            this.btnExecuteCommandSet.Size = new System.Drawing.Size(187, 23);
            this.btnExecuteCommandSet.TabIndex = 1;
            this.btnExecuteCommandSet.Text = "Végrehajt";
            this.btnExecuteCommandSet.UseVisualStyleBackColor = true;
            this.btnExecuteCommandSet.Click += new System.EventHandler(this.btnExecuteCommandSet_Click);
            // 
            // edtConfigFile
            // 
            this.edtConfigFile.Location = new System.Drawing.Point(6, 19);
            this.edtConfigFile.Name = "edtConfigFile";
            this.edtConfigFile.Size = new System.Drawing.Size(187, 20);
            this.edtConfigFile.TabIndex = 0;
            this.edtConfigFile.Text = "mintacommandset.xml";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btnExecuteCommand);
            this.groupBox6.Controls.Add(this.edtCommand);
            this.groupBox6.Location = new System.Drawing.Point(424, 212);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(417, 74);
            this.groupBox6.TabIndex = 8;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Execute command";
            // 
            // btnExecuteCommand
            // 
            this.btnExecuteCommand.Location = new System.Drawing.Point(224, 45);
            this.btnExecuteCommand.Name = "btnExecuteCommand";
            this.btnExecuteCommand.Size = new System.Drawing.Size(187, 23);
            this.btnExecuteCommand.TabIndex = 1;
            this.btnExecuteCommand.Text = "Végrehajt";
            this.btnExecuteCommand.UseVisualStyleBackColor = true;
            this.btnExecuteCommand.Click += new System.EventHandler(this.btnExecuteCommand_Click);
            // 
            // edtCommand
            // 
            this.edtCommand.Location = new System.Drawing.Point(6, 19);
            this.edtCommand.Name = "edtCommand";
            this.edtCommand.Size = new System.Drawing.Size(405, 20);
            this.edtCommand.TabIndex = 0;
            this.edtCommand.Text = "READGPI";
            // 
            // btnReadAndGetSample
            // 
            this.btnReadAndGetSample.Location = new System.Drawing.Point(6, 19);
            this.btnReadAndGetSample.Name = "btnReadAndGetSample";
            this.btnReadAndGetSample.Size = new System.Drawing.Size(188, 23);
            this.btnReadAndGetSample.TabIndex = 9;
            this.btnReadAndGetSample.Text = "OlvasElkérMinta 1";
            this.btnReadAndGetSample.UseVisualStyleBackColor = true;
            this.btnReadAndGetSample.Click += new System.EventHandler(this.btnReadAndGetSample_Click);
            // 
            // btnReadAndGetSample2
            // 
            this.btnReadAndGetSample2.Location = new System.Drawing.Point(6, 48);
            this.btnReadAndGetSample2.Name = "btnReadAndGetSample2";
            this.btnReadAndGetSample2.Size = new System.Drawing.Size(188, 23);
            this.btnReadAndGetSample2.TabIndex = 10;
            this.btnReadAndGetSample2.Text = "OlvasElkérMinta 2";
            this.btnReadAndGetSample2.UseVisualStyleBackColor = true;
            this.btnReadAndGetSample2.Click += new System.EventHandler(this.btnReadAndGetSample2_Click);
            // 
            // bReadIF2Inputs
            // 
            this.bReadIF2Inputs.Location = new System.Drawing.Point(6, 77);
            this.bReadIF2Inputs.Name = "bReadIF2Inputs";
            this.bReadIF2Inputs.Size = new System.Drawing.Size(188, 23);
            this.bReadIF2Inputs.TabIndex = 11;
            this.bReadIF2Inputs.Text = "Inputok olv. (IF2)";
            this.bReadIF2Inputs.UseVisualStyleBackColor = true;
            this.bReadIF2Inputs.Click += new System.EventHandler(this.bReadIF2Inputs_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.btnReadAndGetSample);
            this.groupBox7.Controls.Add(this.bReadIF2Inputs);
            this.groupBox7.Controls.Add(this.btnReadAndGetSample2);
            this.groupBox7.Location = new System.Drawing.Point(628, 38);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(200, 112);
            this.groupBox7.TabIndex = 12;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "groupBox7";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(33, 652);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 34);
            this.button1.TabIndex = 13;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(141, 652);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 34);
            this.button2.TabIndex = 14;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(249, 652);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(85, 34);
            this.button3.TabIndex = 15;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 709);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.ExecuteCommandSet);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.edtConnectString);
            this.Name = "Main";
            this.Text = "VRHReaderFrameworkWCFTestApp";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ExecuteCommandSet.ResumeLayout(false);
            this.ExecuteCommandSet.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox edtConnectString;
        private System.Windows.Forms.Button btnReaders;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstReaders;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSetResultRequestType;
        private System.Windows.Forms.ListBox lstResultRequestType;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.Button btnGetResults;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnSetCycle;
        private System.Windows.Forms.TextBox edtCycle;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.TextBox edtReadTimeout;
        private System.Windows.Forms.CheckBox chkBoxResultOnThread;
        private System.Windows.Forms.CheckBox chkTimeoutMode;
        private System.Windows.Forms.GroupBox ExecuteCommandSet;
        private System.Windows.Forms.TextBox edtCommandSet;
        private System.Windows.Forms.Button btnExecuteCommandSet;
        private System.Windows.Forms.TextBox edtConfigFile;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnExecuteCommand;
        private System.Windows.Forms.TextBox edtCommand;
        private System.Windows.Forms.Button btnReadAndGetSample;
        private System.Windows.Forms.Button btnReadAndGetSample2;
        private System.Windows.Forms.Button bReadIF2Inputs;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

