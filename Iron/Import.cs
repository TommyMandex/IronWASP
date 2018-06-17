//
// Copyright 2011-2013 Lavakumar Kuppan
//
// This file is part of IronWASP
//
// IronWASP is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// IronWASP is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with IronWASP.  If not, see http://www.gnu.org/licenses/.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace IronWASP
{
    internal class Import
    {
        internal static Thread ImportThread;

        internal static void ImportLogsFromIronwaspProjectFile(string ProjectDir)
        {
            //IronUI.OpenImportForm();
            IronUI.SetUIVisibility(false);
            try
            {
                ImportThread.Abort();
            }
            catch { }
            ImportThread = new Thread(StartLogsImportFromIronwaspProjectFile);
            ImportThread.Start(ProjectDir);
        }

        internal static void StartLogsImportFromIronwaspProjectFile(object ProjectDir)
        {
            IronProxy.Stop();
            DirectoryInfo Di = new DirectoryInfo(ProjectDir.ToString());
            string NewDirName = string.Format("{0}\\ImportedFrom{1}", Di.Parent.FullName, Di.Name);
            int Counter = 0;
            while(Directory.Exists(NewDirName))
            {
                Counter++;
                NewDirName = string.Format("{0}\\ImportedFrom{1}_{2}", Di.Parent.FullName, Di.Name, Counter);
            }
            Directory.CreateDirectory(NewDirName);
            IronDB.UpdateLogFilePaths(NewDirName);
            IronDB.CreateNewLogFiles();
            //Proxy.ironlog
            //Probes.ironlog
            File.Copy(string.Format("{0}\\Proxy.ironlog", ProjectDir), string.Format("{0}\\Proxy.ironlog", NewDirName), true);
            File.Copy(string.Format("{0}\\Probes.ironlog", ProjectDir), string.Format("{0}\\Probes.ironlog", NewDirName), true);
            IronUI.StartUpdatingFullUIFromDB();
        }

        internal static void ImportBurpLog(string BurpLogFile)
        {
            IronUI.OpenImportForm();
            IronUI.SetUIVisibility(false);
            try
            {
                ImportThread.Abort();
            }
            catch { }
            ImportThread = new Thread(StartImportBurpLog);
            ImportThread.Start(BurpLogFile);
        }

        internal static void StartImportBurpLog(Object BurpLogFile)
        {
            try
            {
                using (StreamReader SR = new StreamReader(BurpLogFile.ToString()))
                {
                    ReadBurpMessages(SR, BurpLogFile.ToString());
                }
            }
            catch(Exception Exp)
            {
                IronUI.CloseImportForm();
                MessageBox.Show("Unable to import log - " + Exp.Message);
                return;
            }
            IronUI.SetUIVisibility(true);
            IronUI.CloseImportForm();
        }

        static void ReadBurpMessages(StreamReader Reader, string FileName)
        {
            try
            {
                XmlReaderSettings Settings = new XmlReaderSettings();
                Settings.ProhibitDtd = false;
                using (XmlReader XR = XmlReader.Create(Reader, Settings))
                {
                    ReadBurpXmlExport(XR);
                }
            }
            catch
            {
                try
                {
                    Reader.Close();
                }
                catch { }
                using (Reader = new StreamReader(FileName))
                {
                    ReadBurpLog(Reader);
                }
            }
        }

        static void ReadBurpXmlExport(XmlReader Reader)
        {
            Session Sess = null;

            while (Reader.Read())
            {
                if (Reader.NodeType == XmlNodeType.Element)
                {
                    switch (Reader.Name)
                    {
                        case("item"):
                            Sess = null;
                            break;
                        case("request"):
                            Request Req = null;
                            try
                            {
                                Req = new Request(ReadRequestResponseNodeValue(Reader), false, true);
                            }
                            catch { }
                            if (Req != null)
                            {
                                Sess = AddImportedRequestToIronWASP(Req);
                            }
                            break;
                        case ("response"):
                            Response Res = null;
                            try
                            {
                                Res = new Response(ReadRequestResponseNodeValue(Reader));
                            }
                            catch{}
                            if (Sess != null)
                            {
                                AddImportedResponseToIronWASP(Res, Sess);
                            }
                            break;
                    }
                }
            }
        }

        static string ReadRequestResponseNodeValue(XmlReader Reader)
        {
            bool Base64 = true;
            try
            {
                if (Reader.GetAttribute("base64") == "false")
                {
                    Base64 = false;
                }
            }
            catch { }
            Reader.Read();
            
            if (Base64)
            {
                try
                {
                    return Tools.Base64Decode(Reader.Value);
                }
                catch{}
            }
            return Reader.Value;
        }

        static void ReadBurpLog(StreamReader Reader)
        {
            Queue<string> ResponseLines = new Queue<string>();
            string MetaLine = "";

            List<string> Lines = new List<string>();
            Lines.Add(Reader.ReadLine());
            Lines.Add(Reader.ReadLine());
            Lines.Add(Reader.ReadLine());
            
            if (Reader.EndOfStream) return;
            if (Lines[0].Equals("======================================================") && Lines[1].IndexOf("  http") > 5 && Lines[1].IndexOf("  http") < 20 & Lines[2].Equals("======================================================"))
            {
                MetaLine = Lines[1];
                string[] Result = ReadBurpMessage(Reader);
                ProcessBurpMessage(Result[0], MetaLine);
                MetaLine = Result[1];
                if (MetaLine.Length == 0) return;
            }
            else
            {
                return;
            }

            while(!Reader.EndOfStream)
            {
                MetaLine = Lines[1];
                string[] Result = ReadBurpMessage(Reader);
                ProcessBurpMessage(Result[0], MetaLine);
                MetaLine = Result[1];
                if (MetaLine.Length == 0) return;
            }
        }

        static string[] ReadBurpMessage(StreamReader Reader)
        {
            string[] Result = new string[2];
            Queue<string> MessageLines = new Queue<string>();
            StringBuilder MessageBuilder = new StringBuilder();
            while(MessageLines.Count < 7 && !Reader.EndOfStream)
            {
                MessageLines.Enqueue(Reader.ReadLine());
            }
            while (!Reader.EndOfStream)
            {
                string[] ResponseBuffer = MessageLines.ToArray();
                if (ResponseBuffer[0].Equals("======================================================") && ResponseBuffer[1].Equals("") && ResponseBuffer[2].Equals("") && ResponseBuffer[3].Equals("") && ResponseBuffer[4].Equals("======================================================") && ResponseBuffer[5].IndexOf("  http") > 5 && ResponseBuffer[5].IndexOf("  http") < 20 && ResponseBuffer[6].Equals("======================================================"))
                {
                    Result[0] = MessageBuilder.ToString();
                    Result[1] = ResponseBuffer[5];
                    break;
                }
                else
                {
                    MessageBuilder.AppendLine(MessageLines.Dequeue());
                    MessageLines.Enqueue(Reader.ReadLine());
                }
                if (Reader.EndOfStream)
                {
                    Result[0] = MessageBuilder.ToString();
                    Result[1] = "";
                    break;
                }
            }
            return Result;
        }

        static void ProcessBurpMessage(string BurpMessage, string MetaLine)
        {
            string[] BurpMessageParts = BurpMessage.Split(new string[] { "\r\n======================================================\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
            Session IrSe = null;
            if (BurpMessageParts.Length > 0)
            {
                Request Req = ReadBurpRequest(BurpMessageParts[0], MetaLine);
                if (Req != null)
                {
                    try
                    {
                        IrSe = AddImportedRequestToIronWASP(Req);
                    }
                    catch { }
                    //IrSe = new Session(Req);
                    //IrSe.ID = Interlocked.Increment(ref Config.ProxyRequestsCount);
                    //IronUpdater.AddProxyRequest(IrSe.Request.GetClone(true));
                    //PassiveChecker.AddToCheckRequest(IrSe);
                }
            }
            if (BurpMessageParts.Length == 2)
            {
                if (IrSe != null)
                {
                    try
                    {
                        Response Res = new Response(BurpMessageParts[1]);
                        AddImportedResponseToIronWASP(Res, IrSe);
                        //IrSe.Response = Res;
                        //IrSe.Response.ID = IrSe.Request.ID;
                        //IronUpdater.AddProxyResponse(IrSe.Response.GetClone(true));
                        //PassiveChecker.AddToCheckResponse(IrSe);
                    }
                    catch {}
                }
            }
        }

        static Session AddImportedRequestToIronWASP(Request Req)
        {
            Session IrSe = new Session(Req);
            IrSe.ID = Interlocked.Increment(ref Config.ProxyRequestsCount);
            IronUpdater.AddProxyRequest(IrSe.Request.GetClone(true));
            PassiveChecker.AddToCheckRequest(IrSe);
            return IrSe;
        }

        static void AddImportedResponseToIronWASP(Response Res, Session IrSe)
        {
            IrSe.Response = Res;
            IrSe.Response.ID = IrSe.Request.ID;
            IronUpdater.AddProxyResponse(IrSe.Response.GetClone(true));
            PassiveChecker.AddToCheckResponse(IrSe);
        }


        static Request ReadBurpRequest(string RequestString, string MetaLine)
        {
            string[] MetaParts = MetaLine.Split(new string[] { "://" }, StringSplitOptions.RemoveEmptyEntries);
            if (MetaParts.Length != 2) return null;
            bool SSL = false;
            if (MetaParts[0].EndsWith("https")) SSL = true;
            try
            {
                Request Req = new Request(RequestString, SSL);
                return Req;
            }
            catch
            {
                return null;
            }
        }
    }
}
