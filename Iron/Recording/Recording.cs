using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;

namespace IronWASP.Recording
{
    /*
    public class Recording
    {
        string RecordingName = "";

        List<SessionRecordingStep> Steps = new List<SessionRecordingStep>();

        SessionRecordingType RecType = SessionRecordingType.Login;

        Analysis.LogAssociations Associations = null;
        string Username = "";
        string Password = "";
        string CsrfParameterName = "";

        /*
        public Recording(SessionRecordingAnalysisResult Result, string RecName)
        {
            this.Steps = Result.Steps;
            this.RecordingName = RecName;
        }

        public Recording(Analysis.LogAssociations LogAssos, string Uname, string Pwd)
        {
            this.Associations = LogAssos;
            this.Username = Uname;
            this.Password = Pwd;
            this.RecType = SessionRecordingType.Login;
        }

        public Recording(Analysis.LogAssociations LogAssos, string CsrfParamName)
        {
            this.Associations = LogAssos;
            this.CsrfParameterName = CsrfParamName;
            this.RecType = SessionRecordingType.Csrf;
        }
        */
    /*
        public string Name
        {
            get
            {
                return this.RecordingName;
            }
        }

        public SessionRecordingType RecordingType
        {
            get
            {
                return RecType; ;
            }
        }

        internal void SetName(string RecName)
        {
            this.RecordingName = RecName;
        }
        /*
        public int StepCount
        {
            get
            {
                return Steps.Count;
            }
        }

        internal void AddStep(SessionRecordingStep Step)
        {
            lock (Steps)
            {
                this.Steps.Add(Step);
            }
        }

        internal void InsertStep(int StepId, SessionRecordingStep Step)
        {
            if (StepId > Steps.Count + 1 || StepId < 1)
            {
                throw new Exception(string.Format("Step id {0} is outside the range of available steps. Try a number between the range {1}-{2}", StepId, 1, Steps.Count + 1));
            }
            lock (Steps)
            {
                this.Steps.Insert(StepId - 1, Step);
            }
        }

        public SessionRecordingStep GetStep(int StepId)
        {
            if (StepId > Steps.Count || StepId < 1)
            {
                throw new Exception(string.Format("No step with that id {0} is available. Try a number between the range {1}-{2}", StepId, 1, Steps.Count));
            }
            return this.Steps[StepId - 1];
        }

        public void DeleteStep(int StepId)
        {
            if (StepId > Steps.Count || StepId < 1)
            {
                throw new Exception(string.Format("No step with that id {0} is available. Try a number between the range {1}-{2}", StepId, 1, Steps.Count));
            }
            lock (Steps)
            {
                Steps.RemoveAt(StepId - 1);
            }
        }
        
    }
    */
    //public class LoginRecording : Recording
    public class Recording
    {
        static Dictionary<string, Recording> LoadedRecordings = new Dictionary<string, Recording>();
        
        string RecordingName = "";

        Analysis.LogAssociations LoginAssociations = null;
        //Analysis.LogAssociations CsrfAssociations = null;

        Analysis.LogReplayAssociations LoginReplayAssociations = null;
        
        string Username = "";
        string Password = "";

        string intCsrfParameterName = "";

        Request LoginCheckRequest = null;
        Response LoginCheckResponseWhenLoggedIn = null;
        Response LoginCheckResponseWhenLoggedOut = null;

        internal int WorkflowId = 0;

        public CookieStore Cookies = new CookieStore();

        Analysis.LogAssociation LoginRequestAsso = null;


        //Thread synchronisation realted
        //ManualResetEvent MSR = new ManualResetEvent(false);
        bool MostRecentIsLoggedInResult = false;
        string MostRecentCsrfTokenValue = "";
        Queue<ManualResetEvent> IsLoggedInQueue = new Queue<ManualResetEvent>();
        Queue<ManualResetEvent> DoLoginQueue = new Queue<ManualResetEvent>();
        Queue<ManualResetEvent> GetCsrfTokenQueue = new Queue<ManualResetEvent>();
        int ExecutingThreadId = 0;


        public static void Add(Recording Rec)
        {
            if (LoadedRecordings.ContainsKey(Rec.RecordingName))
            {
                throw new Exception("Recording with this name already exists");
            }
            else
            {
                lock (LoadedRecordings)
                {
                    LoadedRecordings[Rec.RecordingName] = Rec;
                }
            }
        }

