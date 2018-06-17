namespace IronWASP.Workflow
{
    partial class WorkflowScannerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowScannerWindow));
            this.WorkflowsGrid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostnamesScopeGrid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ScanBtn = new System.Windows.Forms.Button();
            this.StatusProgressBar = new System.Windows.Forms.ProgressBar();
            this.StatusLbl = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.WorkflowsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HostnamesScopeGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // WorkflowsGrid
            // 
            this.WorkflowsGrid.AllowUserToAddRows = false;
            this.WorkflowsGrid.AllowUserToDeleteRows = false;
            this.WorkflowsGrid.AllowUserToResizeRows = false;
            this.WorkflowsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.WorkflowsGrid.BackgroundColor = System.Drawing.Color.White;
            this.WorkflowsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.WorkflowsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn9,
            this.dataGridViewTextBoxColumn27,
            this.Column1});
            this.WorkflowsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.WorkflowsGrid.GridColor = System.Drawing.Color.White;
            this.WorkflowsGrid.Location = new System.Drawing.Point(9, 41);
            this.WorkflowsGrid.Margin = new System.Windows.Forms.Padding(0);
            this.WorkflowsGrid.MultiSelect = false;
            this.WorkflowsGrid.Name = "WorkflowsGrid";
            this.WorkflowsGrid.ReadOnly = true;
            this.WorkflowsGrid.RowHeadersVisible = false;
            this.WorkflowsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.WorkflowsGrid.Size = new System.Drawing.Size(288, 151);
            this.WorkflowsGrid.TabIndex = 25;
            // 
            // dataGridViewCheckBoxColumn9
            // 
            this.dataGridViewCheckBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewCheckBoxColumn9.HeaderText = "ID";
            this.dataGridViewCheckBoxColumn9.MinimumWidth = 20;
            this.dataGridViewCheckBoxColumn9.Name = "dataGridViewCheckBoxColumn9";
            this.dataGridViewCheckBoxColumn9.ReadOnly = true;
            this.dataGridViewCheckBoxColumn9.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewCheckBoxColumn9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewCheckBoxColumn9.Width = 30;
            // 
            // dataGridViewTextBoxColumn27
            // 
            this.dataGridViewTextBoxColumn27.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn27.HeaderText = "LOG RANGE";
            this.dataGridViewTextBoxColumn27.Name = "dataGridViewTextBoxColumn27";
            this.dataGridViewTextBoxColumn27.ReadOnly = true;
            this.dataGridViewTextBoxColumn27.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn27.Width = 68;
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "WORFLOW NAME";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // HostnamesScopeGrid
            // 
            this.HostnamesScopeGrid.AllowUserToAddRows = false;
            this.HostnamesScopeGrid.AllowUserToDeleteRows = false;
            this.HostnamesScopeGrid.AllowUserToResizeRows = false;
            this.HostnamesScopeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.HostnamesScopeGrid.BackgroundColor = System.Drawing.Color.White;
            this.HostnamesScopeGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.HostnamesScopeGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewTextBoxColumn1});
            this.HostnamesScopeGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.HostnamesScopeGrid.GridColor = System.Drawing.Color.White;
            this.HostnamesScopeGrid.Location = new System.Drawing.Point(309, 41);
            this.HostnamesScopeGrid.Margin = new System.Windows.Forms.Padding(0);
            this.HostnamesScopeGrid.MultiSelect = false;
            this.HostnamesScopeGrid.Name = "HostnamesScopeGrid";
            this.HostnamesScopeGrid.ReadOnly = true;
            this.HostnamesScopeGrid.RowHeadersVisible = false;
            this.HostnamesScopeGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.HostnamesScopeGrid.Size = new System.Drawing.Size(316, 151);
            this.HostnamesScopeGrid.TabIndex = 26;
            this.HostnamesScopeGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HostnamesScopeGrid_CellClick);
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewCheckBoxColumn1.HeaderText = "";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.ReadOnly = true;
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewCheckBoxColumn1.Width = 20;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "SELECT HOSTNAMES IN WORKFLOWS TO SCAN";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ScanBtn
            // 
            this.ScanBtn.Location = new System.Drawing.Point(34, 210);
            this.ScanBtn.Name = "ScanBtn";
            this.ScanBtn.Size = new System.Drawing.Size(118, 23);
            this.ScanBtn.TabIndex = 27;
            this.ScanBtn.Text = "Scan Workflows";
            this.ScanBtn.UseVisualStyleBackColor = true;
            this.ScanBtn.Click += new System.EventHandler(this.ScanBtn_Click);
            // 
            // StatusProgressBar
            // 
            this.StatusProgressBar.Location = new System.Drawing.Point(211, 210);
            this.StatusProgressBar.MarqueeAnimationSpeed = 50;
            this.StatusProgressBar.Name = "StatusProgressBar";
            this.StatusProgressBar.Size = new System.Drawing.Size(337, 23);
            this.StatusProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.StatusProgressBar.TabIndex = 28;
            this.StatusProgressBar.Visible = false;
            // 
            // StatusLbl
            // 
            this.StatusLbl.AutoSize = true;
            this.StatusLbl.Location = new System.Drawing.Point(33, 255);
            this.StatusLbl.Name = "StatusLbl";
            this.StatusLbl.Size = new System.Drawing.Size(151, 13);
            this.StatusLbl.TabIndex = 29;
            this.StatusLbl.Text = "                                                ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(555, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Identified workflows are shows below. After all workflows are run click on the \'S" +
    "can Workflows\' button to scan them. ";
            // 
            // WorkflowScannerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 361);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StatusLbl);
            this.Controls.Add(this.StatusProgressBar);
            this.Controls.Add(this.ScanBtn);
            this.Controls.Add(this.HostnamesScopeGrid);
            this.Controls.Add(this.WorkflowsGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(650, 400);
            this.MinimumSize = new System.Drawing.Size(650, 400);
            this.Name = "WorkflowScannerWindow";
            this.Text = "Workflow Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WorkflowScannerWindow_FormClosing);
            this.Load += new System.EventHandler(this.WorkflowScannerWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.WorkflowsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HostnamesScopeGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.DataGridView WorkflowsGrid;
        internal System.Windows.Forms.DataGridView HostnamesScopeGrid;
        private System.Windows.Forms.Button ScanBtn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.ProgressBar StatusProgressBar;
        private System.Windows.Forms.Label StatusLbl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewCheckBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn27;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
    }
}