using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP
{
    public class SiteMap
    {
        public static List<string> GetSitesList()
        {
            List<string> BaseUrls = ToBaseUrls(IronDB.GetUniqueHostsAndSslFromProbeLog());
            foreach(string Url in ToBaseUrls(IronDB.GetUniqueHostsAndSslFromProxyLog()))
            {
                if (!BaseUrls.Contains(Url)) BaseUrls.Add(Url);
            }
            return BaseUrls;
        }

        static List<string> ToBaseUrls(List<Dictionary<string, string>> Dicts)
        {
            List<string> BaseUrls = new List<string>();
            foreach (Dictionary<string, string> Dict in Dicts)
            {
                if (Dict["SSL"] == "1")
                {
                    BaseUrls.Add(string.Format("https://{0}/", Dict["HostName"]));
                }
                else
                {
                    BaseUrls.Add(string.Format("http://{0}/", Dict["HostName"]));
                }
            }
            return BaseUrls;
        }
    }
}
