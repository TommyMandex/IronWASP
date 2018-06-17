using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.RestApi
{
    internal class JavaScriptTracerApi
    {
        internal static void LoadApis()
        {
            ApiCallHandler.AddCoreHandler("jstracer/log_msg", LogMessageFromBrowser);
        }
        
        internal static void LogMessageFromBrowser(Request Req, Response Res)
        {
            if (Req.Url.Equals(string.Format("{0}jstracer/log_msg", ApiCallHandler.CoreApiUrlStart), StringComparison.OrdinalIgnoreCase))
            {
                Res.BodyString = "OK";
                JavaScriptTracer.LogTraceMessage(Req.BodyString);
            }
        }
    }
}
