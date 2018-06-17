using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace IronWASP.IronHtml
{
    public class FormElement : Element
    {
        List<InputElement> InputElements = new List<InputElement>();
        List<Element> SelectElements = new List<Element>();
        
        public string Method
        {
            get
            {
                if (HasAttribute("method"))
                {
                    return GetAttribute("method");
                }
                return "GET";
            }
        }

        public string Action
        {
            get
            {
                if (HasAttribute("action"))
                {
                    return GetAttribute("action").TrimStart();
                }
                return "";
            }
        }

        public bool IsAbsoluteAction
        {
            get
            {
                try
                {
                    Request Req = new Request(Action);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsJavaScriptAction
        {
            get
            {
                if (Action.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }

        public string GetAbsoluteAction(Request Req)
        {
            if(!IsJavaScriptAction)
            {
                return  Req.RelativeUrlToAbsoluteUrl(Action);
            }
            else
            {
                return Req.FullUrl;
            }
        }

        public int InputFieldCount
        {
            get
            {
                return InputElements.Count;
            }
        }

        public int ParametersCount
        {
            get
            {
                int NIFC = 0;
                List<string> RadioElementNames = new List<string>();

                foreach (InputElement IE in InputElements)
                {
                    if(IE.HasName)
                    {
                        if (IE.ElementType == InputElementType.Radio)
                        {
                            if (!RadioElementNames.Contains(IE.Name))
                            {
                                RadioElementNames.Add(IE.Name);
                                NIFC++;
                            }
                        }
                        else
                        {
                            NIFC++;
                        }
                    }
                }
                foreach (Element Ele in SelectElements)
                {
                    if (Ele.HasName)
                    {
                        NIFC++;
                    }
                }
                return NIFC;
            }
        }

        public bool HasPasswordField
        {
            get
            {
                foreach (InputElement InEl in InputElements)
                {
                    if (InEl.ElementType == InputElementType.Password) return true;
                }
                return false;
            }
        }

        public FormElement(HtmlAgilityPack.HtmlNode Node, int NodeIndex) : base(Node, NodeIndex)
        {
            HtmlNodeCollection NodesColl = Node.SelectNodes(".//input");
            if (NodesColl != null)
            {
                for (int i = 0; i < NodesColl.Count; i++)
                {
                    InputElements.Add(new InputElement(NodesColl[i], i));
                }
            }

            NodesColl = Node.SelectNodes(".//select");
            if (NodesColl != null)
            {
                for (int i = 0; i < NodesColl.Count; i++)
                {
                    SelectElements.Add(new Element(NodesColl[i], i));
                }
            }
        }

        public bool HasInputField(string Name)
        {
            foreach (InputElement InEl in InputElements)
            {
                if (InEl.HasName && InEl.Name.Equals(Name)) return true;
            }
            return false;
        }
        public bool HasSelectField(string Name)
        {
            foreach (Element El in SelectElements)
            {
                if (El.HasName && El.Name.Equals(Name)) return true;
            }
            return false;
        }

        public InputElement GetInputField(string Name)
        {
            foreach (InputElement InEl in InputElements)
            {
                if (InEl.HasName && InEl.Name.Equals(Name)) return InEl;
            }
            return null;
        }

        public List<InputElement> GetInputFields(string Name)
        {
            List<InputElement> Fields = new List<InputElement>();
            foreach (InputElement InEl in InputElements)
            {
                if (InEl.HasName && InEl.Name.Equals(Name)) Fields.Add(InEl);
            }
            return Fields;
        }

        public List<string> GetSelectOptions(string Name)
        {
            List<string> Fields = new List<string>();
            foreach (Element El in SelectElements)
            {
                if (El.HasName && El.Name.Equals(Name))
                {
                    HtmlNodeCollection OptionColl = El.SelectNodes(".//option");
                    if (OptionColl != null)
                    {
                        for(int i=0; i < OptionColl.Count; i++)
                        {
                            try
                            {
                                Fields.Add((new Element(OptionColl[i], i)).Attributes.Get("value").Value);
                            }
                            catch 
                            {
                                //Exception thrown probably because there was not attribute named 'value' 
                            }
                        }
                    }
                }
            }
            return Fields;
        }
        public Request GetFormSubmission(Request Req)
        {
            Request NewFormSubReq = Crawler.GetFormSubmissionWithActualValue(Req, Node, new CookieStore());
            foreach (string Name in Req.Headers.GetNames())
            {
                if (!(Name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || Name.Equals("Cookie", StringComparison.OrdinalIgnoreCase) || Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
                {
                    NewFormSubReq.Headers.Set(Name, Req.Headers.GetAll(Name));
                }
            }
            return NewFormSubReq;
        }

        public Request GetFormSubmissionWithHiddenValuesFromFormAndOtherFromSecondArgument(Request Req, Request ReqToUpdateFrom)
        {
            Request FormSub = Crawler.GetFormSubmissionWithActualValue(Req, Node, new CookieStore());
            Parameters SourceParams = ReqToUpdateFrom.Query;
            Parameters DestiParams = FormSub.Query;
            if (FormSub.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                SourceParams = ReqToUpdateFrom.Body;
                DestiParams = FormSub.Body;
            }
            foreach (string Name in DestiParams.GetNames())
            {
                if (HasInputField(Name))
                {
                    InputElement InEl = GetInputField(Name);
                    if (InEl.ElementType != InputElementType.Hidden)
                    {
                        if (FormSub.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                        {
                            FormSub.Body.Set(Name, SourceParams.GetAll(Name));
                        }
                        else
                        {
                            FormSub.Query.Set(Name, SourceParams.GetAll(Name));
                        }

                    }
                }
            }
            return FormSub;
        }

        public bool DoInputFieldNamesMatchRequest(Request Req)
        {
            return DoesInputFieldsMatchRequestParameters(Req, 1);
        }

        public bool DoHiddenInputFieldValuesMatchRequest(Request Req)
        {
            return DoesInputFieldsMatchRequestParameters(Req, 2);
        }

        public bool DoAllInputFieldValuesMatchRequest(Request Req)
        {
            return DoesInputFieldsMatchRequestParameters(Req, 3);
        }

        bool DoesInputFieldsMatchRequestParameters(Request Req, int MatchLevel)
        {
            Parameters Params = null;
            if (this.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                Params = Req.Body;
            }
            else
            {
                Params = Req.Query;
            }

            if (Params.Count != this.ParametersCount) return false;
            foreach (string Name in Params.GetNames())
            {
                if (!(this.HasInputField(Name) || this.HasSelectField(Name))) return false;
                if (MatchLevel > 1)
                {
                    if (this.HasSelectField(Name))
                    {
                        if (MatchLevel > 2)
                        {
                            if (!this.GetSelectOptions(Name).Contains(Params.Get(Name))) return false;
                        }
                    }
                    else
                    {
                        List<string> Values = new List<string>();
                        foreach (InputElement InEl in this.GetInputFields(Name))
                        {
                            if (MatchLevel == 2)
                            {
                                if (InEl.ElementType == InputElementType.Hidden) Values.Add(InEl.Value);
                            }
                            else
                            {
                                Values.Add(InEl.Value);
                            }
                        }
                        foreach (string Val in Values)
                        {
                            if (!Params.GetAll(Name).Contains(Val)) return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
