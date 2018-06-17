using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IronWASP.FormatPlugins
{
    public class LightXmlParser
    {
        int Pointer = 0;
        LightXmlParserStates CurrentState = LightXmlParserStates.XmlStartElement;

        Stack<string> TagStack = new Stack<string>();

        List<string[]> ParseOutTextNodes(string InputXml)
        {
            
            

            /*
            while (Pointer < InputXml.Length)
            {
                switch (InputXml[Pointer])
                {

                }
            }
            */
            return new List<string[]>();
        }

        void ReadTillElementStart()
        {

        }

        void ReadTillElementEnd()
        {

        }

        void ReadTillAttributeEnd()
        {

        }

        void ReadVersionInfo()
        {

        }
    }

    public enum LightXmlParserStates
    {
        XmlStartElement,
        XmlEndElement,
        XmlSingleQuotedAttribute,
        XmlDoubleQuotedAttribute,
        XmlTextNode,
        None
    }
}
