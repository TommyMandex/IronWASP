using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace IronWASP.Workflow
{
    public class Workflow
    {
        List<int> intLogIds = new List<int>();
        Analysis.LogAssociations intWorkflowAssociations = null;
        WorkflowSource intFlowSource = WorkflowSource.RestApi;
        WorkflowType intFlowType = WorkflowType.Normal;
        string intName = "";
        Dictionary<string, string> intInfo = new Dictionary<string, string>();
        string intUserAgent = "";
        string intLogSource = "";
        int intWorkflowId = 0;

        public Workflow(List<int> _LogIds, string _LogSource, string _UserAgent, WorkflowSource _FlowSource, WorkflowType _FlowType)
        {
            this.intLogIds = _LogIds;
            this.intLogSource = _LogSource;
            this.intUserAgent = _UserAgent;
            this.intFlowSource = _FlowSource;
            this.intFlowType = _FlowType;
        }

        internal string GetLogIdsAsString()
        {
            StringBuilder SB = new StringBuilder();
            foreach (int LogId in intLogIds)
            {
                SB.Append(LogId);
                SB.Append(", ");
            }
            return SB.ToString().TrimEnd().TrimEnd(',');
        }

        internal static List<int> ParseLogIdString(string LogIdsStr)
        {
            List<int> Ids = new List<int>();
            foreach (string IdStr in LogIdsStr.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    Ids.Add(Int32.Parse(IdStr.Trim()));
                }
                catch { }
            }
            return Ids;
        }

        public void SetInfoJson(string _InfoJson)
        {
            this.intInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(_InfoJson);
        }

        public void SetInfo(Dictionary<string, string> _Info)
        {
            if (_Info == null)
            {
                this.intInfo = new Dictionary<string, string>();
            }
            else
            {
                this.intInfo = _Info;
            }
        }

        public void SetInfo(string Key, string Value)
        {
            this.intInfo[Key] = Value;
        }

        public void SetId(int _WorkflowId)
        {
            this.intWorkflowId = _WorkflowId;
        }

        public void SetName(string _Name)
        {
            this.intName = _Name;
        }

        public int Id
        {
            get
            {
                return intWorkflowId;
            }
        }

        public string Name
        {
            get
            {
                return intName;
            }
        }

        public Dictionary<string, string> Info
        {
            get
            {
                return intInfo;
            }
        }

        public string InfoJson
        {
            get
            {
                return JsonConvert.SerializeObject(intInfo);
            }
        }

        public WorkflowSource FlowSource
        {
            get
            {
                return intFlowSource;
            }
        }

        public WorkflowType FlowType
        {
            get
            {
                return intFlowType;
            }
        }

        public string UserAgent
        {
            get
            {
                return intUserAgent;
            }
        }

        public string LogSource
        {
            get
            {
                return intLogSource;
            }
        }

        public Analysis.LogAssociations WorkflowAssociations
        {
            get
            {
                if (this.intWorkflowAssociations == null)
                {
                    this.CalculateLogAssociations();
                }
                return this.WorkflowAssociations;
            }
        }

        public void CalculateLogAssociations()
        {
            Analysis.LogAnalyzer LogAna = new Analysis.LogAnalyzer();
            Dictionary<string, Analysis.LogAssociations> Result = LogAna.Analyze(this.intLogIds, this.LogSource);
            if (Result.ContainsKey(this.UserAgent))
            {
                this.intWorkflowAssociations = Result[this.UserAgent];
            }
            else
            {
                this.intWorkflowAssociations = null;
            }
        }
    }

    public enum WorkflowType
    {
        Normal,
        Login
    }

    public enum WorkflowSource
    {
        RestApi,
        User
    }
}
