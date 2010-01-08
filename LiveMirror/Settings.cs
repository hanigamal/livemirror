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
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace LiveMirror
{
    public class Setttings
    {
        System.Xml.Schema.XmlSchema schema = new System.Xml.Schema.XmlSchema();

        Dictionary<string, string> settings = new Dictionary<string, string>();
        Dictionary<string, Type> typeHints = new Dictionary<string, Type>();
        public Dictionary<string, string> Settings { get { return settings; } }

        /// <summary>
        /// Gets, Sets or Adds a setting
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (settings.ContainsKey(key))
                    return settings[key];
                return "";
            }
            set
            {
                if (settings.ContainsKey(key))
                    settings[key] = value;
                else
                    settings.Add(key, value);
            }
        }

        public Setttings()
        {
            try
            {
                Stream schemeStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LiveMirror.SettingsSchema.xsd");
                System.Diagnostics.Debug.Assert(schemeStream != null, "No stream returned from GetManifestResourceStream for xml schema");
                schema = System.Xml.Schema.XmlSchema.Read(
                    schemeStream,
                    new System.Xml.Schema.ValidationEventHandler(
                        delegate(object sender, System.Xml.Schema.ValidationEventArgs args)
                        {
                            if (args.Severity == System.Xml.Schema.XmlSeverityType.Error)
                                throw new Exception("Invalid XML Schema : " + args.Message);
                        })
                    );

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail("Failed to load XML Schema from embedded resource : ", ex.Message);
            }
        }
        /// <summary>
        /// Loads the xml string provided. Validated against SettingsSchema.xsd
        /// </summary>
        /// <param name="xml">The XML String</param>
        public void FromXML(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            FromXML(doc);
        }
        /// <summary>
        /// Loads the XmlDocument provided. Validated against SettingsSchema.xsd
        /// </summary>
        /// <param name="doc">The loaded XmlDocument</param>
        public void FromXML(XmlDocument doc)
        {
            //If the schema was loaded sucessfully, validate the document against it
            if (schema != null)
            {
                bool xmlvalid = true;
                string lastXmlError = "";

                //Load and validate against the xml schema
                doc.Schemas.Add(schema);
                doc.Validate(new System.Xml.Schema.ValidationEventHandler(
                            delegate(object sender, System.Xml.Schema.ValidationEventArgs args)
                            {
                                if (args.Severity == System.Xml.Schema.XmlSeverityType.Error)
                                {
                                    xmlvalid = false;
                                    lastXmlError = args.Message;
                                }
                                System.Diagnostics.Debug.Assert(!(args.Severity == System.Xml.Schema.XmlSeverityType.Warning), "XML Warning parsing Settings.xml",args.Message);
                            }));
                //throw exception if there was an error in the xml file
                if (!xmlvalid)
                    throw new Exception("Invalid Settings.xml file used. " + lastXmlError);
            }

            XmlNamespaceManager nameMgr = new XmlNamespaceManager(doc.NameTable);
            nameMgr.AddNamespace("x", "Xnet");
            //Get all setting nodes
            XmlNodeList nodes = doc.SelectNodes("/x:Settings/x:Setting", nameMgr);
            if (nodes == null)
                return;
            foreach (XmlNode node in nodes)
            {
                //Exctrace the name and values for each setting
                XmlAttribute name = node.Attributes["Name"];
                XmlAttribute value = node.Attributes["Value"];
                //Ignore setting if one is missing
                if (name == null || value == null)
                    continue;
                //Add or update setting.=
                if (settings.ContainsKey(name.Value))
                    settings[name.Value] = value.Value;
                else
                    settings.Add(name.Value, value.Value);
            }
        }
        /// <summary>
        /// Saves all settings to an XML Document
        /// </summary>
        /// <returns></returns>
        public string ToXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("", "Settings", "Xnet");
            root.InnerText = ""; //Root element must not be self closing
            doc.AppendChild(root);
            //Add the <?xml blah blah
            doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "UTF-8", null), doc.DocumentElement);

            foreach (var setting in settings.OrderBy(s => s.Key))
            {
                //Setting element <Settting>
                XmlNode node = doc.CreateElement("Setting", "Xnet");

                //Name attribute Name="Key"
                XmlAttribute name = doc.CreateAttribute("Name");
                name.Value = setting.Key;
                node.Attributes.Append(name);

                //Value attribute Value="Value"
                XmlAttribute value = doc.CreateAttribute("Value");
                value.Value = setting.Value;
                node.Attributes.Append(value);

                //TypeHint attribute if there is one
                if (typeHints.ContainsKey(setting.Key))
                {
                    XmlAttribute typeHint = doc.CreateAttribute("TypeHint");
                    typeHint.Value = GetTypeHint(setting.Key).FullName;
                    node.Attributes.Append(typeHint);
                }

                //Add to document roots
                root.AppendChild(node);
            }
            //Five lines just to get out an xml document...
            StringWriter sw = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;
            doc.WriteTo(xtw);
            return sw.ToString();
        }
        /// <summary>
        /// Determins weather there is a setting with a given name
        /// </summary>
        /// <param name="name">the setting name</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return settings.ContainsKey(name);
        }
        /// <summary>
        /// Removes a given setting
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            typeHints.Remove(name);
            return settings.Remove(name);
        }
        /// <summary>
        /// Gets the type hint for a given name. Returns string type if not set.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Type GetTypeHint(string name)
        {
            if (typeHints.ContainsKey(name))
                return typeHints[name];
            else return typeof(string);
        }
        /// <summary>
        /// Sets the data type that the setting is holding. Only to suggest to the user or third-party software what type the data should contain.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void SetTypeHint(string name, Type type)
        {
            if (typeHints.ContainsKey(name))
                typeHints[name] = type;
            else
                typeHints.Add(name, type);
        }

    }
}
