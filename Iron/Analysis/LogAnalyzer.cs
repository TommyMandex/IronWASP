using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.Analysis
{
    public class LogAnalyzer
    {

        Dictionary<string, Dictionary<int, Session>> LogsByUa = new Dictionary<string, Dictionary<int, Session>>();

        List<LogAssociationType> UninterestingAssociationTypes = new List<LogAssociationType>()
            {
                LogAssociationType.ExternalCss,
                LogAssociationType.ExternalImage,
                LogAssociationType.ExternalScript
            };

        public Dictionary<string, LogAssociations> Analyze(int StartId, int EndId, string Source)
        {
            List<int> LogIds = new List<int>();
            for (int i = StartId; i <= EndId; i++)
            {
                LogIds.Add(i);
            }
            LoadLogs(LogIds, Source);
            return MapLogs();
        }

        public Dictionary<string, LogAssociations> Analyze(List<int> _LogIds, string Source)
        {
            LoadLogs(_LogIds, Source);
            return MapLogs();
        }

        public LogAssociations AnalyzeSessionsFromSameUa(List<Session> Sessions)
        {
            Dictionary<int, Session> SessionsDict = new Dictionary<int, Session>();
            foreach (Session Sess in Sessions)
            {
                SessionsDict[Sess.LogId] = Sess;
            }
            List<LogAssociation> LogAssos = MapLogs(SessionsDict);
            return new LogAssociations(LogAssos);
        }

        void LoadLogs(List<int> _LogIds, string Source)
        {
            LogsByUa.Clear();

            List<Session> LoginLogs = new List<Session>();
            List<Session> LogoutLogs = new List<Session>();
            foreach (int i in _LogIds)
            {
                Session Sess = Session.FromLog(i, Source);

                string Ua = "";
                if (Sess.Request.Headers.Has("User-Agent")) Ua = Sess.Request.Headers.Get("User-Agent");
                if (!LogsByUa.ContainsKey(Ua)) LogsByUa[Ua] = new Dictionary<int, Session>();
                LogsByUa[Ua][Sess.LogId] = Sess;
            }
        }

        public Dictionary<string, LogAssociations> MapLogs()
        {
            Dictionary<string, LogAssociations> LogAssociationsByUa = new Dictionary<string, LogAssociations>();
            //StringBuilder SB = new StringBuilder();
            foreach (string Ua in LogsByUa.Keys)
            {
                List<LogAssociation> Associations = MapLogs(LogsByUa[Ua]);
                LogAssociationsByUa[Ua] = new LogAssociations(Associations);
                //SB.Append("Associations for: "); SB.AppendLine(Ua);
                //SB.AppendLine("----------------------");
                //foreach (LogAssociation Asso in Associations)
                //{
                //    SB.AppendLine(Asso.ToString());
                //}
                //SB.AppendLine("----------------------");
            }
            //return SB.ToString();
            return LogAssociationsByUa;
        }

        List<LogAssociation> MapLogs(Dictionary<int, Session> Sessions)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            
            Dictionary<int, List<int>> SessionUrlToRefererUrlMappings = new Dictionary<int, List<int>>();//Store the ID of the session and the list of ids of sessions which have a referrer header matching the session request url.
            Dictionary<int, List<int>> SessionRootUrlToRefererUrlMappings = new Dictionary<int, List<int>>();//Store the ID of the session and the list of ids of sessions which have a referrer header matching the session request url's root.
            Dictionary<int, List<int>> SessionUrlToRefererUrlMismatchMappings = new Dictionary<int, List<int>>();//Store the ID of the session and the list of ids of sessions which don't have a referrer header matching the session request url but appear after the session request in log order.
            Dictionary<int, List<int>> SessionUrlToRefererUrlMissingMappings = new Dictionary<int, List<int>>();//Store the ID of the session and the list of ids of sessions which don't have a referrer header at all but appear after the session request in log order.

            Dictionary<string, List<int>> SessionsWithSameUrls = new Dictionary<string, List<int>>();//Store the Request Url and the ID of the sessions that have this Url
            Dictionary<string, List<int>> SessionsWithSameRootUrls = new Dictionary<string, List<int>>();//Store the Request Url and the ID of the sessions that have this root Url
            
            List<int> LogIds = new List<int>(Sessions.Keys);
            LogIds.Sort();
            for (int i = 0; i < LogIds.Count; i++)
            {
                Session Sess = Sessions[LogIds[i]];
                if(Sess.Response == null) continue;
                string FullUrl = Sess.Request.FullUrl;
                string RootUrl = Sess.Request.BaseUrl;
                
                bool IncludeThisSessForRootUrlMatch = false;                
                List<string> MetaRefereOrigin = Sess.Response.Html.GetDecodedValues("meta", "name", "referrer", "content");// <meta name="referrer" content="origin">
                if (MetaRefereOrigin.Count > 0) IncludeThisSessForRootUrlMatch = true;

                for (int ii = i + 1; ii < LogIds.Count; ii++)
                {
                    Session RefSess = Sessions[LogIds[ii]];
                    
                    if (RefSess.Request.Headers.Has("Referer"))
                    {
                        if(FullUrl.Equals(RefSess.Request.Headers.Get("Referer")))
                        {
                            if (!SessionsWithSameUrls.ContainsKey(FullUrl)) SessionsWithSameUrls[FullUrl] = new List<int>();
                            if (SessionsWithSameUrls.ContainsKey(FullUrl) && !SessionsWithSameUrls[FullUrl].Contains(Sess.LogId)) SessionsWithSameUrls[FullUrl].Add(Sess.LogId);
                            
                            if (!SessionUrlToRefererUrlMappings.ContainsKey(Sess.LogId)) SessionUrlToRefererUrlMappings[Sess.LogId] = new List<int>();
                            if (!SessionUrlToRefererUrlMappings[Sess.LogId].Contains(RefSess.LogId)) SessionUrlToRefererUrlMappings[Sess.LogId].Add(RefSess.LogId);
                        }
                        else if (IncludeThisSessForRootUrlMatch && RootUrl.Equals(RefSess.Request.Headers.Get("Referer")))
                        {
                            if (!SessionsWithSameRootUrls.ContainsKey(RootUrl)) SessionsWithSameRootUrls[RootUrl] = new List<int>();
                            if (SessionsWithSameRootUrls.ContainsKey(RootUrl) && !SessionsWithSameRootUrls[RootUrl].Contains(Sess.LogId)) SessionsWithSameRootUrls[RootUrl].Add(Sess.LogId);

                            if (!SessionRootUrlToRefererUrlMappings.ContainsKey(Sess.LogId)) SessionRootUrlToRefererUrlMappings[Sess.LogId] = new List<int>();
                            if (!SessionRootUrlToRefererUrlMappings[Sess.LogId].Contains(RefSess.LogId)) SessionRootUrlToRefererUrlMappings[Sess.LogId].Add(RefSess.LogId);
                        }
                        else
                        {
                            if (!SessionUrlToRefererUrlMismatchMappings.ContainsKey(Sess.LogId)) SessionUrlToRefererUrlMismatchMappings[Sess.LogId] = new List<int>();
                            if (!SessionUrlToRefererUrlMismatchMappings[Sess.LogId].Contains(RefSess.LogId)) SessionUrlToRefererUrlMismatchMappings[Sess.LogId].Add(RefSess.LogId);
                        }
                    }
                    else
                    {
                        if (!SessionUrlToRefererUrlMissingMappings.ContainsKey(Sess.LogId)) SessionUrlToRefererUrlMissingMappings[Sess.LogId] = new List<int>();
                        if (!SessionUrlToRefererUrlMissingMappings[Sess.LogId].Contains(RefSess.LogId)) SessionUrlToRefererUrlMissingMappings[Sess.LogId].Add(RefSess.LogId);
                    }
                }
            }

            List<int> SessionsThatHaveAnUniqueUrlAmongReferHeaderMatches = new List<int>();
            List<int> SessionsThatDontHaveAnUniqueUrlAmongReferHeaderMatches = new List<int>();

            List<int> SessionsThatHaveAnUniqueRootUrlAmongReferHeaderMatches = new List<int>();
            List<int> SessionsThatDontHaveAnUniqueRootUrlAmongReferHeaderMatches = new List<int>();

            foreach (string FullUrl in SessionsWithSameUrls.Keys)
            {
                if (SessionsWithSameUrls[FullUrl].Count == 1)
                {
                    if (!SessionsThatHaveAnUniqueUrlAmongReferHeaderMatches.Contains(SessionsWithSameUrls[FullUrl][0]))
                    {
                        SessionsThatHaveAnUniqueUrlAmongReferHeaderMatches.Add(SessionsWithSameUrls[FullUrl][0]);
                    }
                }
                else if (SessionsWithSameUrls[FullUrl].Count > 1)
                {
                    foreach (int LogId in SessionsWithSameUrls[FullUrl])
                    {
                        if (!SessionsThatDontHaveAnUniqueUrlAmongReferHeaderMatches.Contains(LogId))
                        {
                            SessionsThatDontHaveAnUniqueUrlAmongReferHeaderMatches.Add(LogId);
                        }
                    }
                }
            }

            foreach (string FullUrl in SessionsWithSameRootUrls.Keys)
            {
                if (SessionsWithSameRootUrls[FullUrl].Count == 1)
                {
                    if (!SessionsThatHaveAnUniqueRootUrlAmongReferHeaderMatches.Contains(SessionsWithSameRootUrls[FullUrl][0]))
                    {
                        SessionsThatHaveAnUniqueRootUrlAmongReferHeaderMatches.Add(SessionsWithSameRootUrls[FullUrl][0]);
                    }
                }
                else if (SessionsWithSameRootUrls[FullUrl].Count > 1)
                {
                    foreach (int LogId in SessionsWithSameRootUrls[FullUrl])
                    {
                        if (!SessionsThatDontHaveAnUniqueRootUrlAmongReferHeaderMatches.Contains(LogId))
                        {
                            SessionsThatDontHaveAnUniqueRootUrlAmongReferHeaderMatches.Add(LogId);
                        }
                    }
                }
            }

            foreach (int LogId in SessionsThatHaveAnUniqueUrlAmongReferHeaderMatches)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsAssociatedByReferrerToMain = new List<Session>();
                foreach(int RefLogId in SessionUrlToRefererUrlMappings[LogId])
                {
                    SessionsAssociatedByReferrerToMain.Add(Sessions[RefLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsAssociatedByReferrerToMain, RefererAssociationType.FullUrlAndUnique));
            }

            foreach (int LogId in SessionsThatDontHaveAnUniqueUrlAmongReferHeaderMatches)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsAssociatedByReferrerToMain = new List<Session>();
                foreach(int RefLogId in SessionUrlToRefererUrlMappings[LogId])
                {
                    SessionsAssociatedByReferrerToMain.Add(Sessions[RefLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsAssociatedByReferrerToMain, RefererAssociationType.FullUrlButNotUnique));
            }

            foreach (int LogId in SessionsThatHaveAnUniqueRootUrlAmongReferHeaderMatches)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsAssociatedByReferrerToMain = new List<Session>();
                foreach (int RefLogId in SessionRootUrlToRefererUrlMappings[LogId])
                {
                    SessionsAssociatedByReferrerToMain.Add(Sessions[RefLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsAssociatedByReferrerToMain, RefererAssociationType.RootOnlyAndUnique));
            }

            foreach (int LogId in SessionsThatDontHaveAnUniqueRootUrlAmongReferHeaderMatches)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsAssociatedByReferrerToMain = new List<Session>();
                foreach (int RefLogId in SessionRootUrlToRefererUrlMappings[LogId])
                {
                    SessionsAssociatedByReferrerToMain.Add(Sessions[RefLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsAssociatedByReferrerToMain, RefererAssociationType.RootOnlyButNotUnique));
            }

            foreach (int LogId in SessionUrlToRefererUrlMismatchMappings.Keys)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder = new List<Session>();
                foreach (int LaterLogId in SessionUrlToRefererUrlMismatchMappings[LogId])
                {
                    SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder.Add(Sessions[LaterLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder, RefererAssociationType.Mismatch));
            }

            foreach (int LogId in SessionUrlToRefererUrlMissingMappings.Keys)
            {
                Session MainSess = Sessions[LogId];
                List<Session> SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder = new List<Session>();
                foreach (int LaterLogId in SessionUrlToRefererUrlMissingMappings[LogId])
                {
                    SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder.Add(Sessions[LaterLogId]);
                }
                Associations.AddRange(FindAssociations(MainSess, SessionsWithoutReferMatchToMainButAppearingAfterMainInLogOrder, RefererAssociationType.ReferMissing));
            }
            return FinalizeAssociationsByPriority(Sessions, Associations);
        }

        List<LogAssociation> FinalizeAssociationsByPriority(Dictionary<int, Session> Sessions, List<LogAssociation> Associations)
        {
            List<LogAssociation> FinalizedAssociations = new List<LogAssociation>();

            List<int> LogIds = new List<int>(Sessions.Keys);
            LogIds.Sort();
            foreach(int LogId in LogIds)
            {
                LogAssociation Asso = GetBestAssociation(LogId, Associations);
                if (Asso == null)
                {
                    FinalizedAssociations.Add(new LogAssociation(LogAssociationType.UnAssociated, RefererAssociationType.None, IronHtml.UrlInHtmlMatch.None, LogAssociationMatchLevel.Other, null, Sessions[LogId]));
                }
                else
                {
                    if (!UninterestingAssociationTypes.Contains(Asso.AssociationType)) FinalizedAssociations.Add(Asso);
                }
            }
            return FinalizedAssociations;
        }

        LogAssociation GetBestAssociation(int LogId, List<LogAssociation> Associations)
        {
            int ClosestSourceLogId = 0;
            LogAssociation BestAssociation = null;

            foreach (LogAssociation Asso in Associations)
            {
                if (Asso.DestinationLog.LogId != LogId) continue;
                if (Asso.SourceLog.LogId > ClosestSourceLogId)
                {
                    ClosestSourceLogId = Asso.SourceLog.LogId;
                    BestAssociation = null;
                }
                if (Asso.SourceLog.LogId == ClosestSourceLogId)
                {
                    if (BestAssociation == null || Asso.AssociationScore > BestAssociation.AssociationScore)
                    {
                        BestAssociation = Asso;
                    }
                }
            }
            return BestAssociation;
        }

        #region AssociationFindingCore
        List<LogAssociation> FindAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();

            List<int> AssociatedIds = new List<int>();

            Associations.AddRange(FindRedirectAssociations(MainLog, SessionList, RefAssoType));
            Associations.AddRange(FindScriptSourceAssociations(MainLog, SessionList, RefAssoType));
            Associations.AddRange(FindStyleSourceAssociations(MainLog, SessionList, RefAssoType));
            Associations.AddRange(FindImgSourceAssociations(MainLog, SessionList, RefAssoType));
            Associations.AddRange(FindLinkClickAssociations(MainLog, SessionList, RefAssoType));
            Associations.AddRange(FindFormSubmitAssociations(MainLog, SessionList, RefAssoType));
            
            return Associations;
        }

        List<LogAssociation> FindRedirectAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return null;

            if (MainLog.Response.IsRedirect)
            {
                if (MainLog.Response.Headers.Has("Location"))
                {
                    string RedirUrl = MainLog.Response.Headers.Get("Location").Trim();
                    try
                    {
                        Request RedirReq = new Request(RedirUrl);
                        foreach (Session Sess in SessionList)
                        {
                            if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                            if (Sess.Request.FullUrl.Equals(RedirReq.FullUrl) && Sess.Response != null)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.Redirect, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.Other, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                    catch
                    {
                        Request RedirReq = MainLog.Request.GetRedirect(MainLog.Response);
                        foreach (Session Sess in SessionList)
                        {
                            if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                            if (Sess.Request.FullUrl.Equals(RedirReq.FullUrl) && Sess.Response != null)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.Redirect, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.Other, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
            }
            return Associations;
        }

        List<LogAssociation> FindScriptSourceAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return Associations;

            //Match script urls with absolute match and response content type match
            foreach (string ScriptSrc in MainLog.Response.Html.GetDecodedValues("script", "src"))
            {
                try
                {
                    Request ScriptReq = new Request(ScriptSrc.Trim());
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(ScriptReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsJavaScript)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsJavaScript)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalScript, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalScript, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
                catch 
                {
                    Request ScriptReq = new Request(MainLog.Request.RelativeUrlToAbsoluteUrl(ScriptSrc.Trim()));
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(ScriptReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsJavaScript)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsJavaScript)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalScript, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalScript, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
            }
            return Associations;
        }

        List<LogAssociation> FindStyleSourceAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return Associations;
            
            //Match css urls with absolute match and response content type match
            foreach (string CssSrc in MainLog.Response.Html.GetDecodedValues("link", "href"))
            {
                try
                {
                    Request CssReq = new Request(CssSrc.Trim());
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(CssReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsCss)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsCss)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalCss, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalCss, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
                catch 
                {
                    Request CssReq = new Request(MainLog.Request.RelativeUrlToAbsoluteUrl(CssSrc.Trim()));
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(CssReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsCss)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsCss)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalCss, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalCss, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
            }
            return Associations;
        }

        List<LogAssociation> FindImgSourceAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return Associations;

            //Match img urls with absolute match and response content type match
            foreach (string ImgSrc in MainLog.Response.Html.GetDecodedValues("img", "src"))
            {
                try
                {
                    Request ImgReq = new Request(ImgSrc.Trim());
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                       if (Sess.Request.FullUrl.Equals(ImgReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsBinary)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsBinary)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalImage, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalImage, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
                catch 
                {
                    Request ImgReq = new Request(MainLog.Request.RelativeUrlToAbsoluteUrl(ImgSrc.Trim()));
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(ImgReq.FullUrl) && Sess.Response != null)// && Sess.Response.Code == 304 || Sess.Response.IsBinary)
                        {
                            if (Sess.Response.Code == 304 || Sess.Response.IsBinary)
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalImage, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchAndResponseType, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.ExternalImage, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
            }
            return Associations;
        }

        List<LogAssociation> FindLinkClickAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return Associations;
            
            //Match link urls with absolute match
            foreach (string LinkUrl in MainLog.Response.Html.GetDecodedValues("a", "href"))
            {
                try
                {
                    Request LinkReq = new Request(LinkUrl.Trim());
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(LinkReq.FullUrl) && Sess.Response != null)
                        {
                            LogAssociation LogAsso = new LogAssociation(LogAssociationType.LinkClick, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                            Associations.Add(LogAsso);
                        }
                    }
                }
                catch 
                {
                    Request LinkReq = new Request(MainLog.Request.RelativeUrlToAbsoluteUrl(LinkUrl.Trim()));
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (Sess.Request.FullUrl.Equals(LinkReq.FullUrl) && Sess.Response != null)
                        {
                            LogAssociation LogAsso = new LogAssociation(LogAssociationType.LinkClick, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.UrlMatchOnly, MainLog, Sess);
                            Associations.Add(LogAsso);
                        }
                    }
                }
            }
            return Associations;
        }

        List<LogAssociation> FindFormSubmitAssociations(Session MainLog, List<Session> SessionList, RefererAssociationType RefAssoType)
        {
            List<LogAssociation> Associations = new List<LogAssociation>();
            if (MainLog.Response == null) return Associations;

            //Match form submission with absolute url match and absolute field/values match
            foreach (IronHtml.FormElement FormEle in MainLog.Response.Html.GetFormElements())
            {
                try
                {
                    Request FormReq = new Request(FormEle.Action);
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.IsNormal) continue;
                        if (Sess.Request.FullUrl.Equals(FormReq.FullUrl) && Sess.Request.Method.Equals(FormEle.Method, StringComparison.OrdinalIgnoreCase) && Sess.Response != null)
                        {
                            if (FormEle.DoAllInputFieldValuesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.FormNamesAndValues, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else if (FormEle.DoHiddenInputFieldValuesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.FormNamesAndHiddenValuesOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else if (FormEle.DoInputFieldNamesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullAbsolute, LogAssociationMatchLevel.FormNamesOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
                catch 
                {
                    Request FormReq = new Request(MainLog.Request.RelativeUrlToAbsoluteUrl(FormEle.Action));
                    foreach (Session Sess in SessionList)
                    {
                        if (!Sess.Request.IsNormal) continue;
                        if (Sess.Request.FullUrl.Equals(FormReq.FullUrl) && Sess.Request.Method.Equals(FormEle.Method, StringComparison.OrdinalIgnoreCase) && Sess.Response != null)
                        {
                            if (FormEle.DoAllInputFieldValuesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.FormNamesAndValues, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else if (FormEle.DoHiddenInputFieldValuesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.FormNamesAndHiddenValuesOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                            else if (FormEle.DoInputFieldNamesMatchRequest(Sess.Request))
                            {
                                LogAssociation LogAsso = new LogAssociation(LogAssociationType.FormSubmission, RefAssoType, IronHtml.UrlInHtmlMatch.FullRelative, LogAssociationMatchLevel.FormNamesOnly, MainLog, Sess);
                                Associations.Add(LogAsso);
                            }
                        }
                    }
                }
            }
            return Associations;
        }
        #endregion
    }
}
