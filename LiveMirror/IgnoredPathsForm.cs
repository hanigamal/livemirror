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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveMirror
{
    public partial class IgnoredPathsForm : Form
    {
        Config config;
        List<IgnoredPathControl> IgnoredPaths = new List<IgnoredPathControl>();

        public IgnoredPathsForm(Config config)
        {
            this.config = config;

            InitializeComponent();

            new FormPositionSaver("Form.IgnoredPath", this, config);
            foreach(var path in config.Get<string>("Mirror.IgnoredPaths",true,"").Split('|'))
                if (path != "")
                    pnlPaths.Controls.Add(new IgnoredPathControl(path,this));
        }
        public void RemovePath(IgnoredPathControl path)
        {
            pnlPaths.Controls.Remove(path);
            Save();
        }
        public void Save()
        {
            StringBuilder newIgnoredPaths = new StringBuilder();
            foreach (var path in pnlPaths.Controls.OfType<IgnoredPathControl>())
                newIgnoredPaths.AppendFormat("{0}|", path.Path);
            config.Set("Mirror.IgnoredPaths",newIgnoredPaths.ToString());
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = InputBox.Show("Path", "Enter Path", "");
            if (input != "")
            {
                pnlPaths.Controls.Add(new IgnoredPathControl(input, this));
                Save();
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
