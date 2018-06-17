using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.RestApi
{
    internal class PassiveCrawlerApi
    {
        internal static void LoadApis()
        {
            ApiCallHandler.AddCoreHandler("passivecrawler/start", InformStartOfCrawling);
            ApiCallHandler.AddCoreHandler("passivecrawler/end", InformEndOfCrawling);
            ApiCallHandler.AddCoreHandler("passivecrawler/pending_responses", GetPendingResponseStatus);
            ApiCallHandler.AddCoreHandler("passivecrawler/scraped_urls", GetScrapedUrls);
            ApiCallHandler.AddCoreHandler("passivecrawler/new_page_load", InformNewPageLoad);
            ApiCallHandler.AddCoreHandler("passivecrawler/get_wait_time", RespondWithPageWaitTime);
        }
        
        internal static void InformStartOfCrawling(Request Req, Response Res)
        {
            if(Req.Url.Equals(string.Format("{0}passivecrawler/start", ApiCallHandler.CoreApiUrlStart), StringComparison.OrdinalIgnoreCase))
            {
                PassiveCrawler.PassiveCrawlerRunning = true;
                Res.BodyString = "OK";
                PassiveCrawler.SetCrawlerUserAgent(Req.UserAgent);
            }
        }

        internal static void InformEndOfCrawling(Request Req, Response Res)
        {
            if (Req.Url.Equals(string.Format("{0}passivecrawler/end", ApiCallHandler.CoreApiUrlStart), StringComparison.OrdinalIgnoreCase))
            {
                PassiveCrawler.PassiveCrawlerRunning = false;
                Res.BodyString = "OK";
            }
        }

        internal static void GetPendingResponseStatus(Request Req, Response Res)
        {
            if (Req.Url.Equals(string.Format("{0}passivecrawler/pending_responses", ApiCallHandler.CoreApiUrlStart), StringComparison.OrdinalIgnoreCase))
            {
                if (PassiveCrawler.AreAnyRequestsWaitingForResponses())
                {
                    Res.BodyString = "YES";
                }
                else
                {
                    Res.BodyString = "NO";
                }
            }
        }

        internal static void GetScrapedUrls(Request Req, Response Res)
        {
            if (Req.Url.Equals(string.Format("{0}passivecrawler/scraped_urls", ApiCallHandler.CoreApiUrlStart), StringComparison.OrdinalIgnoreCase))
            {
                StringBuilder SB = new StringBuilder();
                foreach (string Url in PassiveCrawler.GetListOfScrapedUrls())
                {
                    SB.AppendLine(Url);
                }
                Res.BodyString = SB.ToString();
            }
        }

        internal static void InformNewPageLoad(Request Req, Response Res)
        {
            PassiveCrawler.NewPageLoad();
            Res.BodyString = "OK";
        }

        internal static void RespondWithPageWaitTime(Request Req, Response Res)
        {
            Res.BodyString = PassiveCrawler.GetPageWaitTime();
        }
    }
}
