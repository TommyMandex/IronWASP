using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IronWASP.Docs
{
    public partial class DocForBrokenAuthTester : Form
    {
        static DocForBrokenAuthTester DocWindow = null;
        
        public DocForBrokenAuthTester()
        {
            InitializeComponent();
        }

        internal static void OpenWindow()
        {
            if (!IsWindowOpen())
            {
                DocWindow = new DocForBrokenAuthTester();
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
