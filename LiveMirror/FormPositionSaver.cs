using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LiveMirror
{
    public class FormPositionSaver
    {
        string formName;
        Form form;
        Config config;
        bool saveEnabled = true;

        public FormPositionSaver(string name, Form form, Config config)
        {
            this.formName = name;
            this.form = form;
            this.config = config;
            form.StartPosition = FormStartPosition.Manual;
            form.Load += new EventHandler(form_Load);
            form.FormClosed += new FormClosedEventHandler(form_FormClosed);
            form.LocationChanged += new EventHandler(form_LocationChanged);
            form.ResizeEnd += new EventHandler(form_ResizeEnd);
        }

        public void Load()
        {
            saveEnabled = false;
            form.Top = config.Get<int>(formName + ".Position.Top", true, form.Top);
            form.Left = config.Get<int>(formName + ".Position.Left", true, form.Left);
            form.Width = config.Get<int>(formName + ".Position.Width", true, form.Width);
            form.Height = config.Get<int>(formName + ".Position.Height", true, form.Height);
            form.WindowState = config.Get<FormWindowState>(formName + ".Position.WindowState", true, FormWindowState.Normal);
            saveEnabled = true;
        }
        public void Save()
        {
            if (!saveEnabled)
                return;
            config.Set(formName + ".Position.Top", form.Top.ToString());
            config.Set(formName + ".Position.Left", form.Left.ToString());
            config.Set(formName + ".Position.Width", form.Width.ToString());
            config.Set(formName + ".Position.Height", form.Height.ToString());
            config.Set(formName + ".Position.WindowState", Enum.GetName(typeof(FormWindowState), form.WindowState));
        }

        void form_ResizeEnd(object sender, EventArgs e)
        {
            Save();
        }

        void form_LocationChanged(object sender, EventArgs e)
        {
            Save();
        }

        void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Save();
        }

        void form_Load(object sender, EventArgs e)
        {
            Load();
        }
    }
}