        internal static void ClearAll()
        {
            lock (LoadedRecordings)
            {
                LoadedRecordings.Clear();
            }
        }

        public string CsrfParameterName
        {
            get
            {
                return this.intCsrfParameterName;
            }
        }

        public static List<string> GetNames()
        {
            return new List<string>(LoadedRecordings.Keys);
        }

        public static bool Has(string Name)
        {
            return GetNames().Contains(Name);
        }

        public static Recording Get(string Name)
        {
            if (LoadedRecordings.ContainsKey(Name))
            {
                return LoadedRecordings[Name];
            }
            else
            {
                return null;
            }
        }

        public static Recording FromXml(string Xml)
        {
            XmlDocument Xdoc = new XmlDocument();
            Xdoc.XmlResolver = null;
            Xdoc.LoadXml(Xml);

            string Name = "";
            string Uname = "";
            string Passwd = "";
            string CsrfPara = "";
            List<Session> Sessions = new List<Session>();
            Request LoginChkReq = null;
            Response ResWhenLoggedIn = null;
            Response ResWhenLoggedOut = null;
            
            try
            {
                Name = Xdoc.SelectNodes("/xml/name")[0].InnerText;
            }
            catch{ throw new Exception("Invalid Recording, name field is missing!");}
            try
            {
                Uname = Tools.Base64Decode(Xdoc.SelectNodes("/xml/username")[0].InnerText);
            }
            catch{ throw new Exception("Invalid Recording, username field is missing!");}
            try
            {
                Passwd = Tools.Base64Decode(Xdoc.SelectNodes("/xml/password")[0].InnerText);
            }
            catch{ throw new Exception("Invalid Recording, password field is missing!");}
            try
            {
                CsrfPara = Tools.Base64Decode(Xdoc.SelectNodes("/xml/csrf_token")[0].InnerText);
            }
            catch{ throw new Exception("Invalid Recording, CSRF token field is missing!");}

            try
            {
                foreach(XmlNode SessionNode in Xdoc.SelectNodes("/xml/sessions/session"))
                {
                    int LogId = Int32.Parse(SessionNode.SelectNodes("log_id")[0].InnerText.Trim());
                    Request Req = Request.FromBinaryString(SessionNode.SelectNodes("request")[0].InnerText.Trim());
                    Response Res = Response.FromBinaryString(SessionNode.SelectNodes("response")[0].InnerText.Trim());
                    Session Sess = new Session(LogId, Req, Res);
                    Sessions.Add(Sess);
                }
            }catch{throw new Exception("Invalid recording, logs are corrupted.");}
            
            try
            {
                LoginChkReq = Request.FromBinaryString(Xdoc.SelectNodes("/xml/login_check_request")[0].InnerText);
            }
            catch { throw new Exception("Invalid recording, Login Check Request is missing."); }
            try
            {
                ResWhenLoggedIn = Response.FromBinaryString(Xdoc.SelectNodes("/xml/response_when_logged_in")[0].InnerText);
            }
            catch { throw new Exception("Invalid recording, Reference Response for logged in sessions is missing."); }
            try
            {
                ResWhenLoggedOut = Response.FromBinaryString(Xdoc.SelectNodes("/xml/response_when_logged_out")[0].InnerText);
            }
            catch { throw new Exception("Invalid recording, Reference Response for logged out sessions is missing."); }

            Analysis.LogAnalyzer Analyzer = new Analysis.LogAnalyzer();
            Analysis.LogAssociations Assos = Analyzer.AnalyzeSessionsFromSameUa(Sessions);
            Recording FromDb = new Recording(Assos, Uname, Passwd, CsrfPara);
            FromDb.SetName(Name);
            FromDb.LoginCheckRequest = LoginChkReq;
            FromDb.LoginCheckResponseWhenLoggedIn = ResWhenLoggedIn;
            FromDb.LoginCheckResponseWhenLoggedOut = ResWhenLoggedOut;
            Analysis.LogAssociation LoginAsso = FromDb.LoginAssociations.GetLastAssociationWithParameterValues(new List<string>() { FromDb.Username, FromDb.Password });
            if (LoginAsso == null)
            {
                throw new Exception("Invalid recording, unable to find login request in the login recording");
            }
            FromDb.LoginRequestAsso = LoginAsso;
            return FromDb;
        }

