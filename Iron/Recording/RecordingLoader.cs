using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace IronWASP.Recording
{
    public partial class RecordingLoader : Form
    {
        static RecordingLoader OpenLoader = null;

        Thread RecordingLoadThread = null;

        public RecordingLoader()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                OpenLoader = new RecordingLoader();
                OpenLoader.Show();
            }
            OpenLoader.Activate();
        }

        static bool IsWindowOpen()
        {
            if (OpenLoader == null)
            {
                return false;
            }
            else if (OpenLoader.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LoadLL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenRecordingFileDialog.Title = "Open a Recording File";
            while (OpenRecordingFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo OpenedFile = new FileInfo(OpenRecordingFileDialog.FileName);
                StreamReader Reader = new StreamReader(OpenedFile.FullName);
                String RecordingXml = Reader.ReadToEnd();
                Reader.Close();
                try
                {
                    RecordingLoadThread.Abort();
                }
                catch { }
                RecordingLoadThread = new Thread(LoadRecording);
                RecordingLoadThread.Start(RecordingXml);
                LoadLL.Enabled = false;
                ShowMessage(false, "Loading recording, this could take a few minutes. Please wait....");
                LoadProgressBar.Visible = true;
                break;
            }
        }

        void LoadRecording(object XmlObj)
        {
            try
            {
                Recording Rec = Recording.FromXml(XmlObj.ToString());
                if (Recording.Has(Rec.Name))
                {
                    ShowMessage(true, "Unable to load this recording, a recording with this name is already loaded.");
                }
                if (Rec.IsLoginRecordingReplayable())
                {
                    Recording.Add(Rec);
                    ShowMessage(false, "Recording loaded successfully! Now you can make use of it in scans and tests.");
                }
                else
                {
                    ShowMessage(true, "Unable to load this recording, attempt to replay it failed.");
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception Exp)
            {
                IronException.Report("Error loading Recording File", Exp);
                ShowMessage(true, "Invalid recording! Could not be loaded.");
            }
        }

        delegate void ShowMessage_d(bool Error, string Message);
        void ShowMessage(bool Error, string Message)
        {
            if (MessageLbl.InvokeRequired)
            {
                ShowMessage_d CALL_d = new ShowMessage_d(ShowMessage);
                MessageLbl.Invoke(CALL_d, new object[] { Error, Message });
            }
            else
            {
                MessageLbl.Text = Message;
                if (Error)
                {
                    MessageLbl.ForeColor = Color.Red;
                }
                else
                {
                    MessageLbl.ForeColor = Color.Black;
                }
                LoadProgressBar.Visible = false;
                LoadLL.Enabled = true;
            }
        }

        private void RecordingLoader_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                RecordingLoadThread.Abort();
            }
            catch { }
        }
    }
}
