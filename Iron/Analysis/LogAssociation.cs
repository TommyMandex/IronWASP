using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.Analysis
{
    public class LogAssociation
    {
        public LogAssociationType AssociationType = LogAssociationType.Unknown;
        public RefererAssociationType RefererMatch = RefererAssociationType.None;
        public LogAssociationMatchLevel MatchLevel = LogAssociationMatchLevel.Other;
        public IronHtml.UrlInHtmlMatch UrlMatch = IronHtml.UrlInHtmlMatch.None;
        
        public Session SourceLog = null;
        public Session DestinationLog = null;

        static List<string> IgnorableExtensions = new List<string>() { "jpg", "gif", "ico", "jpeg", "png", "woff", "swf", "css", "js", "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx" };

        public LogAssociation(LogAssociationType AssoType, RefererAssociationType RefAssoType, IronHtml.UrlInHtmlMatch UrlMatchLevel, LogAssociationMatchLevel MtcLvl, Session SrcLog, Session DestLog)
        {
            AssociationType = AssoType;
            RefererMatch = RefAssoType;
            UrlMatch = UrlMatchLevel;
            MatchLevel = MtcLvl;
            SourceLog = SrcLog;
            DestinationLog = DestLog;
        }

        public int AssociationScore
        {
            get
            {
                //Redirect 3000
                //Form submission 2000
                //Link click 1000
                //others 0
                int AssociationTypeWeight = 1000;
                int RefererMatchWeight = 100;
                int MatchLevelWeight = 10;
                int UrlMatchWeight = 1;

                int Score = 0;
                if (AssociationType == LogAssociationType.UnAssociated) return 0;
                switch (AssociationType)
                {
                    case(LogAssociationType.Redirect):
                        Score += 3 * AssociationTypeWeight;
                        break;
                    case (LogAssociationType.FormSubmission):
                        Score += 2 * AssociationTypeWeight;
                        break;
                    case (LogAssociationType.LinkClick):
                        Score += 1 * AssociationTypeWeight;
                        break;
                    default:
                        Score += 0 * AssociationTypeWeight;
                        break;
                }
                switch (RefererMatch)
                {
                    case(RefererAssociationType.FullUrlAndUnique):
                        Score += 9 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.FullUrlButNotUnique):
                        Score += 8 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.RootOnlyAndUnique):
                        Score += 7 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.RootOnlyButNotUnique):
                        Score += 6 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.ReferMissing):
                        Score += 5 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.None):
                        Score += 4 * RefererMatchWeight;
                        break;
                    case (RefererAssociationType.Mismatch):
                        Score += 3 * RefererMatchWeight;
                        break;
                    default:
                        Score += 2 * RefererMatchWeight;
                        break;
                }
                switch (MatchLevel)
                {
                    case(LogAssociationMatchLevel.FormNamesAndValues):
                        Score += 6 * MatchLevelWeight;
                        break;
                    case (LogAssociationMatchLevel.FormNamesAndHiddenValuesOnly):
                        Score += 5 * MatchLevelWeight;
                        break;
                    case (LogAssociationMatchLevel.UrlMatchAndResponseType):
                        Score += 4 * MatchLevelWeight;
                        break;
                    case (LogAssociationMatchLevel.FormNamesOnly):
                        Score += 3 * MatchLevelWeight;
                        break;
                    case (LogAssociationMatchLevel.UrlMatchOnly):
                        Score += 2 * MatchLevelWeight;
                        break;
                    case (LogAssociationMatchLevel.Other):
                        Score += 1 * MatchLevelWeight;
                        break;
                }
                switch (UrlMatch)
                {
                    case(IronHtml.UrlInHtmlMatch.FullAbsolute):
                        Score += 3 * UrlMatchWeight;
                        break;
                    case (IronHtml.UrlInHtmlMatch.FullRelative):
                        Score += 2 * UrlMatchWeight;
                        break;
                    case (IronHtml.UrlInHtmlMatch.None):
                        Score += 1 * UrlMatchWeight;
                        break;
                }
                return Score;
            }
        }

        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            if (SourceLog != null)
            {
                SB.Append(SourceLog.LogId); SB.Append(" | "); SB.Append(SourceLog.Request.FullUrl);
            }
            SB.AppendLine();
            SB.AppendLine(AssociationType.ToString());
            SB.Append("Url: "); SB.AppendLine(UrlMatch.ToString());
            SB.Append("Level: "); SB.AppendLine(MatchLevel.ToString());
            SB.Append("Referer: "); SB.AppendLine(RefererMatch.ToString());
            if (DestinationLog != null)
            {
                SB.Append(DestinationLog.LogId); SB.Append(" | ");SB.Append(DestinationLog.Request.FullUrl);
            }
            SB.AppendLine();
            return SB.ToString();
        }
        public bool IsIgnorable
        {
            get
            {
                if(IgnorableExtensions.Contains(this.DestinationLog.Request.File.ToLower()))
                {
                    return true;
                }
                if (this.DestinationLog.Response != null)
                {
                    if (this.DestinationLog.Response.Code == 304) return true;
                    if (this.DestinationLog.Response.IsBinary) return true;
                }
                return false;
            }
        }
        public bool DoesHaveParameterValues(List<string> ParameterValues)
        {
            if (this.DestinationLog != null)
            {
                foreach (string ParameterValue in ParameterValues)
                {
                    if (!(this.DestinationLog.Request.Query.HasValue(ParameterValue) || this.DestinationLog.Request.Body.HasValue(ParameterValue)))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }

    public class LogAssociations
    {
        Dictionary<int, LogAssociation> Associations = new Dictionary<int,LogAssociation>();

        public LogAssociations(List<LogAssociation> LogAssoList)
        {
            foreach(LogAssociation Asso in LogAssoList)
            {
                Associations[Asso.DestinationLog.LogId] = Asso;
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
        //        return LogIds[LogIds.Count-1];
        //    }
        //}

        public int NonIgnorableCount
        {
            get
            {
                int NonIgnorableCounter = 0;
                foreach (int AssoId in Associations.Keys)
                {
                    if (!Associations[AssoId].IsIgnorable) NonIgnorableCounter++;
                }
                return NonIgnorableCounter;
            }
        }

        public int Count
        {
            get
            {
                return Associations.Count;
            }
        }
        public List<int> LogIds
        {
            get
            {
                List<int> LogIds = new List<int>(Associations.Keys);
                LogIds.Sort();
                return LogIds;
            }
        }
        public bool HasLog(int LogId)
        {
            return Associations.ContainsKey(LogId);
        }
        public LogAssociation GetAssociation(int LogId)
        {
            return Associations[LogId];
        }
        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            //SB.Append("Associations for: "); SB.AppendLine(Ua);
            SB.AppendLine("----------------------");
            //foreach (LogAssociation Asso in Associations)
            foreach(int LogId in LogIds)
            {
                SB.AppendLine(Associations[LogId].ToString());
            }
            SB.AppendLine("----------------------");
            return SB.ToString();
        }
        public List<Analysis.LogAssociation> GetAssociationsWithParameterValues(List<string> ParameterValues)
        {
            List<Analysis.LogAssociation> Results = new List<LogAssociation>();
            foreach (int i in this.LogIds)
            {
                Analysis.LogAssociation Asso = this.GetAssociation(i);
                if (Asso.DoesHaveParameterValues(ParameterValues))
                {
                    Results.Add(Asso);
                }
            }
            return Results;
        }

        public Analysis.LogAssociation GetFirstAssociationWithParameterValues(List<string> ParameterValues)
        {
            return GetFirstOrLastAssociationWithParameterValues(ParameterValues, true);
        }
        public Analysis.LogAssociation GetLastAssociationWithParameterValues(List<string> ParameterValues)
        {
            return GetFirstOrLastAssociationWithParameterValues(ParameterValues, false);
        }
        public Analysis.LogAssociation GetFirstOrLastAssociationWithParameterValues(List<string> ParameterValues, bool First)
        {
            List<Analysis.LogAssociation> Assos = GetAssociationsWithParameterValues(ParameterValues);
            Dictionary<int, Analysis.LogAssociation> AssosDict = new Dictionary<int, LogAssociation>();
            
            foreach (Analysis.LogAssociation Asso in Assos)
            {
                AssosDict[Asso.DestinationLog.LogId] = Asso;
            }

            List<int> LogIdsWithParamterValues = new List<int>(AssosDict.Keys);
            LogIdsWithParamterValues.Sort();
            if (LogIdsWithParamterValues.Count > 0)
            {
                if (First)
                {
                    return AssosDict[LogIdsWithParamterValues[0]];
                }
                else
                {
                    return AssosDict[LogIdsWithParamterValues[LogIdsWithParamterValues.Count - 1]];
                }
            }
            return null;
        }
    }

    public enum LogAssociationType
    {
        LinkClick,
        FormSubmission,
        ExternalScript,
        ExternalCss,
        ExternalImage,
        Redirect,
        Unknown,
        UnAssociated
    }

    public enum LogAssociationMatchLevel
    {
        UrlMatchOnly,
        UrlMatchAndResponseType,
        FormNamesAndValues,
        FormNamesAndHiddenValuesOnly,
        FormNamesOnly,
        Other
    }

    public enum RefererAssociationType
    {
        FullUrlAndUnique,
        RootOnlyAndUnique,
        FullUrlButNotUnique,
        RootOnlyButNotUnique,
        None,
        Mismatch,
        ReferMissing
    }
}
