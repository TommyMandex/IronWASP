using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace IronWASP.Hawas
{
    public partial class DomXssAnalyzer : Form
    {
        Thread AnalysisThread = null;
        static DomXssAnalyzer OpenDomXssAnalyzer = null;
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
    <title>DOM XSS Analysis Results</title>
    <link rel='stylesheet' type='text/css' href='style.css'>
</head>
<body>
    <div id='menu_title'><h2>DOM XSS Analysis Results</h2>
    The DOM XSS sources and sinks found in the logs are organised page-wise below. Click on a specific url to view full details.</div>
    
    <div id='menu'>
";
        static string IndexPageBottom = @"</div></body></html>";

        static string PageTop = @"<html><head><title></title><link rel='stylesheet' type='text/css' href='style.css'></head><body>";
        static string PageBottom = @"</body></html>";

        static string Css = @"

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
.sink_title
{
    margin: 10px;
    color: #070;
    font-weight: bold;
}
.sink_list
{
    margin-left: 20px;
    background-color: #8F5;
}
.source_title
{
    margin: 10px;
    color: #B7D;
    font-weight: bold;
}
.source_list
{
    margin-left: 20px;
    background-color: #F8D;
}

#menu
{
    
}
#attr_js
{
    border: solid;
    border-color: #600;
    padding: 15px;
    margin: 10px;
    word-wrap: break-word;
}

#tag_js
{
    border: solid;
    border-color: #606;
    padding: 15px;
    margin: 10px;
    word-wrap: break-word;
}
#ext_js
{
    border: solid;
    border-color: #066;
    padding: 15px;
    margin: 10px;
    word-wrap: break-word;
}
#attr_js_start
{
    padding-top: 2px;
    padding-bottom: 2px;
    margin-bottom: 5px;
    background-color: #AAA;
}
#tag_js_start
{
    padding-top: 2px;
    padding-bottom: 2px;
    margin-bottom: 5px;
    background-color: #AAA;    
}
#ext_js_url
{
    padding-top: 2px;
    padding-bottom: 2px;
    margin-bottom: 5px;
    background-color: #AAA;
}
.source_match
{
    background-color: #8F5;
    font-weight: bold;
}
.sink_match
{
    background-color: #F8D;
    font-weight: bold;
}

";

        public DomXssAnalyzer()
        {
            InitializeComponent();
            SourceRegex = new Regex(@"(location\s*(&nbsp;)*[\[.])|([.\[]\s*(&nbsp;)*[""']?\s*(&nbsp;)*(arguments|dialogArguments|innerHTML|write(ln)?|open(Dialog)?|showModalDialog|cookie|URL|documentURI|baseURI|referrer|name|opener|parent|top|content|self|frames)\W)|(localStorage|sessionStorage|Database)", RegexOptions.Compiled | RegexOptions.Multiline);
            SinkRegex = new Regex(@"((src|href|data|location|code|value|action)\s*(&nbsp;)*[""'\]]*\s*(&nbsp;)*\+?\s*(&nbsp;)*=)|((replace|assign|navigate|getResponseHeader|open(Dialog)?|showModalDialog|eval|evaluate|execCommand|execScript|setTimeout|setInterval)\s*(&nbsp;)*[""'\]]*\s*(&nbsp;)*\()", RegexOptions.Compiled | RegexOptions.Multiline);
            JquerySinkRegex = new Regex(@"after\(|\.append\(|\.before\(|\.html\(|\.prepend\(|\.replaceWith\(|\.wrap\(|\.wrapAll\(|\$\(|\.globalEval\(|\.add\(|jQUery\(|\$\(|\.parseHTML\(", RegexOptions.Compiled | RegexOptions.Multiline);
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenDomXssAnalyzer = new DomXssAnalyzer();
                OpenDomXssAnalyzer.Show();
            }
            OpenDomXssAnalyzer.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenDomXssAnalyzer == null)
            {
                return false;
            }
            else if (OpenDomXssAnalyzer.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SelectOutputFolderLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            while (true)
            {
                DialogResult Result = OutputFolderDialog.ShowDialog();
                if (Result == DialogResult.OK)
                {
                    OutputDir = Directory.CreateDirectory(string.Format("{0}\\results_{1}", OutputFolderDialog.SelectedPath, DateTime.Now.ToString("yyyy_MMM_d__HH_mm_ss_tt") + "_ticks_" + DateTime.Now.Ticks.ToString()));
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
            int i = 1;
            try
            {
                File.WriteAllText(string.Format("{0}\\style.css", OutputDir.FullName), Css);
                while (i < Config.LastProxyLogId)
                {
                    if (!AnalyzedLogs.Contains(i))
                    {
                        AnalyzeLogId(i);
                    }
                    i++;
                }
                CreateIndex();
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                IronException.Report("Error in DOM XSS Analyzer", Exp);
            }
            ShowAnalysisEndInUi();
        }

        void CreateIndex()
        {
            StringBuilder IB = new StringBuilder(IndexPageTop);
            foreach (string BaseUrl in FullResults.Keys)
            {
                if (FullResults[BaseUrl].Count > 0)
                {
                    IB.AppendLine(string.Format("<div class='host'>{0}</div>", BaseUrl));
                    IB.AppendLine("<ol>");
                    foreach (PageTaintResult PTR in FullResults[BaseUrl])
                    {
                        IB.AppendLine("<li>");
                        IB.AppendLine(string.Format("<a href='{0}.html'>{1}</a>", PTR.LogId, PTR.Req.Url));
                        IB.AppendLine("<table cellpadding='1' cellspacing='1'>");
                        IB.AppendLine(string.Format("<tr><td><span class='sink_title'>Sinks:</span></td><td>{0}</td><td><span class='sink_list'>{1}</span></td></tr>", PTR.SinkCount, string.Join(", ", PTR.Sinks.ToArray()).Trim().Trim(',')));
                        IB.AppendLine(string.Format("<tr><td><span class='source_title'>Sources:</span></td><td>{0}</td><td><span class='source_list'>{1}</span></td></tr>", PTR.SourceCount, string.Join(", ", PTR.Sources.ToArray()).Trim().Trim(',')));
                        IB.AppendLine("</table>");
                        IB.AppendLine("</li>");
                    }
                    IB.AppendLine("</ol>");
                }
            }
            IB.AppendLine(IndexPageBottom);
            File.WriteAllText(string.Format("{0}\\index.html", OutputDir.FullName), IB.ToString());
            ShowStatusMsg("Open the below file in browser to view the analysis results");
            ShowOutputFile(string.Format("{0}\\index.html", OutputDir.FullName));
        }

        void AnalyzeLogId(int LogId)
        {
            Session Sess = Session.FromProxyLog(LogId);
            List<string> Scripts = new List<string>();
            if (Sess.Response != null)
            {
                if (Sess.Response.IsHtml)
                {
                    if (!FullResults.ContainsKey(Sess.Request.BaseUrl))
                    {
                        FullResults[Sess.Request.BaseUrl] = new List<PageTaintResult>();
                    }

                    ShowStatusMsg(string.Format("Analyzing log id {0}", LogId));

                    StringBuilder PB = new StringBuilder();
                    //PB.AppendLine("<html><head><title></title><link rel='stylesheet' type='text/css' href='style.css'></head><body>");
                    int SourceCount = 0;
                    int SinkCount = 0;

                    PageTaintResult PTR = new PageTaintResult();
                    PTR.Req = new Request(Sess.Request.FullUrl);

                    Scripts = Sess.Response.Html.GetJavaScriptFromAttributes();
                    if (Scripts.Count > 0)
                    {
                        //PB.AppendLine("//Script from attributes");
                        PB.AppendLine("<div id='attr_js'>");
                        for (int i=0; i < Scripts.Count; i++)
                        {
                            TaintResult TR = FindTaints(Scripts[i]);
                            SourceCount += TR.SourceCount;
                            SinkCount += TR.SinkCount;
                            PB.AppendLine(string.Format("<div id='attr_js_start'>//Contents of JS attribute no: {0}</div>", i + 1));
                            PB.AppendLine(TR.HighlightedCode);
                            PB.AppendLine("<br><br>");
                            PTR.AddTaintResult(TR, LogId);
                        }
                        PB.AppendLine("</div>");
                    }

                    Scripts = Sess.Response.Html.GetJavaScriptFromScriptTags();
                    if (Scripts.Count > 0)
                    {
                        //PB.AppendLine("//Script from script tags");
                        PB.AppendLine("<div id='tag_js'>");
                        for (int i = 0; i < Scripts.Count; i++)
                        {
                            TaintResult TR = FindTaints(Scripts[i]);
                            SourceCount += TR.SourceCount;
                            SinkCount += TR.SinkCount;
                            PB.AppendLine(string.Format("<div id='tag_js_start'>//Contents of Script tag no: {0}</div>", i + 1));
                            PB.AppendLine(TR.HighlightedCode);
                            PB.AppendLine("<br><br>");
                            PTR.AddTaintResult(TR, LogId);
                        }
                        PB.AppendLine("</div>");
                    }

                    List<string> Urls = Sess.Response.Html.GetDecodedValues("script", "src");
                    if (Urls.Count > 0)
                    {
                        //PB.AppendLine("//Script from external files");
                        PB.AppendLine("<div id='ext_js'>");
                        foreach (string Url in Urls)
                        {
                            string FinalUrl = Sess.Request.RelativeUrlToAbsoluteUrl(Url);
                            Request FinalUrlReq = new Request(FinalUrl);
                            if (!FinalUrl.Equals(Sess.Request.FullUrl))
                            {
                                foreach (LogRow LR in IronDB.GetRecordsFromProxyLog(LogId, 1000))
                                {
                                    if (LR.Host.Equals(FinalUrlReq.Host) && (LR.Url.Equals(FinalUrlReq.Url)) && (LR.SSL == FinalUrlReq.SSL))
                                    {
                                        int LogIdToFetch = 0;
                                        if (LR.Code == 304 && JsSourceCodeLogs.ContainsKey(FinalUrlReq.FullUrl))
                                        {
                                            LogIdToFetch = JsSourceCodeLogs[FinalUrlReq.FullUrl];
                                        }
                                        else if (LR.Code == 200)
                                        {
                                            LogIdToFetch = LR.ID;
                                        }
                                        if (LogIdToFetch > 0)
                                        {
                                            Session JsSess = Session.FromProxyLog(LogIdToFetch);
                                            if (JsSess.Response != null)
                                            {
                                                if (JsSess.Response.IsJavaScript)
                                                {
                                                    TaintResult TR = FindTaints(JsSess.Response.BodyString);
                                                    SourceCount += TR.SourceCount;
                                                    SinkCount += TR.SinkCount;
                                                    PB.AppendLine(string.Format("<div id='ext_js_url'>//Contents of - {0}</div>", FinalUrlReq.FullUrl));
                                                    PB.AppendLine(TR.HighlightedCode);
                                                    PB.AppendLine("<br><br>");
                                                    PTR.AddTaintResult(TR, LogId);
                                                    if (!JsSourceCodeLogs.ContainsKey(FinalUrlReq.FullUrl))
                                                    {
                                                        JsSourceCodeLogs[FinalUrlReq.FullUrl] = LogIdToFetch;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        PB.AppendLine("</div>");
                    }
                    if ((PTR.SourceCount + PTR.SinkCount) > 0)
                    {
                        FullResults[Sess.Request.BaseUrl].Add(PTR);
                    }

                    File.WriteAllText(string.Format("{0}\\{1}.html", OutputDir.FullName, LogId), string.Format("{0}{1}{2}", PageTop, PB.ToString().Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("<div&nbsp;id=", "<div id=").Replace("<div&nbsp;class=", "<div class=").Replace("<span&nbsp;id=", "<span id=").Replace("<span&nbsp;class=", "<span class="), PageBottom));
                }
            }
        }

        TaintResult FindTaints(string Code)
        {
            TaintResult TR = new TaintResult();

            Code = Tools.HtmlEncode(IronJint.Beautify(Code));
            
            foreach (Match M in SinkRegex.Matches(Code))
            {
                if (M.Success)
                {
                    if (!TR.Sinks.Contains(M.Value))
                    {
                        Code = Code.Replace(M.Value, string.Format("<span class='sink_match'>{0}</span>", M.Value));
                        TR.Sinks.Add(M.Value);
                    }
                    TR.SinkCount++;
                }
            }
            foreach (Match M in JquerySinkRegex.Matches(Code))
            {
                if (M.Success)
                {
                    if (!TR.Sinks.Contains(M.Value))
                    {
                        Code = Code.Replace(M.Value, string.Format("<span class='sink_match'>{0}</span>", M.Value));
                        TR.Sinks.Add(M.Value);
                    }
                    TR.SinkCount++;
                }
            }

            foreach (Match M in SourceRegex.Matches(Code))
            {
                if (M.Success)
                {
                    if (!TR.Sources.Contains(M.Value))
                    {
                        Code = Code.Replace(M.Value, string.Format("<span class='source_match'>{0}</span>", M.Value));
                        TR.Sources.Add(M.Value);
                    }
                    TR.SourceCount++;
                }
            }

            TR.HighlightedCode = Code.Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
            return TR;
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

        private void DomXssAnalyzer_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAnalysis();
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
            OutputDir = Directory.CreateDirectory(string.Format("{0}\\domxss_results\\{1}", IronDB.LogPath, DateTime.Now.Ticks.ToString()));
        }

        void StopAnalysis()
        {
            try
            {
                AnalysisThread.Abort();
            }
            catch { }
        }
    }

    internal class PageTaintResult
    {
        internal Request Req = null;
        internal List<string> Sources = new List<string>();
        internal List<string> Sinks = new List<string>();
        internal int SourceCount = 0;
        internal int SinkCount = 0;
        internal int LogId = 0;

        internal void AddTaintResult(TaintResult TR, int _LogId)
        {
            this.AddSources(TR.Sources);
            this.AddSinks(TR.Sinks);
            this.SourceCount += TR.SourceCount;
            this.SinkCount += TR.SinkCount;
            this.LogId = _LogId;
        }

        void AddSources(List<string> _Sources)
        {
            foreach (string Source in _Sources)
            {
                if (!this.Sources.Contains(Source)) Sources.Add(Source);
            }
        }

        void AddSinks(List<string> _Sinks)
        {
            foreach (string Sink in _Sinks)
            {
                if (!this.Sinks.Contains(Sink)) Sinks.Add(Sink);
            }
        }
    }

    internal class TaintResult
    {
        
        internal string HighlightedCode = "";
        internal List<string> Sources = new List<string>();
        internal List<string> Sinks = new List<string>();
        internal int SourceCount = 0;
        internal int SinkCount = 0;
    }
}
