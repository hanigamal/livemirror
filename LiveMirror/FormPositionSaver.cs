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
using System.Text;
using System.Windows.Forms;

namespace LiveMirror
{
    /// <summary>
    /// Saves the position of a form in a config file
    /// </summary>
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
