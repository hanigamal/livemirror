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
