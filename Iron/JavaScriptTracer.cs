using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace IronWASP
{
    public class JavaScriptTracer
    {
        internal static bool MonitorEval = true;
        internal static bool MonitorSetTimeout = true;
        internal static bool MonitorSetInterval = true;
        internal static bool MonitorUserDefinedMethods = true;
        internal static bool MonitorFunctionMethods = true;

        internal static bool MonitorInnerHtmlAssignments = true;

        internal static bool MonitorXhr = true;

        internal static bool InjectJavaScript = false;
        internal static string TopJs = "";
        internal static string BottomJs = "";

        static Dictionary<string, Dictionary<int, Data>> DataDict = new Dictionary<string, Dictionary<int, Data>>();

        //static Dictionary<int, string> AjaxWatchDict = new Dictionary<int, string>();

        static Dictionary<string, List<Data>> AjaxPartHolderDict = new Dictionary<string, List<Data>>();

        static Thread AnalysisThread = null;

        static bool Running = true;

        static List<ObservationMsg> ObservationsMsgs = new List<ObservationMsg>();
        static List<AjaxCallMsg> AjaxCallMsgs = new List<AjaxCallMsg>();
        static List<DomChangeMsg> DomChangeMsgs = new List<DomChangeMsg>();
        static List<NativeMethodCallMsg> NativeMethodCallMsgs = new List<NativeMethodCallMsg>();

        static long AjaxResponseWaitTimeout = 200000000;
        static long MaxAgeForDataInDict = 200000000;

        internal static void StartAnalyzer()
        {
            AnalysisThread = new Thread(AnalyzeDicts);
            AnalysisThread.Start();
        }

        internal static void StopAnalyzer()
        {
            try
            {
                Running = false;
                AnalysisThread.Abort();
            }
            catch { }
        }

        internal static void LoadJsToInset()
        {
            try
            {
                TopJs = File.ReadAllText(string.Format("{0}\\top.js", Config.Path));
            }
            catch (Exception Exp)
            {
                IronException.Report("Error loading top.js", Exp);
            }
            try
            {
                BottomJs = File.ReadAllText(string.Format("{0}\\btm.js", Config.Path));
            }
            catch (Exception Exp)
            {
                IronException.Report("Error loading btm.js", Exp);
            }
        }

        internal static void ProcessSession(Session IrSe)
        {
            if (IrSe.Response == null)
            {
                if (IrSe.Request.Headers.Has("x-org-ironwasp-js-trace-ajax"))
                {
                    string AjaxId = IrSe.Request.Headers.Get("x-org-ironwasp-js-trace-ajax");
                    IrSe.FiddlerSession.oFlags.Add("IronFlag-JS-Trace-XHR-ID", AjaxId);

                    Data D = new Data(IrSe);
                    AddTraceMessagesToList(D);
                    lock (AjaxPartHolderDict)
                    {
                        if (!AjaxPartHolderDict.ContainsKey(AjaxId))
                        {
                            AjaxPartHolderDict[AjaxId] = new List<Data>();
                        }
                        AjaxPartHolderDict[AjaxId].Add(D);
                    }
                    IrSe.Request.Headers.Remove("x-org-ironwasp-js-trace-ajax");
                    IrSe.UpdateFiddlerSessionFromIronSession();
                }
            }
            else
            {
                if (!IrSe.FiddlerSession.isFlagSet(Fiddler.SessionFlags.RequestGeneratedByFiddler))
                {
                    if (IrSe.FiddlerSession.oFlags.ContainsKey("IronFlag-JS-Trace-XHR-ID"))
                    {
                        string AjaxId = IrSe.FiddlerSession.oFlags["IronFlag-JS-Trace-XHR-ID"];
                        if (AjaxId.Length > 0)
                        {
                            Data D = new Data(IrSe);
                            AddTraceMessagesToList(D);
                            lock (AjaxPartHolderDict)
                            {
                                if (AjaxPartHolderDict.ContainsKey(AjaxId))
                                {
                                    AjaxPartHolderDict[AjaxId].Add(D);
                                }
                            }
                        }
                    }
                    InjectTracingJavaScript(IrSe);
                }
            }
        }

        static void AddTraceMessagesToList(Data D)
        {
            if (D.Type == DataType.MethodCall)
            {
                lock (NativeMethodCallMsgs)
                {
                    NativeMethodCallMsgs.Add(new NativeMethodCallMsg(D));
                }
            }
            else if (D.Type == DataType.DomChange)
            {
                lock (DomChangeMsgs)
                {
                    DomChangeMsgs.Add(new DomChangeMsg(D));
                }
            }
            else if (D.Type == DataType.AjaxCall)
            {
                lock (AjaxCallMsgs)
                {
                    AjaxCallMsgs.Add(new AjaxCallMsg(D));
                }
            }
        }

        internal static void InjectTracingJavaScript(Session IrSe)
        {
            if (!IrSe.FiddlerSession.isFlagSet(Fiddler.SessionFlags.RequestGeneratedByFiddler) && InjectJavaScript)
            {
                if (TopJs.Length > 0 || BottomJs.Length > 0)
                {
                    if (IrSe.Response.IsHtml)
                    {
                        if (IrSe.Response.BodyString.Contains("<html") && IrSe.Response.BodyString.Contains("</html>"))
                        {
                            bool ResponseUpdated = false;
                            string ConfigSettings = GetConfigJs();

                            int FirstMark = IrSe.Response.BodyString.IndexOf("<head>") + 6;
                            int LastMark = IrSe.Response.BodyString.LastIndexOf("</html>");
                            string Head = IrSe.Response.BodyString.Substring(0, FirstMark).Trim();
                            string Middle = IrSe.Response.BodyString.Substring(FirstMark, LastMark-FirstMark).Trim();
                            string Last = IrSe.Response.BodyString.Substring(LastMark).Trim();

                            if (Head.StartsWith("<"))
                            {
                                StringBuilder SB = new StringBuilder();
                                SB.Append(Head);
                                if (TopJs.Length > 0)
                                {
                                    SB.Append(string.Format("<script>{0}\r\n{1}</script>", ConfigSettings, TopJs));
                                    ResponseUpdated = true;
                                }
                                SB.Append(Middle);
                                if (BottomJs.Length > 0)
                                {
                                    SB.Append(string.Format("<script>{0}{1}</script>", ConfigSettings, BottomJs));
                                    ResponseUpdated = true;
                                }
                                SB.Append(Last);
                                IrSe.Response.BodyString = SB.ToString();
                            }
                            /*
                            if (TopJs.Length > 0)
                            {
                                IrSe.Response.BodyString =  IrSe.Response.BodyString.Replace("<head>", string.Format("<head><script>{0}\r\n{1}</script>", ConfigSettings, TopJs));
                                ResponseUpdated = true;
                            }
                            if (BottomJs.Length > 0)
                            {
                                IrSe.Response.BodyString = IrSe.Response.BodyString.Replace("</html>", string.Format("<script>{0}{1}</script></html>", ConfigSettings, BottomJs));
                                ResponseUpdated = true;
                            }
                            */

                            if (ResponseUpdated)
                            {
                                if (IrSe.Response.Headers.Has("Content-Security-Policy"))
                                {
                                    IrSe.Response.Headers.Remove("Content-Security-Policy");
                                }
                                IrSe.UpdateFiddlerSessionFromIronSession();
                            }
                        }
                    }
                }
            }
        }

        static string GetConfigJs()
        {
            Newtonsoft.Json.Linq.JObject ConfigObj = new Newtonsoft.Json.Linq.JObject();

            ConfigObj["eval"] = MonitorEval;
            ConfigObj["setTimeout"] = MonitorSetTimeout;
            ConfigObj["setInterval"] = MonitorSetInterval;
            ConfigObj["Function"] = MonitorFunctionMethods;
            ConfigObj["nonNative"] = MonitorUserDefinedMethods;
            ConfigObj["innerHTML"] = MonitorInnerHtmlAssignments;
            ConfigObj["XHR"] = MonitorXhr;

            return string.Format("if (typeof __org_ironwasp_js__ === 'undefined'){{__org_ironwasp_js__ = {{}};__org_ironwasp_js__.config = {0};}}", ConfigObj.ToString());
        }

        internal static void LogTraceMessage(string Msg)
        {
            try
            {
                Newtonsoft.Json.Linq.JToken Json = Tools.ParseAsJson(Msg);
                if (Json is Newtonsoft.Json.Linq.JArray)
                {
                    foreach (Newtonsoft.Json.Linq.JObject Jobj in (Json as Newtonsoft.Json.Linq.JArray))
                    {
                        Process(Jobj);
                    }
                }
                else if (Json is Newtonsoft.Json.Linq.JObject)
                {
                    Process(Json as Newtonsoft.Json.Linq.JObject);
                }
            }
            catch (Exception Exp)
            {
                IronException.Report("Error reading JavaScript Trace message from browser", Exp);
            }
        }

        static void Process(Newtonsoft.Json.Linq.JObject Msg)
        {
            //ShowInOverview(Msg);
            Data D = new Data(Msg);
            AddTraceMessagesToList(D);
            if (D.Type == DataType.AjaxOpen)
            {
                lock (AjaxPartHolderDict)
                {
                    if (!AjaxPartHolderDict.ContainsKey(D.XhrId))
                    {
                        AjaxPartHolderDict[D.XhrId] = new List<Data>();
                    }
                    AjaxPartHolderDict[D.XhrId].Add(D);
                }
            }
            else
            {
                lock (DataDict)
                {
                    if (!DataDict.ContainsKey(D.WindowId))
                    {
                        DataDict[D.WindowId] = new Dictionary<int, Data>();
                    }
                    DataDict[D.WindowId][D.DataId] = D;
                }
            }
        }

        static void AnalyzeDicts()
        {
            while (Running)
            {
                try
                {
                    if (InjectJavaScript)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }

                    AnalyzeAjaxPlaceHolderDict();
                    AnalyzeDataDict();

                    lock (ObservationsMsgs)
                    {
                        IronUI.AddJsTraceObservation(new List<ObservationMsg>(ObservationsMsgs));
                        ObservationsMsgs.Clear();
                    }

                    lock (AjaxCallMsgs)
                    {
                        IronUI.AddJsTraceAjaxCall(new List<AjaxCallMsg>(AjaxCallMsgs));
                        AjaxCallMsgs.Clear();
                    }

                    lock (DomChangeMsgs)
                    {
                        IronUI.AddJsTraceDomChangeCall(new List<DomChangeMsg>(DomChangeMsgs));
                        DomChangeMsgs.Clear();
                    }

                    lock (NativeMethodCallMsgs)
                    {
                        IronUI.AddJsTraceNativeMethodCall(new List<NativeMethodCallMsg>(NativeMethodCallMsgs));
                        NativeMethodCallMsgs.Clear();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception Exp)
                {
                    IronException.Report("Error analysing ", Exp);
                }
            }
        }

        static void AnalyzeDataDict()
        {
            List<string> WindowIdsToRemove = new List<string>();
            List<string> WindowIds = new List<string>(DataDict.Keys);
            foreach (string WindowId in WindowIds)
            {
                List<int> DataIds = new List<int>(DataDict[WindowId].Keys);
                List<int> DataIdsToRemove = new List<int>();
                foreach (int DataId in DataIds)
                {
                    if (DateTime.Now.Ticks - DataDict[WindowId][DataId].Time > MaxAgeForDataInDict)
                    {
                        DataIdsToRemove.Add(DataId);
                    }
                    if (DataDict[WindowId][DataId].Type == DataType.PageUnload)
                    {
                        WindowIdsToRemove.Add(WindowId);
                    }

                    if (!DataDict[WindowId][DataId].Analyzed)
                    {
                        DataDict[WindowId][DataId].Analyze();

                        ObservationsMsgs.AddRange(DataDict[WindowId][DataId].ObservationMsgs);
                        DataDict[WindowId][DataId].ObservationMsgs.Clear();
                    }
                    if(DataDict[WindowId][DataId].Type == DataType.AjaxCall && DataDict[WindowId][DataId].AjaxResponse != null)
                    {
                        if(!DataDict[WindowId][DataId].AjaxResponseAnalyzed)
                        {
                            Data AjaxCallD = DataDict[WindowId][DataId];
                            foreach(int DID in DataIds)
                            {
                                if (DataDict[WindowId][DID].Type == DataType.DomChange || DataDict[WindowId][DID].Type == DataType.MethodCall)
                                {
                                    if (DataDict[WindowId][DID].TimeInData > AjaxCallD.TimeInData && DataDict[WindowId][DID].TimeInData < (AjaxCallD.TimeInData + 10000))
                                    {
                                        DataDict[WindowId][DID].Analyze(AjaxCallD);
                                    }
                                }
                            }
                        }
                    }
                    
                }
                foreach(int DataId in DataIdsToRemove)
                {
                    if(DataDict[WindowId].ContainsKey(DataId))
                    {
                        DataDict[WindowId].Remove(DataId);
                    }
                }
            }
            lock (DataDict)
            {
                foreach (string WindowIdToRemove in WindowIdsToRemove)
                {
                    DataDict.Remove(WindowIdToRemove);
                }
            }
        }

        static void AnalyzeAjaxPlaceHolderDict()
        {
            List<string> XhrIdsToRemove = new List<string>();
            
            List<string> XhrIds = new List<string>(AjaxPartHolderDict.Keys);

            foreach (string XhrId in XhrIds)
            {
                Data OpenData = null;
                Data RequestData = null;
                Data ResponseData = null;
                Data CombinedData = null;
                foreach (Data D in AjaxPartHolderDict[XhrId])
                {
                    if (D.Type == DataType.AjaxOpen)
                    {
                        OpenData = D;
                    }
                    else if (D.Type == DataType.AjaxRequest)
                    {
                        RequestData = D;
                    }
                    else if (D.Type == DataType.AjaxResponse)
                    {
                        ResponseData = D;
                    }
                    else if (D.Type == DataType.AjaxCall)
                    {
                        CombinedData = D;
                    }
                }

                if (CombinedData == null)
                {
                    if (OpenData != null)
                    {
                        if (RequestData != null)
                        {
                            OpenData.AddAjaxRequest(RequestData);
                            CombinedData = OpenData;
                            if (ResponseData != null)
                            {
                                CombinedData.AddAjaxResponse(ResponseData);
                                if (!DataDict.ContainsKey(CombinedData.WindowId))
                                {
                                    DataDict[CombinedData.WindowId] = new Dictionary<int, Data>();
                                }
                                DataDict[CombinedData.WindowId][CombinedData.DataId] = CombinedData;
                            }
                            else
                            {
                                AjaxPartHolderDict[XhrId].Add(CombinedData);
                            }
                        }
                    }
                }
                else
                {
                    if (CombinedData.AjaxResponse == null)
                    {
                        if (ResponseData != null)
                        {
                            if (DataDict.ContainsKey(CombinedData.WindowId))
                            {
                                if (DataDict[CombinedData.WindowId].ContainsKey(CombinedData.DataId))
                                {
                                    DataDict[CombinedData.WindowId][CombinedData.DataId].AddAjaxResponse(ResponseData);
                                    XhrIdsToRemove.Add(XhrId);
                                }
                            }
                        }
                    }
                }

                if(ResponseData == null)
                {
                    long RequestTime = 0;
                    if(CombinedData != null)
                    {
                        RequestTime = CombinedData.Time;   
                    }
                    else if (RequestData != null)
                    {
                        RequestTime = RequestData.Time;
                    }
                    else if (OpenData != null)
                    {
                        RequestTime = OpenData.Time;
                    }
                    if (DateTime.Now.Ticks - RequestTime > AjaxResponseWaitTimeout)
                    {
                        XhrIdsToRemove.Add(XhrId);
                    }
                }
            }
            foreach(string XhrId in XhrIdsToRemove)
            {
                if (AjaxPartHolderDict.ContainsKey(XhrId))
                {
                    AjaxPartHolderDict.Remove(XhrId);
                }
            }
        }

        /*
        internal static void ShowObservation()
        {

        }
        */
        /*
        internal static void ShowInOverview(Newtonsoft.Json.Linq.JObject Msg)
        {

            switch(Msg.Value<string>("action"))
            {
                case ("EvalCalled"):
                    IronUI.AddJsTraceNativeMethodCall(new NativeMethodCallMsg() { Method = "eval", Details = IronJint.Beautify(Msg["value"]["args"].ToString()) });
                    break;
                case ("FunctionCalled"):
                    IronUI.AddJsTraceNativeMethodCall(new NativeMethodCallMsg() { Method = "Function", Details = IronJint.Beautify(Msg["value"]["args"].ToString()) });
                    break;
                case ("SetTimeoutCalled"):
                    IronUI.AddJsTraceNativeMethodCall(new NativeMethodCallMsg() { Method = "setTimeout", Details = IronJint.Beautify(Msg["value"]["args"].ToString()) });
                    break;
                case ("SetIntervalCalled"):
                    IronUI.AddJsTraceNativeMethodCall(new NativeMethodCallMsg() { Method = "setInterval", Details = IronJint.Beautify(Msg["value"]["args"].ToString()) });
                    break;
                case ("XhrOpenCalled"):
                    IronUI.AddJsTraceAjaxCall(new AjaxCallMsg() { Details = Msg["value"].ToString() });
                    break;
                case ("XhrSendCalled"):
                    IronUI.AddJsTraceAjaxCall(new AjaxCallMsg() { Details = Msg["value"].ToString() });
                    break;
                case ("AttributeChanged"):
                    IronUI.AddJsTraceDomChangeCall(new DomChangeMsg() { Action = "Attribute Changed", Details = Msg["value"].ToString() });
                    break;
                case ("NodeAdded"):
                    IronUI.AddJsTraceDomChangeCall(new DomChangeMsg() { Action = "Node Added", Details = Msg["value"].ToString() });
                    break;
                case ("PageLoaded"):
                    IronUI.AddJsTraceObservation(new ObservationMsg() { Type = "Page Loaded", Details = Msg["value"].ToString() });
                    break;
                case ("PageUnloading"):
                    IronUI.AddJsTraceObservation(new ObservationMsg() { Type = "Page Unloading", Details = Msg["value"].ToString() });
                    break;
            }
        }
        */


        public class Data
        {
            static int DataIdCounter = 0;

            public long Time = 0;
            public long TimeInData = 0;
            public string WindowId = "";
            public string Action = "";
            public string Url = "";
            public string Origin = "";
            public string XhrId = "";
            //public int XhrLogId = 0;
            public DataType Type;
            public SinkType SinkType;
            public string[] Arguments = new string[] { };
            public string[] Nodes = new string[] { };
            public string NodeName = "";
            public string AttributeName = "";
            public string AttributeValue = "";

            public int DataId = 0;

            public Data AjaxRequest = null;
            public Data AjaxResponse = null;

            public string AjaxRequestBody = "";
            public string AjaxResponseBody = "";

            public bool Analyzed = false;
            public bool AjaxResponseAnalyzed = false;

            public Dictionary<DataLocation, List<DataValue>> Values = new Dictionary<DataLocation, List<DataValue>>();

            public List<ObservationMsg> ObservationMsgs = new List<ObservationMsg>();

            public Dictionary<DataLocation, List<string>> Reflections = new Dictionary<DataLocation, List<string>>();

            public Data(Session Sess)
            {
                this.DataId = Interlocked.Increment(ref DataIdCounter);

                this.Url = Sess.Request.FullUrl;
                ParseUrl();
                this.Time = DateTime.Now.Ticks;
                if (Sess.Response == null)
                {
                    this.Type = DataType.AjaxRequest;
                    AddRequestBodyValues(Sess.Request.BodyString, Sess.Request.IsJson);
                    /*
                    if (Sess.Request.IsNormal)
                    {
                        this.Values[DataLocation.JsonValueInRequestBody] = new List<DataValue>();
                        foreach (string Name in Sess.Request.Body.GetNames())
                        {
                            this.Values[DataLocation.JsonValueInRequestBody].AddRange(DataValue.Parse(Sess.Request.Body.GetAll(Name)));
                        }
                    }
                    else
                    {
                        AddRequestBodyValues(Sess.Request.BodyString, Sess.Request.IsJson);
                    }
                    */
                }
                else
                {
                    this.Type = DataType.AjaxResponse;
                    AddResponseBodyValues(Sess.Response.BodyString);
                }
            }

            public Data(Newtonsoft.Json.Linq.JObject Msg)
            {
                this.DataId = Interlocked.Increment(ref DataIdCounter);

                this.Time = DateTime.Now.Ticks;
                this.TimeInData = Msg.Value<long>("time");
                this.WindowId = Msg.Value<string>("window_id");
                this.Action = Msg.Value<string>("action");
                this.Url = Msg.Value<string>("url");

                switch (this.Action)
                {
                    case ("EvalCalled"):
                        this.SinkType = JavaScriptTracer.SinkType.eval;
                        break;
                    case ("FunctionCalled"):
                        this.SinkType = JavaScriptTracer.SinkType.Function;
                        break;
                    case ("SetTimeoutCalled"):
                        this.SinkType = JavaScriptTracer.SinkType.setTimeout;
                        break;
                    case ("SetIntervalCalled"):
                        this.SinkType = JavaScriptTracer.SinkType.setInterval;
                        break;
                    case ("XhrOpenCalled"):
                        this.SinkType = JavaScriptTracer.SinkType.XhrOpen;
                        break;
                    case ("AttributeChanged"):
                        this.SinkType = JavaScriptTracer.SinkType.Attribute;
                        break;
                    case ("NodeAdded"):
                        this.SinkType = JavaScriptTracer.SinkType.Html;
                        break;
                }

                switch (this.Action)
                {
                    case ("EvalCalled"):
                    case ("FunctionCalled"):
                    case ("SetTimeoutCalled"):
                    case ("SetIntervalCalled"):
                        this.Type = DataType.MethodCall;
                        ParseArguments(Msg);
                        break;
                    case ("XhrOpenCalled"):
                        this.Type = DataType.AjaxOpen;
                        this.XhrId = Msg["value"].Value<string>("xhr_id");
                        ParseArguments(Msg);
                        break;
                    case ("AttributeChanged"):
                    case ("NodeAdded"):
                        this.Type = DataType.DomChange;
                        ParseNodes(Msg);
                        break;
                    case ("PageLoaded"):
                        this.Type = DataType.PageLoad;
                        break;
                    case ("PageUnloading"):
                        this.Type = DataType.PageUnload;
                        break;
                    case ("XhrSendCalled"):
                        throw new Exception("XhrSendCalled");
                }
                ParseUrl();
            }

            void ParseUrl()
            {
                Request Req = new Request(this.Url);
                this.Origin = Req.BaseUrl;

                Values[DataLocation.Url] = DataValue.Parse(this.Url.Substring(Req.BaseUrl.Length));
            }

            public void ParseArguments(Newtonsoft.Json.Linq.JObject Msg)
            {
                if (Msg["value"]["args"] is Newtonsoft.Json.Linq.JObject)
                {
                    List<string> ArgList = new List<string>();
                    foreach(Newtonsoft.Json.Linq.JProperty Arg in (Msg["value"]["args"] as Newtonsoft.Json.Linq.JObject).Properties())
                    {
                        ArgList.Add((Arg.Value as Newtonsoft.Json.Linq.JValue).Value.ToString());
                    }
                    this.Arguments = ArgList.ToArray();
                }
            }

            public void ParseNodes(Newtonsoft.Json.Linq.JObject Msg)
            {
                if (this.SinkType == JavaScriptTracer.SinkType.Attribute)
                {
                    this.NodeName = Msg["value"].Value<string>("nodeName");
                    this.AttributeName = Msg["value"].Value<string>("attributeName");
                    this.AttributeValue = Msg["value"].Value<string>("attributeValue");
                }
                else if (this.SinkType == JavaScriptTracer.SinkType.Html)
                {
                    List<string> NodeList = new List<string>();
                    foreach (var Ele in Msg["value"])
                    {
                        try
                        {
                            NodeList.Add((Ele as Newtonsoft.Json.Linq.JValue).Value.ToString());
                        }
                        catch
                        {
                            NodeList.Add(Ele.ToString());
                        }
                    }
                    this.Nodes = NodeList.ToArray();
                }
            }

            /*
            public void ParseValues(Newtonsoft.Json.Linq.JObject Msg)
            {
                //Values[DataLocation.Url] = DataValue.Parse(this.Url);
             
                Request Req = new Request(this.Url);
                Values[DataLocation.UrlPath] = DataValue.Parse(Req.UrlPathParts);
                Values[DataLocation.QueryParameterValue] = new List<DataValue>();
                foreach(string Name in Req.Query.GetNames())
                {
                    Values[DataLocation.QueryParameterValue].AddRange(DataValue.Parse(Req.Query.GetAll(Name)));
                }
                if (this.Url.Contains("#"))
                {
                    string UrlHash = this.Url.Split(new char[]{'#'}, 2)[1];
                    if(UrlHash.Length > 0)
                    {
                        ParseUrlHash(UrlHash);
                    }
                }
                
            }
            */

            /*
            public void ParseUrlHash(string UrlHash)
            {
                this.Values[DataLocation.UrlHash] = DataValue.Parse(UrlHash);
            }
            */


            public List<string> ParseJson(Newtonsoft.Json.Linq.JToken Json)
            {
                List<string> JsonValues = new List<string>();
                ParseJsonValue(Json, JsonValues);
                return JsonValues;
            }

            void ParseJsonValue(Newtonsoft.Json.Linq.JToken Node, List<string> JsonValues)
            {
                if (Node.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    foreach (var Prop in (Node as Newtonsoft.Json.Linq.JObject).Properties())
                    {
                        ParseJsonValue(Prop, JsonValues);
                    }
                }
                else if (Node.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                {
                    foreach (var Ele in Node)
                    {
                        ParseJsonValue(Ele, JsonValues);
                    }
                }
                else if (Node is Newtonsoft.Json.Linq.JValue)
                {
                    JsonValues.Add((Node as Newtonsoft.Json.Linq.JValue).Value.ToString());
                }
            }

            public void AddRequestBodyValues(string Body, bool IsJson)
            {
                this.AjaxRequestBody = Body;
                if (IsJson)
                {
                    this.Values[DataLocation.JsonValueInRequestBody] = DataValue.Parse(ParseJson(Tools.ParseAsJson(Body)));
                }
                else
                {
                    this.Values[DataLocation.AjaxBody] = DataValue.Parse(Body);
                }
            }

            public void AddResponseBodyValues(string Body)
            {
                this.AjaxResponseBody = Body;
            }

            /*
            void AddBodyValues(string Body, DataLocation Loc)
            {
                if (Tools.IsJson(Body))
                {
                    this.Values[Loc] = DataValue.Parse(ParseJson(Tools.ParseAsJson(Body)));
                }
                else
                {
                    this.Values[Loc] = DataValue.Parse(Body);
                }
            }
            */

            public void AddAjaxRequest(Data AjaxRequestData)
            {
                if (this.Type == DataType.AjaxOpen && AjaxRequestData.Type == DataType.AjaxRequest)
                {
                    this.Type = DataType.AjaxCall;
                    this.AjaxRequest = AjaxRequestData;
                    AddTraceMessagesToList(this);
                }
                else
                {
                    throw new Exception("Trying to combined invalid types of Data objects");
                }
            }
            public void AddAjaxResponse(Data AjaxResponseData)
            {
                if (this.Type == DataType.AjaxCall && AjaxResponseData.Type == DataType.AjaxResponse)
                {
                    this.AjaxResponse = AjaxResponseData;
                    this.FindAjaxReflections();
                    AddTraceMessagesToList(this);
                }
                else
                {
                    throw new Exception("Trying to combined invalid types of Data objects");
                }
            }

            public void Analyze()
            {
                if (this.Type == DataType.MethodCall)
                {
                    foreach (string Arg in this.Arguments)
                    {
                        CheckAgainstUrlValues(Arg, this.SinkType);
                    }
                }
                else if (this.Type == DataType.DomChange)
                {
                    if (this.SinkType == JavaScriptTracer.SinkType.Html)
                    {
                        foreach (string HtmlPart in this.Nodes)
                        {
                            CheckAgainstUrlValues(HtmlPart, this.SinkType);
                        }
                    }
                    else if (this.SinkType == JavaScriptTracer.SinkType.Attribute)
                    {
                        CheckAgainstUrlValues(this.AttributeValue, JavaScriptTracer.SinkType.Attribute);
                    }
                }
                this.Analyzed = true;
            }

            public void Analyze(Data AjaxCallData)
            {
                /*
                 * Must take X-domain response data as a source and check t against sinks. Also if there are any reflections in same domain ajax response then they should be treated as source as well and checked
                */
                this.AjaxResponseAnalyzed = true;
            }

            public void CheckAgainstUrlValues(string SinkVal, SinkType ST)
            {
                foreach (DataValue Val in Values[DataLocation.Url])
                {
                    string Found = Val.IsFoundIn(SinkVal);
                    if (Found.Length > 0)
                    {
                        if (this.SinkType == JavaScriptTracer.SinkType.Attribute)
                        {
                            this.ObservationMsgs.Add(new ObservationMsg(this.NodeName, this.AttributeName, SinkVal, this.Url, Found));
                        }
                        else
                        {
                            this.ObservationMsgs.Add(new ObservationMsg(ST, Found, SinkVal, this.Url));
                        }
                    }
                }
            }

            void FindAjaxReflections()
            {
                if (this.AjaxResponse != null)
                {
                    if (this.AjaxRequest.Values.ContainsKey(DataLocation.Url))
                    {
                        foreach (DataValue Val in this.AjaxRequest.Values[DataLocation.Url])
                        {
                            string Found = Val.IsFoundIn(this.AjaxResponse.AjaxResponseBody);
                            if (Found.Length > 0)
                            {
                                if (!this.Reflections.ContainsKey(DataLocation.Url))
                                {
                                    this.Reflections[DataLocation.Url] = new List<string>();
                                }
                                this.Reflections[DataLocation.Url].Add(Found);
                            }
                        }
                    }
                    if (this.AjaxRequest.Values.ContainsKey(DataLocation.JsonValueInRequestBody))
                    {
                        foreach (DataValue Val in this.AjaxRequest.Values[DataLocation.JsonValueInRequestBody])
                        {
                            string Found = Val.IsFoundIn(this.AjaxResponse.AjaxResponseBody);
                            if (Found.Length > 0)
                            {
                                if (!this.Reflections.ContainsKey(DataLocation.AjaxBody))
                                {
                                    this.Reflections[DataLocation.AjaxBody] = new List<string>();
                                }
                                this.Reflections[DataLocation.AjaxBody].Add(Found);
                            }
                        }
                    }
                    if (this.AjaxRequest.Values.ContainsKey(DataLocation.AjaxBody))
                    {
                        foreach (DataValue Val in this.AjaxRequest.Values[DataLocation.AjaxBody])
                        {
                            string Found = Val.IsFoundIn(this.AjaxResponse.AjaxResponseBody);
                            if (Found.Length > 0)
                            {
                                if (!this.Reflections.ContainsKey(DataLocation.AjaxBody))
                                {
                                    this.Reflections[DataLocation.AjaxBody] = new List<string>();
                                }
                                this.Reflections[DataLocation.AjaxBody].Add(Found);
                            }
                        }
                    }
                }
            }
        }

        public class DataValue
        {
            public string Normal = "";
            public string UrlEncoded = "";
            public string UrlDecoded = "";
            public string JsonEncoded = "";

            static Regex MatchRegex = new Regex(@"[\w]{4,}", RegexOptions.Compiled);

            public DataValue(string Val)
            {
                this.Normal = Val;
                this.UrlEncoded = Tools.UrlEncode(Val);
                this.UrlDecoded = Tools.UrlDecode(Val);
                this.JsonEncoded = Tools.JsonEncode(Val);

                if (UrlEncoded == Val) UrlEncoded = "";
                if (UrlDecoded == Val) UrlDecoded = "";
                if (JsonEncoded == Val) JsonEncoded = "";

                if (UrlEncoded == UrlDecoded) UrlDecoded = "";
                if (UrlEncoded == JsonEncoded) JsonEncoded = "";
                if (UrlDecoded == JsonEncoded) JsonEncoded = "";
            }

            public string IsFoundIn(string SinkVal)
            {
                if (SinkVal.Contains(this.Normal))
                {
                    return this.Normal;
                }
                else if (this.UrlDecoded.Length > 0 && SinkVal.Contains(this.UrlDecoded))
                {
                    return this.UrlDecoded;
                }
                else if (this.UrlEncoded.Length > 0 && SinkVal.Contains(this.UrlEncoded))
                {
                    return this.UrlEncoded;
                }
                else if (this.JsonEncoded.Length > 0 && SinkVal.Contains(this.JsonEncoded))
                {
                    return this.JsonEncoded;
                }
                else
                {
                    return "";
                }
            }

            public ValueMatchType IsEqual(string Val)
            {
                if (this.Normal == Val)
                {
                    return ValueMatchType.Original;
                }
                else if (this.UrlDecoded == Val)
                {
                    return ValueMatchType.UrlDecoded;
                }
                else if (this.UrlEncoded == Val)
                {
                    return ValueMatchType.UrlEncoded;
                }
                else if (this.JsonEncoded == Val)
                {
                    return ValueMatchType.JsonEncoded;
                }
                else
                {
                    return ValueMatchType.None;
                }
            }

            public static List<DataValue> Parse(List<string> Input)
            {
                List<DataValue> Values = new List<DataValue>();
                foreach (string In in Input)
                {
                    Values.AddRange(Parse(In));
                }
                return Values;
            }

            public static List<DataValue> Parse(string Input)
            {
                List<DataValue> Values = new List<DataValue>();
                foreach (Match M in MatchRegex.Matches(Input))
                {
                    Values.Add(new DataValue(M.Value));
                }
                return Values;
            }
        }

        public enum ValueMatchType
        {
            Original,
            UrlEncoded,
            UrlDecoded,
            JsonEncoded,
            None
        }

        public enum DataType
        {
            MethodCall,
            DomChange,
            AjaxCall,
            //These three will be stored in the AjaxPartHolderDict and once combined will be converted to AjaxCall and moved to the DataDict
            AjaxOpen,
            AjaxRequest,
            AjaxResponse,
            //
            PageLoad,
            PageUnload
        }

        public enum SinkType
        {
            eval,
            setInterval,
            setTimeout,
            Function,
            XhrOpen,
            Attribute,
            Html
        }

        public enum DataLocation
        {
            Url,
            /*
            UrlPath,
            QueryParameterValue,
            UrlHash,
            */
            AjaxBody,
            JsonValueInRequestBody,
            JsonValueInResponseBody
        }

        public enum SourceType
        {
            CorResponse,//Response values in COR response
            Location,//values in location.href
            AjaxRequest//values in ajax request that were reflected back in response
        }

        public class JsTraceMsg
        {
            public string Url = "";
            public string Details = "";
            
            public string GetMsgHeading()
            {
                return string.Format("<i<b>>URL of the page:<i</b>> <i<cb>>{0}<i</cb>><i<br>><i<br>>", this.Url);
            }

            public static string HiglightMatch(string Match, string Full)
            {
                return Full.Replace(Match, string.Format("<i<h1>>{0}<i</h1>>", Match));
            }
        }

        public class ObservationMsg : JsTraceMsg
        {
            public string Time = "";
            public string Type = "";

            //public string Message = "";

            public ObservationMsg(SinkType Sink, string SourceValue, string SinkValue, string _Url)
            {
                this.Url = _Url;
                StringBuilder SB = new StringBuilder(this.GetMsgHeading());

                if (Sink == SinkType.XhrOpen)
                {
                    this.Type = "Url to XMLHttpRequest";
                    SB.Append(string.Format("The text <i<h>>{0}<i</h>> was found in the second argument of the <i<cb>>open<i</cb>> method of <i<cb>>XMLHttpRequest<i</cb>>. ", SourceValue));
                    SB.Append(string.Format("This text could have possibly come from the URL of the page:<i<br>><i<h>>{0}<i</h>>", HiglightMatch(SourceValue, _Url)));
                    
                    SB.Append(string.Format("<i<br>><i<br>>Full value of the argument is:<i<br>> <i<cb>>{0}<i</cb>>.", HiglightMatch(SourceValue, SinkValue)));

                    SB.Append("<i<br>><i<br>>If it is possible to control this parameter then you can perform CSRF attacks by entering relative URLs here and perform cross-origin attacks by entering absolute URLs");
                }
                else if (Sink == SinkType.eval || Sink == SinkType.setTimeout || Sink == SinkType.setInterval || Sink == SinkType.Function)
                {
                    this.Type = string.Format("Url to {0}", Sink);
                    SB.Append(string.Format("The text <i<h>>{0}<i</h>> was found in argument of <i<cb>>{1}<i</cb>> function. ", SourceValue, Sink));
                    SB.Append(string.Format("This text could have possibly come from the URL of the page:<i<br>><i<h>>{0}<i</h>>", HiglightMatch(SourceValue, _Url)));

                    SB.Append("<i<br>><i<br>>");
                    SB.Append(GetMessageForInjectionInJs(SourceValue, SinkValue));

                    SB.Append(string.Format("<i<br>><i<br>>Full value of the argument is:<i<br>> <i<cb>>{0}<i</cb>>.", HiglightMatch(SourceValue, SinkValue)));                    
                }
                else if (Sink == SinkType.Html)
                {
                    this.Type = "Url to HTML";
                    SB.Append(string.Format("The text <i<h>>{0}<i</h>> was found in was found in a piece of HTML that was added to the page.", SourceValue));
                    SB.Append(string.Format("This text could have possibly come from the URL of the page:<i<br>><i<h>>{0}<i</h>>", HiglightMatch(SourceValue, _Url)));

                    SB.Append(string.Format("<i<br>><i<br>>Full value of the HTML that was added is:<i<br>> <i<cb>>{0}<i</cb>>.", HiglightMatch(SourceValue, SinkValue)));
                }
                this.Details = SB.ToString();
            }

            public ObservationMsg(string TagName, string AttributeName, string AttributeValue, string _Url, string SourceValue)
            {
                this.Url = _Url;
                StringBuilder SB = new StringBuilder(this.GetMsgHeading());
                this.Type = "Url to Attribute";
                SB.Append(string.Format("The text <i<h>>{0}<i</h>> was found in was found in the new value of the <i<cb>>{1}<i</cb>> attribute of the <i<cb>>{2}<i</cb>> tag in the page.", SourceValue, AttributeName, TagName));
                SB.Append(string.Format("This text could have possibly come from the URL of the page:<i<br>><i<h>>{0}<i</h>>", HiglightMatch(SourceValue, _Url)));

                SB.Append(string.Format("<i<br>><i<br>>Full value of this attribute is:<i<br>> <i<cb>>{0}<i</cb>>.", HiglightMatch(SourceValue, AttributeValue)));

                this.Details = SB.ToString();
            }

            /*
            string GetMessageForInjectionInHtml(string Text, string Html)
            {
                HTML ParsedHtml = new HTML(Html);
                List<ReflectionContext> Contexts = ParsedHtml.GetContext(Text);
            }

            string GetMessageForHtmlContext(string Text, ReflectionContext Context)
            {
                switch(Context)
                {
                    case(ReflectionContext.UrlAttribute):
                        break;
                    case (ReflectionContext.EventAttribute):
                        break;
                    case (ReflectionContext.ElementName):
                        break;
                    case (ReflectionContext.AttributeName):
                        break;
                    case (ReflectionContext.AttributeValueWithSingleQuote):
                        break;
                    case (ReflectionContext.AttributeValueWithDoubleQuote):
                        break;
                    case (ReflectionContext.Html):
                        break;
                }
            }
            */

            string GetMessageForInjectionInJs(string Text, string Js)
            {
                List<string> Contexts = CodeContext.GetJavaScriptContext(Js, Text);
                if (Contexts.Count == 0) return "";
                List<string> UniqueContexts = new List<string>();
                foreach(string Context in Contexts)
                {
                    if(!UniqueContexts.Contains(Context))
                    {
                        UniqueContexts.Add(Context);
                    }
                }
                StringBuilder SB = new StringBuilder();
                SB.Append(string.Format("The text <i<co>>{0}<i<co>> appears in the JavaScript in {1} context(s).", Text, UniqueContexts.Count));
                if(UniqueContexts.Count == 1)
                {
                    SB.Append(string.Format("The context in which the text appears is <i<cb>>{0}<i</cb>>", CodeContext.ContextDescriptions[UniqueContexts[0]]));
                    SB.Append("<i<br>>");
                    SB.Append(GetMessageForJsContext(UniqueContexts[0], Text));
                }
                else if(UniqueContexts.Count > 1)
                {
                    SB.Append("<i<br>>The contexts are:<i<br>>");
                    foreach(string C in UniqueContexts)
                    {
                        SB.Append(string.Format("  <i<cb>>{0}<i</cb>>", CodeContext.ContextDescriptions[C]));
                        SB.Append("<i<br>>");
                        SB.Append(GetMessageForJsContext(C, Text));
                        SB.Append("<i<br>>"); SB.Append("<i<br>>");
                    }
                }
                return SB.ToString();
            }

            string GetMessageForJsContext(string Context, string Text)
            {
                StringBuilder SB = new StringBuilder();
                switch (Context)
                {
                    case (CodeContext.SingleQuotedStringContext):
                        SB.Append(string.Format("The text <i<co>>{0}<i</co>> appears between single quotes. In order to inject JavaScript that can be executed you would have to close out the single quotes.", Text));
                        SB.Append(string.Format("<i<br>>An example of such a payload is <i<cb>><i<b>>{0}' + alert(123) + 'x<i</b>><i</cb>>", Text));
                        break;
                    case (CodeContext.DoubleQuotedStringContext):
                        SB.Append(string.Format("The text <i<co>>{0}<i</co>> appears between double quotes. In order to inject JavaScript that can be executed you would have to close out the double quotes.", Text));
                        SB.Append(string.Format("<i<br>>An example of such a payload is <i<cb>><i<b>>{0}\" + alert(123) + \"x<i</b>><i</cb>>", Text));
                        break;
                    case (CodeContext.SingleLineCommentContext):
                        SB.Append(string.Format("The text <i<co>>{0}<i</co>> appears inside a single line comment. In order to inject JavaScript that can be executed you would have to come out of the the comment area by entering a new line character (\\r or \\n).", Text));
                        SB.Append(string.Format("<i<br>>An example of such a payload is <i<cb>><i<b>>{0}\\r\\n alert(123);//<i</b>><i</cb>>", Text));
                        break;
                    case (CodeContext.MultiLineCommentContext):
                        SB.Append(string.Format("The text <i<co>>{0}<i</co>> appears inside a multi-line comment. In order to inject JavaScript that can be executed you would have to come out of the the comment area by entering */.", Text));
                        SB.Append(string.Format("<i<br>>An example of such a payload is <i<cb>><i<b>>{0}*/ alert(123);/*<i</b>><i</cb>>", Text));
                        break;
                    case (CodeContext.NormalStringContext):
                        SB.Append(string.Format("The text <i<co>>{0}<i</co>> appears in executable section of JavaScript, i.e; It does not appear inside quoted strings or comments sections.", Text));
                        SB.Append("You should be able to directly enter some JavaScript function and get it to execute.");
                        break;
                }
                return SB.ToString();
            }
        }

        public class AjaxCallMsg : JsTraceMsg
        {
            public string Time = "";
            public string XhrId = "";
            public string SourceOrigin = "";
            public string TargetOrigin = "";
            public bool Reflection = false;

            public AjaxCallMsg(Data D)
            {
                this.XhrId = D.XhrId;
                this.SourceOrigin = D.Origin;
                this.Url = D.Url;

                if (D.AjaxRequest != null)
                {
                    if (D.AjaxRequest.Origin.Equals(D.Origin))
                    {
                        this.TargetOrigin = " - ";
                    }
                    else
                    {
                        this.TargetOrigin = D.AjaxRequest.Origin;
                    }
                }
                StringBuilder SB = new StringBuilder(this.GetMsgHeading());
                if (D.AjaxResponse != null)
                {
                    if (D.Reflections.Count > 0)
                    {
                        Reflection = true;
                        SB.Append("The following values from the Ajax request were found in the Ajax response as well. They were perhaps reflected back by the server:");
                        SB.Append("<i<br>>");
                        if (D.Reflections.ContainsKey(DataLocation.Url))
                        {
                            SB.Append("<i<b>>From the url of the Ajax request:<i</b>>");
                            SB.Append("<i<br>>");
                            foreach (string Ref in D.Reflections[DataLocation.Url])
                            {
                                SB.Append(string.Format("  <i<h>>{0}<i</h>>", Ref));
                                SB.Append("<i<br>>");
                            }
                        }
                        if (D.Reflections.ContainsKey(DataLocation.AjaxBody))
                        {
                            SB.Append("<i<b>>From the body of the Ajax request:<i</b>>");
                            SB.Append("<i<br>>");
                            foreach (string Ref in D.Reflections[DataLocation.AjaxBody])
                            {
                                SB.Append(string.Format("  <i<h>>{0}<i</h>>", Ref));
                                SB.Append("<i<br>>");
                            }
                        }
                    }
                    SB.Append("<i<br>><i<br>>");
                    SB.Append("The full Ajax response body content is:");
                    SB.Append("<i<br>>");
                    SB.Append(string.Format("<i<cb>>{0}<i</cb>>", D.AjaxResponse.AjaxResponseBody));
                }
                else
                {
                    if (D.Origin != D.AjaxRequest.Origin)
                    {
                        SB.Append(string.Format("A Cross Origin Ajax request was made from <i<h>>{0}<i</h>> to <i<h>>{1}<i</h>>", D.Origin, D.AjaxRequest.Url));
                    }
                    else
                    {
                        SB.Append(string.Format("An Ajax request was made to <i<h>>{0}<i</h>>", D.AjaxRequest.Url));
                    }
                    if(D.Arguments.Length > 1)
                    {
                        SB.Append("<i<br>>");
                        SB.Append(string.Format("The value of the url argument passed to the <i<cb>>open<i</cb>> method of <i<cb>>XMLHttRequest<i</cb>> object is <i<h>>{0}<i</h>>", D.Arguments[1]));
                    }
                    if(D.AjaxRequest.AjaxRequestBody.Length > 0)
                    {
                        SB.Append("<i<br>>");
                        SB.Append("The value of the argument passed to the <i<cb>>send<i</cb>> method of XMLHttRequest object is :");
                        SB.Append("<i<br>>");
                        try
                        {
                            SB.Append(string.Format("<i<cb>>{0}<i</cb>>", IronJint.Beautify(D.AjaxRequest.AjaxRequestBody)));
                        }
                        catch 
                        {
                            SB.Append(string.Format("<i<cb>>{0}<i</cb>>", D.AjaxRequest.AjaxRequestBody));
                        }
                    }
                }
                this.Details = SB.ToString();
            }
        }

        public class NativeMethodCallMsg : JsTraceMsg
        {
            public string Time = "";
            public string Origin = "";
            public string Method = "";

            public NativeMethodCallMsg(Data D)
            {
                this.Origin = D.Origin;
                this.Url = D.Url;
                this.Method = D.SinkType.ToString();

                StringBuilder SB = new StringBuilder(this.GetMsgHeading());
                SB.Append(string.Format("<i<h>>{0}<i</h>> method was called with the following arguments:", D.SinkType));
                SB.Append("<i<br>>");
                for (int i = 0; i < D.Arguments.Length; i++)
                {
                    SB.Append(string.Format("<i<br>> <i<cg>><i<b>>Argument {0}:<i</b>><i</cg>> <i<br>>", i + 1));
                    try
                    {
                        SB.Append(string.Format("  <i<cb>>{0}<i</cb>>", IronJint.Beautify(D.Arguments[i])));
                    }
                    catch
                    {
                        SB.Append(D.Arguments[i]);
                    }
                    SB.Append("<i<br>>");
                }
                this.Details = SB.ToString();
            }
        }

        public class DomChangeMsg : JsTraceMsg
        {
            public string Time = "";
            public string Origin = "";
            public string Action = "";

            public DomChangeMsg(Data D)
            {
                this.Origin = D.Origin;
                this.Url = D.Url;

                StringBuilder SB = new StringBuilder(this.GetMsgHeading());
                if (D.SinkType == SinkType.Attribute)
                {
                    this.Action = "Attribute Changed";
                    SB.Append(string.Format("The value of the <i<h>>{0}<i</h>> attribute of the <i<h>>{1}<i</h>> tag was changed to <i<h>>{2}<i</h>>.", D.AttributeName, D.NodeName, D.AttributeValue));
                }
                else if (D.SinkType == SinkType.Html)
                {
                    this.Action = "HTML Node Added/Modified";
                    SB.Append("The following HTML nodes were added to the page: ");
                    SB.Append("<i<br>>");
                    for (int i = 0; i < D.Nodes.Length; i++)
                    {
                        SB.Append(string.Format("<i<br>> <i<cg>><i<b>>HTML Node {0}:<i</b>><i</cg>> <i<br>>", i + 1));
                        SB.Append(string.Format("<i<cb>>{0}<i</cb>>", D.Nodes[i]));
                        SB.Append("<i<br>>");
                    }
                }

                this.Details = SB.ToString();
            }
        }
    }
}
