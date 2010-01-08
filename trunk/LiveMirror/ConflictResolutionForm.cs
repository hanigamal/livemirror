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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LiveMirror
{
    /// <summary>
    /// Presents users with all detected conflicts and their resolutions
    /// </summary>
    public partial class ConflictResolutionForm : Form
    {
        /// <summary>
        /// Modally shows the form with the given conflicts
        /// </summary>
        /// <param name="conflicts">Conflicts</param>
        /// <param name="config"></param>
        /// <returns>Ok or Cancel</returns>
        public static DialogResult Show(List<ConflictResloution.Conflict> conflicts, Config config)
        {
            //Create the form
            ConflictResolutionForm form = new ConflictResolutionForm(config);

            foreach (ConflictResloution.Conflict conflict in conflicts)
            {
                //New file action control
                FileActionControl fileAction = new FileActionControl();
                //Map all action clicks to the respective resolution
                fileAction.Action1 += new EventHandler((sender, e) =>
                {
                    //Perform the resolution
                    conflict.PerformResolution(conflict.Resolutions[0]);
                    //Remove the control from the panel
                    form.pnlConflicts.Controls.Remove(fileAction);
                });
                fileAction.Action2 += new EventHandler((sender, e) => 
                {
                    conflict.PerformResolution(conflict.Resolutions[1]);
                    form.pnlConflicts.Controls.Remove(fileAction);
                });
                fileAction.Action3 += new EventHandler((sender, e) => 
                {
                    conflict.PerformResolution(conflict.Resolutions[2]);
                    form.pnlConflicts.Controls.Remove(fileAction);
                });
                //Function to insert spaces in between camel cased words ie BlahBlah => Blah Blah
                Func<Enum,string> getText = e => Regex.Replace(e.ToString(), "([a-z])([A-Z])", "$1 $2", RegexOptions.None);
                //Setup the file action control
                fileAction.SetFileInfo(conflict.RelativePathName, getText(conflict.Type));
                //Set resolutions
                if (!conflict.Resolutions[0].Equals(ConflictResloution.Resolution.None))
                    fileAction.SetAction1(getText(conflict.Resolutions[0].Type));
                if (!conflict.Resolutions[1].Equals(ConflictResloution.Resolution.None))
                    fileAction.SetAction2(getText(conflict.Resolutions[1].Type));
                if (!conflict.Resolutions[2].Equals(ConflictResloution.Resolution.None))
                    fileAction.SetAction3(getText(conflict.Resolutions[2].Type));
                //Add to panel
                form.pnlConflicts.Controls.Add(fileAction);

            }
            //Show the form modally
            return form.ShowDialog();
        }
        /// <summary>
        /// Holds the application configuration
        /// </summary>
        Config config;
        private ConflictResolutionForm(Config config)
        {
            InitializeComponent();

            this.config = config;

            new FormPositionSaver("Form.ConflictResolution", this, config);
        }


        private void pnlConflicts_ControlRemoved(object sender, ControlEventArgs e)
        {
            //Close the form if all conflicts have been resolved
            if (pnlConflicts.Controls.Count == 0)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void NewConflictResolutionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (pnlConflicts.Controls.Count == 0)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
        }
    }
}
