using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP
{
    public class ApiCallHandler
    {
        static int ProxyLogRangeStart = 0;
        static int ProxyLogRangeEnd = 0;

        public const string CoreApiUrlStart = "/ironwasp/api/core/";
        public const string CustomApiUrlStart = "/ironwasp/api/custom/";

        static Dictionary<string, ApiCallHandlerMethod> CoreCallHandlers = new Dictionary<string, ApiCallHandlerMethod>(StringComparer.OrdinalIgnoreCase);
        static Dictionary<string, ApiCallHandlerMethod> CustomCallHandlers = new Dictionary<string, ApiCallHandlerMethod>(StringComparer.OrdinalIgnoreCase);
        
        internal static void Handle(Fiddler.Session Sess)
        {
            Session IrSe = new Session(Sess);
            Response Res = new Response("HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nOK");
            Res.Headers.Set("Access-Control-Allow-Origin", "*");

            string ApiUrl = IrSe.Request.UrlPath.Substring(14);
            bool MatchFound = false;
            if (ApiUrl.StartsWith("core/", StringComparison.OrdinalIgnoreCase))
            {
                ApiUrl = ApiUrl.Substring(5);
                if (CoreCallHandlers.ContainsKey(ApiUrl))
                {
                    MatchFound = true;
                    try
                    {
                        CoreCallHandlers[ApiUrl](IrSe.Request, Res);
                    }
                    catch(Exception Exp)
                    {
                        Res.BodyString = string.Format("Error executing API call.\r\nError details:\r\n{0}\r\n{1}", Exp.Message, Exp.StackTrace);
                    }
                }
            }
            else if (ApiUrl.StartsWith("custom/", StringComparison.OrdinalIgnoreCase))
            {
                ApiUrl = ApiUrl.Substring(7);
                if (CustomCallHandlers.ContainsKey(ApiUrl))
                {
                    MatchFound = true;
                    try
                    {
                        CustomCallHandlers[ApiUrl](IrSe.Request, Res);
                    }
                    catch (Exception Exp)
                    {
                        Res.BodyString = string.Format("Error executing API call.\r\nError details:\r\n{0}\r\n{1}", Exp.Message, Exp.StackTrace);
                    }
                }
            }
            if (!MatchFound)
            {
                StringBuilder SB = new StringBuilder();
                SB.AppendLine("No API call handler registered for this URL");
                SB.AppendLine();
                SB.AppendLine("The following are the registered URLs:");
                foreach (string Url in CoreCallHandlers.Keys)
                {
                    SB.Append(CoreApiUrlStart); SB.AppendLine(Url);
                }
                Res.BodyString = SB.ToString();
                foreach (string Url in CustomCallHandlers.Keys)
                {
                    SB.Append(CustomApiUrlStart); SB.AppendLine(Url);
                }
                Res.BodyString = SB.ToString();
            }


            //switch (ApiUrl)
            //{
            //    case("LogRangeStart"):
            //        ProxyLogRangeStart = Config.LastProxyLogId;
            //        ProxyLogRangeEnd = 0;
            //        break;
            //    case ("LogRangeEnd"):
            //        ProxyLogRangeEnd = Config.LastProxyLogId;
            //        break;
            //    case ("ScanLogRange"):
            //        break;
            //    default:
            //        if (CustomCallHandlers.ContainsKey(ApiUrl))
            //        {
            //            CustomCallHandlers[ApiUrl](IrSe.Request, Res);
            //        }
            //        else
            //        {
            //        }
            //        break;
            //}
            Sess.utilCreateResponseAndBypassServer();
            Sess.oResponse.headers.AssignFromString(Res.GetHeadersAsString());
            Sess.responseBodyBytes = Res.BodyArray;
        }

        //Python:
        //ApiCallHandler.AddHandler("123", pyer)

        //Ruby:
        //lll = lambda{|req, res| rrer(req, res)}
        //ApiCallHandler.AddHandler("123", lll)

        public delegate void ApiCallHandlerMethod(Request Req, Response Res);

        internal static void AddCoreHandler(string Url, ApiCallHandlerMethod Method)
        {
            if (Url.StartsWith("/"))
            {
                Url = Url.TrimStart('/');
            }
            if (CoreCallHandlers.ContainsKey(Url))
            {
                throw new Exception("This API URL is already registered, try another one.");
            }
            else
            {
                lock (CoreCallHandlers)
                {
                    CoreCallHandlers[Url] = Method;
                }
            }
        }

        public static void AddHandler(string Url, ApiCallHandlerMethod Method)
        {
            if (Url.StartsWith("/"))
            {
                Url = Url.TrimStart('/');
            }
            if (CustomCallHandlers.ContainsKey(Url))
            {
                throw new Exception("This API URL is already registered, try another one.");
            }
            else
            {
                lock (CustomCallHandlers)
                {
                    CustomCallHandlers[Url] = Method;
                }
            }
        }
    }
}
