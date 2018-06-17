using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.Analysis
{
    public class LogReplayer
    {
        public delegate Request UpdateRequestBeforeReplaying(Request Req, LogAssociation CorrespondingOriginalLogAssociation);

        LogAssociations Associations = null;
        CookieStore Cookies = new CookieStore();
        UpdateRequestBeforeReplaying ReqUpdater = null;

        int CurrentPlayIndex = 0;
        Request CurrentRequestBeingPlayed = null;
        LogReplayAssociation CurrentAssociationBeingPlayed = null;
        LogReplayAssociation PreviousAssociationToOneBeingPlayed = null;

        List<LogReplayAssociation> PlayAssociations = new List<LogReplayAssociation>();

        public LogReplayer(LogAssociations _Associations)
        {
            this.Associations = _Associations;
        }
        public LogReplayer(LogAssociations _Associations, CookieStore _Cookies)
        {
            this.Associations = _Associations;
            this.Cookies = _Cookies;
        }
        public LogReplayer(LogAssociations _Associations, UpdateRequestBeforeReplaying _ReqUpdater)
        {
            this.Associations = _Associations;
            this.ReqUpdater = _ReqUpdater;
        }
        public LogReplayer(LogAssociations _Associations, UpdateRequestBeforeReplaying _ReqUpdater, CookieStore _Cookies)
        {
            this.Associations = _Associations;
            this.ReqUpdater = _ReqUpdater;
            this.Cookies = _Cookies;
        }

        public bool HasMoreStepsToPlay()
        {
            if (CurrentPlayIndex < Associations.NonIgnorableCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public LogReplayAssociations Play()
        {
            this.PlayAssociations.Clear();
            while (HasMoreStepsToPlay())
            {
                PrepareStepForPlay();
                PlayStep();
            }
            return new LogReplayAssociations(this.PlayAssociations, this.Cookies);
        }

        public Request PrepareStepForPlay()
        {
            if (PlayAssociations.Count == 0 && Associations.NonIgnorableCount > 0)
            {
                foreach (int LogId in Associations.LogIds)
                {
                    LogAssociation Asso = Associations.GetAssociation(LogId);
                    if (!Asso.IsIgnorable)
                    {
                        LogReplayAssociation PlayAsso = new LogReplayAssociation(Asso);
                        PlayAssociations.Add(PlayAsso);
                    }
                }
            }

            //LogReplayAssociation PlayAsso = PlayAssociations[CurrentPlayIndex];

            CurrentAssociationBeingPlayed = PlayAssociations[CurrentPlayIndex];

            //LogReplayAssociation PreviousAsso = null;

            PreviousAssociationToOneBeingPlayed = null;
            foreach (LogReplayAssociation PrePlayAsso in PlayAssociations)
            {
                if (PrePlayAsso.OriginalAssociation != null
                    && PrePlayAsso.OriginalAssociation.DestinationLog != null
                    && CurrentAssociationBeingPlayed.OriginalAssociation.SourceLog != null
                    && CurrentAssociationBeingPlayed.OriginalAssociation.SourceLog.LogId == PrePlayAsso.OriginalAssociation.DestinationLog.LogId)
                {
                    PreviousAssociationToOneBeingPlayed = PrePlayAsso;
                }
            }
            //Request Req = GetRequest(CurrentAsso, PreviousAsso);
            CurrentRequestBeingPlayed = GetRequest(CurrentAssociationBeingPlayed, PreviousAssociationToOneBeingPlayed);
            
            CurrentRequestBeingPlayed.Cookie.RemoveAll();
            CurrentRequestBeingPlayed.SetCookie(Cookies);
            CurrentRequestBeingPlayed.SetSource("LogReplayer");
            if (ReqUpdater != null)
            {
                CurrentRequestBeingPlayed = ReqUpdater(CurrentRequestBeingPlayed, CurrentAssociationBeingPlayed.OriginalAssociation);
            }

            return CurrentRequestBeingPlayed;
        }

        public Response PlayStep()
        {
            Response Res = CurrentRequestBeingPlayed.Send();
            Cookies.Add(CurrentRequestBeingPlayed, Res);
            if (PreviousAssociationToOneBeingPlayed != null && PreviousAssociationToOneBeingPlayed.ReplayAssociation != null && PreviousAssociationToOneBeingPlayed.ReplayAssociation.DestinationLog != null)
            {
                CurrentAssociationBeingPlayed.ReplayAssociation = new LogAssociation(LogAssociationType.Unknown, RefererAssociationType.None, IronHtml.UrlInHtmlMatch.None, LogAssociationMatchLevel.Other, PreviousAssociationToOneBeingPlayed.ReplayAssociation.DestinationLog, new Session(CurrentRequestBeingPlayed, Res));
            }
            else
            {
                CurrentAssociationBeingPlayed.ReplayAssociation = new LogAssociation(LogAssociationType.Unknown, RefererAssociationType.None, IronHtml.UrlInHtmlMatch.None, LogAssociationMatchLevel.Other, null, new Session(CurrentRequestBeingPlayed, Res));
            }
            CurrentPlayIndex++;
            return Res;
        }

        /*
        public LogReplayAssociations Play(LogAssociations Associations)
        {
            return Play(Associations, null, new CookieStore());
        }

        public LogReplayAssociations Play(LogAssociations Associations, CookieStore Cookies)
        {
            return Play(Associations, null, Cookies);
        }

        public LogReplayAssociations Play(LogAssociations Associations, UpdateRequestBeforeReplaying ReqUpdater)
        {
            return Play(Associations, ReqUpdater, new CookieStore());
        }
        public LogReplayAssociations Play(LogAssociations Associations, UpdateRequestBeforeReplaying ReqUpdater, CookieStore Cookies)
        {
            List<LogReplayAssociation> PlayAssociations = new List<LogReplayAssociation>();
            foreach (int LogId in Associations.LogIds)
            {
                LogAssociation Asso = Associations.GetAssociation(LogId);
                if (!Asso.IsIgnorable)
                {
                    LogReplayAssociation PlayAsso = new LogReplayAssociation(Asso);
                    PlayAssociations.Add(PlayAsso);
                }
            }
            foreach (LogReplayAssociation PlayAsso in PlayAssociations)
            {
                LogReplayAssociation PreviousAsso = null;
                foreach (LogReplayAssociation PrePlayAsso in PlayAssociations)
                {
                    if (PrePlayAsso.OriginalAssociation != null
                        && PrePlayAsso.OriginalAssociation.DestinationLog != null
                        && PlayAsso.OriginalAssociation.SourceLog != null
                        && PlayAsso.OriginalAssociation.SourceLog.LogId == PrePlayAsso.OriginalAssociation.DestinationLog.LogId)
                    {
                        PreviousAsso = PrePlayAsso;
                    }
                }
                PlayAssociation(PlayAsso, PreviousAsso, Cookies, ReqUpdater);
            }
            return new LogReplayAssociations(PlayAssociations, Cookies);
        }
        */
 
        //LogReplayAssociation PlayAssociation(LogAssociation CurrentAsso, List<LogReplayAssociation> PlayAssociations, LogAssociations Associations, CookieStore Cookies)
        /*
        void PlayAssociation(LogReplayAssociation CurrentAsso, LogReplayAssociation PreviousAsso, CookieStore Cookies, UpdateRequestBeforeReplaying ReqUpdater)
        {
            Request Req = GetRequest(CurrentAsso, PreviousAsso);
            Req.Cookie.RemoveAll();
            Req.SetCookie(Cookies);
            Req.SetSource("LogReplayer");
            if (ReqUpdater != null)
            {
                Req = ReqUpdater(Req, CurrentAsso.OriginalAssociation);
            }
            Response Res = Req.Send();
            Cookies.Add(Req, Res);
            if (PreviousAsso != null && PreviousAsso.ReplayAssociation != null && PreviousAsso.ReplayAssociation.DestinationLog != null)
            {
                CurrentAsso.ReplayAssociation = new LogAssociation(LogAssociationType.Unknown, RefererAssociationType.None, IronHtml.UrlInHtmlMatch.None, LogAssociationMatchLevel.Other, PreviousAsso.ReplayAssociation.DestinationLog, new Session(Req, Res));
            }
            else
            {
                CurrentAsso.ReplayAssociation = new LogAssociation(LogAssociationType.Unknown, RefererAssociationType.None, IronHtml.UrlInHtmlMatch.None, LogAssociationMatchLevel.Other, null, new Session(Req, Res));
            }
        }
        */
 
        Request GetRequest(LogReplayAssociation PlayAsso, LogReplayAssociation PreviousPlayAsso)
        {
            LogAssociation OriAsso = PlayAsso.OriginalAssociation;
            if (OriAsso.AssociationType == LogAssociationType.UnAssociated || OriAsso.AssociationType == LogAssociationType.Unknown)
            {
                return OriAsso.DestinationLog.Request.GetClone();
            }
            if (OriAsso.SourceLog == null || PreviousPlayAsso == null || PreviousPlayAsso.ReplayAssociation == null || PreviousPlayAsso.ReplayAssociation.DestinationLog == null)
            {
                return OriAsso.DestinationLog.Request.GetClone();
            }

            Request Req = GetRequest(OriAsso, PreviousPlayAsso.ReplayAssociation.DestinationLog);
            if (Req == null)
            {
                //do something here
                //check in nearnest logs for redirect based variations otherwise just send the old request
            }
            else
            {
                return Req;
            }

            return OriAsso.DestinationLog.Request.GetClone();
        }

        Request GetRequest(LogAssociation CurrentAsso, Session PlaySess)
        {
            switch (CurrentAsso.AssociationType)
            {
                case(LogAssociationType.LinkClick):
                    return GetLinkClick(CurrentAsso, PlaySess);
                case(LogAssociationType.FormSubmission):
                    return GetFormSubmission(CurrentAsso, PlaySess);
                case(LogAssociationType.Redirect):
                    return GetRedirect(CurrentAsso, PlaySess);
                default:
                    return null;
            }
        }

        Request GetLinkClick(LogAssociation CurrentAsso, Session PlaySess)
        {
            Response Res = PlaySess.Response;
            List<IronHtml.LinkElement> BestMatches = new List<IronHtml.LinkElement>();
            List<IronHtml.LinkElement> SecondBestMatches = new List<IronHtml.LinkElement>();

            IronHtml.LinkElementCollection NewLinkElements = new IronHtml.LinkElementCollection(Res.Html.GetLinkElements());

            Request ReqToMatchAgainst = CurrentAsso.DestinationLog.Request;

            //List<IronHtml.LinkElement> LinkElements = CurrentAsso.DestinationLog.Response.Html.GetLinkElements();
            foreach (IronHtml.LinkElement LinkEle in CurrentAsso.SourceLog.Response.Html.GetLinkElements())
            {
                if (LinkEle.IsAbsoluteHref)
                {
                    if (LinkEle.Href.Equals(ReqToMatchAgainst.FullUrl))
                    {
                        BestMatches.Add(LinkEle);
                    }
                }
                else if(!LinkEle.IsJavaScriptHref)
                {
                    if (LinkEle.GetLinkClick(CurrentAsso.SourceLog.Request).FullUrl.Equals(ReqToMatchAgainst.FullUrl))
                    {
                        SecondBestMatches.Add(LinkEle);
                    }
                }
            }

            foreach (List<IronHtml.LinkElement> Matches in new List<List<IronHtml.LinkElement>>() { BestMatches, SecondBestMatches })
            {
                //check by link id
                foreach (IronHtml.LinkElement LinkEle in Matches)
                {
                    if (LinkEle.HasId)
                    {
                        List<IronHtml.Element> MatchingClicks = NewLinkElements.GetElementsWithId(LinkEle.Id);
                        if (MatchingClicks.Count > 0)
                        {
                            return ((IronHtml.LinkElement)MatchingClicks[0]).GetLinkClick(PlaySess.Request);
                        }
                    }
                }
                //check by link name
                foreach (IronHtml.LinkElement LinkEle in Matches)
                {
                    if (LinkEle.HasName)
                    {
                        List<IronHtml.Element> MatchingClicks = NewLinkElements.GetElementsWithName(LinkEle.Name);
                        if (MatchingClicks.Count > 0)
                        {
                            return ((IronHtml.LinkElement)MatchingClicks[0]).GetLinkClick(PlaySess.Request);
                        }
                    }
                }
                //check by inner text
                foreach (IronHtml.LinkElement LinkEle in Matches)
                {
                    if (LinkEle.InnerText.Trim().Length > 0)
                    {
                        List<IronHtml.Element> MatchingClicks = NewLinkElements.GetElementsWithInnerText(LinkEle.InnerText);
                        if (MatchingClicks.Count > 0)
                        {
                            return ((IronHtml.LinkElement)MatchingClicks[0]).GetLinkClick(PlaySess.Request);
                        }
                    }
                }
            }

            foreach (List<IronHtml.LinkElement> Matches in new List<List<IronHtml.LinkElement>>() { BestMatches, SecondBestMatches })
            {
                foreach (IronHtml.LinkElement LinkEle in Matches)
                {
                    if (NewLinkElements.Count > LinkEle.Index)
                    {
                        foreach (IronHtml.LinkElement NewLinkEle in NewLinkElements.GetElements())
                        {
                            if (NewLinkEle.Index == LinkEle.Index)
                            {
                                return NewLinkEle.GetLinkClick(PlaySess.Request);
                            }
                        }
                    }
                }
            }

            //foreach (List<IronHtml.LinkElement> Matches in new List<List<IronHtml.LinkElement>>() { BestMatches, SecondBestMatches })
            //{
            //    //check by class name
            //    foreach (IronHtml.LinkElement LinkEle in Matches)
            //    {
            //        if (LinkEle.HasClass)
            //        {
            //            List<IronHtml.Element> MatchingClicks = NewLinkClicks.GetElementsWithClass(LinkEle.Class);
            //            if (MatchingClicks.Count > 0)
            //            {
            //                return ((IronHtml.LinkElement)MatchingClicks[0]).GetLinkClick(PlaySess.Request);
            //            }
            //        }
            //    }
            //}
            return null;
        }

        Request GetFormSubmission(LogAssociation CurrentAsso, Session PlaySess)
        {
            Response Res = PlaySess.Response;
            List<IronHtml.FormElement> BestMatches = new List<IronHtml.FormElement>();
            List<IronHtml.FormElement> SecondBestMatches = new List<IronHtml.FormElement>();

            IronHtml.FormElementCollection NewFormElements = new IronHtml.FormElementCollection(Res.Html.GetFormElements());

            Request ReqToMatchAgainst = CurrentAsso.DestinationLog.Request;

            //List<IronHtml.FormElement> FormElements = CurrentAsso.DestinationLog.Response.Html.GetFormElements();
            foreach (IronHtml.FormElement FormEle in CurrentAsso.SourceLog.Response.Html.GetFormElements())
            {
                if (FormEle.DoAllInputFieldValuesMatchRequest(ReqToMatchAgainst))
                {
                    BestMatches.Add(FormEle);
                }
                else if (FormEle.DoHiddenInputFieldValuesMatchRequest(ReqToMatchAgainst))
                {
                    BestMatches.Add(FormEle);
                }
                else if (FormEle.DoInputFieldNamesMatchRequest(ReqToMatchAgainst))
                {
                    SecondBestMatches.Add(FormEle);
                }                
            }

            foreach (List<IronHtml.FormElement> Matches in new List<List<IronHtml.FormElement>>() { BestMatches, SecondBestMatches })
            {
                //check by link id
                foreach (IronHtml.FormElement FormEle in Matches)
                {
                    if (FormEle.HasId)
                    {
                        List<IronHtml.Element> MatchingForms = NewFormElements.GetElementsWithId(FormEle.Id);
                        if (MatchingForms.Count > 0 && ((IronHtml.FormElement)MatchingForms[0]).DoInputFieldNamesMatchRequest(ReqToMatchAgainst))
                        {
                            return ((IronHtml.FormElement)MatchingForms[0]).GetFormSubmissionWithHiddenValuesFromFormAndOtherFromSecondArgument(PlaySess.Request, ReqToMatchAgainst);
                        }
                    }
                }
                //check by link name
                foreach (IronHtml.FormElement FormEle in Matches)
                {
                    if (FormEle.HasName)
                    {
                        List<IronHtml.Element> MatchingForms = NewFormElements.GetElementsWithName(FormEle.Name);
                        if (MatchingForms.Count > 0 && ((IronHtml.FormElement)MatchingForms[0]).DoInputFieldNamesMatchRequest(ReqToMatchAgainst))
                        {
                            return ((IronHtml.FormElement)MatchingForms[0]).GetFormSubmissionWithHiddenValuesFromFormAndOtherFromSecondArgument(PlaySess.Request, ReqToMatchAgainst);
                        }
                    }
                }
            }

            foreach (IronHtml.FormElement NewFormEle in NewFormElements.GetElements())
            {
                if (NewFormEle.DoInputFieldNamesMatchRequest(ReqToMatchAgainst))
                {
                    return NewFormEle.GetFormSubmissionWithHiddenValuesFromFormAndOtherFromSecondArgument(PlaySess.Request, ReqToMatchAgainst);
                }
            }

            return null;
        }
        Request GetRedirect(LogAssociation CurrentAsso, Session PlaySess)
        {
            if (PlaySess.Response.IsRedirect)
            {
                return PlaySess.Request.GetRedirect(PlaySess.Response);
            }
            return null;
        }
    }
}
