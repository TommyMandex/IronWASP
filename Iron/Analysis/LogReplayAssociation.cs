using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.Analysis
{
    public class LogReplayAssociation
    {
        public LogAssociation OriginalAssociation = null;
        public LogAssociation ReplayAssociation = null;
        //public CookieStore Cookies = new CookieStore();

        public LogReplayAssociation(LogAssociation OriAsso)
        {
            this.OriginalAssociation = OriAsso;
        }
        public LogReplayAssociation(LogAssociation OriAsso, LogAssociation RepAsso)
        {
            this.OriginalAssociation = OriAsso;
            this.ReplayAssociation = RepAsso;
        }

        public override string ToString()
        {
            return ReplayAssociation.ToString();
        }
    }

    public class LogReplayAssociations
    {
        Dictionary<int, LogReplayAssociation> Associations = new Dictionary<int, LogReplayAssociation>();
        Dictionary<int, LogReplayAssociation> AssociationsByOriginalId = new Dictionary<int, LogReplayAssociation>();
        public CookieStore Cookies = new CookieStore();

        public LogReplayAssociations(List<LogReplayAssociation> LogAssoList, CookieStore CookSt)
        {
            this.Cookies = CookSt;
            foreach (LogReplayAssociation Asso in LogAssoList)
            {
                Associations[Asso.ReplayAssociation.DestinationLog.LogId] = Asso;
                if (Asso.OriginalAssociation != null)
                {
                    AssociationsByOriginalId[Asso.OriginalAssociation.DestinationLog.LogId] = Asso;
                }
            }
        }
        //public int FirstLogId
        //{
        //    get
        //    {
        //        return LogIds[0];
        //    }
        //}
        //public int LastLogId
        //{
        //    get
        //    {
        //        return LogIds[LogIds.Count - 1];
        //    }
        //}
        public int Count
        {
            get
            {
                return Associations.Count;
            }
        }
        //public int FirstOriginalLogId
        //{
        //    get
        //    {
        //        return LogIds[0];
        //    }
        //}
        //public int LastOriginalLogId
        //{
        //    get
        //    {
        //        return LogIds[LogIds.Count - 1];
        //    }
        //}
        public int OriginalCount
        {
            get
            {
                return AssociationsByOriginalId.Count;
            }
        }
        public List<int> LogIds
        {
            get
            {
                List<int> UnsortedLogIds = new List<int>(Associations.Keys);
                UnsortedLogIds.Sort();
                return UnsortedLogIds;
            }
        }
        public List<int> OriginalLogIds
        {
            get
            {
                List<int> UnsortedLogIds = new List<int>(AssociationsByOriginalId.Keys);
                UnsortedLogIds.Sort();
                return UnsortedLogIds;
            }
        }
        public bool HasLog(int LogId)
        {
            return Associations.ContainsKey(LogId);
        }
        public bool HasOriginalLog(int LogId)
        {
            return AssociationsByOriginalId.ContainsKey(LogId);
        }
        public LogReplayAssociation GetAssociation(int LogId)
        {
            return Associations[LogId];
        }
        public LogReplayAssociation GetAssociationByOriginalId(int LogId)
        {
            return AssociationsByOriginalId[LogId];
        }
        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            //SB.Append("Associations for: "); SB.AppendLine(Ua);
            SB.AppendLine("----------------------");
            //foreach (LogAssociation Asso in Associations)
            foreach (int LogId in LogIds)
            {
                SB.AppendLine(Associations[LogId].ToString());
            }
            SB.AppendLine("----------------------");
            return SB.ToString();
        }
    }
}
