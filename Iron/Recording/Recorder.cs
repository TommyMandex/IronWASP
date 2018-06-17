using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.IO;

namespace IronWASP.Recording
{
    public partial class Recorder : Form
    {
        static Recorder OpenRecorder = null;

        //public int StartLogId = 0;
        //public int LoginCompleteLogId = 0;
        //public int LoggedOutLogId = 0;

        string Username = "";
        string Password = "";

        string CsrfParameterName = "";

        static List<int> LogsWithLoginCreds = new List<int>();
        static List<int> LogsWithCsrfParams = new List<int>();

        int RecordingStartLogId = 0;
        int RecordingCompleteLogId = 0;

        const int DefaultRecordingCompletionWaitTime = 5;

        int RecordingCompletionWaitTime = DefaultRecordingCompletionWaitTime;

        int CurrentStep = 0;

        Thread AnalysisThread = null;
        Thread RecordingCompletionWaitThread = null;

        Recording CurrentRecording = null;

        bool RecordingInProgress = false;

        public Recorder()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenRecorder = new Recorder();
                OpenRecorder.Show();
            }
            OpenRecorder.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenRecorder == null)
            {
                return false;
            }
            else if (OpenRecorder.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal static void AddParameterValues(Session Sess)
        {
            if (IsRecording)
            {
                Request Req = Sess.Request;
                
                List<string> Values = new List<string>();
                foreach (string Name in Req.Query.GetNames())
                {
                    Values.AddRange(Req.Query.GetAll(Name));
                }
                if(Req.BodyType ==  BodyFormatType.UrlFormEncoded)
                {
                    foreach (string Name in Req.Body.GetNames())
                    {
                        Values.AddRange(Req.Body.GetAll(Name));
                    }
                }
                else
                {
                    FormatPlugin FP =  FormatPlugin.Get(Req.BodyType);
                    if(FP != null)
                    {
                        try
                        {
                            string[,] ParaValues = FormatPlugin.XmlToArray(FP.ToXmlFromRequest(Req));
                            for(int i=0; i < ParaValues.GetLength(0); i++)
                            {
                                Values.Add(ParaValues[i,1]);
                            }
                        }
                        catch{}
                    }
                }
                if (Values.Contains(OpenRecorder.Username) && Values.Contains(OpenRecorder.Password))
                {
                    lock (LogsWithLoginCreds)
                    {
                        LogsWithLoginCreds.Add(Sess.LogId);
                    }
                    CheckIfRecordingGoalsMet();
                }
            }
        }

        static void ClearRecordedData()
        {
            LogsWithLoginCreds.Clear();
            LogsWithCsrfParams.Clear();
        }

        internal static void AddHiddenFieldValues(Session Sess)
        {
            List<string> HiddenFields = new List<string>();
            if (IsRecording)
            {
                if (OpenRecorder.CsrfParameterName.Length > 0)
                {
                    Response Res = Sess.Response;
                    if (Res.IsHtml)
                    {
                        List<HtmlAgilityPack.HtmlNode> Nodes = Res.Html.GetNodes("input", "type", "hidden");
                        foreach (HtmlAgilityPack.HtmlNode Node in Nodes)
                        {
                            IronHtml.InputElement InEl = new IronHtml.InputElement(Node, 0);
                            if (InEl.Name.Equals(OpenRecorder.CsrfParameterName) && InEl.Value.Trim().Length > 0)
                            {
                                lock (LogsWithCsrfParams)
                                {
                                    LogsWithCsrfParams.Add(Sess.LogId);
                                    CheckIfRecordingGoalsMet();
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckIfRecordingGoalsMet()
        {
            try
            {
                if (IsRecording)
                {
                    if (LogsWithLoginCreds.Count > 0)
                    {
                        OpenRecorder.UpdateLoginCredsFindingInRecording();
                    }
                    bool GoalsMet = false;
                    if (OpenRecorder.CsrfParameterName.Length > 0)
                    {
                        foreach (int LoginLogId in LogsWithLoginCreds)
                        {
                            foreach (int CsrfLogId in LogsWithCsrfParams)
                            {
                                if (CsrfLogId >= LoginLogId)
                                {
                                    GoalsMet = true;
                                    break;
                                }
                            }
                            if (GoalsMet)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (LogsWithLoginCreds.Count > 0)
                        {
                            GoalsMet = true;
                        }
                    }
                    if (GoalsMet)
                    {
                        //OpenRecorder.RecordingCompleteLogId = Config.LastProxyLogId;
                        OpenRecorder.UpdateRecordingCompletion();
                    }
                }
            }
            catch { }
        }

        public static bool IsRecording
        {
            get
            {
                try
                {
                    if(OpenRecorder.RecordingInProgress)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch 
                {
                    return false;
                }
            }
        }

        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {   
                StartRecording();
            }
        }

        void StartRecording()
        {
            TopMost = true;
            ControlBox = false;

            ClearRecordedData();
            
            RecordingStartLogId = Config.LastProxyLogId + 1;

            RecordStepStartBtn.Text = "Stop Recording";
            RecordStepLoginInstructionLbl.Visible = true;
            if (CsrfParameterName.Length > 0)
            {
                RecordStepCsrfInstructionLbl.Visible = true;
            }
            RecordStepStatusHeaderLbl.Visible = true;
            RecordStepLoginStatusLbl.Visible = true;
            if (CsrfParameterName.Length > 0)
            {
                RecordStepCsrfStatusLbl.Visible = true;
            }
            RecordingInProgress = true;
        }

        void StopRecording()
        {
            RecordingInProgress = false;
            ResetRecordStep();
            TopMost = false;
            ControlBox = false;
        }

        delegate void ShowAnalysisStart_d();
        void ShowAnalysisStart()
        {
            if (this.InvokeRequired)
            {
                ShowAnalysisStart_d CALL_d = new ShowAnalysisStart_d(ShowAnalysisStart);
                this.Invoke(CALL_d, new object[] { });
            }
            else
            {
                TestStepHeaderLbl.Text = "Recording is being tested to confirm if it can be successfully replayed. This can take a few minutes....";
                TestStepProgressBar.Visible = true;
                TestStepStatusTB.Visible = false;
                TestStepRetryLL.Visible = false;
                TestStepWaitMoreBtn.Visible = false;
                TestStepDontWaitBtn.Visible = false;
            }
        }

        delegate void HandleAnalysisResult_d(bool Success);
        void HandleAnalysisResult(bool Success)
        {
            if (this.InvokeRequired)
            {
                HandleAnalysisResult_d CALL_d = new HandleAnalysisResult_d(HandleAnalysisResult);
                this.Invoke(CALL_d, new object[] { Success });
            }
            else
            {
                TestStepProgressBar.Visible = false;
                ControlBox = true;

                if (Success)
                {
                    GoToSaveStep();
                }
                else
                {
                    TestStepHeaderLbl.Text = "Test complete!";
                    TestStepStatusTB.Text = "The recording could not be successfully replayed. If you are still logged in to the application, logout and click 'Retry' below. If that does not fix it then the application is too complex to be recording with the current version of IronWASP.";
                    TestStepStatusTB.Visible = true;
                    TestStepRetryLL.Visible = true;
                }
            }
        }

        delegate void UpdateLoginCredsFindingInRecording_d();
        void UpdateLoginCredsFindingInRecording()
        {
            if (this.InvokeRequired)
            {
                UpdateLoginCredsFindingInRecording_d CALL_d = new UpdateLoginCredsFindingInRecording_d(UpdateLoginCredsFindingInRecording);
                this.Invoke(CALL_d, new object[] { });
            }
            else
            {
                RecordStepLoginStatusLbl.Visible = true;
                RecordStepLoginStatusLbl.ForeColor = Color.Green;
                RecordStepLoginStatusLbl.Text = "Request containing the Login credentials has been found in the recording!!";
            }
        }

        delegate void UpdateCsrfFieldFindingInRecording_d();
        void UpdateCsrfFieldFindingInRecording()
        {
            if (this.InvokeRequired)
            {
                UpdateCsrfFieldFindingInRecording_d CALL_d = new UpdateCsrfFieldFindingInRecording_d(UpdateCsrfFieldFindingInRecording);
                this.Invoke(CALL_d, new object[] { });
            }
            else
            {
                RecordStepCsrfStatusLbl.Visible = true;
                RecordStepCsrfStatusLbl.ForeColor = Color.Green;
                RecordStepCsrfStatusLbl.Text = "CSRF token parameter has been found in a hidden input field.!!";
            }
        }

        delegate void UpdateRecordingCompletion_d();
        void UpdateRecordingCompletion()
        {
            if (this.InvokeRequired)
            {
                UpdateRecordingCompletion_d CALL_d = new UpdateRecordingCompletion_d(UpdateRecordingCompletion);
                this.Invoke(CALL_d, new object[] { });
            }
            else
            {
                GoToTestStep();
            }
        }

        delegate void UpdateRecordingCompletionWaitTimeStatus_d();
        void UpdateRecordingCompletionWaitTimeStatus()
        {
            if (this.InvokeRequired)
            {
                UpdateRecordingCompletionWaitTimeStatus_d CALL_d = new UpdateRecordingCompletionWaitTimeStatus_d(UpdateRecordingCompletionWaitTimeStatus);
                this.Invoke(CALL_d, new object[] { });
            }
            else
            {
                TestStepHeaderLbl.Text = string.Format("Waiting for {0} seconds to let the current page finish loading in the browser incase it is not fully loaded.", RecordingCompletionWaitTime);
                TestStepWaitMoreBtn.Visible = true;
                TestStepDontWaitBtn.Visible = true;
                TestStepProgressBar.Visible = false;
            }
        }

        void DoAnalysisOfRecording()
        {
            try
            {
                Analysis.LogAnalyzer LogAna = new Analysis.LogAnalyzer();
                //Dictionary<string, Analysis.LogAssociations> LoginAssosDict = LogAna.Analyze(RecordingStartLogId, LoginRecordingDoneLogId, "Proxy");
                
                //Check if the last log has been written to the db
                //We wait for max of 10 seconds if it is still not written then we proceed further so that an exception is thrown when processing
                int WaitTime = 0;
                while (WaitTime < 10000)
                {
                    try
                    {
                        Session.FromProxyLog(RecordingCompleteLogId);
                        break;
                    }
                    catch { }
                    Thread.Sleep(1000);
                    WaitTime = WaitTime + 1000;
                }

                Dictionary<string, Analysis.LogAssociations> LoginAssosDict = LogAna.Analyze(RecordingStartLogId, RecordingCompleteLogId, "Proxy");
                List<string> Creds = new List<string>() { Username, Password };
                string CorrectUa = "";
                Analysis.LogAssociations LoginAssos = null;
                foreach (string Ua in LoginAssosDict.Keys)
                {
                    if (LoginAssosDict[Ua].GetAssociationsWithParameterValues(Creds).Count > 0)
                    {
                        CorrectUa = Ua;
                        LoginAssos = LoginAssosDict[Ua];
                        break;
                    }
                }
                if (LoginAssos == null)
                {
                    HandleAnalysisResult(false);
                    return;
                }

                /*
                Dictionary<string, Analysis.LogAssociations> CsrfAssosDict = LogAna.Analyze(LoginRecordingDoneLogId, CsrfParameterRecordingDoneLogId, "Proxy");
                Analysis.LogAssociations CsrfAssos = null;
                if (CsrfAssosDict.ContainsKey(CorrectUa))
                {
                    CsrfAssos = CsrfAssosDict[CorrectUa];
                }
                if (CsrfParameterName.Length > 0 && CsrfAssos == null)
                {
                    HandleAnalysisResult(false);
                    return;
                }
                */
                 
                CurrentRecording = new Recording(LoginAssos, Username, Password, CsrfParameterName);
                if (!CurrentRecording.IsLoginRecordingReplayable())
                {
                    HandleAnalysisResult(false);
                    return;
                }
                CurrentRecording.DoLogin();
                if (CsrfParameterName.Length > 0)
                {
                    string CT = CurrentRecording.GetCsrfToken();
                    if (CT.Length == 0)
                    {
                        HandleAnalysisResult(false);
                        return;
                    }
                }
            }
            catch (ThreadAbortException) { }//Ingore them
            catch (Exception Exp)
            {
                IronException.Report("Error analyzing recording", Exp);
                HandleAnalysisResult(false);
                return;
            }
            Workflow.Workflow Flow = CurrentRecording.ToWorkflow();
            HandleAnalysisResult(true);
        }

        void WaitForRecordingCompletion()
        {
            try
            {
                while (RecordingCompletionWaitTime > 0)
                {
                    Thread.Sleep(1000);
                    RecordingCompletionWaitTime--;
                    UpdateRecordingCompletionWaitTimeStatus();
                }
                RecordingCompleteLogId = Config.LastProxyLogId;
                StartAnalysisOfRecording();
            }
            catch (ThreadAbortException) { }
        }

        void StartAnalysisOfRecording()
        {
            ShowAnalysisStart();
            try
            {
                AnalysisThread.Abort();
            }
            catch { }
            AnalysisThread = new Thread(DoAnalysisOfRecording);
            AnalysisThread.Start();
        }

        bool SaveRecording()
        {
            string XmlString = CurrentRecording.ToXml();

            SaveRecordingDialog.FileName = CurrentRecording.Name;

            while (SaveRecordingDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo Info = new FileInfo(SaveRecordingDialog.FileName);
                if (Info.Name.Length == 0)
                {
                    MessageBox.Show("Please enter a name");
                }
                else if (!Info.Name.EndsWith(".sessrec"))
                {
                    MessageBox.Show("The file extension must be .sessrec");
                }
                else
                {
                    try
                    {
                        StreamWriter Writer = new StreamWriter(Info.FullName);
                        Writer.Write(XmlString);
                        Writer.Close();
                        return true;
                    }
                    catch (Exception Exp)
                    {
                        MessageBox.Show(string.Format("Unable to save file: {0}", new object[] { Exp.Message }));
                    }
                    break;
                }
            }
            return false;
        }

        void Reset()
        {
            try
            {
                AnalysisThread.Abort();
            }
            catch { }
            ResetConfigureStep();
            ResetRecordStep();
            ResetTestStep();
            ResetSaveStep();
            BaseTabs.SelectTab(0);
        }

        void ResetConfigureStep()
        {
            ConfigureStepUsernameTB.Text = "";
            ConfigureStepPasswordTB.Text = "";
            ConfigureStepCsrfTokenTB.Text = "";
            ConfigureStepErrorLbl.Text = "";
        }

        void ResetRecordStep()
        {
            ClearRecordedData();
            RecordStepStartBtn.Text = "Start Recording";
            RecordStepLoginInstructionLbl.Visible = false;
            RecordStepCsrfInstructionLbl.Visible = false;
            RecordStepStatusHeaderLbl.Visible = false;
            RecordStepLoginStatusLbl.Visible = false;
            RecordStepCsrfStatusLbl.Visible = false;
        }

        void ResetTestStep()
        {
            TestStepHeaderLbl.Text = "Waiting for 5 seconds to let the current page finish loading in the browser incase it is not fully loaded.";
            TestStepProgressBar.Visible = false;
            TestStepRetryLL.Visible = false;
            TestStepStatusTB.Text = "";
            TestStepStatusTB.Visible = false;
        }

        void ResetSaveStep()
        {
            SaveStepErrorLbl.Text = "";
            SaveStepNameTB.Text = "";
            SaveStepErrorLbl.Text = "";
            SaveStepConfirmationMsbLbl.Text = "";
        }

        void GoToConfigureStep()
        {
            ResetConfigureStep();
            CurrentStep = 0;
            BaseTabs.SelectTab(CurrentStep);
        }
        void GoToRecordStep()
        {
            ResetRecordStep();
            CurrentStep = 1;
            BaseTabs.SelectTab(CurrentStep);
        }
        void GoToTestStep()
        {
            ResetTestStep();
            TopMost = false;
            TestStepProgressBar.Visible = false;
            TestStepWaitMoreBtn.Visible = true;
            TestStepDontWaitBtn.Visible = true;
            
            try
            {
                RecordingCompletionWaitThread.Abort();
            }
            catch { }
            RecordingCompletionWaitTime = DefaultRecordingCompletionWaitTime;
            RecordingCompletionWaitThread = new Thread(WaitForRecordingCompletion);
            RecordingCompletionWaitThread.Start();
            CurrentStep = 2;
            BaseTabs.SelectTab(CurrentStep);
        }
        void GoToSaveStep()
        {
            ResetSaveStep();
            CurrentStep = 3;
            BaseTabs.SelectTab(CurrentStep);
        }

        void ShowConfigureStepError(string ErrorMsg)
        {
            ConfigureStepErrorLbl.Text = ErrorMsg;
            ConfigureStepErrorLbl.Visible = true;
        }

        void ShowSaveStepError(string ErrorMsg)
        {
            SaveStepErrorLbl.Text = ErrorMsg;
            SaveStepErrorLbl.Visible = true;
        }

        bool ProcessConfigureStepInput()
        {
            string Uname = ConfigureStepUsernameTB.Text;
            string Passwd = ConfigureStepPasswordTB.Text;

            if (Uname.Trim().Length == 0 || Passwd.Trim().Length == 0)
            {
                ShowConfigureStepError("Username and Password values cannot be blank");
                return false;
            }
            this.Username = Uname;
            this.Password = Passwd;
            if (ConfigureStepCsrfTokenTB.Text.Trim().Length > 0)
            {
                this.CsrfParameterName = ConfigureStepCsrfTokenTB.Text;
            }
            return true;
        }

        private void IronRecorder_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopEverything();
        }

        private void IronRecorder_Load(object sender, EventArgs e)
        {
            ResetConfigureStep();
        }

        private void RecordStepStartBtn_Click(object sender, EventArgs e)
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void BaseTabs_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex != CurrentStep)
            {
                BaseTabs.SelectTab(CurrentStep);
            }
        }

        private void SaveStepSaveLL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (SaveStepNameTB.Text.Trim().Length == 0)
            {
                ShowSaveStepError("Recording name cannot be empty");
                return;
            }
            if (!Regex.IsMatch(SaveStepNameTB.Text.Trim(), "^[a-zA-Z0-9_]+$"))
            {
                ShowSaveStepError("Recording name can only contain alphabets and numbers and _ symbol");
                return;
            }
            if (Recording.Has(SaveStepNameTB.Text.Trim()))
            {
                ShowSaveStepError("A recording with this name has already been loaded, try another name.");
                return;
            }

            CurrentRecording.SetName(SaveStepNameTB.Text.Trim());
            CurrentRecording.WorkflowId = IronDB.LogWorkflow(CurrentRecording.ToWorkflow());
            Recording.Add(CurrentRecording);
            SaveStepSaveLL.Enabled = false;
            SaveStepNameTB.Enabled = false;
            SaveStepErrorLbl.Visible = false;
            SaveStepConfirmationMsbLbl.Text = "Recording saved! You can now use it in scans/test.";

            /*
            if (SaveRecording())
            {
                Recording.Add(CurrentRecording);
                SaveStepSaveLL.Enabled = false;
                SaveStepNameTB.Enabled = false;
                SaveStepErrorLbl.Visible = false;
                SaveStepConfirmationMsbLbl.Text = "Recording saved! You can now use it in scans/test and also load it from the saved file in future.";
            }
            else
            {
                ShowSaveStepError("Error saving recording to a file.");
            }
            */
        }

        private void ConfigureStepSubmitBtn_Click(object sender, EventArgs e)
        {
            if (ProcessConfigureStepInput())
            {
                GoToRecordStep();
            }
        }

        private void TestStepRetryLL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartAnalysisOfRecording();
        }

        private void TestStepDontWaitBtn_Click(object sender, EventArgs e)
        {
            RecordingCompletionWaitTime = 0;
        }

        private void TestStepWaitMoreBtn_Click(object sender, EventArgs e)
        {
            RecordingCompletionWaitTime += 5;
        }

        private void RecordStepCancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void StopEverything()
        {
            try
            {
                AnalysisThread.Abort();
            }
            catch { }
            try
            {
                RecordingCompletionWaitThread.Abort();
            }
            catch { }
        }

        private void TestStepCancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
