namespace IronWASP.Recording
{
    partial class Recorder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Recorder));
            this.FirstTextboxLbl = new System.Windows.Forms.Label();
            this.ConfigureStepPasswordTB = new System.Windows.Forms.TextBox();
            this.ConfigureStepUsernameTB = new System.Windows.Forms.TextBox();
            this.InstructionLbl = new System.Windows.Forms.Label();
            this.TopMsgLbl = new System.Windows.Forms.Label();
            this.BaseTabs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ConfigureStepCsrfTokenTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConfigureStepSubmitBtn = new System.Windows.Forms.Button();
            this.ConfigureStepErrorLbl = new System.Windows.Forms.Label();
            this.SecondTextboxLbl = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.RecordStepStatusHeaderLbl = new System.Windows.Forms.Label();
            this.RecordStepCsrfStatusLbl = new System.Windows.Forms.Label();
            this.RecordStepLoginStatusLbl = new System.Windows.Forms.Label();
            this.RecordStepCancelBtn = new System.Windows.Forms.Button();
            this.RecordStepStartBtn = new System.Windows.Forms.Button();
            this.RecordStepCsrfInstructionLbl = new System.Windows.Forms.Label();
            this.RecordStepLoginInstructionLbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.TestStepWaitMoreBtn = new System.Windows.Forms.Button();
            this.TestStepDontWaitBtn = new System.Windows.Forms.Button();
            this.TestStepRetryLL = new System.Windows.Forms.LinkLabel();
            this.TestStepStatusTB = new System.Windows.Forms.TextBox();
            this.TestStepHeaderLbl = new System.Windows.Forms.Label();
            this.TestStepCancelBtn = new System.Windows.Forms.Button();
            this.TestStepProgressBar = new System.Windows.Forms.ProgressBar();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.SaveStepConfirmationMsbLbl = new System.Windows.Forms.Label();
            this.SaveStepErrorLbl = new System.Windows.Forms.Label();
            this.SaveStepSaveLL = new System.Windows.Forms.LinkLabel();
            this.SaveStepNameTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SaveRecordingDialog = new System.Windows.Forms.SaveFileDialog();
            this.BaseTabs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // FirstTextboxLbl
            // 
            this.FirstTextboxLbl.AutoSize = true;
            this.FirstTextboxLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.FirstTextboxLbl.Location = new System.Drawing.Point(15, 36);
            this.FirstTextboxLbl.Name = "FirstTextboxLbl";
            this.FirstTextboxLbl.Size = new System.Drawing.Size(258, 13);
            this.FirstTextboxLbl.TabIndex = 2;
            this.FirstTextboxLbl.Text = "Enter the value of the username to be used for Login:";
            // 
            // ConfigureStepPasswordTB
            // 
            this.ConfigureStepPasswordTB.Location = new System.Drawing.Point(314, 59);
            this.ConfigureStepPasswordTB.Name = "ConfigureStepPasswordTB";
            this.ConfigureStepPasswordTB.Size = new System.Drawing.Size(254, 20);
            this.ConfigureStepPasswordTB.TabIndex = 17;
            // 
            // ConfigureStepUsernameTB
            // 
            this.ConfigureStepUsernameTB.Location = new System.Drawing.Point(314, 33);
            this.ConfigureStepUsernameTB.Name = "ConfigureStepUsernameTB";
            this.ConfigureStepUsernameTB.Size = new System.Drawing.Size(254, 20);
            this.ConfigureStepUsernameTB.TabIndex = 16;
            // 
            // InstructionLbl
            // 
            this.InstructionLbl.AutoSize = true;
            this.InstructionLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InstructionLbl.ForeColor = System.Drawing.Color.Black;
            this.InstructionLbl.Location = new System.Drawing.Point(5, 4);
            this.InstructionLbl.Name = "InstructionLbl";
            this.InstructionLbl.Size = new System.Drawing.Size(341, 17);
            this.InstructionLbl.TabIndex = 4;
            this.InstructionLbl.Text = "Enter the requested details below to configure the recorder:";
            // 
            // TopMsgLbl
            // 
            this.TopMsgLbl.AutoSize = true;
            this.TopMsgLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.TopMsgLbl.Location = new System.Drawing.Point(16, 127);
            this.TopMsgLbl.Name = "TopMsgLbl";
            this.TopMsgLbl.Size = new System.Drawing.Size(360, 13);
            this.TopMsgLbl.TabIndex = 3;
            this.TopMsgLbl.Text = "NOTE: If you don\'t want to handle CSRF tokens then leave this field blank.";
            // 
            // BaseTabs
            // 
            this.BaseTabs.Controls.Add(this.tabPage1);
            this.BaseTabs.Controls.Add(this.tabPage2);
            this.BaseTabs.Controls.Add(this.tabPage3);
            this.BaseTabs.Controls.Add(this.tabPage4);
            this.BaseTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BaseTabs.Location = new System.Drawing.Point(0, 0);
            this.BaseTabs.Margin = new System.Windows.Forms.Padding(0);
            this.BaseTabs.Name = "BaseTabs";
            this.BaseTabs.Padding = new System.Drawing.Point(0, 0);
            this.BaseTabs.SelectedIndex = 0;
            this.BaseTabs.Size = new System.Drawing.Size(584, 211);
            this.BaseTabs.TabIndex = 21;
            this.BaseTabs.Selected += new System.Windows.Forms.TabControlEventHandler(this.BaseTabs_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ConfigureStepCsrfTokenTB);
            this.tabPage1.Controls.Add(this.TopMsgLbl);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.ConfigureStepSubmitBtn);
            this.tabPage1.Controls.Add(this.ConfigureStepPasswordTB);
            this.tabPage1.Controls.Add(this.ConfigureStepErrorLbl);
            this.tabPage1.Controls.Add(this.SecondTextboxLbl);
            this.tabPage1.Controls.Add(this.ConfigureStepUsernameTB);
            this.tabPage1.Controls.Add(this.FirstTextboxLbl);
            this.tabPage1.Controls.Add(this.InstructionLbl);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(576, 185);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "    Step 1 - Configure    ";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ConfigureStepCsrfTokenTB
            // 
            this.ConfigureStepCsrfTokenTB.Location = new System.Drawing.Point(314, 100);
            this.ConfigureStepCsrfTokenTB.Name = "ConfigureStepCsrfTokenTB";
            this.ConfigureStepCsrfTokenTB.Size = new System.Drawing.Size(254, 20);
            this.ConfigureStepCsrfTokenTB.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Olive;
            this.label1.Location = new System.Drawing.Point(15, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Enter the name of the Parameter with CSRF protection token:";
            // 
            // ConfigureStepSubmitBtn
            // 
            this.ConfigureStepSubmitBtn.Location = new System.Drawing.Point(417, 153);
            this.ConfigureStepSubmitBtn.Name = "ConfigureStepSubmitBtn";
            this.ConfigureStepSubmitBtn.Size = new System.Drawing.Size(151, 23);
            this.ConfigureStepSubmitBtn.TabIndex = 14;
            this.ConfigureStepSubmitBtn.Text = "Submit Values";
            this.ConfigureStepSubmitBtn.UseVisualStyleBackColor = true;
            this.ConfigureStepSubmitBtn.Click += new System.EventHandler(this.ConfigureStepSubmitBtn_Click);
            // 
            // ConfigureStepErrorLbl
            // 
            this.ConfigureStepErrorLbl.AutoSize = true;
            this.ConfigureStepErrorLbl.ForeColor = System.Drawing.Color.Red;
            this.ConfigureStepErrorLbl.Location = new System.Drawing.Point(19, 158);
            this.ConfigureStepErrorLbl.Name = "ConfigureStepErrorLbl";
            this.ConfigureStepErrorLbl.Size = new System.Drawing.Size(51, 13);
            this.ConfigureStepErrorLbl.TabIndex = 20;
            this.ConfigureStepErrorLbl.Text = "Error msg";
            this.ConfigureStepErrorLbl.Visible = false;
            // 
            // SecondTextboxLbl
            // 
            this.SecondTextboxLbl.AutoSize = true;
            this.SecondTextboxLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.SecondTextboxLbl.Location = new System.Drawing.Point(15, 58);
            this.SecondTextboxLbl.Name = "SecondTextboxLbl";
            this.SecondTextboxLbl.Size = new System.Drawing.Size(254, 13);
            this.SecondTextboxLbl.TabIndex = 19;
            this.SecondTextboxLbl.Text = "Enter the value of the Password to be used for login:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.RecordStepStatusHeaderLbl);
            this.tabPage2.Controls.Add(this.RecordStepCsrfStatusLbl);
            this.tabPage2.Controls.Add(this.RecordStepLoginStatusLbl);
            this.tabPage2.Controls.Add(this.RecordStepCancelBtn);
            this.tabPage2.Controls.Add(this.RecordStepStartBtn);
            this.tabPage2.Controls.Add(this.RecordStepCsrfInstructionLbl);
            this.tabPage2.Controls.Add(this.RecordStepLoginInstructionLbl);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(576, 185);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "    Step 2 - Record    ";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // RecordStepStatusHeaderLbl
            // 
            this.RecordStepStatusHeaderLbl.AutoSize = true;
            this.RecordStepStatusHeaderLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecordStepStatusHeaderLbl.ForeColor = System.Drawing.Color.Black;
            this.RecordStepStatusHeaderLbl.Location = new System.Drawing.Point(4, 119);
            this.RecordStepStatusHeaderLbl.Name = "RecordStepStatusHeaderLbl";
            this.RecordStepStatusHeaderLbl.Size = new System.Drawing.Size(382, 17);
            this.RecordStepStatusHeaderLbl.TabIndex = 20;
            this.RecordStepStatusHeaderLbl.Text = "Recording will be completed when the following conditions are met:";
            // 
            // RecordStepCsrfStatusLbl
            // 
            this.RecordStepCsrfStatusLbl.AutoSize = true;
            this.RecordStepCsrfStatusLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecordStepCsrfStatusLbl.ForeColor = System.Drawing.Color.Red;
            this.RecordStepCsrfStatusLbl.Location = new System.Drawing.Point(13, 159);
            this.RecordStepCsrfStatusLbl.Name = "RecordStepCsrfStatusLbl";
            this.RecordStepCsrfStatusLbl.Size = new System.Drawing.Size(413, 17);
            this.RecordStepCsrfStatusLbl.TabIndex = 19;
            this.RecordStepCsrfStatusLbl.Text = "- Looking for page containing CSRF token parameter in hidden input field";
            // 
            // RecordStepLoginStatusLbl
            // 
            this.RecordStepLoginStatusLbl.AutoSize = true;
            this.RecordStepLoginStatusLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecordStepLoginStatusLbl.ForeColor = System.Drawing.Color.Red;
            this.RecordStepLoginStatusLbl.Location = new System.Drawing.Point(13, 140);
            this.RecordStepLoginStatusLbl.Name = "RecordStepLoginStatusLbl";
            this.RecordStepLoginStatusLbl.Size = new System.Drawing.Size(368, 17);
            this.RecordStepLoginStatusLbl.TabIndex = 18;
            this.RecordStepLoginStatusLbl.Text = "- Looking for request with specified login credentials in recording";
            // 
            // RecordStepCancelBtn
            // 
            this.RecordStepCancelBtn.Location = new System.Drawing.Point(472, 156);
            this.RecordStepCancelBtn.Name = "RecordStepCancelBtn";
            this.RecordStepCancelBtn.Size = new System.Drawing.Size(97, 23);
            this.RecordStepCancelBtn.TabIndex = 16;
            this.RecordStepCancelBtn.Text = "Cancel";
            this.RecordStepCancelBtn.UseVisualStyleBackColor = true;
            this.RecordStepCancelBtn.Click += new System.EventHandler(this.RecordStepCancelBtn_Click);
            // 
            // RecordStepStartBtn
            // 
            this.RecordStepStartBtn.Location = new System.Drawing.Point(11, 24);
            this.RecordStepStartBtn.Name = "RecordStepStartBtn";
            this.RecordStepStartBtn.Size = new System.Drawing.Size(127, 23);
            this.RecordStepStartBtn.TabIndex = 15;
            this.RecordStepStartBtn.Text = "Start Recording";
            this.RecordStepStartBtn.UseVisualStyleBackColor = true;
            this.RecordStepStartBtn.Click += new System.EventHandler(this.RecordStepStartBtn_Click);
            // 
            // RecordStepCsrfInstructionLbl
            // 
            this.RecordStepCsrfInstructionLbl.AutoSize = true;
            this.RecordStepCsrfInstructionLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecordStepCsrfInstructionLbl.ForeColor = System.Drawing.Color.Navy;
            this.RecordStepCsrfInstructionLbl.Location = new System.Drawing.Point(12, 73);
            this.RecordStepCsrfInstructionLbl.Name = "RecordStepCsrfInstructionLbl";
            this.RecordStepCsrfInstructionLbl.Size = new System.Drawing.Size(551, 17);
            this.RecordStepCsrfInstructionLbl.TabIndex = 8;
            this.RecordStepCsrfInstructionLbl.Text = "- And then browse to a page that contains the CSRF token parameter value as a hid" +
    "den input field";
            // 
            // RecordStepLoginInstructionLbl
            // 
            this.RecordStepLoginInstructionLbl.AutoSize = true;
            this.RecordStepLoginInstructionLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecordStepLoginInstructionLbl.ForeColor = System.Drawing.Color.Navy;
            this.RecordStepLoginInstructionLbl.Location = new System.Drawing.Point(12, 55);
            this.RecordStepLoginInstructionLbl.Name = "RecordStepLoginInstructionLbl";
            this.RecordStepLoginInstructionLbl.Size = new System.Drawing.Size(438, 17);
            this.RecordStepLoginInstructionLbl.TabIndex = 7;
            this.RecordStepLoginInstructionLbl.Text = "- Now login to the application using the credentials given in the previous step.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(4, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(564, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Open browser, go to blank page, clear all cookies, set IronWASP as proxy and clic" +
    "k \'Start Recording\'";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.TestStepWaitMoreBtn);
            this.tabPage3.Controls.Add(this.TestStepDontWaitBtn);
            this.tabPage3.Controls.Add(this.TestStepRetryLL);
            this.tabPage3.Controls.Add(this.TestStepStatusTB);
            this.tabPage3.Controls.Add(this.TestStepHeaderLbl);
            this.tabPage3.Controls.Add(this.TestStepCancelBtn);
            this.tabPage3.Controls.Add(this.TestStepProgressBar);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(576, 185);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "    Step 3 - Test    ";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // TestStepWaitMoreBtn
            // 
            this.TestStepWaitMoreBtn.Location = new System.Drawing.Point(250, 35);
            this.TestStepWaitMoreBtn.Name = "TestStepWaitMoreBtn";
            this.TestStepWaitMoreBtn.Size = new System.Drawing.Size(209, 23);
            this.TestStepWaitMoreBtn.TabIndex = 23;
            this.TestStepWaitMoreBtn.Text = "Still loading. Wait for 5 Seconds more ";
            this.TestStepWaitMoreBtn.UseVisualStyleBackColor = true;
            this.TestStepWaitMoreBtn.Click += new System.EventHandler(this.TestStepWaitMoreBtn_Click);
            // 
            // TestStepDontWaitBtn
            // 
            this.TestStepDontWaitBtn.Location = new System.Drawing.Point(20, 35);
            this.TestStepDontWaitBtn.Name = "TestStepDontWaitBtn";
            this.TestStepDontWaitBtn.Size = new System.Drawing.Size(199, 23);
            this.TestStepDontWaitBtn.TabIndex = 22;
            this.TestStepDontWaitBtn.Text = "Don\'t wait. Page has fully loaded";
            this.TestStepDontWaitBtn.UseVisualStyleBackColor = true;
            this.TestStepDontWaitBtn.Click += new System.EventHandler(this.TestStepDontWaitBtn_Click);
            // 
            // TestStepRetryLL
            // 
            this.TestStepRetryLL.AutoSize = true;
            this.TestStepRetryLL.Location = new System.Drawing.Point(14, 167);
            this.TestStepRetryLL.Name = "TestStepRetryLL";
            this.TestStepRetryLL.Size = new System.Drawing.Size(32, 13);
            this.TestStepRetryLL.TabIndex = 21;
            this.TestStepRetryLL.TabStop = true;
            this.TestStepRetryLL.Text = "Retry";
            this.TestStepRetryLL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TestStepRetryLL_LinkClicked);
            // 
            // TestStepStatusTB
            // 
            this.TestStepStatusTB.BackColor = System.Drawing.SystemColors.Window;
            this.TestStepStatusTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TestStepStatusTB.ForeColor = System.Drawing.Color.Red;
            this.TestStepStatusTB.Location = new System.Drawing.Point(8, 107);
            this.TestStepStatusTB.Multiline = true;
            this.TestStepStatusTB.Name = "TestStepStatusTB";
            this.TestStepStatusTB.ReadOnly = true;
            this.TestStepStatusTB.Size = new System.Drawing.Size(560, 48);
            this.TestStepStatusTB.TabIndex = 19;
            // 
            // TestStepHeaderLbl
            // 
            this.TestStepHeaderLbl.AutoSize = true;
            this.TestStepHeaderLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestStepHeaderLbl.ForeColor = System.Drawing.Color.Black;
            this.TestStepHeaderLbl.Location = new System.Drawing.Point(5, 5);
            this.TestStepHeaderLbl.Name = "TestStepHeaderLbl";
            this.TestStepHeaderLbl.Size = new System.Drawing.Size(568, 17);
            this.TestStepHeaderLbl.TabIndex = 18;
            this.TestStepHeaderLbl.Text = "Waiting for 5 seconds to let the current page finish loading in the browser incas" +
    "e it is not fully loaded.";
            // 
            // TestStepCancelBtn
            // 
            this.TestStepCancelBtn.Location = new System.Drawing.Point(471, 159);
            this.TestStepCancelBtn.Name = "TestStepCancelBtn";
            this.TestStepCancelBtn.Size = new System.Drawing.Size(97, 23);
            this.TestStepCancelBtn.TabIndex = 17;
            this.TestStepCancelBtn.Text = "Cancel";
            this.TestStepCancelBtn.UseVisualStyleBackColor = true;
            this.TestStepCancelBtn.Click += new System.EventHandler(this.TestStepCancelBtn_Click);
            // 
            // TestStepProgressBar
            // 
            this.TestStepProgressBar.Location = new System.Drawing.Point(20, 74);
            this.TestStepProgressBar.Name = "TestStepProgressBar";
            this.TestStepProgressBar.Size = new System.Drawing.Size(536, 23);
            this.TestStepProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.TestStepProgressBar.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.SaveStepConfirmationMsbLbl);
            this.tabPage4.Controls.Add(this.SaveStepErrorLbl);
            this.tabPage4.Controls.Add(this.SaveStepSaveLL);
            this.tabPage4.Controls.Add(this.SaveStepNameTB);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Controls.Add(this.label6);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(576, 185);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "    Step 4 - Save    ";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // SaveStepConfirmationMsbLbl
            // 
            this.SaveStepConfirmationMsbLbl.AutoSize = true;
            this.SaveStepConfirmationMsbLbl.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveStepConfirmationMsbLbl.ForeColor = System.Drawing.Color.Green;
            this.SaveStepConfirmationMsbLbl.Location = new System.Drawing.Point(4, 104);
            this.SaveStepConfirmationMsbLbl.Name = "SaveStepConfirmationMsbLbl";
            this.SaveStepConfirmationMsbLbl.Size = new System.Drawing.Size(81, 17);
            this.SaveStepConfirmationMsbLbl.TabIndex = 22;
            this.SaveStepConfirmationMsbLbl.Text = "Confirm msg";
            // 
            // SaveStepErrorLbl
            // 
            this.SaveStepErrorLbl.AutoSize = true;
            this.SaveStepErrorLbl.ForeColor = System.Drawing.Color.Red;
            this.SaveStepErrorLbl.Location = new System.Drawing.Point(8, 82);
            this.SaveStepErrorLbl.Name = "SaveStepErrorLbl";
            this.SaveStepErrorLbl.Size = new System.Drawing.Size(51, 13);
            this.SaveStepErrorLbl.TabIndex = 21;
            this.SaveStepErrorLbl.Text = "Error msg";
            this.SaveStepErrorLbl.Visible = false;
            // 
            // SaveStepSaveLL
            // 
            this.SaveStepSaveLL.AutoSize = true;
            this.SaveStepSaveLL.Location = new System.Drawing.Point(476, 43);
            this.SaveStepSaveLL.Name = "SaveStepSaveLL";
            this.SaveStepSaveLL.Size = new System.Drawing.Size(32, 13);
            this.SaveStepSaveLL.TabIndex = 20;
            this.SaveStepSaveLL.TabStop = true;
            this.SaveStepSaveLL.Text = "Save";
            this.SaveStepSaveLL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SaveStepSaveLL_LinkClicked);
            // 
            // SaveStepNameTB
            // 
            this.SaveStepNameTB.Location = new System.Drawing.Point(166, 40);
            this.SaveStepNameTB.Name = "SaveStepNameTB";
            this.SaveStepNameTB.Size = new System.Drawing.Size(298, 20);
            this.SaveStepNameTB.TabIndex = 19;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.label5.Location = new System.Drawing.Point(8, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(154, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Enter a name for this recording:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(6, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(261, 17);
            this.label6.TabIndex = 18;
            this.label6.Text = "Save this recording to use in scans and tests. ";
            // 
            // SaveRecordingDialog
            // 
            this.SaveRecordingDialog.DefaultExt = "sessrec";
            this.SaveRecordingDialog.Filter = "Recordings|*.sessrec";
            this.SaveRecordingDialog.SupportMultiDottedExtensions = true;
            // 
            // Recorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 211);
            this.Controls.Add(this.BaseTabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(600, 250);
            this.MinimumSize = new System.Drawing.Size(600, 250);
            this.Name = "Recorder";
            this.Text = "Record Login and CSRF Token Handling Sequence";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IronRecorder_FormClosing);
            this.Load += new System.EventHandler(this.IronRecorder_Load);
            this.BaseTabs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label FirstTextboxLbl;
        private System.Windows.Forms.Label InstructionLbl;
        private System.Windows.Forms.Label TopMsgLbl;
        private System.Windows.Forms.TextBox ConfigureStepPasswordTB;
        private System.Windows.Forms.TextBox ConfigureStepUsernameTB;
        private System.Windows.Forms.Label SecondTextboxLbl;
        private System.Windows.Forms.Label ConfigureStepErrorLbl;
        private System.Windows.Forms.SaveFileDialog SaveRecordingDialog;
        private System.Windows.Forms.TabControl BaseTabs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox ConfigureStepCsrfTokenTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label RecordStepLoginInstructionLbl;
        private System.Windows.Forms.Label RecordStepCsrfInstructionLbl;
        private System.Windows.Forms.Button RecordStepCancelBtn;
        private System.Windows.Forms.Button RecordStepStartBtn;
        private System.Windows.Forms.Label RecordStepCsrfStatusLbl;
        private System.Windows.Forms.Label RecordStepLoginStatusLbl;
        private System.Windows.Forms.Label RecordStepStatusHeaderLbl;
        private System.Windows.Forms.Button TestStepCancelBtn;
        private System.Windows.Forms.ProgressBar TestStepProgressBar;
        private System.Windows.Forms.Label TestStepHeaderLbl;
        private System.Windows.Forms.TextBox TestStepStatusTB;
        private System.Windows.Forms.TextBox SaveStepNameTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel SaveStepSaveLL;
        private System.Windows.Forms.Label SaveStepErrorLbl;
        private System.Windows.Forms.LinkLabel TestStepRetryLL;
        private System.Windows.Forms.Button TestStepWaitMoreBtn;
        private System.Windows.Forms.Button TestStepDontWaitBtn;
        private System.Windows.Forms.Label SaveStepConfirmationMsbLbl;
        private System.Windows.Forms.Button ConfigureStepSubmitBtn;
    }
}