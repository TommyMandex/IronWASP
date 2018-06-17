namespace IronWASP
{
    partial class SessionView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BaseTabs = new System.Windows.Forms.TabControl();
            this.RequestTab = new System.Windows.Forms.TabPage();
            this.ResponseTab = new System.Windows.Forms.TabPage();
            this.ReqView = new IronWASP.RequestView();
            this.ResView = new IronWASP.ResponseView();
            this.BaseTabs.SuspendLayout();
            this.RequestTab.SuspendLayout();
            this.ResponseTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // BaseTabs
            // 
            this.BaseTabs.Controls.Add(this.RequestTab);
            this.BaseTabs.Controls.Add(this.ResponseTab);
            this.BaseTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BaseTabs.Location = new System.Drawing.Point(0, 0);
            this.BaseTabs.Margin = new System.Windows.Forms.Padding(0);
            this.BaseTabs.Name = "BaseTabs";
            this.BaseTabs.Padding = new System.Drawing.Point(0, 0);
            this.BaseTabs.SelectedIndex = 0;
            this.BaseTabs.Size = new System.Drawing.Size(600, 200);
            this.BaseTabs.TabIndex = 0;
            // 
            // RequestTab
            // 
            this.RequestTab.Controls.Add(this.ReqView);
            this.RequestTab.Location = new System.Drawing.Point(4, 22);
            this.RequestTab.Margin = new System.Windows.Forms.Padding(0);
            this.RequestTab.Name = "RequestTab";
            this.RequestTab.Size = new System.Drawing.Size(592, 174);
            this.RequestTab.TabIndex = 0;
            this.RequestTab.Text = "    Request    ";
            this.RequestTab.UseVisualStyleBackColor = true;
            // 
            // ResponseTab
            // 
            this.ResponseTab.Controls.Add(this.ResView);
            this.ResponseTab.Location = new System.Drawing.Point(4, 22);
            this.ResponseTab.Margin = new System.Windows.Forms.Padding(0);
            this.ResponseTab.Name = "ResponseTab";
            this.ResponseTab.Size = new System.Drawing.Size(592, 174);
            this.ResponseTab.TabIndex = 1;
            this.ResponseTab.Text = "    Response    ";
            this.ResponseTab.UseVisualStyleBackColor = true;
            // 
            // ReqView
            // 
            this.ReqView.BackColor = System.Drawing.Color.White;
            this.ReqView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReqView.Location = new System.Drawing.Point(0, 0);
            this.ReqView.Margin = new System.Windows.Forms.Padding(0);
            this.ReqView.Name = "ReqView";
            this.ReqView.ReadOnly = false;
            this.ReqView.Size = new System.Drawing.Size(592, 174);
            this.ReqView.TabIndex = 0;
            // 
            // ResView
            // 
            this.ResView.BackColor = System.Drawing.Color.White;
            this.ResView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResView.IncludeReflectionTab = true;
            this.ResView.Location = new System.Drawing.Point(0, 0);
            this.ResView.Margin = new System.Windows.Forms.Padding(0);
            this.ResView.Name = "ResView";
            this.ResView.ReadOnly = false;
            this.ResView.Size = new System.Drawing.Size(592, 174);
            this.ResView.TabIndex = 0;
            // 
            // SessionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BaseTabs);
            this.Name = "SessionView";
            this.Size = new System.Drawing.Size(600, 200);
            this.BaseTabs.ResumeLayout(false);
            this.RequestTab.ResumeLayout(false);
            this.ResponseTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl BaseTabs;
        private System.Windows.Forms.TabPage RequestTab;
        private System.Windows.Forms.TabPage ResponseTab;
        private RequestView ReqView;
        private ResponseView ResView;
    }
}
