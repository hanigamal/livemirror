using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace LiveMirror
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DefaultTraceListener traceListener = new DefaultTraceListener();
            traceListener.AssertUiEnabled = true;
            Debug.Listeners.Add(traceListener);
            if (Debugger.IsAttached)
                Application.Run(new ConfigForm());
            else
            {
                try
                {
                    Application.Run(new ConfigForm());
                }
                catch (Exception ex)
                {
                    string errorMsg = "Unhandled Exception :\t" + ex.Message + "" + Environment.NewLine
                         + "Raised in :\t\t" + ex.TargetSite + Environment.NewLine
                         + "-----------------------------------------------------------------" + Environment.NewLine
                         + "Stack Trace : \n" + ex.StackTrace + ")";

                    MessageBox.Show(errorMsg, "Fatal Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }
    }
}
