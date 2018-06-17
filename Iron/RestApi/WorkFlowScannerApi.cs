using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.RestApi
{
    internal class WorkFlowScannerApi
    {
        internal static void LoadApis()
        {
            ApiCallHandler.AddCoreHandler("workflow/start", InformStartOfWorkFlow);
            ApiCallHandler.AddCoreHandler("workflow/end", InformEndOfWorkFlow);
            ApiCallHandler.AddCoreHandler("workflow/start_scan", StartScanOfWorkFlows);
            ApiCallHandler.AddCoreHandler("workflow/get_scan_status", GetStatusOfWorkFlowScans);
            //ApiCallHandler.AddCoreHandler("workflow/get_hosts", InformEndOfWorkFlow);
        }

        internal static void InformStartOfWorkFlow(Request Req, Response Res)
        {
            if (Req.Query.Has("name"))
            {
                Workflow.WorkflowScanner.MarkWorkFlowStart(Req.Query.Get("name"));
            }
            else
            {
                Workflow.WorkflowScanner.MarkWorkFlowStart("");
            }
            Res.BodyString = "OK";
        }

        internal static void InformEndOfWorkFlow(Request Req, Response Res)
        {
            Workflow.WorkflowScanner.MarkWorkFlowEnd();
            Res.BodyString = "OK";
        }

        internal static void StartScanOfWorkFlows(Request Req, Response Res)
        {
            Workflow.WorkflowScanner.StartWorkFlowScans();
            Res.BodyString = "OK";
        }

        internal static void GetStatusOfWorkFlowScans(Request Req, Response Res)
        {
            if (Workflow.WorkflowScanner.IsScanInProgress())
            {
                Res.BodyString = "RUNNING";
            }
            else
            {
                Res.BodyString = "DONE";
            }
        }
    }
}
