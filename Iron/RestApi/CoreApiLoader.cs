using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.RestApi
{
    internal class CoreApiLoader
    {
        internal static void LoadCoreCustomApiRegistration()
        {
            PassiveCrawlerApi.LoadApis();
            WorkFlowScannerApi.LoadApis();
            JavaScriptTracerApi.LoadApis();
        }
    }
}
