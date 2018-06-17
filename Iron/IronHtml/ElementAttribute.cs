using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.IronHtml
{
    public class ElementAttribute
    {
        HtmlAgilityPack.HtmlAttribute Attr;
        string AttrName = "";
        string AttrValue = "";

        public ElementAttribute(HtmlAgilityPack.HtmlAttribute Attr)
        {
            this.Attr = Attr;
        }

        public string Name
        {
            get
            {
                return Attr.Name;
            }
        }

        public string Value
        {
            get
            {
                return Attr.Value;
            }
        }
    }
}
