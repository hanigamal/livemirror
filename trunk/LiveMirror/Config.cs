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
using System.IO;
using System.Diagnostics;

namespace LiveMirror
{
    /// <summary>
    /// Provides a string key, string data configuration store.
    /// Also provides a way to convert data from a string to a given type.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The setting store. handles saving/loading
        /// </summary>
        Setttings settings;

        public Config()
        {
            settings = new Setttings();
        }
        /// <summary>
        /// Loads an XML file
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    settings.FromXML(File.ReadAllText(filename, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Debug.Fail(String.Format("Could not load settings file '{0}' : {1}", filename, ex.Message));
            }
        }
        /// <summary>
        /// Saves settings to XML File
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, settings.ToXML(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.Fail(String.Format("Could not save settings file '{0}' : {1}",filename,ex.Message));
            }
        }
        /// <summary>
        /// Gets a setting in string format. Thread-safe
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Get(string name)
        {
            string value;
            lock (settings)
            {
                value = settings[name];
            }
            return value;
        }
        /// <summary>
        /// Gets a tries to convert data to a given type using Enum.Parse or Convert.ChangeType.
        /// Can reset the value to a given default fallback if unable to convert or retrive the data.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the data to</typeparam>
        /// <param name="name">Setting Name</param>
        /// <param name="resetOnFail">Weather to set the setting to the fallback if it cannot be converted</param>
        /// <param name="fallback">Returned if data cannot be converted</param>
        /// <returns></returns>
        public TResult Get<TResult>(string name, bool resetOnFail, TResult fallback)// where TResult : IConvertible
        {
            //Set to fallback if the setting is not set
            if (!settings.Contains(name))
                settings[name] = fallback.ToString();
            //Get the setting value string
            string value = Get(name);
            try
            {
                //Set the TypeHint for this setting
                settings.SetTypeHint(name, typeof(TResult));

                //Convert enums with Enum.Parse, since Convert.ChangeType cant
                if (typeof(TResult).IsSubclassOf(typeof(Enum)))
                    return (TResult)Enum.Parse(typeof(TResult), value);

                //Return the converted value
                return (TResult)Convert.ChangeType(value, typeof(TResult));
            }
            catch
            {
                //Couldnt convert the data, reset it if required
                if (resetOnFail)
                    Set(name, fallback.ToString());
                return fallback;
            }
        }
        /// <summary>
        /// Sets a setting with the ToString of a given object
        /// </summary>
        /// <param name="name">The setting name</param>
        /// <param name="value">The value object</param>
        public void Set(string name, object value)
        {
            Set(name, value.ToString());
        }
        /// <summary>
        /// Sets a setting. Thread-Safe
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Set(string name, string value)
        {
            lock (settings)
            {
                settings[name] = value;
            }
        }
        /// <summary>
        /// Removes a setting. Thread-safe
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            lock (settings) settings.Remove(name);
        }
    }
}