using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IronWASP.Workflow
{
    public partial class WorkflowScannerWindow : Form
    {
        static WorkflowScannerWindow OpenWorkflowScanner = null;
        
        public WorkflowScannerWindow()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenWorkflowScanner = new WorkflowScannerWindow();
                OpenWorkflowScanner.Show();
            }
            OpenWorkflowScanner.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenWorkflowScanner == null)
            {
                return false;
            }
            else if (OpenWorkflowScanner.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal static void UpdateScanStatusInUi(bool Running, string Message)
        {
            try
            {
                OpenWorkflowScanner.UpdateScanStatus(Running, Message);
            }catch{ }
        }

        delegate void UpdateScanStatus_d(bool Running, string Message);
        void UpdateScanStatus(bool Running, string Message)
        {
            if (StatusProgressBar.InvokeRequired)
            {
                UpdateScanStatus_d CALL_d = new UpdateScanStatus_d(UpdateScanStatus);
                StatusProgressBar.Invoke(CALL_d, new object[]{Running, Message});
            }
            else
            {
                StatusProgressBar.Visible = Running;
                StatusLbl.Text = Message;
                if (!Running)
                {
                    ScanBtn.Text = "Scan Workflows";
                }
            }
        }


        internal static void UpdateWorkflowEntryInUi(int[] WorkflowMarker, string WorkflowName)
        {
            try
            {
                OpenWorkflowScanner.UpdateWorkflowEntry(WorkflowMarker, WorkflowName);
            }
            catch { }
        }

        delegate void UpdateWorkflowEntry_d(int[] WorkflowMarker, string WorkflowName);
        void UpdateWorkflowEntry(int[] WorkflowMarker, string WorkflowName)
        {
            if (StatusProgressBar.InvokeRequired)
            {
                UpdateWorkflowEntry_d CALL_d = new UpdateWorkflowEntry_d(UpdateWorkflowEntry);
                StatusProgressBar.Invoke(CALL_d, new object[] { WorkflowMarker, WorkflowName });
            }
            else
            {
                WorkflowsGrid.Rows.Add(new object[] { WorkflowsGrid.Rows.Count + 1, string.Format("{0} - {1}", WorkflowMarker[0], WorkflowMarker[1]), WorkflowName });
            }
        }


        internal static void UpdateWorkflowHostEntryInUi(string Host)
        {
            try
            {
                OpenWorkflowScanner.UpdateWorkflowHostEntry(Host);
            }
            catch { }
        }

        delegate void UpdateWorkflowHostEntry_d(string Host);
        void UpdateWorkflowHostEntry(string Host)
        {
            if (StatusProgressBar.InvokeRequired)
            {
                UpdateWorkflowHostEntry_d CALL_d = new UpdateWorkflowHostEntry_d(UpdateWorkflowHostEntry);
                StatusProgressBar.Invoke(CALL_d, new object[] { Host });
            }
            else
            {
                HostnamesScopeGrid.Rows.Add(new object[] { true, Host });
            }
        }

        private void ScanBtn_Click(object sender, EventArgs e)
        {
            if (ScanBtn.Text.Equals("Scan Workflows"))
            {
                ScanBtn.Text = "Stop Scan";
                List<string> AllowedHosts = new List<string>();
                foreach (DataGridViewRow Row in HostnamesScopeGrid.Rows)
                {
                    if ((bool)Row.Cells[0].Value)
                    {
                        AllowedHosts.Add(Row.Cells[1].Value.ToString());
                    }
                }
                StatusProgressBar.Visible = true;
                WorkflowScanner.SetAllowedWorkflowHosts(AllowedHosts);
                WorkflowScanner.StartWorkFlowScans();
            }
            else
            {
                WorkflowScanner.StopWorkFlowScans();
                StatusProgressBar.Visible = false;
                ScanBtn.Text = "Scan Workflows";
            }
        }

        private void WorkflowScannerWindow_Load(object sender, EventArgs e)
        {
            foreach (string Host in WorkflowScanner.GetWorkflowHostsList())
            {
                HostnamesScopeGrid.Rows.Add(new object[]{true, Host});
            }
            foreach (int[] Marker in WorkflowScanner.GetWorkflowMarkersList())
            {
                WorkflowsGrid.Rows.Add(new object[] { WorkflowsGrid.Rows.Count + 1, string.Format("{0} - {1}", Marker[0], Marker[1])});
            }
        }

        private void WorkflowScannerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WorkflowScanner.StopWorkFlowScans();
            }
            catch { }
        }

        private void HostnamesScopeGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (HostnamesScopeGrid.SelectedRows.Count > 0)
            {
                if ((bool)HostnamesScopeGrid.SelectedRows[0].Cells[0].Value)
                {
                    HostnamesScopeGrid.SelectedRows[0].Cells[0].Value = false;
                }
                else
                {
                    HostnamesScopeGrid.SelectedRows[0].Cells[0].Value = true;
                }
            }
        }
    }
}
