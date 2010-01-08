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

            //New Debug Listener
            DefaultTraceListener traceListener = new DefaultTraceListener();
            traceListener.AssertUiEnabled = true;
            Debug.Listeners.Add(traceListener);

            //Trick to wait for the IDE to attatch to the process
            Debug.Assert(true);

            if (Debugger.IsAttached)
                Application.Run(new ConfigForm());
            else
            {
                //Catch unhandled exceptions if there is no debugger attached
                try
                {
                    Application.Run(new ConfigForm());
                }
                catch (Exception ex)
                {
                    string errorMsg = (
                           DateTime.Now.ToUniversalTime() + "\n"
                         + "Unhandled Exception :\t" + ex.Message + "\n"
                         + "Raised in :\t\t" + ex.TargetSite + "\n\n"
                         + "The Program cannot continue and will now exit.\n\n"
                         + "If submitting an error report for this,\n"
                         + "Please include debug.txt from the programs execution directory.\n"
                         + "-----------------------------------------------------------------\n"
                         + "Stack Trace : \n" + ex.StackTrace + ")"
                         ).Replace("\n",Environment.NewLine);

                    try
                    {
                        errorMsg = DateTime.Now.ToUniversalTime() + Environment.NewLine + errorMsg;
                        System.IO.File.WriteAllText("debug.txt", errorMsg);
                        MessageBox.Show(errorMsg, "Fatal Error!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    catch
                    { }
                }
            }
        }
    }
}
