using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace IronWASP
{
    public partial class SessionView : UserControl
    {
        int intLogId = 0;
        string intLogSource = "Proxy";
        
        public SessionView()
        {
            InitializeComponent();
        }

        internal void LoadAndShowSession(int LogId, string LogSource)
        {
            this.intLogId = LogId;
            this.intLogSource = LogSource;

            if (this.ReqView.InvokeRequired)
            {
                Thread T = new Thread(LoadAndShowSession);
                T.Start();
            }
            else
            {
                LoadAndShowSession();
            }
        }

        void LoadAndShowSession()
        {
            this.ReqView.ClearRequest();
            this.ResView.ClearResponse();
            this.ReqView.ShowProgressBar(true);
            this.ResView.ShowProgressBar(true);
            Session Sess = null;
            try
            {
                Sess = Session.FromLog(this.intLogId, this.intLogSource);
            }
            catch { }
            this.ReqView.ShowProgressBar(false);
            this.ResView.ShowProgressBar(false);
            if (Sess != null)
            {
                if (Sess.Request != null)
                {
                    this.ReqView.SetRequest(Sess.Request);
                }
                if (Sess.Response != null)
                {
                    this.ResView.SetResponse(Sess.Response, Sess.Request);
                }
            }
        }
    }
}
