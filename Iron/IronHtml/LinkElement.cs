using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.IronHtml
{
    public class LinkElement : Element
    {
        public LinkElement(HtmlAgilityPack.HtmlNode Node, int NodeIndex) : base(Node, NodeIndex)
        {
            
        }

        public string Href
        {
            get
            {
                if (HasHref)
                {
                    return GetAttribute("href").TrimStart();
                }
                return "";
            }
        }

        public bool HasHref
        {
            get
            {
                return HasAttribute("href");
            }
        }

        public bool IsAbsoluteHref
        {
            get
            {
                try
                {
                    Request Req = new Request(Href);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsJavaScriptHref
        {
            get
            {
                if (Href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }

        public Request GetLinkClick(Request Req)
        {
            Request NewLinkClickReq = new Request(Req.RelativeUrlToAbsoluteUrl(Href));
            foreach (string Name in Req.Headers.GetNames())
            {
                if (!(Name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || Name.Equals("Cookie", StringComparison.OrdinalIgnoreCase) || Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                {
                    NewLinkClickReq.Headers.Set(Name, Req.Headers.GetAll(Name));
                }
            }
            return NewLinkClickReq;
        }
    }
}
