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
    public partial class PrivillegeEscalationTester : Form
    {
        static PrivillegeEscalationTester OpenPrivEscalationTester = null;

        Recording.Recording LoginRecording = null;

        Thread WorkerThread = null;

        bool InResultsStage = false;

        List<LogRow> MatchingRecords = new List<LogRow>();
        Dictionary<string, string> CookieRawToEnocodedMap = new Dictionary<string, string>();

        string FinalCookieStringOfSessionA = "";
        string FinalCookieStringOfSessionB = "";

        string SelectedSessionForTesting = "A";

        Dictionary<string, List<string>> FinalCookieValuesofUserA = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> FinalCookieValuesofUserB = new Dictionary<string, List<string>>();

        public PrivillegeEscalationTester()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenPrivEscalationTester = new PrivillegeEscalationTester();
                OpenPrivEscalationTester.Show();
            }
            OpenPrivEscalationTester.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenPrivEscalationTester == null)
            {
                return false;
            }
            else if (OpenPrivEscalationTester.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void PrivillegeEscalationTester_Load(object sender, EventArgs e)
        {
            WorkerThread = new Thread(GetInitialScopeValuesFromDB);
            WorkerThread.Start();

            RecordingSelectBox.Items.Clear();
            foreach (string Name in Recording.Recording.GetNames())
            {
                RecordingSelectBox.Items.Add(Name);
            }
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
                ConfigureProgressBar.Visible = false;
                ConfigurePanel.Visible = true;
            }                
        }

        delegate void ShowCookieInfo_d(Dictionary<string, Dictionary<string, List<string>>> CookieInfo);
        void ShowCookieInfo(Dictionary<string, Dictionary<string, List<string>>> CookieInfo)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                ShowCookieInfo_d CALL_d = new ShowCookieInfo_d(ShowCookieInfo);
                ConfigurePanel.Invoke(CALL_d, new object[] { CookieInfo });
            }
            else
            {
                SelectSessionsGrid.Rows.Clear();
                foreach (string Host in CookieInfo.Keys)
                {
                    foreach (string Name in CookieInfo[Host].Keys)
                    {
                        foreach (string Value in CookieInfo[Host][Name])
                        {
                            SelectSessionsGrid.Rows.Add(new object[] { false, false, Host, Name, Value });
                        }
                    }
                }
                SessionsProgressBar.Visible = false;
                CandidatesBottomSplit.Visible = true;
                FindCandidatesBtn.Visible = true;
            }
        }

        delegate void ShowCandidatesList_d(Dictionary<string, List<string>> Urls, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> ACandidates, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> BCandidates);
        void ShowCandidatesList(Dictionary<string, List<string>> Urls, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> ACandidates, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> BCandidates)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                ShowCandidatesList_d CALL_d = new ShowCandidatesList_d(ShowCandidatesList);
                ConfigurePanel.Invoke(CALL_d, new object[] { Urls, ACandidates, BCandidates });
            }
            else
            {

                CandidatesProgressBar.Visible = false;
                CandidatesBottomSplit.Visible = true;
                TestCandidatesBtn.Visible = true;


                CandidatesGrid.Rows.Clear();

                FilterTree.Nodes.Clear();
                FilterTree.Nodes.Add("Methods").Checked = true;
                FilterTree.Nodes.Add("File Extensions").Checked = true;
                FilterTree.Nodes.Add("Urls").Checked = true;

                PopulateFilterTree(Urls, ACandidates);
                PopulateFilterTree(Urls, BCandidates);


                FilterTree.ExpandAll();
                
                CandidatesGrid.Rows.Clear();
                foreach (string BaseUrl in Urls.Keys)
                {
                    if (ACandidates.ContainsKey(BaseUrl) && BCandidates.ContainsKey(BaseUrl))
                    {
                        foreach(string UrlPath in Urls[BaseUrl])
                        //foreach (string UrlPath in ACandidates[BaseUrl].Keys)
                        {
                            if (ACandidates[BaseUrl].ContainsKey(UrlPath))
                            {
                                foreach (Dictionary<string, object> Dict in ACandidates[BaseUrl][UrlPath])
                                {
                                    int RowId = CandidatesGrid.Rows.Add(new object[] { SelectUserARB.Checked, Dict["id"], Dict["host"], Dict["method"], Dict["url"], Dict["file"] });
                                    CandidatesGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.Orange;
                                }
                            }
                            if (BCandidates[BaseUrl].ContainsKey(UrlPath))
                            {
                                foreach (Dictionary<string, object> Dict in BCandidates[BaseUrl][UrlPath])
                                {
                                    int RowId = CandidatesGrid.Rows.Add(new object[] { SelectUserBRB.Checked, Dict["id"], Dict["host"], Dict["method"], Dict["url"], Dict["file"] });
                                    CandidatesGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.Green;
                                }
                            }
                        }
                    }
                    else if (ACandidates.ContainsKey(BaseUrl))
                    {
                        foreach (string UrlPath in ACandidates[BaseUrl].Keys)
                        {
                            foreach (Dictionary<string, object> Dict in ACandidates[BaseUrl][UrlPath])
                            {
                                int RowId = CandidatesGrid.Rows.Add(new object[] { SelectUserARB.Checked, Dict["id"], Dict["host"], Dict["method"], Dict["url"], Dict["file"] });
                                CandidatesGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.Orange;
                            }
                        }
                    }
                    else
                    {
                        foreach (string UrlPath in BCandidates[BaseUrl].Keys)
                        {
                            foreach (Dictionary<string, object> Dict in BCandidates[BaseUrl][UrlPath])
                            {
                                int RowId = CandidatesGrid.Rows.Add(new object[] { SelectUserBRB.Checked, Dict["id"], Dict["host"], Dict["method"], Dict["url"], Dict["file"] });
                                CandidatesGrid.Rows[RowId].DefaultCellStyle.BackColor = Color.Green;
                            }
                        }
                    }
                }
                SelectUserARB.Checked = true;
                TestCandidatesBtn.Visible = true;
            }
        }

        delegate void PopulateFilterTree_d(Dictionary<string, List<string>> Urls, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> Candidates);
        void PopulateFilterTree(Dictionary<string, List<string>> Urls, Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> Candidates)
        {
            if (ConfigurePanel.InvokeRequired)
            {
                PopulateFilterTree_d CALL_d = new PopulateFilterTree_d(PopulateFilterTree);
                ConfigurePanel.Invoke(CALL_d, new object[] { Urls, Candidates });
            }
            else
            {
                foreach(string BaseUrl in Urls.Keys)
                //foreach (LogRow LR in Records)
                {
                    if (Candidates.ContainsKey(BaseUrl))
                    {
                        foreach (string UrlPath in Candidates[BaseUrl].Keys)
                        {
                            foreach (Dictionary<string, object> Dict in Candidates[BaseUrl][UrlPath])
                            {
                                //Dict["id"], Dict["host"], Dict["method"], Dict["url"]
                                string Method = Dict["method"].ToString();
                                string Host = Dict["host"].ToString();
                                string Url = Dict["url"].ToString();
                                string File = Dict["file"].ToString();

                                if (!FilterTree.Nodes[0].Nodes.ContainsKey(Method))
                                {
                                    FilterTree.Nodes[0].Nodes.Add(Method, Method).Checked = true;
                                }

                                if (File.Trim().Length == 0)
                                {
                                    File = " - NO EXTENSION - ";
                                }
                                if (!FilterTree.Nodes[1].Nodes.ContainsKey(File))
                                {
                                    FilterTree.Nodes[1].Nodes.Add(File, File).Checked = true;
                                }
                                if (!FilterTree.Nodes[2].Nodes.ContainsKey(Host))
                                {
                                    FilterTree.Nodes[2].Nodes.Add(Host, Host).Checked = true;
                                }
                                TreeNode HostNode = FilterTree.Nodes[2].Nodes[Host];
                                if (!HostNode.Nodes.ContainsKey("/"))
                                {
                                    HostNode.Nodes.Add("/", "/").Checked = true;
                                }
                                Request Req = new Request(string.Format("http://{0}{1}", Host, Url));

                                for (int i = 0; i < Req.UrlPathParts.Count; i++)
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
                        }
                    }
                }
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

        private void FindSessionsBtn_Click(object sender, EventArgs e)
        {
            if (Recording.Recording.Has(RecordingSelectBox.Text))
            {
                LoginRecording = Recording.Recording.Get(RecordingSelectBox.Text);
            }

            CandidatesBottomSplit.Visible = false;
            SessionsProgressBar.Visible = true;
            BaseTabs.SelectTab(1);

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

            try
            {
                WorkerThread.Abort();
            }
            catch { }
            WorkerThread = new Thread(FindCandidatesFromDB);
            WorkerThread.Start(new Dictionary<string, List<string>>() { { "Hosts", SelectedHosts }, { "File", SelectedFileTypes } });
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

        void FindCandidatesFromDB(object FilterDictObj)
        {
            Dictionary<string, List<string>> FilterInfo = (Dictionary<string, List<string>>)FilterDictObj;

            MatchingRecords = IronDB.GetRecordsFromProxyLogMatchingFilters(FilterInfo["Hosts"], FilterInfo["File"], "");

            Dictionary<string, Dictionary<string, List<string>>> CookieInfo = new Dictionary<string, Dictionary<string, List<string>>>();

            CookieRawToEnocodedMap = new Dictionary<string, string>();

            foreach (LogRow LR in MatchingRecords)
            {
                Request Req = Request.FromProxyLog(LR.ID);
                if (Req.Cookie.Count > 0)
                {
                    if (!CookieInfo.ContainsKey(Req.BaseUrl))
                    {
                        CookieInfo[Req.BaseUrl] = new Dictionary<string, List<string>>();
                    }
                    foreach(string Name in Req.Cookie.GetNames())
                    {
                        if (!CookieInfo[Req.BaseUrl].ContainsKey(Name))
                        {
                            CookieInfo[Req.BaseUrl][Name] = new List<string>();
                        }

                        List<string> Values = Req.Cookie.GetAll(Name);
                        for (int i = 0; i < Values.Count; i++)
                        {
                            string Value = Values[i];
                            if (!CookieInfo[Req.BaseUrl][Name].Contains(Value))
                            {
                                CookieInfo[Req.BaseUrl][Name].Add(Value);
                            }
                            if (!CookieRawToEnocodedMap.ContainsKey(Value))
                            {
                                CookieRawToEnocodedMap[Value] = Req.Cookie.RawGetAll(Name)[i];
                            }
                        }
                    }
                }
            }

            ShowCookieInfo(CookieInfo);

            //Show these records on the page
            //ShowMatchingRecordValues(RecordsToTest);
        }

        private void SelectSessionsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (SelectSessionsGrid.SelectedRows.Count > 0)
            {
                DataGridViewRow SelectedRow = SelectSessionsGrid.SelectedRows[0];
                if (e.ColumnIndex == 0)
                {
                    if ((bool)SelectedRow.Cells[0].Value)
                    {
                        SelectedRow.Cells[0].Value = false;
                        SelectedRow.DefaultCellStyle.BackColor = Color.White;
                    }
                    else
                    {
                        SelectedRow.Cells[0].Value = true;
                        SelectedRow.Cells[1].Value = false;
                        SelectedRow.DefaultCellStyle.BackColor = Color.Orange;
                    }
                }
                else if (e.ColumnIndex == 1)
                {
                    if ((bool)SelectedRow.Cells[1].Value)
                    {
                        SelectedRow.Cells[1].Value = false;
                        SelectedRow.DefaultCellStyle.BackColor = Color.White;
                    }
                    else
                    {
                        SelectedRow.Cells[1].Value = true;
                        SelectedRow.Cells[0].Value = false;
                        SelectedRow.DefaultCellStyle.BackColor = Color.Green;
                    }
                }
                else
                {
                    //ShowSelectedCookieParameter();
                }
            }
        }

        void ShowSelectedCookieParameter()
        {
            if (SelectSessionsGrid.SelectedRows.Count > 0)
            {
                CookieValueTB.Text = SelectSessionsGrid.SelectedRows[0].Cells["CookieParamValueClmn"].Value.ToString();
            }
        }

        private void FindCandidatesBtn_Click(object sender, EventArgs e)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> SessionInfo = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>() { { "A", new Dictionary<string, Dictionary<string, List<string>>>() }, { "B", new Dictionary<string, Dictionary<string, List<string>>>() } };
            foreach (DataGridViewRow Row in SelectSessionsGrid.Rows)
            {
                string User = "";
                string Name = "";
                string Value = "";
                string Host = "";
                if ((bool)Row.Cells[0].Value)
                {
                    User = "A";
                }
                else if ((bool)Row.Cells[1].Value)
                {
                    User = "B";
                }
                if (User.Length > 0)
                {
                    Host = Row.Cells[2].Value.ToString();
                    Name = Row.Cells[3].Value.ToString();
                    Value = Row.Cells[4].Value.ToString();
                    if (!SessionInfo[User].ContainsKey(Host)) SessionInfo[User][Host] = new Dictionary<string, List<string>>();
                    if (!SessionInfo[User][Host].ContainsKey(Name)) SessionInfo[User][Host][Name] = new List<string>();
                    if (!SessionInfo[User][Host][Name].Contains(Value)) SessionInfo[User][Host][Name].Add(Value);
                }
            }
            if ((SessionInfo["A"].Count + SessionInfo["B"].Count) == 0)
            {
                MessageBox.Show("No Session Information was selected. Select User A and User B session info.");
                return;
            }
            try
            {
                WorkerThread.Abort();
            }
            catch { }
            CandidatesGrid.Rows.Clear();
            WorkerThread = new Thread(FindFinalCandidatesFromDB);
            WorkerThread.Start(SessionInfo);
            BaseTabs.SelectTab(2);
        }

        void FindFinalCandidatesFromDB(object FilterDictObj)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> FilterInfo = (Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>)FilterDictObj;

            Dictionary<string, List<Dictionary<string, object>>> UserAReqs = new Dictionary<string, List<Dictionary<string, object>>>();
            Dictionary<string, List<Dictionary<string, object>>> UserBReqs = new Dictionary<string, List<Dictionary<string, object>>>();

            Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> UniqueUserAReqs = new Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>>();
            Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>> UniqueUserBReqs = new Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>>();

            foreach (LogRow LR in MatchingRecords)
            {
                Request Req = Request.FromProxyLog(LR.ID);

                if (FilterInfo["A"].ContainsKey(Req.BaseUrl))
                {
                    foreach (string Name in FilterInfo["A"][Req.BaseUrl].Keys)
                    {
                        if (Req.Cookie.Has(Name))
                        {
                            foreach (string Value in FilterInfo["A"][Req.BaseUrl][Name])
                            {
                                if (Req.Cookie.GetAll(Name).Contains(Value))
                                {
                                    if (!UserAReqs.ContainsKey(Req.FullUrl)) UserAReqs[Req.FullUrl] = new List<Dictionary<string, object>>();
                                    Dictionary<string, object> ReqDict = new Dictionary<string, object>() {{"id", Req.ID}, {"method", Req.Method}, {"body", Tools.MD5(Req.BodyString)} };
                                    bool Duplicate = false;
                                    foreach (Dictionary<string, object> ExistingReqDict in UserAReqs[Req.FullUrl])
                                    {
                                        if (ExistingReqDict["method"].ToString().Equals(ReqDict["method"].ToString()) && ExistingReqDict["body"].ToString().Equals(ReqDict["body"].ToString()))
                                        {
                                            Duplicate = true;
                                            break;
                                        }
                                    }
                                    if (!Duplicate)
                                    {
                                        UserAReqs[Req.FullUrl].Add(ReqDict);
                                        FinalCookieStringOfSessionA = Req.CookieString;
                                    }
                                }
                            }
                        }
                    }
                }
                if (FilterInfo["B"].ContainsKey(Req.BaseUrl))
                {
                    foreach (string Name in FilterInfo["B"][Req.BaseUrl].Keys)
                    {
                        if (Req.Cookie.Has(Name))
                        {
                            foreach (string Value in FilterInfo["B"][Req.BaseUrl][Name])
                            {
                                if (Req.Cookie.GetAll(Name).Contains(Value))
                                {
                                    if (!UserBReqs.ContainsKey(Req.FullUrl)) UserBReqs[Req.FullUrl] = new List<Dictionary<string, object>>();
                                    Dictionary<string, object> ReqDict = new Dictionary<string, object>() { { "id", Req.ID }, { "method", Req.Method }, { "body", Tools.MD5(Req.BodyString) } };
                                    bool Duplicate = false;
                                    foreach (Dictionary<string, object> ExistingReqDict in UserBReqs[Req.FullUrl])
                                    {
                                        if (ExistingReqDict["method"].ToString().Equals(ReqDict["method"].ToString()) && ExistingReqDict["body"].ToString().Equals(ReqDict["body"].ToString()))
                                        {
                                            Duplicate = true;
                                            break;
                                        }
                                    }
                                    if (!Duplicate)
                                    {
                                        UserBReqs[Req.FullUrl].Add(ReqDict);
                                        FinalCookieStringOfSessionB = Req.CookieString;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (string Url in UserAReqs.Keys)
            {
                List<Dictionary<string, object>> ADictsToAdd = new List<Dictionary<string, object>>();
                if (UserBReqs.ContainsKey(Url))
                {
                    foreach (Dictionary<string, object> ADict in UserAReqs[Url])
                    {
                        bool Duplicate = false;
                        foreach (Dictionary<string, object> BDict in UserBReqs[Url])
                        {
                            if (ADict["method"].ToString().Equals(BDict["method"].ToString()) && ADict["body"].ToString().Equals(BDict["body"].ToString()))
                            {
                                Duplicate = true;
                                break;
                            }
                        }
                        if (!Duplicate)
                        {
                            ADictsToAdd.Add(ADict);
                        }
                    }
                }
                else
                {
                    ADictsToAdd.AddRange(UserAReqs[Url]);
                }
                
                foreach (Dictionary<string, object> ADict in ADictsToAdd)
                {
                    Request Req = new Request(Url);
                    if (!UniqueUserAReqs.ContainsKey(Req.BaseUrl)) UniqueUserAReqs[Req.BaseUrl] = new Dictionary<string, List<Dictionary<string, object>>>();
                    if (!UniqueUserAReqs[Req.BaseUrl].ContainsKey(Req.UrlPath)) UniqueUserAReqs[Req.BaseUrl][Req.UrlPath] = new List<Dictionary<string, object>>();
                    UniqueUserAReqs[Req.BaseUrl][Req.UrlPath].Add(new Dictionary<string, object>() { { "id", ADict["id"] }, { "host", Req.Host }, { "url", Req.Url }, { "file", Req.File }, { "method", ADict["method"] }, { "body", ADict["body"] } });
                }
            }

            foreach (string Url in UserBReqs.Keys)
            {
                List<Dictionary<string, object>> BDictsToAdd = new List<Dictionary<string, object>>();
                if (UserAReqs.ContainsKey(Url))
                {
                    foreach (Dictionary<string, object> BDict in UserBReqs[Url])
                    {
                        bool Duplicate = false;
                        foreach (Dictionary<string, object> ADict in UserAReqs[Url])
                        {
                            if (ADict["method"].ToString().Equals(BDict["method"].ToString()) && ADict["body"].ToString().Equals(BDict["body"].ToString()))
                            {
                                Duplicate = true;
                                break;
                            }
                        }
                        if (!Duplicate)
                        {
                            BDictsToAdd.Add(BDict);
                        }
                    }
                }
                else
                {
                    BDictsToAdd.AddRange(UserBReqs[Url]);
                }

                foreach (Dictionary<string, object> BDict in BDictsToAdd)
                {
                    Request Req = new Request(Url);
                    if (!UniqueUserBReqs.ContainsKey(Req.BaseUrl)) UniqueUserBReqs[Req.BaseUrl] = new Dictionary<string, List<Dictionary<string, object>>>();
                    if (!UniqueUserBReqs[Req.BaseUrl].ContainsKey(Req.UrlPath)) UniqueUserBReqs[Req.BaseUrl][Req.UrlPath] = new List<Dictionary<string, object>>();
                    UniqueUserBReqs[Req.BaseUrl][Req.UrlPath].Add(new Dictionary<string, object>() { { "id", BDict["id"] }, { "host", Req.Host }, { "url", Req.Url }, { "file", Req.File }, { "method", BDict["method"] }, { "body", BDict["body"] } });
                }
            }

            Dictionary<string, List<string>> UniqueUrlPaths = new Dictionary<string, List<string>>();
            foreach (string BaseUrl in UniqueUserAReqs.Keys)
            {
                if (!UniqueUrlPaths.ContainsKey(BaseUrl)) UniqueUrlPaths[BaseUrl] = new List<string>();
                UniqueUrlPaths[BaseUrl].AddRange(UniqueUserAReqs[BaseUrl].Keys);
            }

            foreach (string BaseUrl in UniqueUserBReqs.Keys)
            {
                if(!UniqueUrlPaths.ContainsKey(BaseUrl)) UniqueUrlPaths[BaseUrl] = new List<string>();
                foreach (string UrlPath in UniqueUserBReqs[BaseUrl].Keys)
                {
                    if (!UniqueUrlPaths[BaseUrl].Contains(UrlPath)) UniqueUrlPaths[BaseUrl].Add(UrlPath);
                }
            }
            foreach (string BaseUrl in UniqueUrlPaths.Keys)
            {
                UniqueUrlPaths[BaseUrl].Sort();
            }
            ShowCandidatesList(UniqueUrlPaths, UniqueUserAReqs, UniqueUserBReqs);
            //Show these records on the page
            //ShowMatchingRecordValues(RecordsToTest);
        }

        private void SelectUserARB_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectUserARB.Checked)
            {
                SelectCandidatesLbl.Text = "If you want to use the Login Sequence Recording of User B during the testing then select one:";
                foreach (DataGridViewRow Row in CandidatesGrid.Rows)
                {
                    if (Row.DefaultCellStyle.BackColor == Color.Orange)
                    {
                        Row.Cells[0].Value = true;
                    }
                    else
                    {
                        Row.Cells[0].Value = false;
                    }
                }
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
                        //CandidatesGrid.SelectedRows[0].Cells[0].Value = true;
                        Color SelectedColor = CandidatesGrid.SelectedRows[0].DefaultCellStyle.BackColor;
                        if ((SelectUserARB.Checked && SelectedColor == Color.Orange) || (SelectUserBRB.Checked && SelectedColor == Color.Green))
                        {
                            CandidatesGrid.SelectedRows[0].Cells[0].Value = true;
                        }
                        else
                        {
                            if (SelectUserARB.Checked)
                            {
                                MessageBox.Show("Cannot select Logs belonging to User B. Change the user selection option above to User B to select these logs.");
                            }
                            else
                            {
                                MessageBox.Show("Cannot select Logs belonging to User A. Change the user selection option above to User A to select these logs.");
                            }
                            return;
                        }
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

        private void SelectUserBRB_CheckedChanged(object sender, EventArgs e)
        {
            if (SelectUserBRB.Checked)
            {
                SelectCandidatesLbl.Text = "If you want to use the Login Sequence Recording of User A during the testing then select one:";
                foreach (DataGridViewRow Row in CandidatesGrid.Rows)
                {
                    if (Row.DefaultCellStyle.BackColor == Color.Green)
                    {
                        Row.Cells[0].Value = true;
                    }
                    else
                    {
                        Row.Cells[0].Value = false;
                    }
                }
            }
        }

        private void TestCandidatesBtn_Click(object sender, EventArgs e)
        {
            if (Recording.Recording.Has(RecordingSelectBox.Text))
            {
                LoginRecording = Recording.Recording.Get(RecordingSelectBox.Text);
            }
            else if (RecordingSelectBox.Text.Trim().Length > 0)
            {
                MessageBox.Show("Invalid Login recording name, enter a valid name.");
                return;
            }
            
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
            ResultsProgressBar.Visible = true;
            InResultsStage = true;
            BaseTabs.SelectTab(3);
            try
            {
                WorkerThread.Abort();
            }
            catch { }
            if (SelectUserARB.Checked)
            {
                SelectedSessionForTesting = "A";
            }
            else
            {
                SelectedSessionForTesting = "B";
            }
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
                ReqToTest.SetSource("PrivillegeEscalationTester");
                if (LoginRecording != null)
                {
                    ReqToTest.SetCookie(LoginRecording.Cookies);
                    if (!LoginRecording.IsLoggedIn())
                    {
                        LoginRecording.DoLogin();
                        if (!LoginRecording.IsLoggedIn())
                        {
                            throw new Exception("Unable to login user!");
                        }
                        ReqToTest.SetCookie(LoginRecording.Cookies);
                    }
                }
                else
                {
                    if (SelectedSessionForTesting == "A")
                    {
                        ReqToTest.CookieString = FinalCookieStringOfSessionB;
                    }
                    else
                    {
                        ReqToTest.CookieString = FinalCookieStringOfSessionA;
                    }

                }
                Response Res = ReqToTest.Send();

                int DiffPercent = Tools.DiffLevel(Sess.Response.ToString(), Res.ToString());

                AddTestResult(Sess.Request.ToBinaryString(), Sess.Response.ToBinaryString(), ReqToTest.ToBinaryString(), Res.ToBinaryString(), DiffPercent, Sess.Request, ReqToTest);
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
                ResultsProgressBar.Visible = false;
            }
        }

        private void ResultsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //ShowSelectedResultItem();
            //Leads to UI freezing as it interlocks with the _SelectionChanged event
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

        private void PrivillegeEscalationTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WorkerThread.Abort();
            }
            catch { }
        }

        private void HostnamesScopeTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode Node in e.Node.Nodes)
            {
                Node.Checked = e.Node.Checked;
            }
        }

        private void HostnamesScopeTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
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
                        if ((Row.DefaultCellStyle.BackColor == Color.Orange && SelectUserARB.Checked) || (Row.DefaultCellStyle.BackColor == Color.Green && SelectUserBRB.Checked))
                        {
                            Row.Cells[0].Value = true;
                        }
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
            if (e.Node.Level == 0) e.Cancel = true;
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

        private void FilterTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                e.Cancel = true;
            }
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
            if (InResultsStage && e.TabPageIndex != 3) e.Cancel = true;
        }

        private void SelectSessionsGrid_SelectionChanged(object sender, EventArgs e)
        {
            ShowSelectedCookieParameter();
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
            Docs.DocForPrivilegeEscalationTester DF = new Docs.DocForPrivilegeEscalationTester();
            DF.Show();
        }
    }
}
