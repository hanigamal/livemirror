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
using System.Text;
using System.Windows.Forms;

namespace LiveMirror
{
    public partial class FileActionControl : UserControl
    {
        public FileActionControl()
        {
            InitializeComponent();
        }
        public event EventHandler Action1;
        public event EventHandler Action2;
        public event EventHandler Action3;

        public void SetAction1(string text)
        {
            btnAction1.Text = text;
            btnAction1.Visible = true;
        }
        public void SetAction2(string text)
        {
            btnAction2.Text = text;
            btnAction2.Visible = true;
        }
        public void SetAction3(string text)
        {
            btnAction3.Text = text;
            btnAction3.Visible = true;
        }
        public void SetFileInfo(string fileName, string status)
        {
            lblFileName.Text = fileName;
            lblStatus.Text = status;
        }

        private void btnAction1_Click(object sender, EventArgs e)
        {
            Action1.Raise(this,e);
        }

        private void btnAction2_Click(object sender, EventArgs e)
        {
            Action2.Raise(this, e);
        }

        private void btnAction3_Click(object sender, EventArgs e)
        {
            Action3.Raise(this, e);
        }


    }
}
