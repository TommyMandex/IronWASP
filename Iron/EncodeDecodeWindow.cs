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
// along with IronWASP.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IronWASP
{
    public partial class EncodeDecodeWindow : Form
    {
        internal static bool UrlEncode = true;
        internal static bool HtmlEncode = false;
        internal static bool HexEncode = false;
        internal static bool Base64Encode = false;
        internal static bool ToHex = false;

        internal static bool UrlDecode = false;
        internal static bool HtmlDecode = false;
        internal static bool HexDecode = false;
        internal static bool Base64Decode = false;

        internal static bool MD5 = false;
        internal static bool SHA1 = false;
        internal static bool SHA256 = false;
        internal static bool SHA384 = false;
        internal static bool SHA512 = false;

        internal static EncodeDecode_d Command;
        internal static string Input = "";
        internal static Thread T;

        bool IsInputBinary = false;

        string LastResultString = "";
        byte[] LastResultsBytes = null;
        string InputString = "";
        byte[] InputBytes = null;
        string LastCommand = "";
        string CurrentCommand = "";

        const string UrlEncodeCmd = "UrlEncode";
        const string UrlDecodeCmd = "UrlDecode";
        const string Base64EncodeCmd = "Base64Encode";
        const string Base64DecodeCmd = "Base64Decode";
        const string HtmlEncodeCmd = "HtmlEncode";
        const string HtmlDecodeCmd = "HtmlDecode";
        const string HexEncodeCmd = "HexEncode";
        const string HexDecodeCmd = "HexDecode";
        const string GzipEncodeCmd = "GzipEncode";
        const string GzipDecodeCmd = "GzipDecode";
        const string ToHexCmd = "ToHex";

        const string Md5HashCmd = "Md5Hash";
        const string ShaHashCmd = "ShaHash";
        const string Sha256HashCmd = "Sha256Hash";
        const string Sha384HashCmd = "Sha384Hash";
        const string Sha512HashCmd = "Sha512Hash";

        public EncodeDecodeWindow()
        {
            InitializeComponent();
        }

        internal delegate string EncodeDecode_d(string Input);

        void StartExecution()
        {
            DisableAllButtons();
            StatusTB.Text = "Executing...";
            OutputTB.ClearData();
            InputString = InputTB.GetText();
            InputBytes = InputTB.GetBytes();
            //Input = InputTB.GetText();
            T = new Thread(ExecuteCommand);
            T.Start();
        }

        void ExecuteCommand()
        {
            string Status = "";
            try
            {
                switch(CurrentCommand)
                {
                    case(UrlEncodeCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.UrlEncodeBytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.UrlEncode(InputString), Status);
                        }
                        break;
                    case(Base64EncodeCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.Base64EncodeBytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.Base64Encode(InputString), Status);
                        }
                        break;
                    case (HtmlEncodeCmd):
                        ShowEncodeDecodeResult(Tools.HtmlEncode(InputString), Status);
                        break;
                    case (HexEncodeCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.HexEncodeBytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.HexEncode(InputString), Status);
                        }
                        break;
                    case (ToHexCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.ToHex(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.ToHex(InputString), Status);
                        }
                        break;
                    case (GzipEncodeCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.GzipEncodeBytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.GzipEncode(InputString), Status);
                        }
                        break;
                    case (UrlDecodeCmd):
                        ShowEncodeDecodeResult(Tools.UrlDecodeToBytes(InputString), Status);
                        break;
                    case (Base64DecodeCmd):
                        ShowEncodeDecodeResult(Tools.Base64DecodeToBytes(InputString), Status);
                        break;
                    case (HtmlDecodeCmd):
                        ShowEncodeDecodeResult(Tools.HtmlDecode(InputString), Status);
                        break;
                    case (HexDecodeCmd):
                        ShowEncodeDecodeResult(Tools.HexDecode(InputString), Status);
                        break;
                    case (GzipDecodeCmd):
                        ShowEncodeDecodeResult(Tools.GzipDecodeToBytes(InputBytes), Status);
                        break;
                    case (Md5HashCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.MD5Bytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.MD5(InputString), Status);
                        }
                        break;
                    case (ShaHashCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.SHA1Bytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.SHA1(InputString), Status);
                        }
                        break;
                    case (Sha256HashCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.SHA256Bytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.SHA256(InputString), Status);
                        }
                        break;
                    case (Sha384HashCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.SHA384Bytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.SHA384(InputString), Status);
                        }
                        break;
                    case (Sha512HashCmd):
                        if (ShouldGetBytesFromInput())
                        {
                            ShowEncodeDecodeResult(Tools.SHA512Bytes(InputBytes), Status);
                        }
                        else
                        {
                            ShowEncodeDecodeResult(Tools.SHA512(InputString), Status);
                        }
                        break;
                    default:
                        ShowEncodeDecodeResult("", "Command not implemented");
                        break;
                }
            }
            catch (Exception Exp)
            {
                ShowEncodeDecodeResult("", "Error: " + Exp.Message);
            }
        }

        delegate void ShowEncodeDecodeResult_d(string Result, string Message);
        internal void ShowEncodeDecodeResult(string Result, string Message)
        {
            if (this.InvokeRequired)
            {
                ShowEncodeDecodeResult_d SEDR_d = new ShowEncodeDecodeResult_d(ShowEncodeDecodeResult);
                this.Invoke(SEDR_d, new object[] { Result, Message });
            }
            else
            {
                OutputTB.SetText(Result);
                if (Message.Length > 0)
                {
                    StatusTB.BackColor = Color.Red;
                }
                else
                {
                    StatusTB.BackColor = SystemColors.Control;
                }
                StatusTB.Text = Message;
                EnableAllEncodeDecodeCommandButtons();
            }
        }

        delegate void ShowEncodeDecodeResultBytes_d(byte[] Result, string Message);
        internal void ShowEncodeDecodeResult(byte[] Result, string Message)
        {
            if (this.InvokeRequired)
            {
                ShowEncodeDecodeResultBytes_d SEDR_d = new ShowEncodeDecodeResultBytes_d(ShowEncodeDecodeResult);
                this.Invoke(SEDR_d, new object[] { Result, Message });
            }
            else
            {
                OutputTB.SetBytes(Result);
                if (Message.Length > 0)
                {
                    StatusTB.BackColor = Color.Red;
                }
                else
                {
                    StatusTB.BackColor = SystemColors.Control;
                }
                StatusTB.Text = Message;
                EnableAllEncodeDecodeCommandButtons();
            }
        }

        void EnableAllEncodeDecodeCommandButtons()
        {
            UrlEncodeBtn.Enabled = true;
            HtmlEncodeBtn.Enabled = true;
            HexEncodeBtn.Enabled = true;
            Base64EncodeBtn.Enabled = true;
            ToHexBtn.Enabled = true;
            GzipEncodeBtn.Enabled = true;
            UrlDecodeBtn.Enabled = true;
            HtmlDecodeBtn.Enabled = true;
            HexDecodeBtn.Enabled = true;
            Base64DecodeBtn.Enabled = true;
            GzipDecodeBtn.Enabled = true;
            MD5Btn.Enabled = true;
            SHA1Btn.Enabled = true;
            SHA256Btn.Enabled = true;
            SHA384Btn.Enabled = true;
            SHA512Btn.Enabled = true;
        }

        void DisableAllButtons()
        {
            UrlEncodeBtn.Enabled = false;
            HtmlEncodeBtn.Enabled = false;
            HexEncodeBtn.Enabled = false;
            Base64EncodeBtn.Enabled = false;
            ToHexBtn.Enabled = false;
            GzipEncodeBtn.Enabled = false;
            UrlDecodeBtn.Enabled = false;
            HtmlDecodeBtn.Enabled = false;
            HexDecodeBtn.Enabled = false;
            Base64DecodeBtn.Enabled = false;
            GzipDecodeBtn.Enabled = false;
            MD5Btn.Enabled = false;
            SHA1Btn.Enabled = false;
            SHA256Btn.Enabled = false;
            SHA384Btn.Enabled = false;
            SHA512Btn.Enabled = false;
        }

        private void UrlEncodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = UrlEncodeCmd;
            //Command = new EncodeDecode_d(Tools.UrlEncode);
            StartExecution();
        }

        private void HtmlEncodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = HtmlEncodeCmd;
            //Command = new EncodeDecode_d(Tools.HtmlEncode);
            StartExecution();
        }

        private void HexEncodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = HexEncodeCmd;
            //Command = new EncodeDecode_d(Tools.HexEncode);
            StartExecution();
        }

        private void Base64EncodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Base64EncodeCmd;
            //Command = new EncodeDecode_d(Tools.Base64Encode);
            StartExecution();
        }

        private void ToHexBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = ToHexCmd;
            //Command = new EncodeDecode_d(Tools.ToHex);
            StartExecution();
        }

        private void GzipEncodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = GzipEncodeCmd;
            StartExecution();
        }

        private void UrlDecodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = UrlDecodeCmd;
            //Command = new EncodeDecode_d(Tools.UrlDecode);
            StartExecution();
        }

        private void HtmlDecodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = HtmlDecodeCmd;
            //Command = new EncodeDecode_d(Tools.HtmlDecode);
            StartExecution();
        }

        private void HexDecodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = HexDecodeCmd;
            //Command = new EncodeDecode_d(Tools.HexDecode);
            StartExecution();
        }

        private void Base64DecodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Base64DecodeCmd;
            //Command = new EncodeDecode_d(Tools.Base64Decode);
            StartExecution();
        }

        private void GzipDecodeBtn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = GzipDecodeCmd;
            StartExecution();
        }

        private void MD5Btn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Md5HashCmd;
            //Command = new EncodeDecode_d(Tools.MD5);
            StartExecution();
        }

        private void SHA1Btn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = ShaHashCmd;
            //Command = new EncodeDecode_d(Tools.SHA1);
            StartExecution();
        }

        private void SHA256Btn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Sha256HashCmd;
            //Command = new EncodeDecode_d(Tools.SHA256);
            StartExecution();
        }

        private void SHA384Btn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Sha384HashCmd;
            //Command = new EncodeDecode_d(Tools.SHA384);
            StartExecution();
        }

        private void SHA512Btn_Click(object sender, EventArgs e)
        {
            LastCommand = CurrentCommand;
            CurrentCommand = Sha512HashCmd;
            //Command = new EncodeDecode_d(Tools.SHA512);
            StartExecution();
        }

        bool ShouldGetBytesFromInput()
        {
            switch (LastCommand)
            {
                case (UrlDecodeCmd):
                case (Base64DecodeCmd):
                case (GzipDecodeCmd):
                case (HexDecodeCmd):
                case (GzipEncodeCmd):
                    return true;
                default:
                    if (IsInputBinary)
                        return true;
                    else
                        return false;
            }
        }

        bool ShouldGetBytesFromResult()
        {
            switch (CurrentCommand)
            {
                case (UrlDecodeCmd):
                case (Base64DecodeCmd):
                case (GzipDecodeCmd):
                case (HexDecodeCmd):
                case (GzipEncodeCmd):
                    return true;
                default:
                    return false;
            }
        }

        private void EncodeOutToEncodeInBtn_Click(object sender, EventArgs e)
        {
            if (ShouldGetBytesFromResult())
            {
                InputTB.SetBytes(OutputTB.GetBytes());
            }
            else
            {
                InputTB.SetText(OutputTB.GetText());
            }
        }

        private void InputTB_ValueChanged()
        {
            this.IsInputBinary = InputTB.IsBinary;
        }
    }
}
