namespace IronWASP.Hawas
{
    partial class DomXssAnalyzer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DomXssAnalyzer));
            this.AnalysisProgressBar = new System.Windows.Forms.ProgressBar();
            this.OutputFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SelectOutputFolderLbl = new System.Windows.Forms.LinkLabel();
            this.OutputTB = new System.Windows.Forms.TextBox();
            this.StatusLbl = new System.Windows.Forms.Label();
            this.StartBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // AnalysisProgressBar
            // 
            this.AnalysisProgressBar.Location = new System.Drawing.Point(74, 132);
            this.AnalysisProgressBar.MarqueeAnimationSpeed = 50;
            this.AnalysisProgressBar.Name = "AnalysisProgressBar";
            this.AnalysisProgressBar.Size = new System.Drawing.Size(440, 23);
            this.AnalysisProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.AnalysisProgressBar.TabIndex = 0;
            this.AnalysisProgressBar.Visible = false;
            // 
            // SelectOutputFolderLbl
            // 
            this.SelectOutputFolderLbl.AutoSize = true;
            this.SelectOutputFolderLbl.Location = new System.Drawing.Point(192, 99);
            this.SelectOutputFolderLbl.Name = "SelectOutputFolderLbl";
            this.SelectOutputFolderLbl.Size = new System.Drawing.Size(195, 13);
            this.SelectOutputFolderLbl.TabIndex = 1;
            this.SelectOutputFolderLbl.TabStop = true;
            this.SelectOutputFolderLbl.Text = "Select the folder to save analysis results";
            this.SelectOutputFolderLbl.Visible = false;
            this.SelectOutputFolderLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectOutputFolderLbl_LinkClicked);
            // 
            // OutputTB
            // 
            this.OutputTB.Location = new System.Drawing.Point(12, 188);
            this.OutputTB.Name = "OutputTB";
            this.OutputTB.Size = new System.Drawing.Size(560, 20);
            this.OutputTB.TabIndex = 2;
            this.OutputTB.Visible = false;
            // 
            // StatusLbl
            // 
            this.StatusLbl.AutoSize = true;
            this.StatusLbl.Location = new System.Drawing.Point(71, 168);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(115, 13);
            this.StatusLbl.TabIndex = 3;
            this.StatusLbl.Text = "                                    ";
            // 
            // StartBtn
            // 
            this.StartBtn.Location = new System.Drawing.Point(10, 94);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(176, 23);
            this.StartBtn.TabIndex = 4;
            this.StartBtn.Text = "Start Analysis";
            this.StartBtn.UseVisualStyleBackColor = true;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Palatino Linotype", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(562, 83);
            this.label1.TabIndex = 13;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // DomXssAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 211);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StartBtn);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.OutputTB);
            this.Controls.Add(this.SelectOutputFolderLbl);
            this.Controls.Add(this.AnalysisProgressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(600, 250);
            this.MinimumSize = new System.Drawing.Size(600, 250);
            this.Name = "DomXssAnalyzer";
            this.Text = "DomXss Analyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DomXssAnalyzer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar AnalysisProgressBar;
        private System.Windows.Forms.FolderBrowserDialog OutputFolderDialog;
        private System.Windows.Forms.LinkLabel SelectOutputFolderLbl;
        private System.Windows.Forms.TextBox OutputTB;
        private System.Windows.Forms.Label StatusLbl;
        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.Label label1;
    }
}