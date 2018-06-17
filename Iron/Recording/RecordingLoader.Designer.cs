namespace IronWASP.Recording
{
    partial class RecordingLoader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordingLoader));
            this.OpenRecordingFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.LoadProgressBar = new System.Windows.Forms.ProgressBar();
            this.MessageLbl = new System.Windows.Forms.Label();
            this.LoadLL = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // OpenRecordingFileDialog
            // 
            this.OpenRecordingFileDialog.Filter = "Sequence Recordings|*sessrec";
            // 
            // LoadProgressBar
            // 
            this.LoadProgressBar.Location = new System.Drawing.Point(12, 119);
            this.LoadProgressBar.Name = "LoadProgressBar";
            this.LoadProgressBar.Size = new System.Drawing.Size(482, 23);
            this.LoadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.LoadProgressBar.TabIndex = 0;
            this.LoadProgressBar.Visible = false;
            // 
            // MessageLbl
            // 
            this.MessageLbl.AutoSize = true;
            this.MessageLbl.ForeColor = System.Drawing.Color.Red;
            this.MessageLbl.Location = new System.Drawing.Point(12, 93);
            this.MessageLbl.Name = "MessageLbl";
            this.MessageLbl.Size = new System.Drawing.Size(136, 13);
            this.MessageLbl.TabIndex = 1;
            this.MessageLbl.Text = "                                           ";
            // 
            // LoadLL
            // 
            this.LoadLL.AutoSize = true;
            this.LoadLL.Location = new System.Drawing.Point(12, 20);
            this.LoadLL.Name = "LoadLL";
            this.LoadLL.Size = new System.Drawing.Size(108, 13);
            this.LoadLL.TabIndex = 2;
            this.LoadLL.TabStop = true;
            this.LoadLL.Text = "Select Recording File";
            this.LoadLL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LoadLL_LinkClicked);
            // 
            // RecordingLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 154);
            this.Controls.Add(this.LoadLL);
            this.Controls.Add(this.MessageLbl);
            this.Controls.Add(this.LoadProgressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecordingLoader";
            this.Text = "Load a Sequence Recording from File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecordingLoader_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog OpenRecordingFileDialog;
        private System.Windows.Forms.ProgressBar LoadProgressBar;
        private System.Windows.Forms.Label MessageLbl;
        private System.Windows.Forms.LinkLabel LoadLL;
    }
}