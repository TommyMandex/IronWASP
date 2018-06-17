using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP.IronHtml
{
    public class ElementCollection
    {
        protected List<Element> Elements = new List<Element>();

        protected ElementCollection()
        {

        }
        
        public ElementCollection(List<Element> Eles)
        {
            Elements = Eles;
        }

        public int Count
        {
            get
            {
                return Elements.Count;
            }
        }

        public List<Element> GetElements()
        {
            return Elements;
        }

        public List<Element> GetElementsWithId(string Id)
        {
            return GetElementsWithPropertyValue("id", Id);
        }

        public List<Element> GetElementsWithName(string Name)
        {
            return GetElementsWithPropertyValue("name", Name);
        }

        public List<Element> GetElementsWithClass(string Class)
        {
            return GetElementsWithPropertyValue("class", Class);
        }

        public List<Element> GetElementsWithInnerText(string InnerText)
        {
            return GetElementsWithPropertyValue("innertext", InnerText);
        }

        List<Element> GetElementsWithPropertyValue(string Property, string Value)
        {
            List<Element> Result = new List<Element>();
            foreach (Element Ele in Elements)
            {
                switch (Property)
                {
                    case("id"):
                        if (Ele.HasId && Ele.Id.Equals(Value)) Result.Add(Ele);
                        break;
                    case ("name"):
                        if (Ele.HasName && Ele.Name.Equals(Value)) Result.Add(Ele);
                        break;
                    case ("class"):
                        if (Ele.HasClass && Ele.Class.Equals(Value)) Result.Add(Ele);
                        break;
                    case ("innertext"):
                        if (Ele.InnerText.Equals(Value)) Result.Add(Ele);
                        break;
                }
            }
            return Result;
        }
    }

    public class LinkElementCollection:ElementCollection
    {
        public LinkElementCollection(List<LinkElement> LinkEles)
        {
            List<Element> Eles = new List<Element>();
            foreach (LinkElement LinkEle in LinkEles)
            {
                Eles.Add(LinkEle);
            }
            base.Elements = Eles;
        }
    }

    public class FormElementCollection : ElementCollection
    {
        public FormElementCollection(List<FormElement> FormEles)
        {
            List<Element> Eles = new List<Element>();
            foreach (FormElement FormEle in FormEles)
            {
                Eles.Add(FormEle);
            }
            base.Elements = Eles;
        }
    }
}
