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

namespace LiveMirror
{
    public partial class ConfigForm : Form
    {
        Config config = new Config();
        Mirror mirror;
        string fromDir = "", toDir = "";

        public ConfigForm()
        {
            InitializeComponent();

            config.Load("settings.xml");
            InputBox.Config = config;
            new FormPositionSaver("Form.Config", this, config);
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            SetFromDir(config.Get<string>("Mirror.FromDir", true, ""));
            SetToDir(config.Get<string>("Mirror.ToDir", true, ""));
            chkAutoStart.Checked = config.Get<bool>("Mirror.AutoStart", true, false);
        }

        private void btnFrom_Click(object sender, EventArgs e)
        {
            dlgMirrorDir.SelectedPath = (fromDir != "") ? fromDir : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DialogResult result = dlgMirrorDir.ShowDialog();
            if (result == DialogResult.OK)
            {
                SetFromDir(dlgMirrorDir.SelectedPath);
            }
        }

        private void btnTo_Click(object sender, EventArgs e)
        {
            dlgMirrorDir.SelectedPath = (toDir != "") ? toDir : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DialogResult result = dlgMirrorDir.ShowDialog();
            if (result == DialogResult.OK)
            {
                SetToDir(dlgMirrorDir.SelectedPath);
            }
        }
        private void SetFromDir(string dir)
        {
            fromDir = dir;
            lblFrom.Text = fromDir;
            config.Set("Mirror.FromDir", fromDir);
        }
        private void SetToDir(string dir)
        {
            toDir = dir;
            lblTo.Text = toDir;
            config.Set("Mirror.ToDir", toDir);
        }
        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            config.Save("settings.xml");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //try
            //{
                lstChanges.Items.Clear();
                ConflictResloution resolver = new ConflictResloution(config);
                if (resolver.Proccess(fromDir, toDir) > 0)
                {
                    DialogResult result = ConflictResolutionForm.Show(resolver.Conflicts,config);
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show("Cannot mirror, Conflicts not resolved", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                }
                mirror = new Mirror(fromDir, toDir);
                mirror.LogMessage += new EventHandler<EventArgs<string>>(mirror_LogMessage);
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                btnFrom.Enabled = false;
                btnTo.Enabled = false;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        void mirror_LogMessage(object sender, EventArgs<string> e)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate() { mirror_LogMessage(sender, e); }));
            else
            {
                string message = e.Data;
                lstChanges.Items.Add(String.Format("[{0}] {1}",DateTime.Now.ToString("HH:mm:ss"),message));
                lstChanges.TopIndex = lstChanges.Items.Count - 1;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            mirror.Stop();
            mirror = null;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnFrom.Enabled = true;
            btnTo.Enabled = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            btnStop.PerformClick();
            Application.Exit();
        }

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            config.Set("Mirror.AutoStart", chkAutoStart.Checked);
        }

        private void ConfigForm_SizeChanged(object sender, EventArgs e)
        {
            lstChanges.Height = this.Height - 160;
        }

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            if (chkAutoStart.Checked
                && System.IO.Directory.Exists(fromDir)
                && System.IO.Directory.Exists(toDir))
                btnStart.PerformClick();
        }

        private void btnSetIgnored_Click(object sender, EventArgs e)
        {
            new IgnoredPathsForm(config).ShowDialog();
        }

        private void lblFrom_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(lblFrom.Text))
                System.Diagnostics.Process.Start("explorer.exe", lblFrom.Text);
            else
                MessageBox.Show("Directory does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void lblTo_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(lblTo.Text))
                System.Diagnostics.Process.Start("explorer.exe", lblTo.Text);
            else
                MessageBox.Show("Directory does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
