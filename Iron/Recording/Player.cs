using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.Recording
{
    /*
    public class Player
    {
        Recording CurrentRecording = null;
        
        public CookieStore Cookies = new CookieStore();

        Request LoginCheckRequest = null;
        Response LoginCheckResponseWhenLoggedIn = null;
        Response LoginCheckResponseWhenLoggedOut = null;

        public Player(Recording Rec)
        {
            CurrentRecording = Rec;
        }

        public void UpdateCookies(Request Req, Response Res)
        {
            Cookies.Add(Req, Res);
        }

        public bool DoLogin(Recording Rec)
        {
            for (int i = 1; i <= Rec.StepCount; i++)
            {
                SessionRecordingStep Step  = Rec.GetStep(i);
                Step.Req = Step.ReferenceSession.Request.GetClone();

                if (Step.StepType != SessionRecordingStepType.First)
                {
                    if (Step.GetType() == typeof(SessionRecordingSendRequestStep))
                    {
                        SessionRecordingSendRequestStep SendReqStep = (SessionRecordingSendRequestStep)Step;
                        
                        bool UpdatedFromLink = false;
                        bool ParameterUpdated = false;

                        if (SendReqStep.UpdateFromLinkSteps.Count > 0)
                        {
                            UpdatedFromLink = UpdateFromLink(SendReqStep, Rec);
                        }
                        if (!UpdatedFromLink)
                        {
                            if(SendReqStep.UpdateParameterSteps.Count > 0)
                            {
                                ParameterUpdated = UpdateParameters(SendReqStep, Rec);
                            }
                        }
                    }
                    else if (Step.GetType() == typeof(SessionRecordingFollowRedirectStep))
                    {
                        SessionRecordingFollowRedirectStep FollowRedStep = (SessionRecordingFollowRedirectStep)Step;
                        GetRedirect(FollowRedStep, Rec);
                    }
                }
                Step.Req.SetCookie(Cookies);
                Step.Req.SetSource("Recording");
                Step.Res = Step.Req.Send();
                Cookies.Add(Step.Req, Step.Res);
                if (Step.Res.IsRedirect && !Step.ReferenceSession.Response.IsRedirect)
                {
                    bool HasMoreRedirects = true;
                    Request LastRequest = Step.Req;
                    Response LastResponse = Step.Res;
                    int RedirCount = 0;
                    while (HasMoreRedirects)
                    {
                        Request RedirReq = LastRequest.GetRedirect(LastResponse);
                        RedirReq.SetSource("Recording");
                        RedirReq.SetCookie(Cookies);
                        Response RedirRes = RedirReq.Send();
                        Cookies.Add(RedirReq, RedirRes);
                        LastRequest = RedirReq;
                        LastResponse = RedirRes;
                        if (LastResponse.IsRedirect)
                        {
                            HasMoreRedirects = true;
                            RedirCount++;
                        }
                        else
                        {
                            HasMoreRedirects = false;
                        }
                        if (RedirCount > 5)
                        {
                            HasMoreRedirects = false;
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckIfLoggedIn()
        {
            
            return false;
        }

        bool UpdateFromLink(SessionRecordingSendRequestStep Step, Recording Rec)
        {
            foreach (SessionRecordingUpdateFromLinkMinorStep UpdateFromLinkStep in Step.UpdateFromLinkSteps)
            {
                for (int i = 1; i <= Rec.StepCount; i++)
                {
                    SessionRecordingStep RecStep  = Rec.GetStep(i);
                    if (UpdateFromLinkStep.LinkSourceLogId == RecStep.ReferenceSession.LogId)
                    {
                        if(RecStep.Res != null)
                        {
                            if (UpdateFromLink(Step, RecStep.ReferenceSession.Request, RecStep.ReferenceSession.Response, RecStep.Req, RecStep.Res))
                            {
                                return true;
                            }
                        }
                    }
                }

                for (int i = 1; i <= Rec.StepCount; i++)
                {
                    SessionRecordingStep RecStep = Rec.GetStep(i);
                    if (UpdateFromLinkStep.LinkSourceLogId == RecStep.ReferenceSession.LogId)
                    {
                        foreach (Session Sess in RecStep.UnrecordedExtraSessions)
                        {
                            if (UpdateFromLink(Step, RecStep.ReferenceSession.Request, RecStep.ReferenceSession.Response, Sess.Request, Sess.Response))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        bool UpdateFromLink(SessionRecordingSendRequestStep Step, Request RefReq, Response RefRes, Request Req, Response Res)
        {
            List<Request> LinkClicks = Crawler.GetLinkClicks(Req, Res, Cookies);
            
            //If this link is the extact match of the one in the recording then use that
            foreach (Request LinkClick in LinkClicks)
            {
                if (LinkClick.FullUrl.Equals(Step.ReferenceSession.Request.FullUrl))
                {
                    return true;
                }
            }

            //Find the link nodes associated with the link in the reference session and check which link node from current session matches it best

            //Find the link nodes that match the generated request in the reference session
            HtmlAgilityPack.HtmlNodeCollection OldLinkNodes = RefRes.Html.GetNodes("a", "href");
            List<HtmlAgilityPack.HtmlNode> OldMatchingLinkNodes = new List<HtmlAgilityPack.HtmlNode>();
            if (OldLinkNodes != null)
            {
                foreach (HtmlAgilityPack.HtmlNode OldLinkNode in OldLinkNodes)
                {
                    if (OldLinkNode.Attributes["href"] != null)
                    {
                        string Url = Step.ReferenceSession.Request.RelativeUrlToAbsoluteUrl(Tools.HtmlDecode(OldLinkNode.Attributes["href"].Value));
                        if (Step.ReferenceSession.Request.FullUrl.Equals(Url))
                        {
                            OldMatchingLinkNodes.Add(OldLinkNode);
                        }
                    }
                }
            }

            //Order the nodes based on thier ability to be uniquely identified
            Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedOldMatchingLinkNodes = OrderLinkNodes(OldMatchingLinkNodes);

            HtmlAgilityPack.HtmlNodeCollection LinkNodes = Res.Html.GetNodes("a", "href");
            List<HtmlAgilityPack.HtmlNode> ValidLinkNodes = new List<HtmlAgilityPack.HtmlNode>();
            if (LinkNodes != null)
            {
                foreach (HtmlAgilityPack.HtmlNode LinkNode in LinkNodes)
                {
                    if (LinkNode.Attributes["href"] != null)
                    {
                        ValidLinkNodes.Add(LinkNode);
                    }
                }
            }

            HtmlAgilityPack.HtmlNode BestMatchedNode = GetBestMatchingLinkNode(OrderedOldMatchingLinkNodes, ValidLinkNodes);
            if (BestMatchedNode != null)
            {
                Step.Req.FullUrl = Req.RelativeUrlToAbsoluteUrl(Tools.HtmlDecode(BestMatchedNode.Attributes["href"].Value));
                return true;
            }

            foreach (Request LinkReq in LinkClicks)
            {
                if(DoLinkRequestsMatch(LinkReq, Step.ReferenceSession.Request))
                {
                    Step.Req.FullUrl = LinkReq.FullUrl;
                    return true;
                }
            }
            return false;
        }

        bool UpdateParameters(SessionRecordingSendRequestStep Step, Recording Rec)
        {
            foreach (SessionRecordingUpdateParameterMinorStep UpdateParamStep in Step.UpdateParameterSteps)
            {
                for (int i = 1; i <= Rec.StepCount; i++)
                {
                    SessionRecordingStep RecStep = Rec.GetStep(i);
                    if (UpdateParamStep.ParameterSourceLogId == RecStep.ReferenceSession.LogId)
                    {
                        if (RecStep.Res != null)
                        {
                            if (UpdateParameters(Step, UpdateParamStep, RecStep.ReferenceSession.Request, RecStep.ReferenceSession.Response, RecStep.Req, RecStep.Res))
                            {
                                return true;
                            }
                        }
                    }
                }

                for (int i = 1; i <= Rec.StepCount; i++)
                {
                    SessionRecordingStep RecStep = Rec.GetStep(i);
                    if (UpdateParamStep.ParameterSourceLogId == RecStep.ReferenceSession.LogId)
                    {
                        foreach (Session Sess in RecStep.UnrecordedExtraSessions)
                        {
                            if (UpdateParameters(Step, UpdateParamStep, RecStep.ReferenceSession.Request, RecStep.ReferenceSession.Response, Sess.Request, Sess.Response))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        bool UpdateParameters(SessionRecordingSendRequestStep Step, SessionRecordingUpdateParameterMinorStep ParamUpdateStep, Request RefReq, Response RefRes, Request Req, Response Res)
        {
            List<HtmlAgilityPack.HtmlNode> FormNodes = Res.Html.GetForms();

            List<HtmlAgilityPack.HtmlNode> OldFormNodes = RefRes.Html.GetForms();

            List<HtmlAgilityPack.HtmlNode> OldMatchingFormNodes = new List<HtmlAgilityPack.HtmlNode>();

            foreach (HtmlAgilityPack.HtmlNode OldFormNode in OldFormNodes)
            {
                if(Crawler.DoesFormNodesMatchRequest(Step.ReferenceSession.Request, OldFormNode))
                {
                    OldMatchingFormNodes.Add(OldFormNode);
                }
            }

            Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedOldFormNodes = OrderFormNodes(OldMatchingFormNodes);

            HtmlAgilityPack.HtmlNode BestMatch = GetBestMatchingFormNode(OrderedOldFormNodes, FormNodes);

            if (BestMatch != null)
            {
                Request FormReq = Crawler.GetFormSubmission(Step.Req, BestMatch, Cookies);
                UpdateParameter(Step, ParamUpdateStep, FormReq, BestMatch);
                return true;
            }
            else
            {
                List<Request> FormSubs = Crawler.GetFormSubmissions(Req, Res, new CookieStore());
                Request BestMatchReq = GetBestMatchingUrl(Step.Req, FormSubs);
                
                if (BestMatchReq != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode FormNode in FormNodes)
                    {
                        Request FormReq = Crawler.GetFormSubmission(Req, FormNode, new CookieStore());
                        if (FormReq.FullUrl.Equals(BestMatchReq.FullUrl))
                        {
                            UpdateParameter(Step, ParamUpdateStep, FormReq, FormNode);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void UpdateParameter(SessionRecordingSendRequestStep Step, SessionRecordingUpdateParameterMinorStep ParamUpdateStep, Request RequestToUpdateFrom, HtmlAgilityPack.HtmlNode FormNode)
        {
            IronHtml.FormElement FormEle = new IronHtml.FormElement(FormNode, -1);
            //Only update values of hidden fields, the others are user entered values so they remain the same across sessions
            if (FormEle.HasInputField(ParamUpdateStep.ParameterName) && FormEle.GetInputField(ParamUpdateStep.ParameterName).ElementType == IronHtml.InputElementType.Hidden)
            {
                if (ParamUpdateStep.ParameterSection == SessionRecordingRequestSection.Query)
                {
                    Step.Req.Query.Set(ParamUpdateStep.ParameterName, RequestToUpdateFrom.Query.Get(ParamUpdateStep.ParameterName));
                }
                else if (ParamUpdateStep.ParameterSection == SessionRecordingRequestSection.Body)
                {
                    Step.Req.Body.Set(ParamUpdateStep.ParameterName, RequestToUpdateFrom.Body.Get(ParamUpdateStep.ParameterName));
                }
            }
        }

        bool GetRedirect(SessionRecordingFollowRedirectStep Step, Recording Rec)
        {
            for (int i = 1; i <= Rec.StepCount; i++)
            {
                SessionRecordingStep RecStep = Rec.GetStep(i);
                if (Step.RedirectSourceLogId == RecStep.ReferenceSession.LogId)
                {
                    if (RecStep.Res != null)
                    {
                        if (GetRedirect(Step, RecStep.ReferenceSession.Request, RecStep.ReferenceSession.Response))
                        {
                            return true;
                        }
                    }
                }
            }

            for (int i = 1; i <= Rec.StepCount; i++)
            {
                SessionRecordingStep RecStep = Rec.GetStep(i);
                if (Step.RedirectSourceLogId == RecStep.ReferenceSession.LogId)
                {
                    foreach (Session Sess in RecStep.UnrecordedExtraSessions)
                    {
                        if (GetRedirect(Step, Sess.Request, Sess.Response))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        bool GetRedirect(SessionRecordingFollowRedirectStep Step, Request Req, Response Res)
        {
            return false;
        }

        HtmlAgilityPack.HtmlNode GetBestMatchingLinkNode(Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedOldLinkNodes, List<HtmlAgilityPack.HtmlNode> LinkNodes)
        {
            foreach (string NodeId in OrderedOldLinkNodes["id"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in LinkNodes)
                {
                    if (Node.Attributes["id"] != null && Node.Attributes["id"].Equals(NodeId)) return Node;
                }
            }
            foreach (string NodeName in OrderedOldLinkNodes["name"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in LinkNodes)
                {
                    if (Node.Attributes["name"] != null && Node.Attributes["name"].Equals(NodeName)) return Node;
                }
            }
            foreach (string NodeAttrs in OrderedOldLinkNodes["attrs"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in LinkNodes)
                {
                    if (GetAttributeNameString(Node.Attributes).Equals(NodeAttrs)) return Node;
                }
            }

            return null;
        }

        HtmlAgilityPack.HtmlNode GetBestMatchingFormNode(Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedOldFormNodes, List<HtmlAgilityPack.HtmlNode> FormNodes)
        {
            foreach (string NodeId in OrderedOldFormNodes["id"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in FormNodes)
                {
                    if (Node.Attributes["id"] != null && Node.Attributes["id"].Equals(NodeId)) return Node;
                }
            }
            foreach (string NodeName in OrderedOldFormNodes["name"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in FormNodes)
                {
                    if (Node.Attributes["name"] != null && Node.Attributes["name"].Equals(NodeName)) return Node;
                }
            }
            foreach (string NodeAttrs in OrderedOldFormNodes["attrs"].Keys)
            {
                foreach (HtmlAgilityPack.HtmlNode Node in FormNodes)
                {
                    if (GetAttributeNameString(Node.Attributes).Equals(NodeAttrs)) return Node;
                }
            }
            return null;
        }

        bool DoLinkRequestsMatch(Request LinkReq1, Request LinkReq2)
        {
            if (LinkReq1.Query.Count == LinkReq2.Query.Count)
            {
                if (LinkReq1.Query.Count == 0)
                {
                    if (LinkReq1.UrlPathParts.Count == 0 || LinkReq2.UrlPathParts.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        if (LinkReq1.UrlPathParts.Count == LinkReq2.UrlPathParts.Count)
                        {
                            int UrlPathMismatchCount = 0;
                            for (int i = 0; i < LinkReq1.UrlPathParts.Count; i++)
                            {
                                if (!LinkReq1.UrlPathParts[i].Equals(LinkReq2.UrlPathParts[i]))
                                {
                                    UrlPathMismatchCount++;
                                    if (UrlPathMismatchCount > 1) return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (LinkReq1.UrlPath.Equals(LinkReq2.UrlPath))
                    {
                        List<string> LinkReq1Names = LinkReq1.Query.GetNames();
                        int ParamMismatchCount = 0;
                        foreach (string Name in LinkReq2.Query.GetNames())
                        {
                            if (!LinkReq1Names.Contains(Name)) return false;
                            List<string> LinkReq1Values = LinkReq1.Query.GetAll(Name);
                            foreach (string Value in LinkReq2.Query.GetAll(Name))
                            {
                                if (!LinkReq1Values.Contains(Value))
                                {
                                    ParamMismatchCount++;
                                    if (ParamMismatchCount > 1) return false;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderLinkNodes(List<HtmlAgilityPack.HtmlNode> LinkNodes)
        {
            Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedLinkNodes = new Dictionary<string,Dictionary<string,List<HtmlAgilityPack.HtmlNode>>>(){
            {"id", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"name", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"text", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"attrs", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"other", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            };
            List<int> AddedIds = new List<int>();
            
            //Check for unique id
            for (int i=0; i < LinkNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = LinkNodes[i];
                if (Node.Attributes["id"] != null)
                {
                    if(!OrderedLinkNodes["id"].ContainsKey(Node.Attributes["id"].Value))
                    {
                        OrderedLinkNodes["id"][Node.Attributes["id"].Value] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedLinkNodes["id"][Node.Attributes["id"].Value].Add(Node);
                    AddedIds.Add(i);
                }
            }
            
            //Check for unique name
            for (int i = 0; i < LinkNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = LinkNodes[i];
                if (Node.Attributes["name"] != null)
                {
                    if (!OrderedLinkNodes["name"].ContainsKey(Node.Attributes["name"].Value))
                    {
                        OrderedLinkNodes["name"][Node.Attributes["name"].Value] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedLinkNodes["name"][Node.Attributes["name"].Value].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Check for unique link text
            for (int i = 0; i < LinkNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = LinkNodes[i];
                if (Node.InnerText.Trim().Length > 0)
                {
                    if (!OrderedLinkNodes["text"].ContainsKey(Node.InnerText.Trim()))
                    {
                        OrderedLinkNodes["name"][Node.InnerText.Trim()] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedLinkNodes["name"][Node.InnerText.Trim()].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Check for unique attr names and count
            for (int i = 0; i < LinkNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = LinkNodes[i];
                if (Node.Attributes.Count > 0)
                {
                    string AttrStr = GetAttributeNameString(Node.Attributes);
                    if (!OrderedLinkNodes["attrs"].ContainsKey(AttrStr))
                    {
                        OrderedLinkNodes["attrs"][AttrStr] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedLinkNodes["attrs"][AttrStr].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Add the rest
            for (int i = 0; i < LinkNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = LinkNodes[i];
                
                if (!OrderedLinkNodes["other"].ContainsKey("other"))
                {
                    OrderedLinkNodes["other"]["other"] = new List<HtmlAgilityPack.HtmlNode>();
                }
                OrderedLinkNodes["other"]["other"].Add(Node);
                AddedIds.Add(i);
            }
            return OrderedLinkNodes;
        }

        Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderFormNodes(List<HtmlAgilityPack.HtmlNode> FormNodes)
        {
            Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>> OrderedFormNodes = new Dictionary<string, Dictionary<string, List<HtmlAgilityPack.HtmlNode>>>(){
            {"id", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"name", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"fields", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"text", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            {"other", new Dictionary<string, List<HtmlAgilityPack.HtmlNode>>()},
            };
            List<int> AddedIds = new List<int>();

            //Check for unique id
            for (int i = 0; i < FormNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = FormNodes[i];
                if (Node.Attributes["id"] != null)
                {
                    if (!OrderedFormNodes["id"].ContainsKey(Node.Attributes["id"].Value))
                    {
                        OrderedFormNodes["id"][Node.Attributes["id"].Value] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedFormNodes["id"][Node.Attributes["id"].Value].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Check for unique name
            for (int i = 0; i < FormNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = FormNodes[i];
                if (Node.Attributes["name"] != null)
                {
                    if (!OrderedFormNodes["name"].ContainsKey(Node.Attributes["name"].Value))
                    {
                        OrderedFormNodes["name"][Node.Attributes["name"].Value] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedFormNodes["name"][Node.Attributes["name"].Value].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Check for unique attr names and count
            for (int i = 0; i < FormNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = FormNodes[i];
                if (Node.Attributes.Count > 0)
                {
                    string AttrStr = GetAttributeNameString(Node.Attributes);
                    if (!OrderedFormNodes["attrs"].ContainsKey(AttrStr))
                    {
                        OrderedFormNodes["attrs"][AttrStr] = new List<HtmlAgilityPack.HtmlNode>();
                    }
                    OrderedFormNodes["attrs"][AttrStr].Add(Node);
                    AddedIds.Add(i);
                }
            }

            //Add the rest
            for (int i = 0; i < FormNodes.Count; i++)
            {
                if (AddedIds.Contains(i)) continue;
                HtmlAgilityPack.HtmlNode Node = FormNodes[i];

                if (!OrderedFormNodes["other"].ContainsKey("other"))
                {
                    OrderedFormNodes["other"]["other"] = new List<HtmlAgilityPack.HtmlNode>();
                }
                OrderedFormNodes["other"]["other"].Add(Node);
                AddedIds.Add(i);
            }
            return OrderedFormNodes;
        }


        public static Request GetBestMatchingUrl(Request RefReq, List<Request> Requests)
        {
            //Get the Request from the list that best matches the reference request
            //Returns null if no satisfactory match is found

            foreach (Request Req in Requests)
            {
                if (Req.FullUrl.Equals(Req.FullUrl)) return Req;
            }

            if (RefReq.Query.Count > 0)
            {
                Dictionary<int, List<Request>> QueryMatches = new Dictionary<int, List<Request>>();
                foreach (Request Req in Requests)
                {
                    if (Req.Host.Equals(RefReq.Host, StringComparison.OrdinalIgnoreCase) && Req.Ssl == RefReq.Ssl && Req.UrlPath.Equals(RefReq.UrlPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Req.Query.Count == RefReq.Query.Count)
                        {
                            bool NameMatch = true;
                            int ValMatchCount = 0;

                            List<string> RefNames = RefReq.Query.GetNames();
                            foreach (string Name in Req.Query.GetNames())
                            {
                                if (!RefNames.Contains(Name))
                                {
                                    NameMatch = false;
                                    continue;
                                }
                                List<string> RefValues = RefReq.Query.GetAll(Name);
                                foreach (string Val in Req.Query.GetAll(Name))
                                {
                                    if (RefValues.Contains(Val))
                                    {
                                        ValMatchCount++;
                                    }
                                }
                            }
                            if (NameMatch)
                            {
                                if (!QueryMatches.ContainsKey(ValMatchCount))
                                {
                                    QueryMatches[ValMatchCount] = new List<Request>();
                                }
                                QueryMatches[ValMatchCount].Add(Req);
                            }
                        }
                    }
                }
                if (QueryMatches.Count > 0)
                {
                    List<int> Scores = new List<int>(QueryMatches.Keys);
                    Scores.Sort();
                    return QueryMatches[Scores[Scores.Count - 1]][0];//return the request which has the highest match
                }
            }
            else
            {
                Dictionary<int, List<Request>> UrlPathPartMatches = new Dictionary<int, List<Request>>();
                
                foreach (Request Req in Requests)
                {
                    int MatchCount = 0;
                    if (Req.Host.Equals(RefReq.Host, StringComparison.OrdinalIgnoreCase) && Req.Ssl == RefReq.Ssl)
                    {
                        if (Req.UrlPathParts.Count == RefReq.UrlPathParts.Count)
                        {
                            bool PartsMatchSuceeded = true;
                            for (int i = Req.UrlPathParts.Count-1; i >= 0; i++)
                            {
                                if (Req.UrlPathParts[i].Equals(RefReq.UrlPathParts[i]))
                                {
                                    MatchCount++;
                                }
                                else
                                {
                                    //If any url path part other than the last one mismatches then ignore the request
                                    if (i < Req.UrlPathParts.Count - 1)
                                    {
                                        PartsMatchSuceeded = true;
                                        break;
                                    }
                                }
                            }
                            if (PartsMatchSuceeded)
                            {
                                if (!UrlPathPartMatches.ContainsKey(MatchCount))
                                {
                                    UrlPathPartMatches[MatchCount] = new List<Request>();
                                }
                                UrlPathPartMatches[MatchCount].Add(Req);
                            }
                        }
                    }
                }
                if (UrlPathPartMatches.Count > 0)
                {
                    List<int> Scores = new List<int>(UrlPathPartMatches.Keys);
                    Scores.Sort();
                    return UrlPathPartMatches[Scores[Scores.Count - 1]][0];//return the request which has the highest match
                }
            }
            return null;
        }

        string GetAttributeNameString(HtmlAgilityPack.HtmlAttributeCollection Attributes)
        {
            List<string> Names = new List<string>();
            foreach (HtmlAgilityPack.HtmlAttribute Attr in Attributes)
            {
                Names.Add(Attr.Name);
            }
            Names.Sort();
            StringBuilder SB = new StringBuilder();
            SB.Append(Names.Count); SB.Append("||");
            foreach (string Name in Names)
            {
                SB.Append(Name); SB.Append("||");
            }
            return SB.ToString();
        }
    }
    */
}
