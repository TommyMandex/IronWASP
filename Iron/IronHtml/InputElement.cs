using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace IronWASP.IronHtml
{
    public class InputElement :  Element
    {
        InputElementType EleType = InputElementType.Text;

        public InputElementType ElementType
        {
            get
            {
                return EleType;
            }
        }

        public InputElement(HtmlNode Node, int Index): base(Node, Index)
        {
            SetInputElementType();
        }

        public string Value
        {
            get
            {
                if (HasAttribute("value"))
                {
                    return GetAttribute("value");
                }
                return "";
            }
        }

        void SetInputElementType()
        {
            if (HasAttribute("type"))
            {
                string TypeVal = GetAttribute("type");
                if (TypeVal.Equals("password", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Password;
                }
                else if (TypeVal.Equals("hidden", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Hidden;
                }
                else if (TypeVal.Equals("submit", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Submit;
                }
                else if (TypeVal.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Checkbox;
                }
                else if (TypeVal.Equals("radio", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Radio;
                }
                else if (TypeVal.Equals("text", StringComparison.OrdinalIgnoreCase))
                {
                    EleType = InputElementType.Text;
                }
            }
        }
    }



    public enum InputElementType
    {
        Text,
        Hidden,
        Password,
        Checkbox,
        Radio,
        Submit
    }
}