        public string Name
        {
            get
            {
                return this.RecordingName;
            }
        }

        public Recording(Analysis.LogAssociations LoginLogAssos, string Uname, string Pwd, string CsrfParaName)
        {
            this.LoginAssociations = LoginLogAssos;
            this.Username = Uname;
            this.Password = Pwd;
            this.intCsrfParameterName = CsrfParaName;
        }

        public void SetName(string _Name)
        {
            this.RecordingName = _Name;
        }

        public bool IsLoginRecordingReplayable()
        {
            try
            {
                FindLoggedInAndLoggedOutSampleResponses();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsLoggedIn()
        {
            try
            {
                if (ExecutingThreadId == 0 || Thread.CurrentThread.ManagedThreadId == ExecutingThreadId)
                {
                    ExecutingThreadId = Thread.CurrentThread.ManagedThreadId;
                    Response Res = SendLoginCheckRequest();
                    MostRecentIsLoggedInResult = DoesLoginCheckResponseIndicateLoggedInStatus(Res);
                    ExecutingThreadId = 0;
                    ReleaseAllQueues(false);
                }
                else
                {
                    ManualResetEvent MSR = new ManualResetEvent(false);
                    lock (IsLoggedInQueue)
                    {
                        IsLoggedInQueue.Enqueue(MSR);
                    }
                    MSR.WaitOne();
                    return MostRecentIsLoggedInResult;
                }
            }
            catch(Exception Exp)
            {
                ReleaseAllQueues(false);
                throw Exp;
            }
            return MostRecentIsLoggedInResult;
        }

        void ReleaseAllQueues(bool CalledFromDoLogin)
        {
            int DoLoginQueueCount = DoLoginQueue.Count;
            lock (DoLoginQueue)
            {
                while (DoLoginQueue.Count > 0)
                {
                    try
                    {
                        DoLoginQueue.Dequeue().Set();
                    }
                    catch { }
                }
            }
            //If DoLogin queue is not empty then don't release IsLoggedIn and GetCsrfToken queues until the DoLogin queue runs
            //This way is user was logged out then DoLogin queue will relogin the user and after that these two queues can run and return 
            //And When the DoLogin method calls this method then release all queues
            //If the IsLoggedIn method has return True in its last call then its safe to release all queues as user is already loggedin
            //If the DoLogin queue is empty then these queues must be released so that the called calls the DoLogin method
            if (DoLoginQueueCount == 0 || CalledFromDoLogin || MostRecentIsLoggedInResult)
            {
                lock (IsLoggedInQueue)
                {
                    while (IsLoggedInQueue.Count > 0)
                    {
                        try
                        {
                            IsLoggedInQueue.Dequeue().Set();
                        }
                        catch { }
                    }
                }
                lock (GetCsrfTokenQueue)
                {
                    while (GetCsrfTokenQueue.Count > 0)
                    {
                        try
                        {
                            GetCsrfTokenQueue.Dequeue().Set();
                        }
                        catch { }
                    }
                }
            }
        }

        public bool DoLogin()
        {
            bool DoLoginResult = false;
            try
            {
                if (ExecutingThreadId == 0 || Thread.CurrentThread.ManagedThreadId == ExecutingThreadId)
                {
                    ExecutingThreadId = Thread.CurrentThread.ManagedThreadId;
                    Analysis.LogReplayer Replayer = new Analysis.LogReplayer(LoginAssociations);
                    LoginReplayAssociations = Replayer.Play();
                    this.Cookies = LoginReplayAssociations.Cookies;
                    DoLoginResult = IsLoggedIn();
                    GetCsrfToken();
                    ExecutingThreadId = 0;
                    ReleaseAllQueues(true);
                }
                else
                {
                    ManualResetEvent MSR = new ManualResetEvent(false);
                    lock (DoLoginQueue)
                    {
                        DoLoginQueue.Enqueue(MSR);
                    }
                    MSR.WaitOne();
                    if (!MostRecentIsLoggedInResult)
                    {
                        return DoLogin();
                    }
                    return DoLoginResult;
                }
            }
            catch (Exception Exp)
            {
                ReleaseAllQueues(true);
                throw Exp;
            }
            return DoLoginResult;
        }

        Response SendLoginCheckRequest()
        {
            Request Req = LoginCheckRequest.GetClone();
            Req.Cookie.RemoveAll();
            Req.SetCookie(Cookies);
            Req.SetSource("LoginCheck");
            Response Res = Req.Send();
            Cookies.Add(Req, Res);
            return Res;
        }

        bool DoesLoginCheckResponseIndicateLoggedInStatus(Response Res)
        {
            if (LoginCheckResponseWhenLoggedIn == null || LoginCheckResponseWhenLoggedOut == null)
            {
                FindLoggedInAndLoggedOutSampleResponses();
            }

            if (LoginCheckResponseWhenLoggedIn.Code != LoginCheckResponseWhenLoggedOut.Code)
            {
                if (Res.Code == LoginCheckResponseWhenLoggedIn.Code)
                {
                    return true;
                }
            }
            if (LoginCheckResponseWhenLoggedIn.IsRedirect && LoginCheckResponseWhenLoggedOut.IsRedirect)
            {
                if (Res.IsRedirect)
                {
                    Request LoggedInRedirect = LoginCheckRequest.GetRedirect(LoginCheckResponseWhenLoggedIn);
                    Request LoggedOutRedirect = LoginCheckRequest.GetRedirect(LoginCheckResponseWhenLoggedOut);
                    if (!LoggedInRedirect.FullUrl.Equals(LoggedOutRedirect.FullUrl))
                    {
                        Request CurrentRedirect = LoginCheckRequest.GetRedirect(Res);
                        if (CurrentRedirect.FullUrl.Equals(LoggedInRedirect.FullUrl))
                        {
                            return true;
                        }
                        else if (CurrentRedirect.FullUrl.Equals(LoggedOutRedirect.FullUrl))
                        {
                            return false;
                        }
                    }
                    else if(!LoggedInRedirect.UrlPath.Equals(LoggedOutRedirect.UrlPath))
                    {
                        Request CurrentRedirect = LoginCheckRequest.GetRedirect(Res);
                        if (CurrentRedirect.UrlPath.Equals(LoggedInRedirect.UrlPath))
                        {
                            return true;
                        }
                        else if (CurrentRedirect.UrlPath.Equals(LoggedOutRedirect.UrlPath))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            int LoggedOutPasswordFieldsCount = LoginCheckResponseWhenLoggedOut.Html.Get("input", "type", "password").Count;
            if (LoggedOutPasswordFieldsCount > 0)
            {
                int LoggedInPasswordFieldsCount = LoginCheckResponseWhenLoggedIn.Html.Get("input", "type", "password").Count;
                int ResPasswordFieldsCount = Res.Html.Get("input", "type", "password").Count;
                if(ResPasswordFieldsCount == LoggedInPasswordFieldsCount)
                {
                    if(ResPasswordFieldsCount == 0) return true;
                }
            }
            return false;
        }

        void FindLoggedInAndLoggedOutSampleResponses()
        {
            Analysis.LogAssociation LoginAsso = LoginAssociations.GetLastAssociationWithParameterValues(new List<string>() { Username, Password });
            if (LoginAsso == null)
            {
                throw new Exception("Unable to find login request in the login recording");
            }
            this.LoginRequestAsso = LoginAsso;

            Analysis.LogReplayer ValidCredsReplayer = new Analysis.LogReplayer(LoginAssociations);
            Analysis.LogReplayAssociations ValidCredsAssociations = ValidCredsReplayer.Play();

            Analysis.LogReplayer InvalidCredsReplayer = new Analysis.LogReplayer(LoginAssociations, UpdateLastLoginRequestWithInvalidCreds);
            Analysis.LogReplayAssociations InValidCredsAssociations = InvalidCredsReplayer.Play();

            //now compare ValidCredsAssociations and InvalidCredsAssociations and decide which one will be the LoginCheckRequest and also update the LoginCheckResponseWhenLoggedIn and LoginCheckResponseWhenLoggedOut values
            List<Analysis.LogAssociation> LoggedInCheckResponeCandidates = new List<Analysis.LogAssociation>();
            List<Analysis.LogAssociation> LoggedOutCheckResponeCandidates = new List<Analysis.LogAssociation>();
            foreach (int i in ValidCredsAssociations.OriginalLogIds)// . FirstOriginalLogId; i <= ValidCredsAssociations.LastOriginalLogId; i++)
            {
                if (i <= LoginAsso.DestinationLog.LogId) continue;//We don't want to include the 

                if (InValidCredsAssociations.HasOriginalLog(i))
                {
                    if (CanBeLoggedInLoggedOutResCandidate(ValidCredsAssociations.GetAssociationByOriginalId(i).ReplayAssociation, InValidCredsAssociations.GetAssociationByOriginalId(i).ReplayAssociation))
                    {
                        LoggedInCheckResponeCandidates.Add(ValidCredsAssociations.GetAssociationByOriginalId(i).ReplayAssociation);
                        LoggedOutCheckResponeCandidates.Add(InValidCredsAssociations.GetAssociationByOriginalId(i).ReplayAssociation);
                    }
                }
            }
            for (int i=0; i < LoggedInCheckResponeCandidates.Count; i++)
            {
                Analysis.LogAssociation InResCand = LoggedInCheckResponeCandidates[i];
                Analysis.LogAssociation OutResCand = LoggedOutCheckResponeCandidates[i];
                if (InResCand.DestinationLog.Request.Host.Equals(LoginAsso.DestinationLog.Request.Host))
                {
                    LoginCheckRequest = InResCand.DestinationLog.Request.GetClone();
                    LoginCheckResponseWhenLoggedIn = InResCand.DestinationLog.Response.GetClone();
                    LoginCheckResponseWhenLoggedOut = OutResCand.DestinationLog.Response.GetClone();
                    break;
                }
                else if (!Tools.IsValidIpv4(InResCand.DestinationLog.Request.Host) && !Tools.IsValidIpv6(LoginAsso.DestinationLog.Request.Host))
                {
                    string[] InReqParts = InResCand.DestinationLog.Request.Host.Split('.');
                    string[] LoginReqParts = LoginAsso.DestinationLog.Request.Host.Split('.');
                    if (InReqParts.Length > 1 && LoginReqParts.Length > 1)
                    {
                        if (LoginReqParts[LoginReqParts.Length - 1].Equals(InReqParts[InReqParts.Length - 1]) && LoginReqParts[LoginReqParts.Length - 2].Equals(InReqParts[InReqParts.Length - 2]))
                        {
                            LoginCheckRequest = InResCand.DestinationLog.Request.GetClone();
                            LoginCheckResponseWhenLoggedIn = InResCand.DestinationLog.Response.GetClone();
                            LoginCheckResponseWhenLoggedOut = OutResCand.DestinationLog.Response.GetClone();
                            break;
                        }
                    }
                }
            }
            if (LoginCheckRequest == null || LoginCheckResponseWhenLoggedIn == null || LoginCheckResponseWhenLoggedOut == null)
            {
                if (LoggedInCheckResponeCandidates.Count > 0 && LoggedOutCheckResponeCandidates.Count > 0)
                {
                    LoginCheckRequest = LoggedInCheckResponeCandidates[LoggedInCheckResponeCandidates.Count - 1].DestinationLog.Request.GetClone();
                    LoginCheckResponseWhenLoggedIn = LoggedInCheckResponeCandidates[LoggedInCheckResponeCandidates.Count - 1].DestinationLog.Response.GetClone();
                    LoginCheckResponseWhenLoggedOut = LoggedOutCheckResponeCandidates[LoggedOutCheckResponeCandidates.Count - 1].DestinationLog.Response.GetClone();
                }
            }
            if (LoginCheckRequest == null || LoginCheckResponseWhenLoggedIn == null || LoginCheckResponseWhenLoggedOut == null)
            {
                Request TestLoginCheckReq = LoginAsso.DestinationLog.Request.GetClone();
                TestLoginCheckReq.BodyString = "";
                TestLoginCheckReq.Method = "GET";
                TestLoginCheckReq.CookieString = "";
                if(TestLoginCheckReq.Url.Contains(Tools.UrlEncode(Username)) && TestLoginCheckReq.Url.Contains(Tools.UrlEncode(Password)))
                {
                    TestLoginCheckReq.Query.RemoveAll();
                }
                List<Response> LoggedInReses = new List<Response>();
                TestLoginCheckReq.SetSource("LoginCheck");
                TestLoginCheckReq.SetCookie(ValidCredsAssociations.Cookies);
                Response LoggedInResCandidate = TestLoginCheckReq.Send();

                TestLoginCheckReq.CookieString = "";
                TestLoginCheckReq.SetCookie(InValidCredsAssociations.Cookies);
                Response LoggedOutResCandidate = TestLoginCheckReq.Send();

                if (CanBeLoggedInLoggedOutResCandidate(LoggedInResCandidate, LoggedOutResCandidate))
                {
                    LoginCheckRequest = TestLoginCheckReq.GetClone();
                    LoginCheckResponseWhenLoggedIn = LoggedInResCandidate.GetClone();
                    LoginCheckResponseWhenLoggedOut = LoggedOutResCandidate.GetClone();
                }
            }
            if (LoginCheckRequest == null || LoginCheckResponseWhenLoggedIn == null || LoginCheckResponseWhenLoggedOut == null)
            {
                throw new Exception("Unable to find a suitable logged in status check response");
            }
        }

        bool CanBeLoggedInLoggedOutResCandidate(Analysis.LogAssociation LoggedInAsso, Analysis.LogAssociation LoggedOutAsso)
        {
            if (!(LoggedInAsso.DestinationLog != null && LoggedOutAsso.DestinationLog != null && LoggedInAsso.DestinationLog.Response != null && LoggedOutAsso.DestinationLog.Response != null))
            {
                return false;
            }
            return CanBeLoggedInLoggedOutResCandidate(LoggedInAsso.DestinationLog.Response, LoggedOutAsso.DestinationLog.Response);
        }

        bool CanBeLoggedInLoggedOutResCandidate(Response LoggedInRes, Response LoggedOutRes)
        {
            if (LoggedInRes.Code == 304 || LoggedOutRes.Code == 304)
            {
                return false;
            }
            if (((LoggedInRes.Code == 302 || LoggedInRes.Code == 301) && LoggedOutRes.Code == 200)
            || (LoggedInRes.Code == 200 && (LoggedOutRes.Code == 301 || LoggedOutRes.Code == 302)))
            {
                return true;
            }
            if (LoggedInRes.Html.Get("input", "type", "password").Count == 0 && LoggedOutRes.Html.Get("input", "type", "password").Count > 0)
            {
                return true;
            }
            return false;
        }

        Request UpdateLastLoginRequestWithInvalidCreds(Request Req, Analysis.LogAssociation CorrespondingOriginalLogAssociation)
        {
            if (CorrespondingOriginalLogAssociation.DestinationLog.LogId == LoginRequestAsso.DestinationLog.LogId)
            {
                foreach (string Name in Req.Query.GetNames())
                {
                    List<string> Vals = Req.Query.GetAll(Name);
                    for (int i = 0; i < Vals.Count; i++)
                    {
                        if (Vals[i] == Username || Vals[i] == Password)
                        {
                            Vals[i] = "XX" + Vals[i];
                        }
                    }
                    Req.Query.Set(Name, Vals);
                }
                foreach (string Name in Req.Body.GetNames())
                {
                    List<string> Vals = Req.Body.GetAll(Name);
                    for (int i = 0; i < Vals.Count; i++)
                    {
                        if (Vals[i] == Username || Vals[i] == Password)
                        {
                            Vals[i] = "XX" + Vals[i];
                        }
                    }
                    Req.Body.Set(Name, Vals);
                }
            }
            return Req;
        }

        public string GetCsrfToken()
        {
            try
            {
                if (ExecutingThreadId == 0 || Thread.CurrentThread.ManagedThreadId == ExecutingThreadId)
                {
                    ExecutingThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (intCsrfParameterName.Length > 0 && LoginReplayAssociations != null)
                    {
                        List<int> ReplayLogIds = LoginReplayAssociations.LogIds;
                        foreach (int ReplayLogId in ReplayLogIds)
                        {
                            Analysis.LogReplayAssociation Asso = LoginReplayAssociations.GetAssociation(ReplayLogId);
                            if (Asso.OriginalAssociation.DestinationLog.LogId >= LoginRequestAsso.DestinationLog.LogId)
                            {
                                if (Asso.ReplayAssociation.DestinationLog.Response != null)
                                {
                                    List<string> Values = Asso.ReplayAssociation.DestinationLog.Response.Html.GetValues("input", "name", intCsrfParameterName, "value");
                                    foreach (string Val in Values)
                                    {
                                        if (Val.Trim().Length > 0)
                                        {
                                            MostRecentCsrfTokenValue = Val;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ExecutingThreadId = 0;
                    ReleaseAllQueues(false);
                }
                else
                {
                    ManualResetEvent MSR = new ManualResetEvent(false);
                    lock (GetCsrfTokenQueue)
                    {
                        GetCsrfTokenQueue.Enqueue(MSR);
                    }
                    MSR.WaitOne();
                    return MostRecentCsrfTokenValue;
                }
            }
            catch (Exception Exp)
            {
                ReleaseAllQueues(false);
                throw Exp;
            }
            return MostRecentCsrfTokenValue;
        }

        public string ToXml()
        {
            StringWriter SW = new StringWriter();
            XmlTextWriter XW = new XmlTextWriter(SW);
            XW.Formatting = Formatting.Indented;
            XW.WriteStartElement("xml");
            XW.WriteStartElement("version"); XW.WriteValue("1.0"); XW.WriteEndElement();
            XW.WriteStartElement("name"); XW.WriteValue(this.Name); XW.WriteEndElement();
            XW.WriteStartElement("username"); XW.WriteValue(Tools.Base64Encode(Username)); XW.WriteEndElement();
            XW.WriteStartElement("password"); XW.WriteValue(Tools.Base64Encode(Password)); XW.WriteEndElement();
            XW.WriteStartElement("csrf_token"); XW.WriteValue(Tools.Base64Encode(intCsrfParameterName)); XW.WriteEndElement();
            
            XW.WriteStartElement("sessions");
            foreach (int LogId in LoginAssociations.LogIds)
            {
                try
                {
                    Analysis.LogAssociation Asso = LoginAssociations.GetAssociation(LogId);
                    XW.WriteStartElement("session");
                    XW.WriteStartElement("log_id"); XW.WriteValue(Asso.DestinationLog.LogId); XW.WriteEndElement();
                    XW.WriteStartElement("request"); XW.WriteValue(Asso.DestinationLog.Request.ToBinaryString()); XW.WriteEndElement();
                    XW.WriteStartElement("response"); XW.WriteValue(Asso.DestinationLog.Response.ToBinaryString()); XW.WriteEndElement();
                    XW.WriteEndElement();
                }
                catch { }
            }
            XW.WriteEndElement();

            XW.WriteStartElement("login_check_request"); XW.WriteValue(LoginCheckRequest.ToBinaryString()); XW.WriteEndElement();
            XW.WriteStartElement("response_when_logged_in"); XW.WriteValue(LoginCheckResponseWhenLoggedIn.ToBinaryString()); XW.WriteEndElement();
            XW.WriteStartElement("response_when_logged_out"); XW.WriteValue(LoginCheckResponseWhenLoggedOut.ToBinaryString()); XW.WriteEndElement();
            
            /*
            XW.WriteStartElement("csrf_token_sessions");
            foreach (int LogId in CsrfAssociations.LogIds)
            {
                try
                {
                    Analysis.LogAssociation Asso = LoginAssociations.GetAssociation(LogId);
                    XW.WriteStartElement("session");
                    XW.WriteStartElement("log_id"); XW.WriteValue(Asso.DestinationLog.LogId); XW.WriteEndElement();
                    XW.WriteStartElement("request"); XW.WriteValue(Asso.DestinationLog.Request.ToBinaryString()); XW.WriteEndElement();
                    XW.WriteStartElement("response"); XW.WriteValue(Asso.DestinationLog.Response.ToBinaryString()); XW.WriteEndElement();
                    XW.WriteEndElement();
                }
                catch { }
            }
            XW.WriteEndElement();
            */
            XW.WriteEndElement();
            XW.Close();
            SW.Close();
            return SW.ToString().Trim();
        }

        public Workflow.Workflow ToWorkflow()
        {
            List<int> LogIds = new List<int>();
            //LoginAssociations.LogIds
            Workflow.Workflow Flow = new Workflow.Workflow(LoginAssociations.LogIds, "Proxy", this.LoginRequestAsso.DestinationLog.Request.UserAgent, Workflow.WorkflowSource.User, Workflow.WorkflowType.Login);
            Flow.SetInfo("RecordingXml", ToXml());
            Flow.SetName(this.Name);
            return Flow;
        }

        public static Recording FromWorkflow(Workflow.Workflow Flow)
        {
            Recording Rec = FromXml(Flow.Info["RecordingXml"]);
            Rec.WorkflowId = Flow.Id;
            return Rec;
        }
    }
}
