using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.IronHtml
{
    public class ElementAttributes
    {
        public List<ElementAttribute> Attributes = new List<ElementAttribute>();
        HtmlAgilityPack.HtmlAttributeCollection Attrs = null;

        public ElementAttributes(HtmlAgilityPack.HtmlAttributeCollection Attrs)
        {
            this.Attrs = Attrs;
        }

        public bool Has(string Name)
        {
            return this.Attrs.Contains(Name);
        }

        public ElementAttribute Get(string Name)
        {
            try
            {
                return new ElementAttribute(Attrs[Name]);
            }
            catch
            {
                return null;
            }
        }
    }
}
