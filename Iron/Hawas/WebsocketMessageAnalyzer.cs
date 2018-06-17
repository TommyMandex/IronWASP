using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace IronWASP.Hawas
{
    public partial class WebsocketMessageAnalyzer : Form
    {
        
        Thread AnalysisThread = null;
        static WebsocketMessageAnalyzer OpenWebsocketMessageAnalyzer = null;
        DirectoryInfo OutputDir = null;

        Dictionary<string, List<PageTaintResult>> FullResults = new Dictionary<string, List<PageTaintResult>>();

        Dictionary<string, int> JsSourceCodeLogs = new Dictionary<string, int>();
        List<int> AnalyzedLogs = new List<int>();

        Regex SourceRegex = null;
        Regex SinkRegex = null;
        Regex JquerySinkRegex = null;

        static string IndexPageTop = @"
<html>
<head>
    <title>WebSocket Messages Analysis Results</title>
    <link rel='stylesheet' type='text/css' href='style.css'>
</head>
<body>
    <div id='menu_title'><h2>WebSocket Messages Analysis Results</h2>
    The WebSocket Messages found in the logs have been grouped as per each WebSocket connection and are organised page-wise below. Click on a specific url to view full details.</div>
    
    <div id='menu'>
    <ol>
";

        static string IndexPageBottom = @"
        </ol>
    </div>
</body>
</html>
";

        static string SessionPageTop = @"
<html>
<head>
    <title>Messages in this WebSocket Session</title>
    <link rel='stylesheet' type='text/css' href='style.css'>
    <script>
        function show(match) {
	        show_hide(match, false);
        }

        function hide(match) {
	        show_hide(match, true);
        }

        function show_hide(match, should_hide) {
            var show_hide_group = function (match, should_hide, gp)
	        {
		        for (var i = 0; i < gp.length; i++)
		        {
			        if (gp[i].style.visibility === 'hidden') continue;
			
			        if(gp[i].innerText.indexOf(match) > -1)
			        {
				        if (should_hide)
				        {
					        gp[i].style.visibility = 'hidden';
				        }
			        }
			        else
			        {
				        if (!should_hide)
				        {
					        gp[i].style.visibility = 'hidden';
				        }
			        }
		        }
	        }
	        var mts = document.getElementsByClassName('msg_to_server');
	        var mtc = document.getElementsByClassName('msg_to_client');
	        show_hide_group(match, should_hide, mts);
	        show_hide_group(match, should_hide, mtc);
        }
        
        function show_all(args) {
	        var mts = document.getElementsByClassName('msg_to_server');
	        var mtc = document.getElementsByClassName('msg_to_client');
	
	        for (var i = 0; i < mts.length; i++)
	        {
		        mts[i].style.visibility = '';
	        }
	        for (var i = 0; i < mtc.length; i++)
	        {
		        mtc[i].style.visibility = ''
	        }
        }

    </script>
</head>
<body>
    <div id='messages'>
";
        static string SessionPageBottom = @"
</div>
</html>
";

        static string Css = @"
.stat_name_mts
{
	padding-left: 25px;
	color: #99F;
}
.stat_name_mtc
{
	padding-left: 25px;
	color: #F99;
}
.stat_value
{
	padding-left: 5px;
	color: #555;
	font-weight: bold;
}
.msg_to_server
{
	border-style: groove;
	margin: 10px;
	margin-left: 10px;
	padding: 10px;
	background-color: #99F;
	word-wrap: normal;
	
}
.msg_to_client
{
	border-style: groove;
	margin: 10px;
	margin-left: 30px;
	padding: 10px;
	background-color: #F99;
	word-wrap: normal;
}


#menu_title
{
    text-align: center;
    padding-bottom: 10px;
    color: #222;
}
.host
{
    width: 99%;
    padding-left: 10px;
    background-color: #DDD;
    color: #000;
    text-align: left;
    overflow: auto;
}
#menu a
{
    color: #33C;
    text-decoration: none;
}
#menu a:hover
{
    color: #33C;
    text-decoration: underline;
    font-weight: bold;
}
li
{
    padding-bottom: 5px;
}
";

        public WebsocketMessageAnalyzer()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenWebsocketMessageAnalyzer = new WebsocketMessageAnalyzer();
                OpenWebsocketMessageAnalyzer.Show();
            }
            OpenWebsocketMessageAnalyzer.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenWebsocketMessageAnalyzer == null)
            {
                return false;
            }
            else if (OpenWebsocketMessageAnalyzer.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        delegate void ShowOutputFile_d(string FileName);
        void ShowOutputFile(string FileName)
        {
            try
            {
                if (AnalysisProgressBar.InvokeRequired)
                {
                    ShowOutputFile_d CALL_d = new ShowOutputFile_d(ShowOutputFile);
                    AnalysisProgressBar.Invoke(CALL_d, new object[] { FileName });
                }
                else
                {
                    OutputTB.Text = FileName;
                    OutputTB.Visible = true;
                }
            }
            catch { }
        }

        delegate void ShowStatusMsg_d(string Msg);
        void ShowStatusMsg(string Msg)
        {
            try
            {
                if (AnalysisProgressBar.InvokeRequired)
                {
                    ShowStatusMsg_d CALL_d = new ShowStatusMsg_d(ShowStatusMsg);
                    AnalysisProgressBar.Invoke(CALL_d, new object[] { Msg });
                }
                else
                {
                    StatusLbl.Text = Msg;
                }
            }
            catch { }
        }

        delegate void ShowAnalysisEndInUi_d();
        void ShowAnalysisEndInUi()
        {
            try
            {
                if (AnalysisProgressBar.InvokeRequired)
                {
                    ShowAnalysisEndInUi_d CALL_d = new ShowAnalysisEndInUi_d(ShowAnalysisEndInUi);
                    AnalysisProgressBar.Invoke(CALL_d, new object[] { });
                }
                else
                {
                    ShowHideProgressBar(false);
                    StartBtn.Text = "Start Analysis";
                }
            }
            catch { }
        }

        delegate void ShowHideProgressBar_d(bool Show);
        void ShowHideProgressBar(bool Show)
        {
            try
            {
                if (AnalysisProgressBar.InvokeRequired)
                {
                    ShowHideProgressBar_d CALL_d = new ShowHideProgressBar_d(ShowHideProgressBar);
                    AnalysisProgressBar.Invoke(CALL_d, new object[] { Show });
                }
                else
                {
                    AnalysisProgressBar.Visible = Show;
                }
            }
            catch { }
        }

        delegate void ShowHideDialogLink_d(bool Show);
        void ShowHideDialogLink(bool Show)
        {
            try
            {
                if (AnalysisProgressBar.InvokeRequired)
                {
                    ShowHideDialogLink_d CALL_d = new ShowHideDialogLink_d(ShowHideDialogLink);
                    AnalysisProgressBar.Invoke(CALL_d, new object[] { Show });
                }
                else
                {
                    SelectOutputFolderLbl.Visible = Show;
                }
            }
            catch { }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (StartBtn.Text.Equals("Start Analysis"))
            {
                try { AnalysisThread.Abort(); }
                catch { }
                OutputTB.Text = "";
                ShowStatusMsg("Creating output directory");
                try
                {
                    CreateResultsDir();
                }
                catch
                {
                    ShowStatusMsg("Unable to create output directory, cannot proceed.");
                    return;
                }
                ShowStatusMsg("Analysis started...");
                AnalysisThread = new Thread(DoAnalysis);
                AnalysisThread.Start();
                StartBtn.Text = "Stop Analysis";
                AnalysisProgressBar.Visible = true;
            }
            else
            {
                StopAnalysis();
                ShowStatusMsg("Analysis stopped");
                AnalysisProgressBar.Visible = false;
                StartBtn.Text = "Start Analysis";
            }

        }

        void CreateResultsDir()
        {
            OutputDir = Directory.CreateDirectory(string.Format("{0}\\websocket_results\\{1}", IronDB.LogPath, DateTime.Now.Ticks.ToString()));
        }

        void StopAnalysis()
        {
            try
            {
                AnalysisThread.Abort();
            }
            catch { }
        }

        private void WebsocketMessageAnalyzer_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAnalysis();
        }

        private void SelectOutputFolderLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            while (true)
            {
                DialogResult Result = OutputFolderDialog.ShowDialog();
                if (Result == DialogResult.OK)
                {
                    OutputDir = Directory.CreateDirectory(string.Format("{0}\\messages_{1}", OutputFolderDialog.SelectedPath, DateTime.Now.ToString("yyyy_MMM_d__HH_mm_ss_tt") + "_ticks_" + DateTime.Now.Ticks.ToString()));
                    ShowHideDialogLink(false);
                    try { AnalysisThread.Abort(); }
                    catch { }
                    AnalysisThread = new Thread(DoAnalysis);
                    AnalysisThread.Start();
                    AnalysisProgressBar.Visible = true;
                    return;
                }
                else if (Result == DialogResult.Cancel)
                {
                    return;
                }
            }
        }

        void DoAnalysis()
        {
            WebSocketSessions Sessions = new WebSocketSessions();
            try
            {
                File.WriteAllText(string.Format("{0}\\style.css", OutputDir.FullName), Css);

                int LogId = 1;
                while (LogId <= Config.GetLastLogId("WebSocket"))
                {
                    try
                    {
                        Session Sess = Session.FromLog(LogId, "WebSocket");
                        if (Sess.Response == null)
                        {
                            Sessions.AddMessage(Sess.Request);
                        }
                        else
                        {
                            Sessions.AddMessage(Sess.Request, Sess.Response);
                        }
                    }
                    catch(Exception Exp) 
                    {
                        IronException.Report("Could not load WebSocket Message in to Message Extractor", Exp);
                        //This could happen when the most recent messages have not yet been written to the DB by the LogCount has been incremented
                    }
                    LogId++;
                }

                StringBuilder IndexPage = new StringBuilder(IndexPageTop);

                int SessionCount = 0;
                foreach (string SessionId in Sessions.SessionIdsList)
                {
                    WebSocketSession WS = Sessions.GetSession(SessionId);
                    SessionCount++;

                    IndexPage.AppendLine("<li>");
                    IndexPage.AppendLine(string.Format("<a href='{0}.html'>{1}</a>", SessionCount, WS.Url));
                    IndexPage.AppendLine("<table cellpadding='1' cellspacing='1'>");
                    IndexPage.AppendLine(string.Format("<tr><td><span class='stat_name_mts'>Messages to Server: </span><span class='stat_value'>{0}</span></td></tr>", WS.Requests.Count));
                    IndexPage.AppendLine(string.Format("<tr><td><span class='stat_name_mtc'>Messages to Client: </span><span class='stat_value'>{0}</span></td></tr>", WS.Responses.Count));
                    IndexPage.AppendLine("</table>");
                    IndexPage.AppendLine("</li>");
                    
                    AnalyzeSession(WS, SessionCount);
                }

                IndexPage.AppendLine(IndexPageBottom);
                File.WriteAllText(string.Format("{0}\\index.html", OutputDir.FullName), IndexPage.ToString());
                ShowStatusMsg("Open the below file in browser to view the analysis results");
                ShowOutputFile(string.Format("{0}\\index.html", OutputDir.FullName));
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                IronException.Report("Error in WebSocket Message Analyzer", Exp);
                ShowStatusMsg("Error!! Check the exceptions area for error details.");
            }
            ShowAnalysisEndInUi();
        }

        void AnalyzeSession(WebSocketSession WS, int SessionCount)
        {
            
            StringBuilder SessionPage = new StringBuilder(SessionPageTop);
            for (int i = 0; i <= WS.GetLastMessageId(); i++)
            {
                if (WS.Requests.ContainsKey(i))
                {
                    SessionPage.AppendLine(string.Format("<div class='msg_to_server'>{0}</div>", Tools.HtmlEncode(WS.Requests[i].Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>"))));
                }
                else if (WS.Responses.ContainsKey(i))
                {
                    SessionPage.AppendLine(string.Format("<div class='msg_to_client'>{0}</div>", Tools.HtmlEncode(WS.Responses[i]).Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>")));
                }
            }
            SessionPage.AppendLine(SessionPageBottom);

            File.WriteAllText(string.Format("{0}\\{1}.html", OutputDir.FullName, SessionCount), SessionPage.ToString());
        }

        private void WebsocketMessageAnalyzer_Load(object sender, EventArgs e)
        {
            
        }
    }

    internal class WebSocketSessions
    {
        internal List<string> SessionIdsList = new List<string>();//This is to keep track of the order in which the IDs were read, since the SessionList dict does not preserve this
        Dictionary<string, WebSocketSession> SessionsList = new Dictionary<string, WebSocketSession>();

        internal void AddMessage(Request Req)
        {
            WebSocketSession WS = GetSession(Req);
            WS.AddRequest(Req);
        }

        internal void AddMessage(Request Req, Response Res)
        {
            WebSocketSession WS = GetSession(Req);
            WS.AddResponse(Req, Res);
        }

        internal WebSocketSession GetSession(string SessionId)
        {
            return SessionsList[SessionId];
        }

        WebSocketSession GetSession(Request Req)
        {
            string SessionId = WebSocketSession.GetSessionId(Req);
            if (!SessionsList.ContainsKey(SessionId))
            {
                SessionsList[WebSocketSession.GetSessionId(Req)] = new WebSocketSession(Req);
                SessionIdsList.Add(SessionId);
            }
            return SessionsList[SessionId];
        }
    }

    internal class WebSocketSession
    {
        internal string Url = "";
        internal string WebSocketKey = "";
        internal string SocketId = "";

        internal string SessionId = "";

        internal Dictionary<int, string> Requests = new Dictionary<int, string>();
        internal Dictionary<int, string> Responses = new Dictionary<int, string>();

        internal WebSocketSession(Request Req)
        {
            this.Url = string.Format("//{0}{1}", Req.Host, Req.Url);
            this.WebSocketKey = Req.Headers.Get("Sec-WebSocket-Key");
            this.SocketId = Req.Headers.Get("IW-WS-SocketID");
            this.SessionId = GetSessionId(Req);
        }

        internal void AddRequest(Request Req)
        {
            Requests[GetMessageId(Req)] = Req.BodyString;
        }

        internal void AddResponse(Request Req, Response Res)
        {
            Responses[GetMessageId(Req)] = Res.BodyString;
        }

        internal static string GetSessionId(Request Req)
        {
            return string.Format("{0}-{1}", Req.Headers.Get("IW-WS-SocketID"), Req.Headers.Get("Sec-WebSocket-Key"));
        }

        int GetMessageId(Request Req)
        {
            return Int32.Parse(Req.Headers.Get("IW-WS-SocketMsgID").Trim());
        }

        internal int GetLastMessageId()
        {
            List<int> Ids = new List<int>(Requests.Keys);
            Ids.AddRange(Responses.Keys);
            Ids.Sort();
            if (Ids.Count > 0)
            {
                return Ids[Ids.Count - 1];
            }
            else
            {
                return 0;
            }
        }
    }
}
