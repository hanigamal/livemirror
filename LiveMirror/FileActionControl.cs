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
