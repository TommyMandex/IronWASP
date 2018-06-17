using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace IronWASP.Hawas
{
    public partial class BrokenAuthTester : Form
    {
        Thread WorkerThread = null;

        static BrokenAuthTester OpenBrokenAuthTester = null;

        bool InResultsStage = false;

        public BrokenAuthTester()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenBrokenAuthTester = new BrokenAuthTester();
                OpenBrokenAuthTester.Show();
            }
            OpenBrokenAuthTester.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenBrokenAuthTester == null)
            {
                return false;
            }
            else if (OpenBrokenAuthTester.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LogRangeAllRB_CheckedChanged(object sender, EventArgs e)
        {
            LogRangeStartTB.Enabled = LogRangeBetweenIdsRB.Checked;
            LogRangeEndTB.Enabled = LogRangeBetweenIdsRB.Checked;
        }

        private void BrokenAuthTester_Load(object sender, EventArgs e)
        {
            WorkerThread = new Thread(GetInitialScopeValuesFromDB);
            WorkerThread.Start();
        }

        void GetInitialScopeValuesFromDB()
        {
            try
            {
                List<string> Hosts = IronDB.GetUniqueHostsFromProxyLog();
                List<string> Files = IronDB.GetUniqueFilesFromProxyLog();
                ShowConfigScopeValues(Hosts, Files);
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp) { IronException.Report("Error reading host and file values from DB", Exp); }
        }

        delegate void ShowConfigScopeValues_d(List<string> Hosts, List<string> Files);
        void ShowConfigScopeValues(List<string> Hosts, List<string> Files)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                ShowConfigScopeValues_d CALL_d = new ShowConfigScopeValues_d(ShowConfigScopeValues);
                ConfigurePanel.Invoke(CALL_d, new object[] { Hosts, Files });
            }
            else
            {                
                List<List<string>> HostPartsList = new List<List<string>>();

                foreach (string Host in Hosts)
                {
                    List<string> Parts = new List<string>(Host.Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries));
                    Parts.Reverse();
                    HostPartsList.Add(Parts);
                }

                HostnamesScopeTree.Nodes.Clear();

                foreach (List<string> HostParts in HostPartsList)
                {
                    HostParts.Reverse();
                    string HostName = string.Join(".", HostParts.ToArray());
                    HostParts.Reverse();

                    string BaseHost = "";
                    if (HostParts.Count > 1)
                    {
                        BaseHost = string.Format("{0}.{1}", HostParts[1], HostParts[0]);
                    }
                    else
                    {
                        BaseHost = HostParts[0];
                    }

                    TreeNode BaseNode = null;

                    if (HostnamesScopeTree.Nodes.ContainsKey(BaseHost))
                    {
                        BaseNode = HostnamesScopeTree.Nodes[BaseHost];
                    }
                    else
                    {
                        BaseNode = HostnamesScopeTree.Nodes.Add(BaseHost, "  " + BaseHost);
                    }

                    for (int i = 2; i < HostParts.Count; i++)
                    {
                        if (BaseNode.Nodes.ContainsKey(HostParts[i]))
                        {
                            BaseNode = BaseNode.Nodes[HostParts[i]];
                        }
                        else
                        {
                            string NodeText = "";
                            StringBuilder SB = new StringBuilder("  ");
                            for (int j = i; j >= 0; j--)
                            {
                                SB.Append(HostParts[j]);
                                SB.Append(".");
                            }
                            NodeText = SB.ToString().Trim('.');
                            BaseNode = BaseNode.Nodes.Add(HostParts[i], NodeText);
                        }
                    }

                }
                HostnamesScopeTree.ExpandAll();

                FileTypesScopeGrid.Rows.Clear();
                foreach (string File in Files)
                {
                    if (File.Trim().Length == 0)
                    {
                        FileTypesScopeGrid.Rows.Add(new object[] { true, " NO EXTENSION " });
                    }
                    else
                    {
                        if (Crawler.ExtenionsToAvoid.Contains(File))
                        {
                            FileTypesScopeGrid.Rows.Add(new object[] { false, File });
                        }
                        else
                        {
                            FileTypesScopeGrid.Rows.Add(new object[] { true, File });
                        }
                    }
                }
                ConfigureStepProgressBar.Visible = false;
                ConfigurePanel.Visible = true;
            }
        }

        delegate void ShowMatchingRecordValues_d(List<LogRow> Records);
        void ShowMatchingRecordValues(List<LogRow> Records)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                ShowMatchingRecordValues_d CALL_d = new ShowMatchingRecordValues_d(ShowMatchingRecordValues);
                ConfigurePanel.Invoke(CALL_d, new object[] { Records });
            }
            else
            {
                CandidatesGrid.Rows.Clear();

                FilterTree.Nodes.Clear();
                FilterTree.Nodes.Add("Methods").Checked = true;
                FilterTree.Nodes.Add("File Extensions").Checked = true;
                FilterTree.Nodes.Add("Urls").Checked = true;

                foreach (LogRow LR in Records)
                {
                    if (!FilterTree.Nodes[0].Nodes.ContainsKey(LR.Method))
                    {
                        FilterTree.Nodes[0].Nodes.Add(LR.Method, LR.Method).Checked = true;
                    }
                    string File = LR.File;
                    if (File.Trim().Length == 0)
                    {
                        File = " - NO EXTENSION - ";
                    }
                    if (!FilterTree.Nodes[1].Nodes.ContainsKey(File))
                    {
                        FilterTree.Nodes[1].Nodes.Add(File, File).Checked = true;
                    }
                    if (!FilterTree.Nodes[2].Nodes.ContainsKey(LR.Host))
                    {
                        FilterTree.Nodes[2].Nodes.Add(LR.Host, LR.Host).Checked = true;
                    }
                    TreeNode HostNode = FilterTree.Nodes[2].Nodes[LR.Host];
                    if (!HostNode.Nodes.ContainsKey("/"))
                    {
                        HostNode.Nodes.Add("/", "/").Checked = true;
                    }
                    Request Req = new Request(string.Format("http://{0}{1}", LR.Host, LR.Url));
                    
                    for(int i=0; i < Req.UrlPathParts.Count; i++)
                    {
                        string Path = Req.UrlPathParts[i];
                        string FullPath = "";
                        if (Req.UrlPathParts.Count > 0)
                        {
                            StringBuilder SB = new StringBuilder();
                            for (int j = 0; j <= i; j++)
                            {
                                SB.Append("/");
                                SB.Append(Req.UrlPathParts[j]);
                            }
                            FullPath = SB.ToString();
                        }
                        else
                        {
                            FullPath = "/";
                        }
                        if (!HostNode.Nodes.ContainsKey(FullPath))
                        {
                            HostNode.Nodes.Add(FullPath, Path).Checked = true;
                            if (!HostNode.Checked)
                            {
                                HostNode.Checked = true;
                            }
                        }
                        HostNode = HostNode.Nodes[FullPath];
                    }
                }

                //Adding the rows after the tree population since every check on the tree node would trigger a filter application on the grids
                foreach (LogRow LR in Records)
                {
                    object[] Fields = LR.ToLogAnalyzerGridRowObjectArray();
                    Fields[0] = true;
                    CandidatesGrid.Rows.Add(Fields);
                }
                FilterTree.ExpandAll();
                CandidatesStepProgressBar.Visible = false;
                SelectCandidatesBotttomSplit.Visible = true;
                TestSelectedCandidatesBtn.Visible = true;
            }
        }

        delegate void AddTestResult_d(string OriginalRequestBinaryString, string OriginalResonseBinaryString, string TestRequestBinaryString, string TestResponseBinaryString, int PercentOfDifference, Request OriginalRequest, Request TestRequest);
        void AddTestResult(string OriginalRequestBinaryString, string OriginalResonseBinaryString, string TestRequestBinaryString, string TestResponseBinaryString, int PercentOfDifference, Request OriginalRequest, Request TestRequest)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                AddTestResult_d CALL_d = new AddTestResult_d(AddTestResult);
                ConfigurePanel.Invoke(CALL_d, new object[] { OriginalRequestBinaryString, OriginalResonseBinaryString, TestRequestBinaryString, TestResponseBinaryString, PercentOfDifference, OriginalRequest, TestRequest });
            }
            else
            {
                /*
                    ID
                    HostName
                    URL
                    Test ID
                    % of diff
                    Ori Req BS
                    Ori Res BS
                    Test Req BS
                    Test Res BS
                */
                int RowId = ResultsGrid.Rows.Add(new object[] { OriginalRequest.ID, OriginalRequest.Host, OriginalRequest.Url, TestRequest.ID, PercentOfDifference, OriginalRequestBinaryString, OriginalResonseBinaryString, TestRequestBinaryString, TestResponseBinaryString });
                if (PercentOfDifference <= 10)
                {
                    ResultsGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.Red;
                }
                else if (PercentOfDifference <= 20)
                {
                    ResultsGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.IndianRed;
                }
                else if (PercentOfDifference <= 30)
                {
                    ResultsGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.OrangeRed;
                }
            }
        }

        delegate void ResultsEnd_d();
        void ResultsEnd()
        {
            if (ConfigurePanel.InvokeRequired)
            {
                ResultsEnd_d CALL_d = new ResultsEnd_d(ResultsEnd);
                ConfigurePanel.Invoke(CALL_d, new object[] { });
            }
            else
            {
                ResultsStepProgressBar.Visible = false;
            }
        }

        List<string> GetSelectedHosts(TreeNode Node)
        {
            List<string> SelectedHosts = new List<string>();
            if (Node.Checked)
            {
                if (!SelectedHosts.Contains(Node.Text.Trim()))
                {
                    SelectedHosts.Add(Node.Text.Trim());
                }
            }
            foreach (TreeNode ChildNode in Node.Nodes)
            {
                SelectedHosts.AddRange(GetSelectedHosts(ChildNode));
            }
            return SelectedHosts;
        }

        private void FindCandidatesBtn_Click(object sender, EventArgs e)
        {
            List<string> SelectedHosts = new List<string>();
            foreach (TreeNode Node in HostnamesScopeTree.Nodes)
            {
                SelectedHosts.AddRange(GetSelectedHosts(Node));
            }
            
            List<string> SelectedFileTypes = new List<string>();
            foreach (DataGridViewRow Row in FileTypesScopeGrid.Rows)
            {
                if ((bool)Row.Cells[0].Value)
                {
                    if (Row.Cells[1].Value.ToString().Equals(" NO EXTENSION "))
                    {
                        SelectedFileTypes.Add("");
                    }
                    else
                    {
                        SelectedFileTypes.Add(Row.Cells[1].Value.ToString());
                    }
                }
            }
            if (SelectedHosts.Count == 0)
            {
                MessageBox.Show("Atleast one host must be selected for testing");
                return;
            }
            if (SelectedFileTypes.Count == 0)
            {
                MessageBox.Show("Atleast one file extension must be selected for testing");
                return;
            }

            SelectCandidatesBotttomSplit.Visible = false;
            CandidatesStepProgressBar.Visible = true;
            BaseTabs.SelectTab(1);
            try
            {
                WorkerThread.Abort();
            }
            catch { }
            WorkerThread = new Thread(FindCandidatesFromDB);
            WorkerThread.Start(new Dictionary<string, List<string>>() { { "Hosts", SelectedHosts }, { "File", SelectedFileTypes } });
        }

        void FindCandidatesFromDB(object FilterDictObj)
        {
            try
            {
                Dictionary<string, List<string>> FilterInfo = (Dictionary<string, List<string>>)FilterDictObj;

                List<LogRow> MatchingRecords = IronDB.GetRecordsFromProxyLogMatchingFilters(FilterInfo["Hosts"], FilterInfo["File"], "");
                List<LogRow> RecordsToTest = new List<LogRow>();
                foreach (LogRow LR in MatchingRecords)
                {
                    Request Req = Request.FromProxyLog(LR.ID);
                    RecordsToTest.Add(LR);
                }

                //Show these records on the page
                ShowMatchingRecordValues(RecordsToTest);
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                MessageBox.Show(string.Format("Error finding candidates - {0}", Exp.Message));
            }
        }

        private void TestSelectedCandidatesBtn_Click(object sender, EventArgs e)
        {
            List<int> SelectedLogIds = new List<int>();
            foreach (DataGridViewRow Row in CandidatesGrid.Rows)
            {
                if ((bool)Row.Cells[0].Value)
                {
                    SelectedLogIds.Add((int)Row.Cells[1].Value);
                }
            }
            if (SelectedLogIds.Count == 0)
            {
                MessageBox.Show("No candidates were selected for testing, select atleast one candidate.");
                return;
            }

            ResultsGrid.Rows.Clear();
            ResultsStepProgressBar.Visible = true;
            InResultsStage = true;
            BaseTabs.SelectTab(2);
            try
            {
                WorkerThread.Abort();
            }
            catch { }
            WorkerThread = new Thread(TestSelectedCandidates);
            WorkerThread.Start(SelectedLogIds);
        }

        void TestSelectedCandidates(object SelectedItemsObj)
        {
            try
            {
                List<int> LogIds = (List<int>)SelectedItemsObj;
                foreach (int LogId in LogIds)
                {
                    TestLog(LogId);
                }
                ResultsEnd();
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                MessageBox.Show(string.Format("Error testing candidates - {0}", Exp.Message));
            }
        }

        void TestLog(int LogId)
        {
            Session Sess = Session.FromProxyLog(LogId);
            if (Sess.Response != null)
            {
                Request ReqToTest = Sess.Request.GetClone();
                ReqToTest.SetSource("BrokenAuthTester");
                Response Res = ReqToTest.Send();

                int DiffPercent = Tools.DiffLevel(Sess.Response.ToString(), Res.ToString());

                AddTestResult(Sess.Request.ToBinaryString(), Sess.Response.ToBinaryString(), ReqToTest.ToBinaryString(), Res.ToBinaryString(), DiffPercent, Sess.Request, ReqToTest);
            }
        }

        private void ResultsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //ShowSelectedResultItem();
        }

        void ShowSelectedResultItem()
        {
            if (ResultsGrid.SelectedRows.Count > 0)
            {
                DataGridViewRow Row = ResultsGrid.SelectedRows[0];
                Request OriginalRequest = Request.FromBinaryString(Row.Cells["OriginalRequestBinaryStringClmn"].Value.ToString());
                Response OriginalResponse = Response.FromBinaryString(Row.Cells["OriginalResponseBinaryStringClmn"].Value.ToString());
                Request TestRequest = Request.FromBinaryString(Row.Cells["TestRequestBinaryStringClmn"].Value.ToString());
                Response TestResponse = Response.FromBinaryString(Row.Cells["TestResponseBinaryStringClmn"].Value.ToString());

                string OriginalRequestString = OriginalRequest.ToString();
                string OriginalResponseString = OriginalResponse.ToString();
                string TestRequestString = TestRequest.ToString();
                string TestResponseString = TestResponse.ToString();

                string[] OriginalVsTestRequestSidebySideResults = DiffWindow.DoSideBySideDiff(OriginalRequestString, TestRequestString);
                string[] OriginalVsTestResponseSidebySideResults = DiffWindow.DoSideBySideDiff(OriginalResponseString, TestResponseString);

                string OriginalVsTestRequestSinglePageResults = DiffWindow.DoSinglePageDiff(OriginalRequestString, TestRequestString);
                string OriginalVsTestResponseSinglePageResults = DiffWindow.DoSinglePageDiff(OriginalResponseString, TestResponseString);

                OriginalVsTestRequestDRV.ShowDiffResults(OriginalVsTestRequestSinglePageResults, OriginalVsTestRequestSidebySideResults[0], OriginalVsTestRequestSidebySideResults[1]);
                OriginalVsTestResponseDRV.ShowDiffResults(OriginalVsTestResponseSinglePageResults, OriginalVsTestResponseSidebySideResults[0], OriginalVsTestResponseSidebySideResults[1]);

                OriginalRequestView.SetRequest(OriginalRequest);
                OriginalResponseView.SetResponse(OriginalResponse, OriginalRequest);
                TestRequestView.SetRequest(TestRequest);
                TestResponseView.SetResponse(TestResponse, TestRequest);
            }
        }

        private void CandidatesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (CandidatesGrid.SelectedRows.Count > 0)
            {
                if (e.ColumnIndex == 0)
                {
                    if ((bool)CandidatesGrid.SelectedRows[0].Cells[0].Value)
                    {
                        CandidatesGrid.SelectedRows[0].Cells[0].Value = false;
                    }
                    else
                    {
                        CandidatesGrid.SelectedRows[0].Cells[0].Value = true;
                    }
                }
                else
                {
                    //ShowSelectedLog();   
                }
            }
        }

        void ShowSelectedLog()
        {
            try
            {
                SessView.LoadAndShowSession((int)CandidatesGrid.SelectedRows[0].Cells[1].Value, "Proxy");
            }
            catch { }
        }

        private void FileTypesScopeGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (FileTypesScopeGrid.SelectedRows.Count > 0)
            {
                if (e.ColumnIndex == 0)
                {
                    if ((bool)FileTypesScopeGrid.SelectedRows[0].Cells[0].Value)
                    {
                        FileTypesScopeGrid.SelectedRows[0].Cells[0].Value = false;
                    }
                    else
                    {
                        FileTypesScopeGrid.SelectedRows[0].Cells[0].Value = true;
                    }
                }
            }
        }

        private void LogRangeStartTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void LogRangeEndTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void HostnamesScopeTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void HostnamesScopeTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode Node in e.Node.Nodes)
            {
                Node.Checked = e.Node.Checked;
            }
        }

        private void FilterTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                e.Cancel = true;
            }
        }

        int CurrentFilterTreeNodeLevel = -1;
        private void FilterTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level > CurrentFilterTreeNodeLevel)
            {
                CurrentFilterTreeNodeLevel = e.Node.Level;
            }
            foreach (TreeNode Node in e.Node.Nodes)
            {
                Node.Checked = e.Node.Checked;
            }
            if (e.Node.Level == CurrentFilterTreeNodeLevel)
            {
                UpdateCandidatesCheckBasedOnFilter();
                CurrentFilterTreeNodeLevel = -1;
            }
        }

        void UpdateCandidatesCheckBasedOnFilter()
        {
            List<string> AllowedMethods = new List<string>();
            Dictionary<string, List<string>> AllowedUrls = new Dictionary<string, List<string>>();
            List<string> AllowedFileExts = new List<string>();

            if (FilterTree.Nodes.Count == 3)
            {
                foreach (TreeNode Node in FilterTree.Nodes[0].Nodes)
                {
                    if (Node.Checked)
                    {
                        AllowedMethods.Add(Node.Name);
                    }
                }

                foreach (TreeNode Node in FilterTree.Nodes[1].Nodes)
                {
                    if (Node.Checked)
                    {
                        AllowedFileExts.Add(Node.Name);
                    }
                }

                foreach (TreeNode Node in FilterTree.Nodes[2].Nodes)
                {
                    if (Node.Checked)
                    {
                        AllowedUrls[Node.Name] = GetSelectedUrlPathsForNode(Node);
                    }
                }
            }

            foreach (DataGridViewRow Row in CandidatesGrid.Rows)
            {
                if (AllowedUrls.ContainsKey(Row.Cells["HostNameSelectClmn"].Value.ToString()) 
                    && AllowedMethods.Contains(Row.Cells["MethodClmn"].Value.ToString()) 
                    && (AllowedFileExts.Contains(Row.Cells["FileClmn"].Value.ToString()) || (Row.Cells["FileClmn"].Value.ToString().Length == 0 && AllowedFileExts.Contains(" - NO EXTENSION - "))))
                {
                    bool UrlMatchFound = false;
                    string Url = Row.Cells["URLClmn"].Value.ToString();
                    string UrlWithQueryMarker = string.Format("{0}?", Url);
                    foreach (string UrlPath in AllowedUrls[Row.Cells["HostNameSelectClmn"].Value.ToString()])
                    {
                        if (UrlPath.Equals(Url))
                        {
                            UrlMatchFound = true;
                            break;
                        }
                        else if (Url.StartsWith(UrlPath) && Url.StartsWith(UrlWithQueryMarker))
                        {
                            UrlMatchFound = true;
                            break;
                        }
                    }
                    if (UrlMatchFound)
                    {
                        Row.Cells[0].Value = true;
                    }
                    else
                    {
                        Row.Cells[0].Value = false;
                    }
                }
                else
                {
                    Row.Cells[0].Value = false;
                }
            }
        }

        List<string> GetSelectedUrlPathsForNode(TreeNode Node)
        {
            List<string> Result = new List<string>();
            foreach (TreeNode ChildNode in Node.Nodes)
            {
                if (ChildNode.Checked)
                {
                    Result.Add(ChildNode.Name);
                }
                Result.AddRange(GetSelectedUrlPathsForNode(ChildNode));
            }
            return Result;
        }

        private void FilterTree_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if(e.Node.Level == 0) e.Cancel = true;
            if (e.Action != TreeViewAction.Unknown && !e.Node.Checked && e.Node.Level > 1)
            {
                TreeNode HostnameNode = GetHostnameNode(e.Node);
                if (HostnameNode != null)
                {
                    if (!HostnameNode.Checked)
                    {
                        e.Cancel = true;
                        MessageBox.Show("The hostname node is not checked, cannot select sections of the host without selecting the hostname.");
                    }
                }
            }
        }

        private void BrokenAuthTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WorkerThread.Abort();
            }
            catch { }
        }

        TreeNode GetHostnameNode(TreeNode CurrentNode)
        {
            if (CurrentNode.Level == 1 && CurrentNode.Parent.Index == 2)
            {
                return CurrentNode;
            }
            else if (CurrentNode.Level > 1)
            {
                return GetHostnameNode(CurrentNode.Parent);
            }
            else
            {
                return null;
            }
        }

        private void BaseTabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (InResultsStage && e.TabPageIndex != 2) e.Cancel = true;
        }

        long TimeWhenLogGridIndexWasLastChanged = 0;
        private void CandidatesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay.TotalMilliseconds > TimeWhenLogGridIndexWasLastChanged + 200)
            {
                ShowSelectedLog();
            }
            TimeWhenLogGridIndexWasLastChanged = (long)DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        private void ResultsGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay.TotalMilliseconds > TimeWhenLogGridIndexWasLastChanged + 200)
            {
                ShowSelectedResultItem();
            }
            TimeWhenLogGridIndexWasLastChanged = (long)DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        private void ShowDocLL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Docs.DocForBrokenAuthTester DF = new Docs.DocForBrokenAuthTester();
            DF.Show();
        }
    }
}
