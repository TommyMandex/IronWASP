using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IronWASP.Workflow
{
    internal class WorkflowScanner
    {
        static List<int[]> WorkFlows = new List<int[]>();
        static int CurrentWorkFlowStartIndex = -1;
        static string CurrentWorkFlowName = "";
        static Thread ScannerThread = null;

        static List<string> Hosts = new List<string>();
        static List<string> AllowedHosts = new List<string>();

        internal static List<int[]> GetWorkflowMarkersList()
        {
            return new List<int[]>(WorkFlows.ToArray());
        }

        internal static List<string> GetWorkflowHostsList()
        {
            return new List<string>(Hosts.ToArray());
        }

        internal static void SetAllowedWorkflowHosts(List<string> _AllowedHosts)
        {
            foreach (string AH in _AllowedHosts)
            {
                if (!AllowedHosts.Contains(AH)) AllowedHosts.Add(AH);
            }
        }

        internal static void CheckWorkflowRequest(Session Sess)
        {
            if (CurrentWorkFlowStartIndex < 1) return;
            
            lock (Hosts)
            {
                if (!Hosts.Contains(Sess.Request.BaseUrl))
                {
                    Hosts.Add(Sess.Request.BaseUrl);
                    WorkflowScannerWindow.UpdateWorkflowHostEntryInUi(Sess.Request.BaseUrl);
                }
            }
        }

        internal static void CheckWorkflowResponse(Session Sess)
        {
            return;
        }

        internal static void MarkWorkFlowStart(string Name)
        {
            CurrentWorkFlowStartIndex = Config.LastProxyLogId + 1;
            CurrentWorkFlowName = Name;
        }

        internal static void MarkWorkFlowEnd()
        {
            if (CurrentWorkFlowStartIndex > 0)
            {
                if (Config.LastProxyLogId >= CurrentWorkFlowStartIndex)
                {
                    WorkFlows.Add(new int[] { CurrentWorkFlowStartIndex, Config.LastProxyLogId });
                    WorkflowScannerWindow.UpdateWorkflowEntryInUi(WorkFlows[WorkFlows.Count - 1], CurrentWorkFlowName);
                }
                CurrentWorkFlowStartIndex = -1;
                CurrentWorkFlowName = "";
            }
        }

        internal static void StartWorkFlowScans()
        {
            try
            {
                ScannerThread.Abort();
            }
            catch { }
            ScannerThread = new Thread(DoScan);
            ScannerThread.Start();
        }


        internal static void StopWorkFlowScans()
        {
            try
            {
                ScannerThread.Abort();
            }
            catch { }
            WorkflowScannerWindow.UpdateScanStatusInUi(false, "Scan stopped");
        }

        internal static bool IsScanInProgress()
        {
            try
            {
                if (ScannerThread.IsAlive) return true;
            }
            catch { }
            return false;
        }

        static void DoScan()
        {
            try
            {
                List<int[]> WorkFlowMarkers = new List<int[]>();
                List<string> HostsToScan = new List<string>();
                lock (WorkFlows)
                {
                    foreach (int[] Marker in WorkFlows)
                    {
                        WorkFlowMarkers.Add(Marker);
                    }
                    WorkFlows.Clear();
                }
                lock (AllowedHosts)
                {
                    foreach (string Host in AllowedHosts)
                    {
                        HostsToScan.Add(Host);
                    }
                    AllowedHosts.Clear();
                }
                foreach (int[] Marker in WorkFlowMarkers)
                {
                    Analysis.LogAnalyzer Analyzer = new Analysis.LogAnalyzer();
                    Dictionary<string, Analysis.LogAssociations> AssociationsDict = Analyzer.Analyze(Marker[0], Marker[1], "Proxy");
                    foreach (string Ua in AssociationsDict.Keys)
                    {
                        ScanAssociation(AssociationsDict[Ua], HostsToScan, Marker);
                    }
                }
                WorkflowScannerWindow.UpdateScanStatusInUi(false, "Scan complete");
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp) 
            {
                IronException.Report("Error scanning workflows", Exp);
            }
        }

        static void ScanAssociation(Analysis.LogAssociations Association, List<string> HostsToScan, int[] Marker)
        {
            if (Association.NonIgnorableCount > 0)
            {
                int Index = 0;
                foreach (int Id in Association.LogIds)
                {
                    Analysis.LogAssociation Asso = Association.GetAssociation(Id);
                    if (!Asso.IsIgnorable && HostsToScan.Contains(Asso.DestinationLog.Request.BaseUrl))
                    {
                        Scanner S = new Scanner(Asso.DestinationLog.Request);
                        if(S.BaseRequest.File.Length == 0 && S.BaseRequest.Query.Count == 0 && S.BaseRequest.UrlPathParts.Count > 1)
                        {
                            S.InjectUrl();
                        }
                        S.InjectQuery();
                        if ( S.BaseRequest.BodyType == BodyFormatType.Soap || 
                             S.BaseRequest.BodyType == BodyFormatType.Json ||
                             S.BaseRequest.BodyType == BodyFormatType.Multipart ||
                             S.BaseRequest.BodyType == BodyFormatType.Xml )
                        {
                            S.BodyFormat = FormatPlugin.Get(S.BaseRequest.BodyType);
                        }
                        S.InjectBody();
                        S.CheckAll();
                        if(S.InjectionPointsCount > 0)
                        {
                            S.WorkFlowLogAssociations = Association;
                            S.IndexOfRequestToScanInWorkFlowLogAssociations = Index;
                            WorkflowScannerWindow.UpdateScanStatusInUi(true, string.Format("Scanning Request no.{0} in workflow between logs {1}-{2}", Index, Marker[0], Marker[1]));
                            S.Scan();
                        }
                        Index++;
                    }
                }
            }
        }
    }
}
