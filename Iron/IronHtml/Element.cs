using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace IronWASP.IronHtml
{
    public class Element
    {
        public ElementAttributes Attributes = null;
        protected HtmlAgilityPack.HtmlNode Node = null;
        int index = -1;


        //public Element(HtmlAgilityPack.HtmlNode Node)
        //{
        //    this.Node = Node;
        //}

        public Element(HtmlAgilityPack.HtmlNode Node, int NodeIndex)
        {
            this.Node = Node;
            this.index = NodeIndex;
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public bool HasIndex
        {
            get
            {
                return (Index >= -1);
            }
        }

        public bool HasId
        {
            get
            {
                return HasAttribute("id");
            }
        }

        public string Id
        {
            get
            {
                return GetAttribute("id");
            }
        }

        public bool HasName
        {
            get
            {
                return HasAttribute("name");
            }
        }

        public string Name
        {
            get
            {
                return GetAttribute("name");
            }
        }

        public bool HasClass
        {
            get
            {
                return HasAttribute("class");
            }
        }

        public string Class
        {
            get
            {
                return GetAttribute("class");
            }
        }

        public string InnerText
        {
            get
            {
                try
                {
                    return Node.InnerText;
                }
                catch
                {
                    return "";
                }
            }
        }

        public HtmlAgilityPack.HtmlNodeCollection SelectNodes(string Xpath)
        {
            return this.Node.SelectNodes(Xpath);
        }

        public bool HasAttribute(string Name)
        {
            return Node.Attributes.Contains(Name);
        }

        public string GetAttribute(string Name)
        {
            if (HasAttribute(Name))
            {
                return Tools.HtmlDecode(Node.Attributes[Name].Value);
            }
            else
            {
                throw new Exception(string.Format("Html element does not have attribute named {0}", Name));
            }
        }

        public List<string> AttributeNames
        {
            get
            {
                List<string> AttrNames = new List<string>();
                foreach (HtmlAgilityPack.HtmlAttribute Attr in Node.Attributes)
                {
                    AttrNames.Add(Attr.Name);
                }
                return AttrNames;
            }
        }
    }
}
