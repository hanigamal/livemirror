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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveMirror
{
    public partial class IgnoredPathControl : UserControl
    {
        IgnoredPathsForm form;
        public string Path { get; private set; }

        public IgnoredPathControl(string path, IgnoredPathsForm form)
        {
            this.Path = path;
            this.form = form;
            
            InitializeComponent();

            lblFileName.Text = Path;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            Path = InputBox.Show("Path", "Enter New Path", Path);
            lblFileName.Text = Path;
            form.Save();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            form.RemovePath(this);
        }
    }
}
