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
    public partial class InputBox : Form
    {
        public static Config Config { private get; set; }
        public static string Show(string title, string prompt, string text)
        {
            InputBox box = new InputBox();
            box.lblPrompt.Text = prompt;
            box.Text = title;
            box.txtInput.Text = text;
            box.txtInput.SelectionStart = text.Length;
            DialogResult result = box.ShowDialog();
            if (result == DialogResult.OK)
                return box.txtInput.Text;
            return text;
        }

        private InputBox()
        {
            if (Config != null)
                new FormPositionSaver("Form.InputBox", this, Config);
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                btnOk.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                btnCancel.PerformClick();
            }
        }
    }
}
