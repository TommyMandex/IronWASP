//
// Copyright 2011-2013 Lavakumar Kuppan
//
// This file is part of IronWASP
//
// IronWASP is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// IronWASP is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with IronWASP.  If not, see http://www.gnu.org/licenses/.
//

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace IronWASP
{
    public class Crawler
    {
        internal List<string> PageSignatures = new List<string>();

        internal List<Request> Requests = new List<Request>();

        internal int MaxDepth = 10;

        Queue<object[]> ToCrawlQueue = new Queue<object[]>();

        Dictionary<int, Thread> CrawlerThreads = new Dictionary<int, Thread>();

        CookieStore Cookies = new CookieStore();

        int ActiveThreadCount = 0;

        internal static int MaxCrawlThreads = 5;

        internal static string UserAgent = "";

        int InCrawlQueueDequeueMode = 0;

        Dictionary<string, Response> NotFoundSignatures = new Dictionary<string, Response>();

        Queue<Request> CrawledRequests = new Queue<Request>();

        List<string> FileNamesToCheck = new List<string>();
        List<string> DirNamesToCheck = new List<string>();

        //This differs slightly from the list in ScanManager:
        //htm, html, xhtml should be crawled to extract links, forms etc
        //xml should be crawled for checks like crossdomain.xml etc
        internal static List<string> ExtenionsToAvoid = new List<string>() { 
            "jpg", "png", "gif","bmp", "ico","exif","jpeg",//image files
            "7z", "zip", "rar","tar", "gz","tgz","bzip", "bzip2","dmg","cab",//compressed files
            "js", "css","svg","svgz","bak",//static web content
            "swf","exe", "jar", "msi","deb","bin","class","war",//executable content
            "rtf", "txt", "pdf", "doc", "docx", "ppt", "pptx","xls","xlsx", "iso","json","xps","tex","csv","pps","tsv","db","log","rss",//document formats
            "mp3","wav","m4a","m4p","aac","dat",//audio content
            "mp4","aaf","3gp","wmv","avi","fla","sol","mov","mpeg","mpg","mpe","ogg","rm",//video content
               };

        //Settings
        internal List<string> UrlsToAvoid = new List<string>();
        internal List<string> HostsToInclude = new List<string>();
        internal bool HTTP = false;
        internal bool HTTPS = false;
        internal string StartingUrl = "/";
        internal string BaseUrl = "/";
        internal string PrimaryHost = "";
        internal bool PerformDirAndFileGuessing = true;
        internal bool IncludeSubDomains = false;

        internal string[] SpecialHeader = new string[2];

        bool Stopped = false;

        public void Start()
        {
            try
            {
                if (HTTP)
                {
                    Request HttpRequest = new Request(string.Format("http://{0}{1}", PrimaryHost, StartingUrl));
                    lock (ToCrawlQueue)
                    {
                        ToCrawlQueue.Enqueue(new object[] { HttpRequest, 0, true });
                    }
                }
                if (HTTPS)
                {
                    Request HttpsRequest = new Request(string.Format("https://{0}{1}", PrimaryHost, StartingUrl));
                    lock (ToCrawlQueue)
                    {
                        ToCrawlQueue.Enqueue(new object[] { HttpsRequest, 0, true });
                    }
                }
                PageSignatures.Clear();
                if (PerformDirAndFileGuessing) SetUpDirAndFileDictionaries();
                Thread T = new Thread(CrawlQueueItem);
                T.Start();
                try
                {
                    CrawlerThreads.Add(T.ManagedThreadId, T);
                }
                catch { }
            }
            catch (ThreadAbortException){}
            catch (Exception Exp)
            {
                IronException.Report("Error in Crawling", Exp.Message, Exp.StackTrace);
                throw (Exp);
            }
        }

        void SetUpDirAndFileDictionaries()
        {
            try
            {
                StreamReader Reader = File.OpenText(Config.RootDir + "/DirNamesDictionary.txt");
                string DirList = Reader.ReadToEnd();
                Reader.Close();
                DirNamesToCheck = new List<string>(DirList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch(Exception Exp)
            {
                IronException.Report("Error loading DirNamesDictionary.txt", Exp);
            }
            try
            {
                StreamReader Reader = File.OpenText(Config.RootDir + "/FileNamesDictionary.txt");
                string FileList = Reader.ReadToEnd();
                Reader.Close();
                FileNamesToCheck = new List<string>(FileList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception Exp)
            {
                IronException.Report("Error loading FileNamesDictionary.txt", Exp);
            }
        }

        void Crawl(object ObjectItem)
        {
            if (Stopped) return;
            try
            {
                object[] Objects = (object[])ObjectItem;
                Request Req = (Request)Objects[0];
                int Depth = (int)Objects[1];
                bool Scraped = (bool)Objects[2];
                Crawl(Req, Depth, Scraped);
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                IronException.Report("Error while Crawling", Exp.Message, Exp.StackTrace);
            }
            finally
            {
                CrawlQueueItem();
            }
        }

        void Crawl(Request Req, int Depth, bool Scraped)
        {
            if (Stopped) return;
            if (Depth > MaxDepth) return;
            if (WasCrawled(Req)) return;
            if (!CanCrawl(Req)) return;

            lock (PageSignatures)
            {
                PageSignatures.Add(GetPageSignature(Req));
            }

            Req.Source = RequestSource.Probe;
            Req.SetCookie(Cookies);
            if (UserAgent.Length > 0) Req.Headers.Set("User-Agent", UserAgent);
            if (SpecialHeader[0] != null) Req.Headers.Set(SpecialHeader[0], SpecialHeader[1]);
            if (Stopped) return;
            Response Res = Req.Send();
            if (Stopped) return;
            Cookies.Add(Req, Res);
            bool Is404File = IsA404(Req, Res);

            if (!Res.IsHtml)
            {
                return;
            }

            if (Depth + 1 > MaxDepth) return;
            List<Request> Redirects = GetRedirects(Req, Res);
            foreach (Request Redirect in Redirects)
            {
                AddToCrawlQueue(Redirect, Depth + 1, true);
            }
            List<Request> LinkClicks = GetLinkClicks(Req, Res);
            foreach (Request LinkClick in LinkClicks)
            {
                AddToCrawlQueue(LinkClick, Depth + 1, true);
            }

            List<Request> FormSubmissions = GetFormSubmissions(Req, Res);
            foreach (Request FormSubmission in FormSubmissions)
            {
                AddToCrawlQueue(FormSubmission, Depth + 1, true);
            }

            Request DirCheck = Req.GetClone();
            DirCheck.Method = "GET";
            DirCheck.Body.RemoveAll();
            DirCheck.Url = DirCheck.UrlDir;

            if (!Req.Url.EndsWith("/"))
            {
                AddToCrawlQueue(DirCheck, Depth + 1, false);
            }

            if (PerformDirAndFileGuessing && !Is404File)
            {
                foreach (string File in FileNamesToCheck)
                {
                    Request FileCheck = DirCheck.GetClone();
                    FileCheck.Url = string.Format("{0}{1}", FileCheck.Url, File);
                    AddToCrawlQueue(FileCheck, Depth + 1, false);
                }

                foreach (string Dir in DirNamesToCheck)
                {
                    Request DirectoryCheck = DirCheck.GetClone();
                    DirectoryCheck.Url = string.Format("{0}{1}/", DirectoryCheck.Url, Dir);
                    AddToCrawlQueue(DirectoryCheck, Depth + 1, false);
                }
            }
            if (Stopped) return;
            if (Scraped || !Is404File)
            {
                lock (CrawledRequests)
                {
                    CrawledRequests.Enqueue(Req.GetClone());
                }
                IronUpdater.AddToSiteMap(Req);
            }
        }

        void CrawlQueueItem()
        {
            if (Stopped) return;
            try
            {
                Interlocked.Increment(ref InCrawlQueueDequeueMode);
                bool Continue = true;
                Interlocked.Decrement(ref ActiveThreadCount);
                lock (ToCrawlQueue)
                {
                    while (ActiveThreadCount < MaxCrawlThreads && Continue)
                    {
                        if (Stopped) return;
                        Continue = false;
                        try
                        {
                            object[] Objects = ToCrawlQueue.Dequeue();
                            Thread T = new Thread(Crawl);
                            T.Start(Objects);
                            try
                            {
                                lock (CrawlerThreads)
                                {
                                    CrawlerThreads.Add(T.ManagedThreadId, T);
                                }
                            }
                            catch { }
                            Interlocked.Increment(ref ActiveThreadCount);
                            Continue = true;
                        }
                        catch { }
                    }
                }                
            }
            catch { }
            try
            {
                lock (CrawlerThreads)
                {
                    CrawlerThreads.Remove(Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch { }
            Interlocked.Decrement(ref InCrawlQueueDequeueMode);
        }

        bool WasCrawled(Request Req)
        {
            string ReqSignature = GetPageSignature(Req);
            lock(PageSignatures)
            {
                if (PageSignatures.Contains(ReqSignature))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        bool CanCrawl(Request Req)
        {
            if (!((Req.SSL && HTTPS) || (!Req.SSL && HTTP))) return false;
            if(!IsHostAllowed(Req.Host)) return false;
            if (!IsCrawlableExtension(Req)) return false;
            if (!Req.Url.Equals(BaseUrl))
            {
                if (BaseUrl.EndsWith("/"))
                {
                    if (!Req.Url.StartsWith(BaseUrl)) return false;
                }
                else
                {
                    if (!Req.Url.StartsWith(string.Format("{0}?", BaseUrl))) return false;
                }
            }
            if (UrlsToAvoid.Contains(Req.Url) || UrlsToAvoid.Contains(Req.UrlPath)) return false;
            return true;
        }

        bool IsCrawlableExtension(Request Req)
        {
            string Ext = Req.File.ToLower().Trim();
            if (ExtenionsToAvoid.Contains(Ext))
                return false;
            else
                return true;
        }

        bool IsHostAllowed(string Host)
        {
            if(Host.Equals(PrimaryHost)) return true;
            if(IncludeSubDomains && Host.EndsWith(string.Format(".{0}", PrimaryHost))) return true;
            foreach(string AH in HostsToInclude)
            {
                if(Host.Equals(AH)) return true;
                if(IncludeSubDomains && Host.EndsWith(string.Format(".{0}", AH))) return true;
            }
            return false;
        }

        bool IsA404(Request Req, Response Res)
        {
            Response NotFoundResponse = null;
            lock (NotFoundSignatures)
            {
                if (NotFoundSignatures.ContainsKey(string.Format("{0}{1}{2}{3}", Req.SSL, Req.Host, Req.UrlDir, Req.File)))
                {
                    NotFoundResponse = NotFoundSignatures[string.Format("{0}{1}{2}{3}", Req.SSL, Req.Host, Req.UrlDir, Req.File)];
                }
            }
            if(NotFoundResponse == null)
            {
                Request NotFoundGetter = Req.GetClone();
                NotFoundGetter.Method = "GET";
                NotFoundGetter.Body.RemoveAll();
                if (Req.File.Length > 0)
                    NotFoundGetter.Url = string.Format("{0}should_not_xist_{1}.{2}", NotFoundGetter.UrlDir, Tools.GetRandomString(10, 15), Req.File);
                else
                    NotFoundGetter.Url = string.Format("{0}should_not_xist_{1}", NotFoundGetter.UrlDir, Tools.GetRandomString(10, 15));
                NotFoundResponse = NotFoundGetter.Send();
                if (Stopped) return true;
                NotFoundResponse.BodyString = "";
                List<string> HeaderNames = NotFoundResponse.Headers.GetNames();
                foreach (string HeaderName in HeaderNames)
                {
                    if (!HeaderName.Equals("Location"))
                    {
                        NotFoundResponse.Headers.Remove(HeaderName);
                    }
                }
                NotFoundResponse.Flags.Add("Url", NotFoundGetter.Url);
                lock (NotFoundSignatures)
                {
                    if (!NotFoundSignatures.ContainsKey(string.Format("{0}{1}{2}{3}", Req.SSL, Req.Host, Req.UrlDir, Req.File)))
                        NotFoundSignatures.Add(string.Format("{0}{1}{2}{3}", Req.SSL, Req.Host, Req.UrlDir, Req.File), NotFoundResponse);
                }
            }
            if(Res.Code == 200 && NotFoundResponse.Code != 200) return false;
            if(Res.Code == 404) return true;
            
            if (Res.Code > 400)
            {
                if (NotFoundResponse.Code == Res.Code) 
                    return true;
                else
                    return false;
            }
            string NotFoundGetterUrl = NotFoundResponse.Flags["Url"].ToString();
            if (Res.Code == 301 || Res.Code == 302 || Res.Code == 303 || Res.Code == 307)
            {
                string RedirectedUrl = Res.Headers.Get("Location");
                if (NotFoundResponse.Code == 301 || NotFoundResponse.Code == 302 || NotFoundResponse.Code == 303 || NotFoundResponse.Code == 307)
                {
                    string NotFoundRedirectedUrl = NotFoundResponse.Headers.Get("Location");
                    if (RedirectedUrl.ToLower().Equals(NotFoundRedirectedUrl.ToLower()))
                        return true;
                    else if (Regex.IsMatch(RedirectedUrl, @".*not\Wfound.*", RegexOptions.IgnoreCase))
                        return true;
                    else if (NotFoundRedirectedUrl.Replace(NotFoundGetterUrl,"").Equals(RedirectedUrl.Replace(Req.Url, "")))
                        return true;
                    else
                    {
                        Request RedirectedLocationReq;
                        if (RedirectedUrl.StartsWith("http://") || RedirectedUrl.StartsWith("https://"))
                        {
                            RedirectedLocationReq = new Request(RedirectedUrl);
                        }
                        else if (RedirectedUrl.StartsWith("/"))
                        {
                            RedirectedLocationReq = Req.GetClone();
                            RedirectedLocationReq.Url = RedirectedUrl;
                        }
                        else
                        {
                            return true;
                        }
                        Request NotFoundRedirectedLocationReq;
                        if (NotFoundRedirectedUrl.StartsWith("http://") || NotFoundRedirectedUrl.StartsWith("https://"))
                        {
                            NotFoundRedirectedLocationReq = new Request(NotFoundRedirectedUrl);
                        }
                        else if (NotFoundRedirectedUrl.StartsWith("/"))
                        {
                            NotFoundRedirectedLocationReq = Req.GetClone();
                            NotFoundRedirectedLocationReq.Url = NotFoundRedirectedUrl;
                        }
                        else
                        {
                            return false;
                        }
                        if (RedirectedLocationReq.Url.Equals(NotFoundRedirectedLocationReq.Url)) return true;
                    }
                }
                else
                    return false;
            }
            return false;
        }

        internal bool IsActive()
        {
            lock (ToCrawlQueue)
            {
                if (ToCrawlQueue.Count > 0) return true;
            }
            if (ActiveThreadCount > 0 || InCrawlQueueDequeueMode > 0)
                return true;
            else
                return false;
        }

        void AddToCrawlQueue(Request Req, int Depth, bool Scraped)
        {
            if (WasCrawled(Req)) return;
            if (!CanCrawl(Req)) return;
            lock (ToCrawlQueue)
            {
                ToCrawlQueue.Enqueue(new object[] { Req, Depth, Scraped });
            }
        }

        List<Request> GetRedirects(Request Req, Response Res)
        {
            return GetRedirects(Req, Res, Cookies);
        }

        public static List<Request> GetRedirects(Request Req, Response Res, CookieStore Cookies)
        {
            List<Request> Redirects = new List<Request>();
            List<string> RedirectUrls = GetRedirectUrls(Req, Res);
            foreach (string RedirectUrl in RedirectUrls)
            {
                try
                {
                    Request RedirectReq = new Request(RedirectUrl);
                    RedirectReq.SetCookie(Cookies);
                    Redirects.Add(RedirectReq);
                }
                catch { }
            }
            return Redirects;
        }

        //public static List<IronHtml.LinkClick> GetLinkClickItems(Request Req, Response Res)
        //{
        //    List<IronHtml.LinkClick> Results = new List<IronHtml.LinkClick>();

        //    if (Res == null || Res.Html == null) return Results;

        //    List<string> LinkValues = new List<string>();
        //    foreach (HtmlAgilityPack.HtmlNode LinkNode in Res.Html.GetNodes("a", "href"))
        //    {
        //        string HrefValue = Tools.HtmlDecode(LinkNode.Attributes["href"].Value.Trim()).Trim();
        //        try
        //        {
        //            Request HrefReq = new Request(HrefValue);
        //            IronHtml.LinkClick LinkClickItem = new IronHtml.LinkClick(HrefReq, LinkNode, IronHtml.UrlInHtmlMatch.FullAbsolute, HrefValue);
        //        }
        //        catch
        //        {
        //            if(HrefValue.StartsWith("/") || HrefValue.TrimStart('.').StartsWith("/"))
        //            {

        //            }
        //        }

        //        LinkValues.Add(Tools.HtmlDecode(RawLinkValue));
        //    }
        //    return LinkValues;
        //}

        List<Request> GetLinkClicks(Request Req, Response Res)
        {
            return GetLinkClicks(Req, Res, Cookies);
        }

        public static List<Request> GetLinkClicks(Request Req, Response Res, CookieStore Cookies)
        {
            List<Request> LinkClicks = new List<Request>();
            List<string> Links = GetLinks(Req, Res);
            foreach (string Link in Links)
            {
                try
                {
                    Request LinkReq = new Request(Link);
                    LinkReq.SetCookie(Cookies);
                    LinkClicks.Add(LinkReq);
                }
                catch { }
            }
            return LinkClicks;
        }

        List<Request> GetFormSubmissions(Request Req, Response Res)
        {
            return GetFormSubmissions(Req, Res, Cookies);
        }

        static List<Request> GetLoginFormSubmissions(Request Req, Response Res, CookieStore Cookies)
        {
            return GetFormSubmissionsByType(Req, Res, Cookies, true);
        }

        static List<Request> GetFormSubmissions(Request Req, Response Res, CookieStore Cookies)
        {
            return GetFormSubmissionsByType(Req, Res, Cookies, false);
        }

        public static List<Request> GetFormSubmissionsWithActualValue(Request Req, Response Res, CookieStore Cookies)
        {
            return GetFormSubmissionsByType(Req, Res, Cookies, false);
        }

        //public static List<IronHtml.FormSubmission> GetFormSubmissionItems(Request Req, Response Res, CookieStore Cookies)
        //{

        //}

        static List<Request> GetFormSubmissionsByType(Request Req, Response Res, CookieStore Cookies, bool LoginFormsOnly)
        {
            List<Request> FormSubmissions = new List<Request>();
            List<HtmlNode> FormNodes = Res.Html.GetForms();
            foreach (HtmlNode FormNode in FormNodes)
            {
                Request FormSub = GetFormSubmission(Req, FormNode, Cookies, LoginFormsOnly);
                if (FormSub != null)
                {
                    FormSubmissions.Add(FormSub);
                }
            }
            return FormSubmissions;
        }

        static Request GetFormSubmission(Request Req, HtmlNode FormNode, CookieStore Cookies)
        {
            return GetFormSubmission(Req, FormNode, Cookies, false);
        }

        static Request GetFormSubmission(Request Req, HtmlNode FormNode, CookieStore Cookies, bool LoginFormOnly)
        {
            return GetFormSubmission(Req, FormNode, Cookies, LoginFormOnly, true);
        }

        public static Request GetFormSubmissionWithActualValue(Request Req, HtmlNode FormNode, CookieStore Cookies)
        {
            return GetFormSubmission(Req, FormNode, Cookies, false, false);
        }

        public static Request GetFormSubmission(Request Req, HtmlNode FormNode, CookieStore Cookies, bool LoginFormOnly, bool FillEmptyFields)
        {
            //Login request signatures:
            //form must have one password type input field
            //three or more parameters must be present in the request query/body

            Request SubReq = Req.GetClone();
            SubReq.Method = "GET";
            SubReq.BodyString = "";

            foreach (HtmlAttribute Attr in FormNode.Attributes)
            {
                if (Attr.Name.Equals("method"))
                {
                    SubReq.Method = Attr.Value.ToUpper();
                }
                else if(Attr.Name.Equals("action"))
                {
                    if (Attr.Value.StartsWith("javascript:")) continue;
                    string ActionUrl = NormalizeUrl(Req, Tools.HtmlDecode(Attr.Value.Trim()));
                    if (ActionUrl.Length > 0)
                    {
                        SubReq.FullUrl = ActionUrl;
                    }
                }
            }

            if (SubReq.Method == "GET")
            {
                SubReq.Query.RemoveAll();
            }
            else
            {
                SubReq.Headers.Set("Content-Type", "application/x-www-form-urlencoded");
            }

            bool PasswordFieldPresent = false;

            foreach (HtmlNode InputNode in FormNode.ChildNodes)
            {
                string Name = "";
                string Value = "";

                foreach (HtmlAttribute Attr in InputNode.Attributes)
                {
                    switch (Attr.Name)
                    {
                        case ("name"):
                            Name = Attr.Value;
                            break;
                        case ("type"):
                            if (Attr.Value.Equals("password", StringComparison.OrdinalIgnoreCase)) PasswordFieldPresent = true;
                            break;
                        case ("value"):
                            Value = Attr.Value;
                            break;
                    }
                }
                if (FillEmptyFields && Value.Length == 0)
                {
                    Value = Tools.GetRandomString(2, 5);
                }
                if (Name.Length > 0)
                {
                    if (SubReq.Method.Equals("GET"))
                        SubReq.Query.Add(Name, Value);
                    else
                        SubReq.Body.Add(Name, Value);
                }
            }
            SubReq.SetCookie(Cookies);
            if (LoginFormOnly)
            {
                if (PasswordFieldPresent)
                {
                    if ((SubReq.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && SubReq.Query.Count >= 3) || (SubReq.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && SubReq.Body.Count >= 3))
                    {
                        return SubReq;
                    }
                }
            }
            else
            {
                return SubReq;
            }
            return null;
        }

        public static List<string> GetRedirectUrls(Request Req, Response Res)
        {
            List<string> RedirectUrls = new List<string>();

            List<string> LocationUrls = new List<string>();
            if (Res.Headers.Has("Location")) LocationUrls.Add(Res.Headers.Get("Location"));
            if (Res.IsHtml)
            {
                List<string> LocationsFromHtml = Res.Html.GetMetaContent("http-equiv", "location");
                foreach (string LocationFromHtml in LocationsFromHtml)
                {
                    LocationUrls.Add(Tools.HtmlDecode(LocationFromHtml));
                }
            }
            
            foreach(string LocationUrl in LocationUrls)
            {
                string NormalizedLocation = NormalizeUrl(Req, LocationUrl);
                if (NormalizedLocation.Length > 0) RedirectUrls.Add(NormalizedLocation);
            }
            
            List<string> RefreshHeaderVals = new List<string>();
            
            if (Res.Headers.Has("Refresh")) RefreshHeaderVals.Add(Res.Headers.Get("Refresh"));
            if (Res.IsHtml)
            {
                List<string> RefreshsFromHtml = Res.Html.GetMetaContent("http-equiv", "refresh");
                foreach (string RefreshFromHtml in RefreshsFromHtml)
                {
                    RefreshHeaderVals.Add(Tools.HtmlDecode(RefreshFromHtml));
                }
            }
            
            foreach(string RefreshHeaderVal in RefreshHeaderVals)
            {
                string[] RefreshHVParts = RefreshHeaderVal.Split(new char[]{';'}, 2);
                if (RefreshHVParts.Length == 2 && RefreshHVParts[1].Length > 0)
                {
                    string NormalizedRefreshUrl = NormalizeUrl(Req, RefreshHVParts[1]);
                    if (NormalizedRefreshUrl.Length > 0) RedirectUrls.Add(NormalizedRefreshUrl);
                }
            }

            return RedirectUrls;
        }

        static List<string> GetLinks(Request Req, Response Res)
        {
            List<string> Links = new List<string>();
            foreach (string Link in Res.Html.Links)
            {
                string NormalizedUrl = NormalizeUrl(Req, Link);
                if (NormalizedUrl.Length > 0) Links.Add(NormalizedUrl);
            }
            return Links;
        }

        internal static string NormalizeUrl(Request Req, string RawLink)
        {
            if (RawLink.IndexOf('#') > -1)
            {
                RawLink = RawLink.Substring(0, RawLink.IndexOf('#'));
            }
            if (RawLink.StartsWith("http://") || RawLink.StartsWith("https://"))
            {
                return RawLink;
            }
            else if (RawLink.StartsWith("//"))
            {
                if (Req.SSL)
                    RawLink = string.Format("https:{0}", RawLink);
                else
                    RawLink = string.Format("http:{0}", RawLink);
            }
            else if (RawLink.StartsWith("/"))
            {
                Request TempReq = Req.GetClone();
                TempReq.Url = RawLink;
                return TempReq.FullUrl;
            }
            else if (RawLink.StartsWith("javascript:") || RawLink.StartsWith("file:"))
            {
                //ignore
            }
            else
            {
                List<string> UrlPathParts = Req.UrlPathParts;
                if (UrlPathParts.Count > 0)
                {
                    if (!Req.Url.EndsWith("/")) UrlPathParts.RemoveAt(UrlPathParts.Count - 1);
                }

                if (RawLink.StartsWith("../"))
                {
                    string[] RawUrlParts = RawLink.Split(new char[] { '/' });
                    List<string> TreatedRawUrlParts = new List<string>(RawUrlParts);
                    foreach (string Part in RawUrlParts)
                    {
                        if (Part.Equals("..") && (UrlPathParts.Count > 0))
                        {
                            UrlPathParts.RemoveAt(UrlPathParts.Count - 1);
                            TreatedRawUrlParts.RemoveAt(0);
                        }
                        else
                        {
                            break;
                        }
                    }
                    UrlPathParts.AddRange(TreatedRawUrlParts);
                    StringBuilder FinalUrlBuilder = new StringBuilder();
                    foreach (string UrlPart in UrlPathParts)
                    {
                        FinalUrlBuilder.Append("/"); FinalUrlBuilder.Append(UrlPart);
                    }
                    Request TempReq = Req.GetClone();
                    TempReq.Url = FinalUrlBuilder.ToString();
                    return TempReq.FullUrl;
                }
                else if (RawLink.Length > 0)
                {
                    return string.Format("{0}{1}{2}", Req.BaseUrl.TrimEnd('/'), Req.UrlDir, RawLink);
                }
            }
            return "";
        }

        public static bool DoesFormNodesMatchRequest(Request Req, HtmlNode FormNode)
        {
            //This method checks if a Request was actually generated from the submission of a particular HTML form node

            //Checks if the method of the request and method of the form node match
            //Checks if the input field names in the form node exactly match the request parameter names
            //Checks if the values of the hidden input field exactly match the corresponding request parameter values
            
            if (FormNode.Attributes["method"] != null)
            {
                if (FormNode.Attributes["method"].Value.Equals("GET", StringComparison.OrdinalIgnoreCase) && !Req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else if (FormNode.Attributes["method"].Value.Equals("POST", StringComparison.OrdinalIgnoreCase) && !Req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            Parameters Params = null;
            if (Req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                Params = Req.Body;
            }
            else
            {
                Params = Req.Query;
            }

            if (FormNode.SelectNodes("input").Count != Params.Count || Params.Count == 0) return false;

            foreach (HtmlNode InputNode in FormNode.SelectNodes("input"))
            {
                string Name = "";
                if (InputNode.Attributes["name"] != null)
                {
                    Name = InputNode.Attributes["name"].Value;
                    if (Req.Method.Equals("GET"))
                    {
                        if (!Req.Query.Has(Name)) return false;
                    }
                    else
                    {
                        if (!Req.Body.Has(Name)) return false;
                    }
                }
                else
                {
                    Name = "";
                }

                if (Name.Length > 0 && InputNode.Attributes["type"] != null)
                {
                    if (InputNode.Attributes["type"].Value.Equals("hidden", StringComparison.OrdinalIgnoreCase))
                    {
                        if (InputNode.Attributes["value"] != null)
                        {
                            string Value = InputNode.Attributes["value"].Value;
                            if (!Params.GetAll(Name).Contains(Value))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public List<HtmlNode> GetFormNodesMatchingRequest(Request Req, List<HtmlNode> FormNodes)
        {
            List<HtmlNode> MatchingFormsNodes = new List<HtmlNode>();
            foreach (HtmlNode FormNode in FormNodes)
            {
                if (DoesFormNodesMatchRequest(Req, FormNode))
                {
                    MatchingFormsNodes.Add(FormNode);
                }
            }
            return MatchingFormsNodes;
        }

        string GetPageSignature(Request Req)
        {
            StringBuilder Signature = new StringBuilder();
            Signature.Append(Req.SSL.ToString());
            Signature.Append(Req.Host);
            Signature.Append(Req.Method);
            Signature.Append(Req.Url);
            Signature.Append(Req.BodyString);
            Signature.Append(Req.CookieString);
            return Tools.MD5(Signature.ToString());
        }

        public List<Request> GetCrawledRequests()
        {
            List<Request> Requests = new List<Request>();
            lock (CrawledRequests)
            {
                Requests = new List<Request>(CrawledRequests.ToArray());
                CrawledRequests.Clear();
            }
            return Requests;
        }

        public void Stop()
        {
            Stopped = true;
            lock (CrawlerThreads)
            {
                //List<int> IDs = new List<int>(CrawlerThreads.Keys);
                //foreach (int ID in IDs)
                //{
                //    try
                //    {
                //        CrawlerThreads[ID].Abort();
                //    }
                //    catch { }
                //}
                CrawlerThreads.Clear();
            }
            lock (NotFoundSignatures)
            {
                NotFoundSignatures.Clear();
            }
        }
    }
}
