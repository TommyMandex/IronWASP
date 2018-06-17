using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IronWASP.Docs
{
    public partial class DocForCsrfTester : Form
    {
        public DocForCsrfTester()
        {
            InitializeComponent();
        }

        static DocForCsrfTester DocWindow = null;

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                DocWindow = new DocForCsrfTester();
                DocWindow.Show();
            }
            DocWindow.Activate();
        }

        static bool IsWindowOpen()
        {
            if (DocWindow == null)
            {
                return false;
            }
            else if (DocWindow.IsDisposed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
