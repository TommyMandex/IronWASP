using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP
{
    public class FormatParameters
    {
        string[,] XmlParameters = null;

        public FormatParameters(string[,] _XmlParameters)
        {
            this.XmlParameters = _XmlParameters;
        }
        
        public int Count
        {
            get
            {
                return this.XmlParameters.GetLength(0);
            }
        }

        public string GetName(int Index)
        {
            return this.XmlParameters[Index, 0];
        }

        public string GetValue(int Index)
        {
            return this.XmlParameters[Index, 1];
        }
    }
}
