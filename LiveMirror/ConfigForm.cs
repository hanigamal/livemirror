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
            try
            {
                lstChanges.Items.Clear();
                ConflictResolutionForm conflict = new ConflictResolutionForm(fromDir, toDir,config);
                DialogResult result = conflict.ShowDialog();
                if (result != DialogResult.OK)
                {
                    MessageBox.Show("Cannot mirror, Conflicts not resolved", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                mirror = new Mirror(fromDir, toDir);
                mirror.LogMessage += new EventHandler<EventArgs<string>>(mirror_LogMessage);
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                btnFrom.Enabled = false;
                btnTo.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void mirror_LogMessage(object sender, EventArgs<string> e)
        {
            string message = e.Data;
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate() { mirror_LogMessage(sender, e); }));
            else
            {
                lstChanges.Items.Add(message);
                lstChanges.TopIndex = lstChanges.Items.Count;
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
