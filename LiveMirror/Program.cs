#region License
/* 
 * LiveMirror - Directory Sync
 * Copyright (C) 2009  Sam Stevens
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 * 
 */
#endregion

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
