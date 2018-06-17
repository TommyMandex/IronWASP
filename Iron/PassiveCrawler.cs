using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP
{
    public class PassiveCrawler
    {
        static string CrawlerUserAgent = "";

        static List<int> RequestsWaitingForResponse = new List<int>();

        static List<string> ScrapedUrls = new List<string>();

        static string[] UrlExtensionsToScrape = new string[] {".asp",".aspx",".axd",".asx",".asmx",".ashx",".cfm",".yaws",".html",".htm",".xhtml",".jhtml",".jsp",".jspx",".do",".action",".pl",".php",".php4",".php3",".phtml",".rhtml",".cgi",".dll"};
        static string[] UrlsStartMarkers = new string[] {"http://","https://","../"};

        internal static bool intPassiveCrawlerRunning = false;

        internal static string PageWaitTime = "0";

        internal static bool PassiveCrawlerRunning
        {
            get
            {
                return intPassiveCrawlerRunning;
            }
            set
            {
                intPassiveCrawlerRunning = value;
                if (!intPassiveCrawlerRunning)
                {
                    lock (RequestsWaitingForResponse)
                    {
                        RequestsWaitingForResponse.Clear();
                    }
                    lock (ScrapedUrls)
                    {
                        ScrapedUrls.Clear();
                    }
                }
            }
        }

        internal static void CapturedCrawlRequest(Session Sess)
        {
            if (!PassiveCrawlerRunning) return;

            Request Req = Sess.Request;
            if (Req.Headers.Has("User-Agent") && Req.Headers.Get("User-Agent").Equals(CrawlerUserAgent))
            {
                lock (RequestsWaitingForResponse)
                {
                    if (!RequestsWaitingForResponse.Contains(Req.LogId))
                    {
                        RequestsWaitingForResponse.Add(Req.LogId);
                    }
                }
            }
        }

        internal static void CapturedCrawlResponse(Session Sess)
        {
            if (!PassiveCrawlerRunning) return;
            
            Request Req = Sess.Request;
            Response Res = Sess.Response;

            if (Req.Headers.Has("User-Agent") && Req.Headers.Get("User-Agent").Equals(CrawlerUserAgent))
            {
                lock (RequestsWaitingForResponse)
                {
                    if (RequestsWaitingForResponse.Contains(Req.LogId))
                    {
                        RequestsWaitingForResponse.Remove(Req.LogId);
                    }
                }
            }
            FindWaitTime(Res);
            ScrapeUrls(Req, Res);
        }

        static void FindWaitTime(Response Res)
        {
            string RefreshHeaderVal = "";
            if (Res.Headers.Has("Refresh"))
            {
                RefreshHeaderVal = Res.Headers.Get("Refresh");
            }
            else
            {
                List<string> Metas = Res.Html.GetMetaContent("http-equiv", "Refresh");
                if(Metas.Count > 0) RefreshHeaderVal= Metas[0];
            }
            if (RefreshHeaderVal.Length > 0)
            {
                string[] RefreshHeaderParts = RefreshHeaderVal.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                if (RefreshHeaderParts.Length > 1)
                {
                    try
                    {
                        string RefPageWaitTime = (Int32.Parse(RefreshHeaderParts[0].Trim()) * 1000).ToString();
                        if (PageWaitTime == "unknown" || PageWaitTime == "0")
                        {
                            PageWaitTime = RefPageWaitTime;
                        }
                        else
                        {
                            try
                            {
                                if (Int32.Parse(RefPageWaitTime) > Int32.Parse(PageWaitTime))
                                {
                                    PageWaitTime = RefPageWaitTime;
                                }
                            }
                            catch { PageWaitTime = RefPageWaitTime; }
                        }
                        return;
                    }
                    catch { }
                }
            }
            if (Res.BodyString.Contains("setTimeout(") || Res.BodyString.Contains("setInterval("))
            {
                int ScrapedWaitTime = ScrapeSetTimeoutWaitTime(Res.BodyString);
                if (ScrapedWaitTime > 0)
                {
                    string TimeoutPageWaitTime = ScrapedWaitTime.ToString();
                    if (PageWaitTime == "unknown" || PageWaitTime == "0")
                    {
                        PageWaitTime = TimeoutPageWaitTime;
                    }
                    else
                    {
                        try
                        {
                            if (Int32.Parse(TimeoutPageWaitTime) > Int32.Parse(PageWaitTime))
                            {
                                PageWaitTime = TimeoutPageWaitTime;
                            }
                        }
                        catch { PageWaitTime = TimeoutPageWaitTime; }
                    }
                }
                else
                {
                    PageWaitTime = "unknown";
                }
            }
        }

        public static int ScrapeSetTimeoutWaitTime(string Source)
        {
            int MaxExtractedWaitTime = 0;
            int TimeWaitTimeOne = GetWaitTime(Source, "setTimeout(");
            int TimeWaitTimeTwo = GetWaitTime(Source, "setInterval(");
            if (TimeWaitTimeOne > TimeWaitTimeTwo)
            {
                MaxExtractedWaitTime = TimeWaitTimeOne;
            }
            else
            {
                MaxExtractedWaitTime = TimeWaitTimeTwo;
            }
            return MaxExtractedWaitTime;
        }

        static int GetWaitTime(string _Source, string _WaitCmd)
        {
            int Pointer = 0;

            int MaxWaitTime = 0;

            const string CodeState = "code";
            const string SingleQuoteState = "single_quote";
            const string DoubleQuoteState = "double_quote";
            const string QuoteLessCommandState = "quote_less_command";
            const string CommandArgumentState = "command_argument";
            const string CommandFirstArgumentEndState = "command_first_argument_end";

            string State = CodeState;

            while (Pointer < _Source.Length)
            {
                switch (State)
                {
                    case (SingleQuoteState):
                        while (Pointer < _Source.Length)
                        {
                            if (_Source[Pointer] == '\'' && _Source[Pointer - 1] != '\\')
                            {
                                State = CommandFirstArgumentEndState;
                                Pointer++;
                                break;
                            }
                            Pointer++;
                        }
                        break;
                    case (DoubleQuoteState):
                        while (Pointer < _Source.Length)
                        {
                            if (_Source[Pointer] == '"' && _Source[Pointer - 1] != '\\')
                            {
                                State = CommandFirstArgumentEndState;
                                Pointer++;
                                break;
                            }
                            Pointer++;
                        }
                        break;
                    case (CommandFirstArgumentEndState):
                        StringBuilder SB = new StringBuilder();
                        while (Pointer < _Source.Length)
                        {
                            if (_Source[Pointer] == ')')
                            {
                                State = CodeState;
                                try
                                {
                                    int WaitTime = Int32.Parse(SB.ToString().Trim().Trim(',').Trim());
                                    if (WaitTime > MaxWaitTime)
                                    {
                                        MaxWaitTime = WaitTime;
                                    }
                                }
                                catch { }
                                Pointer++;
                                break;
                            }
                            else
                            {
                                SB.Append(_Source[Pointer]);
                                Pointer++;
                            }
                        }
                        break;
                    case(QuoteLessCommandState):
                        if (_Source[Pointer] == ',')
                        {
                            State = CommandFirstArgumentEndState;
                            break;
                        }
                        Pointer++;
                        break;
                    case (CommandArgumentState):
                        while (Pointer < _Source.Length)
                        {
                            if (_Source[Pointer] == '"')
                            {
                                State = DoubleQuoteState;
                                Pointer++;
                                break;
                            }
                            else if (_Source[Pointer] == '\'')
                            {
                                State = SingleQuoteState;
                                Pointer++;
                                break;
                            }
                            else if (_Source[Pointer] == ' ' || _Source[Pointer] == '\t')
                            {
                                //ignore
                            }
                            else
                            {
                                State = QuoteLessCommandState;
                                Pointer++;
                                break;
                            }
                            Pointer++;
                        }
                        break;
                    default:
                        if (Pointer + _WaitCmd.Length + 4 < _Source.Length)
                        {
                            bool MatchFound = true;
                            for (int i = 0; i < _WaitCmd.Length; i++)
                            {
                                if (_Source[Pointer + i] != _WaitCmd[i])
                                {
                                    MatchFound = false;
                                    break;
                                }
                            }
                            if (MatchFound)
                            {
                                State = CommandArgumentState;
                                Pointer += _WaitCmd.Length;
                                continue;
                            }
                        }
                        Pointer++;
                        break;
                }
            }
            return MaxWaitTime;
        }

        public static void ScrapeUrls(Request Req, Response Res)
        {
            List<string> LocalScrapedUrls = new List<string>();
            if (Res.IsHtml)
            {
                foreach (string Comment in Res.Html.Comments)
                {
                    LocalScrapedUrls.AddRange(ScrapeUrls(Req, Comment));
                }
                foreach (string Script in Res.Html.GetJavaScript())
                {
                    LocalScrapedUrls.AddRange(ScrapeUrls(Req, Script));
                }
            }
            else if (Res.IsJson)
            {
                FormatParameters JsonParams = FormatPlugin.GetJsonParameters(Res);
                for (int i = 0; i < JsonParams.Count; i++)
                {
                    LocalScrapedUrls.AddRange(ScrapeUrls(Req, JsonParams.GetValue(i)));
                }
            }
            else if (Res.IsJavaScript)
            {
                LocalScrapedUrls.AddRange(ScrapeUrls(Req, Res.BodyString));
            }
            if (Res.IsRedirect)
            {
                try
                {
                    HTML ResHtml = new HTML(Res.BodyString);
                    foreach (string Link in ResHtml.Links)
                    {
                        string FullUrl = Req.RelativeUrlToAbsoluteUrl(Link);
                        if (!FullUrl.Equals(Req.FullUrl))
                        {
                            if (!LocalScrapedUrls.Contains(FullUrl)) LocalScrapedUrls.Add(FullUrl);
                        }
                    }
                }
                catch { }
            }
            lock (ScrapedUrls)
            {
                ScrapedUrls.AddRange(LocalScrapedUrls);
            }
        }

        public static List<string> ScrapeUrls(Request Req, string Text)
        {
            List<string> Urls = new List<string>();
            
            foreach (string UrlStartMarker in UrlsStartMarkers)
            {
                int Pointer = 0;
                while (Pointer < Text.Length)
                {
                    string Quote = "";
                    string UrlValue = "";

                    int UrlStartIndex = Text.IndexOf(UrlStartMarker, Pointer);
                    if (UrlStartIndex > -1)
                    {
                        Quote = GetStartQuote(Text, UrlStartIndex);
                        UrlValue = ReadTillEndOfUrl(Quote, Text, UrlStartIndex);
                        try
                        {
                            string FullUrl = Req.RelativeUrlToAbsoluteUrl(UrlValue);
                            if (!Tools.HasInvalidUrlCharacters(FullUrl) && !FullUrl.Equals(Req.FullUrl))
                            {
                                Request TestReq = new Request(FullUrl);
                                if (!Urls.Contains(FullUrl)) Urls.Add(FullUrl);
                                Pointer = UrlStartIndex + UrlValue.Length;
                                continue;
                            }
                        }
                        catch 
                        {}
                    }
                    else
                    {
                        break;
                    }
                    Pointer = Pointer + UrlStartMarker.Length;
                }
            }

            foreach (string FileExt in UrlExtensionsToScrape)
            {
                int Pointer = 0;
                while (Pointer < Text.Length)
                {
                    int ExtensionStartIndex = Text.IndexOf(FileExt, Pointer);
                    if (ExtensionStartIndex > -1)
                    {
                        string UrlStartPart = ReadTillStartOfUrl(Text, ExtensionStartIndex -1);
                        string Quote = GetStartQuote(Text, ExtensionStartIndex - UrlStartPart.Length);
                        string UrlEndPart = ReadTillEndOfUrl(Quote, Text, ExtensionStartIndex);
                        string Url = string.Concat(UrlStartPart, UrlEndPart);
                        try
                        {
                            string FullUrl = Req.RelativeUrlToAbsoluteUrl(Url);
                            if (!Tools.HasInvalidUrlCharacters(FullUrl) && !FullUrl.Equals(Req.FullUrl))
                            {
                                Request TempReq = new Request(FullUrl);
                                if(!Urls.Contains(FullUrl)) Urls.Add(FullUrl);
                                Pointer = ExtensionStartIndex + UrlEndPart.Length;
                                continue;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        break;
                    }
                    Pointer = Pointer + FileExt.Length;
                }
            }
            return Urls;
        }

        static string ReadTillStartOfUrl(string Text, int Index)
        {
            List<char> Chars = new List<char>();
            int Pointer = Index;
            while(Pointer > -1)
            {
                if (Text[Pointer] == '\'' || Text[Pointer] == '"' || Text[Pointer] == ' ')
                {
                    Chars.Reverse();
                    return new String(Chars.ToArray());
                }
                else
                {
                    Chars.Add(Text[Pointer]);
                }
                Pointer--;
            }
            return "";
        }

        static string GetStartQuote(string Text, int Index)
        {
            if (Index > 0)
            {
                if (Text[Index - 1] == '\'' || Text[Index - 1] == '"') return Text[Index - 1].ToString();
            }
            return "";
        }

        static string ReadTillEndOfUrl(string Quote, string Text, int Index)
        {
            StringBuilder SB = new StringBuilder();

            int Pointer = Index;
            while (Pointer < Text.Length)
            {
                if ((Quote.Length > 0 && Text[Pointer].ToString() == Quote) || Text[Pointer] == ' ' || Text[Pointer] == '\n' || Text[Pointer] == '\r' || Text[Pointer] == '\t')
                {
                    return SB.ToString();
                }
                else
                {
                    SB.Append(Text[Pointer]);
                }
                
                Pointer++;
            }
            return "";
        }

        internal static void SetCrawlerUserAgent(string Ua)
        {
            CrawlerUserAgent = Ua;
        }

        internal static bool AreAnyRequestsWaitingForResponses()
        {
            return RequestsWaitingForResponse.Count > 0;
        }

        internal static List<string> GetListOfScrapedUrls()
        {
            List<string> UrlsToReturn = new List<string>();
            lock (ScrapedUrls)
            {
                UrlsToReturn.AddRange(ScrapedUrls);
                ScrapedUrls.Clear();
            }
            return UrlsToReturn;
        }

        internal static void NewPageLoad()
        {
            PageWaitTime = "0";
            lock (RequestsWaitingForResponse)
            {
                RequestsWaitingForResponse.Clear();
            }
        }

        internal static string GetPageWaitTime()
        {
            return PageWaitTime;
        }
    }
}
